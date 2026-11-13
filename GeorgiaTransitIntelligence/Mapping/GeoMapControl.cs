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

        // Mouse interaction state
        private bool isPanning = false;
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
        private readonly Brush bgBrush = new SolidBrush(Color.FromArgb(242, 244, 248));
        private readonly Brush landBrush = new SolidBrush(Color.FromArgb(255, 255, 255));
        private readonly Pen borderPen = new Pen(Color.FromArgb(215, 220, 230), 1.5f);
        private readonly Pen highwayPen = new Pen(Color.FromArgb(254, 211, 149), 4f);
        private readonly Pen highwayBorderPen = new Pen(Color.FromArgb(220, 160, 90), 5.5f);
        private readonly Pen secondaryRoadPen = new Pen(Color.FromArgb(230, 232, 238), 2f);
        private readonly Pen highlightedPen = new Pen(Color.FromArgb(15, 108, 189), 4.5f);
        private readonly Font cityFont = new Font("Segoe UI", 9F, FontStyle.Bold);
        private readonly Font stopFont = new Font("Segoe UI", 7.5F, FontStyle.Regular);
        private readonly Font badgeFont = new Font("Segoe UI", 8F, FontStyle.Bold);

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
            set { zoom = Math.Clamp(value, 6.0, 16.5); Invalidate(); }
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

            BackColor = Color.FromArgb(242, 244, 248);
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
            centerLat = 42.00;
            centerLon = 43.50;
            zoom = 7.8;
            ViewportChanged?.Invoke(centerLat, centerLon, zoom);
            Invalidate();
        }

        public void ZoomToKutaisi()
        {
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
            ZoomLevel -= 0.8;
            ViewportChanged?.Invoke(centerLat, centerLon, zoom);
        }

        public void FitBounds(double minLat, double maxLat, double minLon, double maxLon)
        {
            centerLat = (minLat + maxLat) / 2.0;
            centerLon = (minLon + maxLon) / 2.0;

            double latSpan = Math.Max(0.02, maxLat - minLat);
            double lonSpan = Math.Max(0.02, maxLon - minLon);

            double zoomLat = Math.Log2(180.0 / latSpan);
            double zoomLon = Math.Log2(360.0 / lonSpan);
            ZoomLevel = Math.Min(zoomLat, zoomLon) + 1.2;

            ViewportChanged?.Invoke(centerLat, centerLon, zoom);
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            if (e.Button == MouseButtons.Left)
            {
                // Check if user clicked on a vehicle marker
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
                int dx = e.X - panStartPoint.X;
                int dy = e.Y - panStartPoint.Y;

                double scale = Math.Pow(2.0, zoom);
                double dLon = -dx * (360.0 / (256.0 * scale));
                double dLat = dy * (180.0 / (256.0 * scale));

                centerLat = Math.Clamp(panStartLat + dLat, 40.5, 44.5);
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

            // 2. Draw Georgia Country Landmass & Shading
            DrawLandmass(g);

            // 3. Draw Highway Corridors & Road Network
            DrawRoadNetwork(g);

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
            DrawCities(g);

            // 7. Draw Active Vehicles (Animated Simulators)
            DrawVehicles(g);

            // 8. Draw HUD / Map Legend & Scale
            DrawHudOverlay(g);
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

        private void DrawVehicles(Graphics g)
        {
            foreach (var v in activeVehicles)
            {
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

            // Distinguish each operator with their official brand identity
            Color mainColor;
            Color borderColor = isSelected ? Color.Yellow : Color.White;
            string badgeText;

            if (v.OperatorName.Contains("Omnibus", StringComparison.OrdinalIgnoreCase))
            {
                mainColor = Color.FromArgb(30, 58, 138); // Deep Royal Navy
                borderColor = isSelected ? Color.Yellow : Color.FromArgb(245, 158, 11); // Gold accent
                badgeText = "OMNI";
            }
            else if (v.OperatorName.Contains("Georgian Bus", StringComparison.OrdinalIgnoreCase))
            {
                mainColor = Color.FromArgb(8, 145, 178); // Cyan / Teal
                badgeText = "GBUS";
            }
            else if (v.OperatorName.Contains("Metro", StringComparison.OrdinalIgnoreCase))
            {
                mainColor = Color.FromArgb(220, 38, 38); // Crimson Red
                badgeText = "MET";
            }
            else if (v.OperatorName.Contains("Citybus", StringComparison.OrdinalIgnoreCase))
            {
                mainColor = Color.FromArgb(142, 36, 170); // Royal Purple
                badgeText = "CITY";
            }
            else if (v.OperatorName.Contains("Marshrutka", StringComparison.OrdinalIgnoreCase) ||
                     v.OperatorName.Contains("Okriba", StringComparison.OrdinalIgnoreCase) ||
                     v.OperatorName.Contains("Imertrans", StringComparison.OrdinalIgnoreCase) ||
                     v.OperatorName.Contains("Sprinter", StringComparison.OrdinalIgnoreCase) ||
                     v.OperatorName.Contains("Taxi", StringComparison.OrdinalIgnoreCase) ||
                     v.Category == TransportCategory.Marshrutka)
            {
                mainColor = Color.FromArgb(234, 88, 12); // Amber Orange for Marshrutka / Route Taxi
                badgeText = "MRSH";
            }
            else // Kutaisi Municipal Bus
            {
                mainColor = Color.FromArgb(2, 132, 199); // Sky Blue
                badgeText = string.IsNullOrEmpty(v.RouteNumber) ? "BUS" : $"#{v.RouteNumber}";
            }

            using var brush = new SolidBrush(mainColor);
            using var borderPen = new Pen(borderColor, isSelected ? 2.8f : 1.6f);

            float radius = isSelected ? 13f : 10.5f;

            // Draw shadow
            g.FillEllipse(new SolidBrush(Color.FromArgb(60, 0, 0, 0)), pt.X - radius + 2, pt.Y - radius + 3, radius * 2, radius * 2);

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

            // If selected, draw tooltip card with details
            if (isSelected)
            {
                string info = $"{v.OperatorName}\nRoute: {v.RouteNumber} ({v.Origin} → {v.Destination})\nHeading: {v.HeadingDegrees:F0}° | Speed: Simulated 80 km/h\nScheduled: {v.ScheduledDeparture:hh\\:mm} | Est. Arrival: {v.EstimatedArrival:hh\\:mm}\nNext: {v.NextStopName}\n[SIMULATED TIMETABLE ESTIMATE]";
                DrawTooltip(g, info, pt.X + 15, pt.Y - 30);
            }
        }

        private void DrawTooltip(Graphics g, string text, float x, float y)
        {
            var font = new Font("Segoe UI", 8F);
            var size = g.MeasureString(text, font);
            var rect = new RectangleF(x, y, size.Width + 12, size.Height + 8);

            using var bg = new SolidBrush(Color.FromArgb(245, 24, 38, 68));
            using var pen = new Pen(Color.FromArgb(100, 200, 255), 1.2f);
            g.FillRectangle(bg, rect);
            g.DrawRectangle(pen, rect.X, rect.Y, rect.Width, rect.Height);
            g.DrawString(text, font, Brushes.White, x + 6, y + 4);
        }

        private void DrawHudOverlay(Graphics g)
        {
            // Draw scale / mode pill in top right
            string modePill = "TIMETABLE SIMULATION — ESTIMATED VEHICLE POSITIONS";
            var size = g.MeasureString(modePill, cityFont);
            var rect = new RectangleF(Width - size.Width - 24, 12, size.Width + 14, 26);

            using var bg = new SolidBrush(Color.FromArgb(220, 24, 38, 68));
            g.FillRectangle(bg, rect);
            g.DrawString(modePill, cityFont, Brushes.Gold, rect.X + 7, rect.Y + 5);

            // Draw zoom level & vehicle count pill in bottom left
            string status = $"Zoom: {zoom:F1}x | Active Vehicles: {activeVehicles.Count} | Municipal Stops: {displayedStops.Count}";
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
            float y = (float)(Height / 2.0 - (lat - centerLat) * (256.0 * scale / 180.0));
            return new PointF(x, y);
        }

        public (double Lat, double Lon) ScreenToGeo(PointF pt)
        {
            double scale = Math.Pow(2.0, zoom);
            double lon = centerLon + (pt.X - Width / 2.0) * (360.0 / (256.0 * scale));
            double lat = centerLat - (pt.Y - Height / 2.0) * (180.0 / (256.0 * scale));
            return (lat, lon);
        }
    }
}
