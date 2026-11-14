using System.Drawing.Drawing2D;
using GeorgiaTransitIntelligence.Models;
using GeorgiaTransitIntelligence.Services;

namespace GeorgiaTransitIntelligence.Mapping
{
    /// <summary>
    /// Interactive vector map control for Georgia transit visualization.
    /// Supports pan, zoom, zoom-dependent rendering, vehicle simulation, route highlighting, and stop inspection.
    /// Fulfills Section 10 and 12 of Master Codex.
    /// </summary>
    public class GeoMapControl : UserControl
    {
        // Viewport state
        private double centerLat = 42.15;
        private double centerLon = 43.30;
        private double zoom = 8.0; // 7.0 = National, 11.0 = Regional, 14.0 = Kutaisi City
        private bool fitCountryOnResize = true;

        // Mouse interaction state
        private bool isPanning = false;
        private StreetMapTiles? streetTiles;
        private PictureBox? yandexLogo;
        private bool usingYandex;
        internal bool YandexKeyRejected => usingYandex && streetTiles?.KeyRejected == true;

        internal void SelectBasemap(bool useStreetMap, string? yandexKey = null)
        {
            streetTiles?.Dispose();
            usingYandex = useStreetMap && yandexKey is not null;
            streetTiles = useStreetMap ? new StreetMapTiles(Invalidate, yandexKey) : null;
            if (yandexLogo is not null) yandexLogo.Visible = false;
            if (usingYandex && yandexLogo is null)
            {
                using var stream = typeof(GeoMapControl).Assembly.GetManifestResourceStream(
                    "GeorgiaTransitIntelligence.Mapping.YandexLogo.png")!;
                using var logo = Image.FromStream(stream);
                yandexLogo = new PictureBox
                {
                    Name = "picYandexLogo",
                    Image = new Bitmap(logo),
                    Size = logo.Size,
                    Location = new Point(Width - logo.Width, Height - logo.Height),
                    Anchor = AnchorStyles.Bottom | AnchorStyles.Right,
                    Cursor = Cursors.Hand,
                    AccessibleName = "Open Yandex Maps",
                    Visible = false
                };
                yandexLogo.Click += (_, _) => System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo("https://yandex.com/maps/") { UseShellExecute = true });
                Controls.Add(yandexLogo);
            }
            Invalidate();
        }

        protected override void OnVisibleChanged(EventArgs e)
        {
            base.OnVisibleChanged(e);
            if (!Visible) streetTiles?.Hide();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                streetTiles?.Dispose();
                yandexLogo?.Image?.Dispose();
            }
            base.Dispose(disposing);
        }
        private Point panStartPoint;
        private double panStartLat;
        private double panStartLon;

        // Data to render
        private List<VehiclePosition> activeVehicles = new();
        private List<(double Lat, double Lon)> highlightedPath = new();
        private Color highlightedPathColor = Color.FromArgb(200, 15, 108, 189);
        private List<MunicipalStop> displayedStops = new();
        private VehiclePosition? selectedVehicle = null;
        private MunicipalStop? selectedStop = null;

        // Geographic boundaries of Georgia: Lat ~41.0 to 43.6, Lon ~40.0 to 46.8
        public static readonly (double Lat, double Lon) GeorgiaCenter = (42.00, 43.50);
        public static readonly (double Lat, double Lon) KutaisiCenter = (42.2662, 42.7180);

        // Styling brushes and pens
        private readonly Brush bgBrush = new SolidBrush(Color.FromArgb(222, 237, 239));
        private readonly Brush landBrush = new SolidBrush(Color.FromArgb(247, 249, 242));
        private readonly Pen borderPen = new Pen(Color.FromArgb(203, 219, 213), 1.5f);
        private readonly Pen highwayPen = new Pen(Color.FromArgb(116, 169, 153), 4f);
        private readonly Pen highwayBorderPen = new Pen(Color.FromArgb(91, 143, 133), 5.5f);
        private readonly Pen secondaryRoadPen = new Pen(Color.FromArgb(217, 226, 221), 2f);
        private readonly Pen highlightedPen = new Pen(Color.FromArgb(15, 108, 189), 4.5f);
        private readonly Font cityFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        private readonly Font stopFont = new Font("Segoe UI", 7.5F, FontStyle.Regular);
        private readonly Font badgeFont = new Font("Segoe UI", 7F, FontStyle.Bold);

        public event Action<VehiclePosition>? VehicleSelected;
        public event Action<MunicipalStop>? StopSelected;
        public event Action<double, double, double>? ViewportChanged;

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public double CenterLatitude
        {
            get => centerLat;
            set { centerLat = value; Invalidate(); }
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public double CenterLongitude
        {
            get => centerLon;
            set { centerLon = value; Invalidate(); }
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public double ZoomLevel
        {
            get => zoom;
            set
            {
                fitCountryOnResize = false;
                zoom = Math.Clamp(value, 6.0, 16.5);
                Invalidate();
            }
        }

        private string operatorFilter = "ALL";

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public string OperatorFilter
        {
            get => operatorFilter;
            set { operatorFilter = value ?? "ALL"; Invalidate(); }
        }

        [System.ComponentModel.Browsable(false)]
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public VehiclePosition? SelectedVehicle => selectedVehicle;

        public GeoMapControl()
        {
            SetStyle(ControlStyles.AllPaintingInWmPaint |
                     ControlStyles.UserPaint |
                     ControlStyles.OptimizedDoubleBuffer |
                     ControlStyles.ResizeRedraw, true);

            BackColor = Color.FromArgb(222, 237, 239);
            Cursor = Cursors.Hand;
        }

        public void SetVehicles(List<VehiclePosition> vehicles)
        {
            activeVehicles = vehicles ?? new List<VehiclePosition>();
            Invalidate();
        }

        public void SetStops(List<MunicipalStop> stops)
        {
            displayedStops = stops ?? new List<MunicipalStop>();
            Invalidate();
        }

        public void SetHighlightedPath(IReadOnlyList<(double Lat, double Lon)> path, Color color)
        {
            highlightedPath = path != null ? path.ToList() : new List<(double Lat, double Lon)>();
            highlightedPathColor = color;
            Invalidate();
        }

        public void ZoomToGeorgia()
        {
            FitBounds(41.05, 43.50, 40.20, 46.70);
            fitCountryOnResize = true;
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            if (fitCountryOnResize && Width > 96 && Height > 96) ZoomToGeorgia();
        }

        public void ZoomToKutaisi()
        {
            fitCountryOnResize = false;
            centerLat = 42.2662;
            centerLon = 42.7180;
            zoom = 13.8;
            ViewportChanged?.Invoke(centerLat, centerLon, zoom);
            Invalidate();
        }

        public void ZoomIn()
        {
            ZoomLevel += 0.8;
            ViewportChanged?.Invoke(centerLat, centerLon, zoom);
        }

        public void ZoomOut()
        {
            double countryZoom = GetFitZoom(41.05, 43.50, 40.20, 46.70);
            if (zoom - 0.8 <= countryZoom)
            {
                ZoomToGeorgia();
                return;
            }
            ZoomLevel -= 0.8;
            ViewportChanged?.Invoke(centerLat, centerLon, zoom);
        }

        public void FitBounds(double minLat, double maxLat, double minLon, double maxLon)
        {
            centerLat = StreetMapTiles.YLatitude(
                (StreetMapTiles.LatitudeY(minLat) + StreetMapTiles.LatitudeY(maxLat)) / 2);
            centerLon = (minLon + maxLon) / 2.0;
            ZoomLevel = GetFitZoom(minLat, maxLat, minLon, maxLon);

            ViewportChanged?.Invoke(centerLat, centerLon, zoom);
        }

        private double GetFitZoom(double minLat, double maxLat, double minLon, double maxLon)
        {
            double lonSpan = Math.Max(0.02, maxLon - minLon);
            double mapHeight = Math.Max(0.02 / 360,
                StreetMapTiles.LatitudeY(minLat) - StreetMapTiles.LatitudeY(maxLat));
            double zoomLat = Math.Log2(Math.Max(1, Height - 96) / (256 * mapHeight));
            double zoomLon = Math.Log2(Math.Max(1, Width - 96) / (256 * lonSpan / 360));
            return Math.Min(zoomLat, zoomLon);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                // Check if user clicked on a vehicle marker
                PointF hub = GeoToScreen(KutaisiCenter.Lat, KutaisiCenter.Lon);
                if (activeVehicles.Any(IsGrouped) && Math.Pow(e.X - hub.X, 2) + Math.Pow(e.Y - hub.Y, 2) <= 256)
                {
                    ZoomToKutaisi();
                    return;
                }
                var clickedVehicle = HitTestVehicle(e.Location);
                if (clickedVehicle != null)
                {
                    selectedVehicle = clickedVehicle;
                    VehicleSelected?.Invoke(clickedVehicle);
                    Invalidate();
                    return;
                }

                // Check if user clicked on a stop marker (at higher zoom)
                if (zoom >= 12.0)
                {
                    var clickedStop = HitTestStop(e.Location);
                    if (clickedStop != null)
                    {
                        selectedStop = clickedStop;
                        StopSelected?.Invoke(clickedStop);
                        Invalidate();
                        return;
                    }
                }

                isPanning = true;
                panStartPoint = e.Location;
                panStartLat = centerLat;
                panStartLon = centerLon;
                Cursor = Cursors.SizeAll;
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            if (isPanning)
            {
                fitCountryOnResize = false;
                int dx = e.X - panStartPoint.X;
                int dy = e.Y - panStartPoint.Y;

                double scale = Math.Pow(2.0, zoom);
                double dLon = -dx * (360.0 / (256.0 * scale));
                double nextLat = StreetMapTiles.YLatitude(StreetMapTiles.LatitudeY(panStartLat) - dy / (256.0 * scale));

                centerLat = Math.Clamp(nextLat, 40.5, 44.5);
                centerLon = Math.Clamp(panStartLon + dLon, 39.0, 48.0);

                ViewportChanged?.Invoke(centerLat, centerLon, zoom);
                Invalidate();
            }
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            base.OnMouseUp(e);
            if (isPanning)
            {
                isPanning = false;
                Cursor = Cursors.Hand;
            }
        }

        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);
            if (e.Delta > 0)
                ZoomIn();
            else if (e.Delta < 0)
                ZoomOut();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            // 1. Draw Background Water / Canvas
            g.FillRectangle(bgBrush, ClientRectangle);

            DrawLandmass(g);
            bool hasStreetTiles = Visible &&
                streetTiles?.Draw(g, ClientRectangle, centerLat, centerLon, zoom) == true;
            if (yandexLogo is not null)
                yandexLogo.Visible = usingYandex && hasStreetTiles;
            if (!hasStreetTiles) DrawRoadNetwork(g);

            // 4. Draw Municipal Stops (if in Kutaisi and zoom >= 12)
            if (zoom >= 12.0)
            {
                DrawMunicipalStops(g);
            }

            // 5. Draw Highlighted Path (if active itinerary)
            if (highlightedPath.Count >= 2)
            {
                DrawHighlightedPath(g);
            }

            // 6. Draw City Labels & Transport Terminals
            if (!hasStreetTiles) DrawCities(g);

            // 7. Draw Active Vehicles (Animated Simulators)
            DrawVehicles(g);

            // 8. Draw HUD / Map Legend & Scale
            DrawHudOverlay(g);
            if (usingYandex && streetTiles?.Failure is { } failure)
            {
                var status = new Rectangle(16, 72, Math.Max(1, Math.Min(Width - 32, 400)), 42);
                g.FillRectangle(Brushes.White, status);
                TextRenderer.DrawText(g, failure, stopFont, status, Color.DarkRed,
                    TextFormatFlags.WordBreak | TextFormatFlags.VerticalCenter);
            }
            if (hasStreetTiles && !usingYandex)
            {
                const string credit = "© OpenStreetMap contributors";
                Size size = TextRenderer.MeasureText(credit, stopFont);
                var creditBounds = new Rectangle(Width - size.Width - 8, Height - size.Height - 8, size.Width + 4, size.Height);
                g.FillRectangle(Brushes.White, creditBounds);
                TextRenderer.DrawText(g, credit, stopFont, creditBounds, Color.FromArgb(24, 48, 54));
            }
        }

        private void DrawLandmass(Graphics g)
        {
            // Vector outline of Georgia roughly covering Lat 41.0 to 43.6, Lon 40.0 to 46.8
            var georgiaPolygon = new (double Lat, double Lon)[]
            {
                (41.50, 41.55), (41.65, 41.64), (41.80, 41.77), (42.15, 41.67),
                (42.38, 41.58), (42.80, 41.10), (43.25, 40.20), (43.50, 40.35),
                (43.40, 41.10), (43.20, 42.45), (43.05, 42.75), (42.80, 43.60),
                (42.65, 44.60), (42.45, 45.40), (42.10, 46.10), (41.90, 46.50),
                (41.60, 46.70), (41.25, 46.40), (41.05, 45.30), (41.20, 44.40),
                (41.35, 43.40), (41.50, 42.50), (41.40, 41.80)
            };

            var screenPoints = georgiaPolygon.Select(pt => GeoToScreen(pt.Lat, pt.Lon)).ToArray();
            g.FillPolygon(landBrush, screenPoints);
            g.DrawPolygon(borderPen, screenPoints);
        }

        private void DrawRoadNetwork(Graphics g)
        {
            // Draw E60 Highway (Tbilisi - Gori - Kutaisi - Batumi)
            var highwayScreenPts = GeometryService.E60HighwayCorridor
                .Select(pt => GeoToScreen(pt.Lat, pt.Lon))
                .ToArray();

            g.DrawLines(highwayBorderPen, highwayScreenPts);
            g.DrawLines(highwayPen, highwayScreenPts);

            // Secondary branches:
            // Kutaisi -> Zugdidi -> Mestia
            var zugdidiPath = new[] { (42.2578, 42.6678), (42.5088, 41.8709), (43.0440, 42.7297) }
                .Select(pt => GeoToScreen(pt.Item1, pt.Item2)).ToArray();
            g.DrawLines(secondaryRoadPen, zugdidiPath);

            // Tbilisi -> Telavi (Kakheti)
            var telaviPath = new[] { (41.6938, 44.8015), (41.9198, 45.4731) }
                .Select(pt => GeoToScreen(pt.Item1, pt.Item2)).ToArray();
            g.DrawLines(secondaryRoadPen, telaviPath);

            // Khashuri -> Borjomi -> Akhaltsikhe
            var southPath = new[] { (41.9942, 43.5999), (41.8398, 43.3850), (41.6390, 42.9826) }
                .Select(pt => GeoToScreen(pt.Item1, pt.Item2)).ToArray();
            g.DrawLines(secondaryRoadPen, southPath);
        }

        private void DrawMunicipalStops(Graphics g)
        {
            using var stopBrush = new SolidBrush(Color.FromArgb(180, 30, 136, 229));
            using var selectedStopBrush = new SolidBrush(Color.FromArgb(220, 220, 38, 38));
            using var stopBorderPen = new Pen(Color.White, 1.2f);

            foreach (var stop in displayedStops)
            {
                var pt = GeoToScreen(stop.Latitude, stop.Longitude);
                if (pt.X < -20 || pt.X > Width + 20 || pt.Y < -20 || pt.Y > Height + 20)
                    continue;

                bool isSelected = selectedStop != null && selectedStop.StopId == stop.StopId;
                float r = isSelected ? 6.5f : 4f;

                g.FillEllipse(isSelected ? selectedStopBrush : stopBrush, pt.X - r, pt.Y - r, r * 2, r * 2);
                g.DrawEllipse(stopBorderPen, pt.X - r, pt.Y - r, r * 2, r * 2);

                if (zoom >= 14.2)
                {
                    g.DrawString(stop.StopName, stopFont, Brushes.DimGray, pt.X + 5, pt.Y - 5);
                }
            }
        }

        private void DrawHighlightedPath(Graphics g)
        {
            using var pen = new Pen(highlightedPathColor, 5.5f)
            {
                DashStyle = DashStyle.Solid,
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };

            var pts = highlightedPath.Select(pt => GeoToScreen(pt.Lat, pt.Lon)).ToArray();
            g.DrawLines(pen, pts);
        }

        private void DrawCities(Graphics g)
        {
            using var cityDotBrush = new SolidBrush(Color.FromArgb(24, 38, 68));
            using var whitePen = new Pen(Color.White, 1.8f);

            foreach (var (cityName, (lat, lon)) in GeometryService.KnownCities)
            {
                // Filter city labels by zoom level
                bool isMajor = cityName is "Tbilisi" or "Kutaisi" or "Batumi" or "Zugdidi" or "Poti";
                if (!isMajor && zoom < 9.5) continue;
                if (cityName.Contains('(') && zoom < 11.5) continue;

                var pt = GeoToScreen(lat, lon);
                if (pt.X < -40 || pt.X > Width + 40 || pt.Y < -40 || pt.Y > Height + 40)
                    continue;

                float r = isMajor ? 5f : 3.5f;
                g.FillEllipse(cityDotBrush, pt.X - r, pt.Y - r, r * 2, r * 2);
                g.DrawEllipse(whitePen, pt.X - r, pt.Y - r, r * 2, r * 2);

                g.DrawString(cityName, cityFont, Brushes.Black, pt.X + 7, pt.Y - 8);
            }
        }

        private bool IsGrouped(VehiclePosition vehicle) =>
            zoom < 10 && vehicle.Category == TransportCategory.MunicipalBus &&
            string.Equals(operatorFilter, "ALL", StringComparison.OrdinalIgnoreCase);

        private void DrawVehicles(Graphics g)
        {
            int grouped = activeVehicles.Count(IsGrouped);
            if (grouped > 0)
            {
                PointF hub = GeoToScreen(KutaisiCenter.Lat, KutaisiCenter.Lon);
                using var clusterBrush = new SolidBrush(VehicleColor(TransportCategory.MunicipalBus));
                using var whiteBorder = new Pen(Color.White, 2f);
                g.FillEllipse(clusterBrush, hub.X - 16, hub.Y - 16, 32, 32);
                g.DrawEllipse(whiteBorder, hub.X - 16, hub.Y - 16, 32, 32);
                using var center = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString(grouped.ToString(), cityFont, Brushes.White, new RectangleF(hub.X - 16, hub.Y - 16, 32, 32), center);
            }

            foreach (var v in activeVehicles)
            {
                if (IsGrouped(v)) continue;
                if (!string.Equals(operatorFilter, "ALL", StringComparison.OrdinalIgnoreCase) &&
                    !v.OperatorName.Contains(operatorFilter, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var pt = GeoToScreen(v.CurrentLatitude, v.CurrentLongitude);
                if (pt.X < -50 || pt.X > Width + 50 || pt.Y < -50 || pt.Y > Height + 50)
                    continue;

                DrawVehicleMarker(g, v, pt);
            }
        }

        private void DrawVehicleMarker(Graphics g, VehiclePosition v, PointF pt)
        {
            bool isSelected = selectedVehicle != null && selectedVehicle.VehicleId == v.VehicleId;

            Color mainColor = VehicleColor(v.Category);
            Color borderColor = isSelected ? Color.Gold : Color.White;
            string badgeText = v.Category switch
            {
                TransportCategory.MunicipalBus => $"#{v.RouteNumber}",
                TransportCategory.Marshrutka => "MR",
                _ => v.OperatorName.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "Bus"
            };

            using var brush = new SolidBrush(mainColor);
            using var borderPen = new Pen(borderColor, isSelected ? 2.8f : 1.6f);

            float radius = isSelected ? 13f : 10.5f;

            // Draw vehicle body circle
            g.FillEllipse(brush, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);
            g.DrawEllipse(borderPen, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);

            // Draw directional pointer arrow if moving
            if (v.HeadingDegrees > 0)
            {
                double rad = (v.HeadingDegrees - 90.0) * (Math.PI / 180.0);
                float arrowDist = radius + 3.5f;
                PointF arrowTip = new PointF(
                    (float)(pt.X + arrowDist * Math.Cos(rad)),
                    (float)(pt.Y + arrowDist * Math.Sin(rad)));

                double radLeft = (v.HeadingDegrees - 90.0 + 150.0) * (Math.PI / 180.0);
                PointF arrowLeft = new PointF(
                    (float)(arrowTip.X + 6.0 * Math.Cos(radLeft)),
                    (float)(arrowTip.Y + 6.0 * Math.Sin(radLeft)));

                double radRight = (v.HeadingDegrees - 90.0 - 150.0) * (Math.PI / 180.0);
                PointF arrowRight = new PointF(
                    (float)(arrowTip.X + 6.0 * Math.Cos(radRight)),
                    (float)(arrowTip.Y + 6.0 * Math.Sin(radRight)));

                using var arrowBrush = new SolidBrush(borderColor);
                g.FillPolygon(arrowBrush, new[] { arrowTip, arrowLeft, arrowRight });
            }

            // Draw route number or mode letter inside
            if (badgeText.Length > 4) badgeText = badgeText[..4];
            var size = g.MeasureString(badgeText, badgeFont);
            g.DrawString(badgeText, badgeFont, Brushes.White, pt.X - size.Width / 2f, pt.Y - size.Height / 2f);

        }

        private static Color VehicleColor(TransportCategory category) => category switch
        {
            TransportCategory.MunicipalBus => Color.FromArgb(3, 105, 161),
            TransportCategory.Marshrutka => Color.FromArgb(194, 65, 12),
            _ => Color.FromArgb(109, 40, 167)
        };

        private void DrawHudOverlay(Graphics g)
        {
            // Draw scale / mode pill in top right
            string modePill = "Estimated vehicle positions";
            var size = g.MeasureString(modePill, cityFont);
            var rect = new RectangleF(16, 16, size.Width + 14, 26);

            using var bg = new SolidBrush(Color.FromArgb(245, 255, 255, 255));
            g.FillRectangle(bg, rect);
            g.DrawString(modePill, cityFont, Brushes.DarkSlateGray, rect.X + 7, rect.Y + 5);

            if (Width >= 540)
            {
                (string Label, TransportCategory Category)[] legend =
                [
                    ("Municipal bus", TransportCategory.MunicipalBus),
                    ("Intercity coach", TransportCategory.IntercityCoach),
                    ("Marshrutka", TransportCategory.Marshrutka)
                ];
                float legendWidth = legend.Sum(entry => g.MeasureString(entry.Label, stopFont).Width + 24) + 8;
                g.FillRectangle(bg, new RectangleF(16, 42, legendWidth, 25));
                float x = 24;
                foreach (var entry in legend)
                {
                    using var dot = new SolidBrush(VehicleColor(entry.Category));
                    g.FillEllipse(dot, x, 50, 8, 8);
                    g.DrawString(entry.Label, stopFont, Brushes.DarkSlateGray, x + 12, 47);
                    x += g.MeasureString(entry.Label, stopFont).Width + 24;
                }
            }

            // Draw zoom level & vehicle count pill in bottom left
            string status = $"{activeVehicles.Count} vehicles · {displayedStops.Count} stops · {zoom:F1}× zoom";
            var statSize = g.MeasureString(status, cityFont);
            var statRect = new RectangleF(12, Height - 36, statSize.Width + 14, 24);
            using var statBg = new SolidBrush(Color.FromArgb(210, 255, 255, 255));
            using var statBorder = new Pen(Color.FromArgb(200, 200, 200));
            g.FillRectangle(statBg, statRect);
            g.DrawRectangle(statBorder, statRect.X, statRect.Y, statRect.Width, statRect.Height);
            g.DrawString(status, cityFont, Brushes.Black, statRect.X + 7, statRect.Y + 4);
        }

        private VehiclePosition? HitTestVehicle(Point pt)
        {
            for (int i = activeVehicles.Count - 1; i >= 0; i--)
            {
                var v = activeVehicles[i];
                if (IsGrouped(v) || !v.OperatorName.Contains(operatorFilter, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(operatorFilter, "ALL", StringComparison.OrdinalIgnoreCase)) continue;
                var screenPt = GeoToScreen(v.CurrentLatitude, v.CurrentLongitude);
                float distSq = (pt.X - screenPt.X) * (pt.X - screenPt.X) + (pt.Y - screenPt.Y) * (pt.Y - screenPt.Y);
                if (distSq <= 169) // 13px radius
                {
                    return v;
                }
            }
            return null;
        }

        private MunicipalStop? HitTestStop(Point pt)
        {
            foreach (var st in displayedStops)
            {
                var screenPt = GeoToScreen(st.Latitude, st.Longitude);
                float distSq = (pt.X - screenPt.X) * (pt.X - screenPt.X) + (pt.Y - screenPt.Y) * (pt.Y - screenPt.Y);
                if (distSq <= 64) // 8px radius
                {
                    return st;
                }
            }
            return null;
        }

        public PointF GeoToScreen(double lat, double lon)
        {
            double scale = Math.Pow(2.0, zoom);
            float x = (float)(Width / 2.0 + (lon - centerLon) * (256.0 * scale / 360.0));
            float y = (float)(Height / 2.0 +
                (StreetMapTiles.LatitudeY(lat) - StreetMapTiles.LatitudeY(centerLat)) * 256.0 * scale);
            return new PointF(x, y);
        }

        public (double Lat, double Lon) ScreenToGeo(PointF pt)
        {
            double scale = Math.Pow(2.0, zoom);
            double lon = centerLon + (pt.X - Width / 2.0) * (360.0 / (256.0 * scale));
            double lat = StreetMapTiles.YLatitude(StreetMapTiles.LatitudeY(centerLat) +
                (pt.Y - Height / 2.0) / (256.0 * scale));
            return (lat, lon);
        }
    }
}
