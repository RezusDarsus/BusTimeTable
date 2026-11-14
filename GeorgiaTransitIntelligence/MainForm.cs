using System.Data.Common;
using GeorgiaTransitIntelligence.Data;
using GeorgiaTransitIntelligence.Models;
using GeorgiaTransitIntelligence.Services;

namespace GeorgiaTransitIntelligence
{
    /// <summary>
    /// Main window for Georgia Transit Intelligence application.
    /// </summary>
    public partial class MainForm : Form
    {
        private readonly BusRouteRepository repository;
        private readonly MunicipalDataLoader municipalLoader;
        private readonly IntercityDataLoader intercityLoader;
        private readonly VehicleSimulationService simulationService;
        private readonly JourneyPlanner journeyPlanner;

        private List<BusRoute> currentDisplayedRoutes = new();
        private List<JourneyItinerary> currentItineraries = new();
        private DateTime lastDepartureBoardMinute;

        public MainForm()
        {
            InitializeComponent();
            StyleSimulationWorkspace();

            // Initialize data layer and services
            repository = new BusRouteRepository();
            municipalLoader = new MunicipalDataLoader();
            intercityLoader = new IntercityDataLoader();
            simulationService = new VehicleSimulationService(municipalLoader, intercityLoader, repository);
            journeyPlanner = new JourneyPlanner(municipalLoader, intercityLoader, repository);

            // Connect map events
            geoMapControl.VehicleSelected += GeoMapControl_VehicleSelected;
            geoMapControl.StopSelected += GeoMapControl_StopSelected;
        }

        private static readonly Color SimulationAccent = Color.FromArgb(20, 77, 151);

        private void StyleSimulationWorkspace()
        {
            Color ink = Color.FromArgb(24, 48, 54);
            Color surface = Color.FromArgb(248, 250, 253);
            pnlSimControls.BackColor = surface;
            lblSimSpeed.ForeColor = ink;
            lblSimClock.ForeColor = SimulationAccent;
            lblSimClock.AutoSize = false;
            lblSimClock.SetBounds(944, 10, 214, 28);
            lblSimClock.TextAlign = ContentAlignment.MiddleRight;

            foreach (Button button in new[] { btnSimReset, btnZoomIn, btnZoomOut, btnZoomGeorgia, btnZoomKutaisi })
            {
                button.BackColor = Color.FromArgb(232, 239, 248);
                button.ForeColor = ink;
            }
            btnSimPlayPause.BackColor = SimulationAccent;

            var mapStyle = new ComboBox
            {
                Name = "cmbSimulationBasemap",
                AccessibleName = "Map background",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Bounds = new Rectangle(btnZoomKutaisi.Right + 8, 12, 126, 26)
            };
            mapStyle.Items.AddRange(["Schematic", "OpenStreetMap", "Yandex Maps"]);
            mapStyle.SelectedIndex = 0;
            int previousMap = 0;
            string? yandexKey = Environment.GetEnvironmentVariable("GTI_YANDEX_TILES_API_KEY")?.Trim();
            mapStyle.SelectedIndexChanged += (_, _) =>
            {
                int selectedMap = mapStyle.SelectedIndex;
                if (selectedMap == previousMap) return;
                if (geoMapControl.YandexKeyRejected) yandexKey = null;
                if (selectedMap == 2 && string.IsNullOrWhiteSpace(yandexKey))
                {
                    yandexKey = PromptForYandexTilesKey();
                    if (yandexKey is null)
                    {
                        mapStyle.SelectedIndex = previousMap;
                        return;
                    }
                }
                previousMap = selectedMap;
                geoMapControl.SelectBasemap(selectedMap != 0, selectedMap == 2 ? yandexKey : null);
            };
            pnlSimControls.Controls.Add(mapStyle);
            pnlSimControls.Resize += (_, _) =>
                lblSimClock.SetBounds(mapStyle.Right + 8, 10,
                    Math.Max(0, pnlSimControls.ClientSize.Width - mapStyle.Right - 24), 28);

            pnlVehicleInspector.Controls.Clear();
            pnlVehicleInspector.BackColor = surface;
            pnlVehicleInspector.Dock = DockStyle.None;
            pnlVehicleInspector.Size = new Size(260, 128);
            tabSimulation.Resize += (_, _) => PlaceSimulationInspector();
            PlaceSimulationInspector();
            pnlVehicleInspector.Padding = new Padding(16);
            pnlVehicleInspector.BorderStyle = BorderStyle.FixedSingle;
            pnlVehicleInspector.BringToFront();
            var inspector = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false,
                AutoScroll = true
            };
            foreach (Label label in new[] { lblInspectorHeader, lblInspectorOperator, lblInspectorRoute,
                     lblInspectorDeparture, lblInspectorArrival, lblInspectorNextStop, lblInspectorStatus })
            {
                label.ForeColor = ink;
                label.MaximumSize = new Size(204, 0);
                label.Margin = new Padding(0, 0, 0, 9);
                inspector.Controls.Add(label);
            }
            lblInspectorHeader.Font = new Font("Segoe UI Semibold", 13F);
            lblInspectorOperator.ForeColor = Color.FromArgb(75, 101, 105);
            lblInspectorStatus.ForeColor = SimulationAccent;
            lblInspectorOperator.Text = "Select a vehicle to inspect its route.";
            pnlVehicleInspector.Controls.Add(inspector);
            SetInspectorDetailsVisible(false);
        }

        private string? PromptForYandexTilesKey()
        {
            using var dialog = new Form
            {
                Text = "Connect Yandex Maps",
                ClientSize = new Size(385, 145),
                FormBorderStyle = FormBorderStyle.FixedDialog,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            var label = new Label
            {
                Text = "Enter a Yandex Tiles API key for this device.",
                AutoSize = true,
                Location = new Point(16, 16)
            };
            var keyInput = new TextBox
            {
                Name = "txtYandexTilesKey",
                AccessibleName = "Yandex Tiles API key",
                UseSystemPasswordChar = true,
                Bounds = new Rectangle(16, 47, 353, 26)
            };
            var connect = new Button
            {
                Text = "Connect",
                DialogResult = DialogResult.OK,
                Enabled = false,
                Bounds = new Rectangle(193, 94, 84, 30)
            };
            var cancel = new Button
            {
                Text = "Cancel",
                DialogResult = DialogResult.Cancel,
                Bounds = new Rectangle(285, 94, 84, 30)
            };
            keyInput.TextChanged += (_, _) => connect.Enabled = !string.IsNullOrWhiteSpace(keyInput.Text);
            dialog.Controls.AddRange([label, keyInput, connect, cancel]);
            dialog.AcceptButton = connect;
            dialog.CancelButton = cancel;
            return dialog.ShowDialog(this) == DialogResult.OK ? keyInput.Text.Trim() : null;
        }

        private void PlaceSimulationInspector()
        {
            pnlVehicleInspector.Location = new Point(
                tabSimulation.ClientSize.Width - pnlVehicleInspector.Width - 18,
                pnlSimControls.Bottom + 18);
        }

        private void SetInspectorDetailsVisible(bool visible)
        {
            pnlVehicleInspector.Height = visible ? Math.Min(360, tabSimulation.ClientSize.Height - 84) : 128;
            lblInspectorRoute.Visible = visible;
            lblInspectorDeparture.Visible = visible;
            lblInspectorArrival.Visible = visible;
            lblInspectorNextStop.Visible = visible;
            lblInspectorStatus.Visible = visible;
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            ConfigureDataGridView();

            cmbFilter.SelectedIndex = 0; // "All"
            cmbSimSpeed.SelectedIndex = 0; // "1x Real-time"
            cmbPlannerOptimization.SelectedIndex = 0; // "Fastest Arrival"

            // Load mandatory CRUD routes
            LoadRoutes();

            // Populate Network Explorer data
            PopulateMunicipalRoutesExplorer();
            PopulateIntercityExplorer();
            PopulateDepartureBoardStops();
            PopulateCoverageReport();

            // Pass municipal stops to map
            geoMapControl.SetStops(municipalLoader.UniqueStops.Values.ToList());

            // Start simulation clock
            simTimer.Start();

            // Background load heavy municipal timetable without freezing UI
            try
            {
                await municipalLoader.LoadTimetableAsync();
                UpdateDepartureBoard();
            }
            catch (IOException)
            {
                lblDepartureBoardStatus.Text = "Timetable could not be read.";
            }
        }

        #region Priority A — University Graded CRUD & Search

        private void ConfigureDataGridView()
        {
            dgvBusRoutes.AutoGenerateColumns = false;
            dgvBusRoutes.Columns.Clear();

            // 1. Id (Hidden - Requirement 6)
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.Id),
                HeaderText = "Id",
                Name = "colId",
                Visible = false
            });

            // 2. Route Number
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.RouteNumber),
                HeaderText = "Route Number",
                Name = "colRouteNumber",
                FillWeight = 85
            });

            // 3. Origin
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.Origin),
                HeaderText = "Origin Station",
                Name = "colOrigin",
                FillWeight = 140
            });

            // 4. Destination
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.Destination),
                HeaderText = "Destination Station",
                Name = "colDestination",
                FillWeight = 140
            });

            // 5. Departure Time
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.DepartureTimeString),
                HeaderText = "Departure Time",
                Name = "colDepartureTime",
                FillWeight = 90
            });

            // 6. Duration
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.DurationDisplay),
                HeaderText = "Duration",
                Name = "colDuration",
                FillWeight = 85
            });

            // 7. Price
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.Price),
                HeaderText = "Price",
                Name = "colPrice",
                FillWeight = 80
            });

            // 8. Service Type (Calculated Property - Requirement 2)
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.ServiceType),
                HeaderText = "Service Type",
                Name = "colServiceType",
                FillWeight = 85
            });

            // 9. Estimated Arrival (Calculated Property)
            dgvBusRoutes.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = nameof(BusRoute.EstimatedArrivalTime),
                HeaderText = "Est. Arrival",
                Name = "colEstimatedArrival",
                FillWeight = 85
            });
        }

        private void LoadRoutes(string? statusMessage = null)
        {
            string searchText = txtSearch.Text;
            string filter = cmbFilter.SelectedItem?.ToString() ?? "All";

            int total;
            try
            {
                var routes = repository.Search(searchText, filter);
                total = repository.CountAll();
                currentDisplayedRoutes = routes;
                dgvBusRoutes.DataSource = routes;
            }
            catch (DbException)
            {
                lblDbStatus.Text = "LocalDB query failed";
                lblStatus.Text = statusMessage is null
                    ? "Could not load routes from LocalDB. Click Refresh to try again."
                    : $"{statusMessage} Grid refresh failed. Click Refresh to try again.";
                return;
            }
            int shown = currentDisplayedRoutes.Count;

            if (statusMessage is not null)
            {
                lblStatus.Text = statusMessage;
            }
            else if (shown == 0)
            {
                lblStatus.Text = "No bus routes match your search criteria.";
            }
            else
            {
                lblStatus.Text = $"Showing {shown} of {total} bus routes.";
            }

            bool usingLocalDb = repository.IsLocalDbAvailable;
            lblDbStatus.ForeColor = usingLocalDb ? Color.DarkGreen : Color.SaddleBrown;
            lblDbStatus.Text = usingLocalDb
                ? "LocalDB [Live Connected]"
                : "Demo mode · Changes last until exit";
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            LoadRoutes();
        }

        private void cmbFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            LoadRoutes();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            txtSearch.Clear();
            cmbFilter.SelectedIndex = 0; // "All"
            LoadRoutes();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            using (BusRouteForm form = new BusRouteForm())
            {
                if (form.ShowDialog(this) == DialogResult.OK)
                {
                    SaveRouteChange(() => repository.Add(form.CurrentRoute), form.CurrentRoute.RouteNumber, "added");
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            OpenEditDialog();
        }

        private void dgvBusRoutes_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                OpenEditDialog();
            }
        }

        private void OpenEditDialog()
        {
            if (dgvBusRoutes.CurrentRow == null)
            {
                MessageBox.Show("Please select a bus route from the grid to edit.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvBusRoutes.CurrentRow.DataBoundItem is BusRoute selected)
            {
                using (BusRouteForm form = new BusRouteForm(selected))
                {
                    if (form.ShowDialog(this) == DialogResult.OK)
                    {
                        SaveRouteChange(() => repository.Update(form.CurrentRoute), form.CurrentRoute.RouteNumber, "updated");
                    }
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvBusRoutes.CurrentRow == null)
            {
                MessageBox.Show("Please select a bus route from the grid to delete.", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            if (dgvBusRoutes.CurrentRow.DataBoundItem is BusRoute selected)
            {
                var result = MessageBox.Show(this,
                    $"Are you sure you want to delete route '{selected.RouteNumber}' from {selected.Origin} to {selected.Destination}?",
                    "Confirm Deletion",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);

                if (result == DialogResult.Yes)
                {
                    SaveRouteChange(() => repository.Delete(selected.Id), selected.RouteNumber, "deleted");
                }
            }
        }

        private void SaveRouteChange(Func<int> save, string routeNumber, string action)
        {
            try
            {
                int affected = save();
                string storage = repository.IsLocalDbAvailable ? string.Empty : " for this session";
                LoadRoutes(affected > 0
                    ? $"Bus route '{routeNumber}' {action}{storage}."
                    : $"Bus route '{routeNumber}' is no longer available.");
            }
            catch (DbException ex)
            {
                lblStatus.Text = $"Could not confirm that route '{routeNumber}' was {action}.";
                MessageBox.Show(this, ex.Message, "Database operation failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvBusRoutes_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= dgvBusRoutes.Rows.Count) return;

            if (dgvBusRoutes.Rows[e.RowIndex].DataBoundItem is BusRoute route)
            {
                // Visual rule based on data (Requirement 6): Highlight Express routes
                if (route.IsExpress)
                {
                    e.CellStyle.BackColor = Color.FromArgb(238, 244, 255);
                    e.CellStyle.SelectionBackColor = Color.FromArgb(180, 210, 255);
                    e.CellStyle.SelectionForeColor = Color.Black;
                }

                // Format price as currency
                if (dgvBusRoutes.Columns[e.ColumnIndex].Name == "colPrice" && e.Value != null)
                {
                    e.Value = $"{((decimal)e.Value):F2} ₾";
                    e.FormattingApplied = true;
                }
            }
        }

        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (currentDisplayedRoutes == null || currentDisplayedRoutes.Count == 0)
            {
                MessageBox.Show("There are no routes displayed in the grid to export.", "Export Empty", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "Export Timetable to CSV (University Bonus)",
                Filter = "CSV Files (*.csv)|*.csv|All Files (*.*)|*.*",
                FileName = "BusRoutes_Timetable_Export.csv"
            };

            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                try
                {
                    int count = CsvExportService.ExportToCsv(currentDisplayedRoutes, sfd.FileName);
                    lblStatus.Text = $"Successfully exported {count} routes to '{Path.GetFileName(sfd.FileName)}'.";
                    MessageBox.Show($"Exported {count} routes successfully.\nPreserved UTF-8 encoding for Georgian Unicode text.",
                        "CSV Export Completed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"CSV export failed: {ex.Message}", "Export Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        #endregion

        #region Priority B — Multimodal Journey Planner

        private void btnPlanJourney_Click(object sender, EventArgs e)
        {
            string origin = cmbPlannerOrigin.Text.Trim();
            string destination = cmbPlannerDestination.Text.Trim();

            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            {
                MessageBox.Show("Please specify both Origin and Destination locations.", "Input Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var opt = cmbPlannerOptimization.SelectedIndex switch
            {
                1 => JourneyOptimization.CheapestKnown,
                2 => JourneyOptimization.FewestTransfers,
                _ => JourneyOptimization.FastestArrival
            };

            currentItineraries = journeyPlanner.PlanJourney(
                origin,
                destination,
                DateTime.Today,
                dtpPlannerTime.Value.TimeOfDay,
                chkAllowMunicipal.Checked,
                chkAllowIntercity.Checked,
                chkAllowMarshrutka.Checked,
                opt);

            dgvItineraries.DataSource = currentItineraries.Select(it => new
            {
                it.Title,
                Departure = it.DepartureTime.ToString(@"hh\:mm"),
                Arrival = it.ArrivalTime.ToString(@"hh\:mm"),
                Duration = $"{it.TotalDurationMinutes / 60}h {it.TotalDurationMinutes % 60:D2}m",
                Fare = $"{it.TotalFareGEL:F2} ₾",
                Transfers = it.TransferCount,
                Walk = $"{it.TotalWalkingMinutes} min",
                Reliability = it.ReliabilityWarning
            }).ToList();

            if (currentItineraries.Count > 0)
            {
                dgvItineraries.Rows[0].Selected = true;
                DisplayItineraryLegs(currentItineraries[0]);
            }
            else
            {
                lstJourneyLegs.Items.Clear();
                MessageBox.Show($"No viable journey found between {origin} and {destination} with the chosen options.",
                    "No Route Found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void dgvItineraries_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvItineraries.CurrentRow != null && dgvItineraries.CurrentRow.Index >= 0 &&
                dgvItineraries.CurrentRow.Index < currentItineraries.Count)
            {
                DisplayItineraryLegs(currentItineraries[dgvItineraries.CurrentRow.Index]);
            }
        }

        private void DisplayItineraryLegs(JourneyItinerary itinerary)
        {
            lstJourneyLegs.Items.Clear();
            lblItineraryDetailsHeader.Text = $"Itinerary: {itinerary.Title} ({itinerary.SummaryText})";

            foreach (var leg in itinerary.Legs)
            {
                var item = new ListViewItem(leg.LegType.ToString());
                item.SubItems.Add(leg.FromLocation);
                item.SubItems.Add(leg.ToLocation);
                item.SubItems.Add($"{leg.DepartureTime:hh\\:mm} - {leg.ArrivalTime:hh\\:mm}");
                item.SubItems.Add($"{leg.DurationMinutes} min");
                item.SubItems.Add(leg.FareGEL.HasValue ? $"{leg.FareGEL.Value:F2} ₾" : "N/A");
                item.SubItems.Add(leg.Instructions);

                // Distinct colors per leg mode
                item.ForeColor = leg.LegType switch
                {
                    JourneyLegType.MunicipalBus => Color.FromArgb(15, 108, 189),
                    JourneyLegType.IntercityCoach => Color.FromArgb(123, 31, 162),
                    JourneyLegType.Marshrutka => Color.FromArgb(230, 81, 0),
                    JourneyLegType.Walking => Color.FromArgb(46, 125, 50),
                    _ => Color.DimGray
                };

                lstJourneyLegs.Items.Add(item);
            }
        }

        private void btnSimulateJourney_Click(object sender, EventArgs e)
        {
            if (dgvItineraries.CurrentRow == null || dgvItineraries.CurrentRow.Index >= currentItineraries.Count)
            {
                MessageBox.Show("Please select an itinerary from the list first.", "Select Itinerary", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var selectedItinerary = currentItineraries[dgvItineraries.CurrentRow.Index];

            // Build path for map highlight
            var pathPoints = new List<(double Lat, double Lon)>();
            foreach (var leg in selectedItinerary.Legs)
            {
                pathPoints.Add((leg.FromLatitude, leg.FromLongitude));
                pathPoints.Add((leg.ToLatitude, leg.ToLongitude));
            }

            // Set map path and switch tab
            geoMapControl.SetHighlightedPath(pathPoints, Color.FromArgb(123, 31, 162));

            // Set simulation clock to journey departure time
            simulationService.CurrentSimulatedTime = DateTime.Today.Add(selectedItinerary.DepartureTime);
            simulationService.IsPaused = false;

            // Switch to simulation tab
            tabControlMain.SelectedTab = tabSimulation;

            // Fit map to journey endpoints
            if (pathPoints.Count > 0)
            {
                double minLat = pathPoints.Min(p => p.Lat);
                double maxLat = pathPoints.Max(p => p.Lat);
                double minLon = pathPoints.Min(p => p.Lon);
                double maxLon = pathPoints.Max(p => p.Lon);
                geoMapControl.FitBounds(minLat, maxLat, minLon, maxLon);
            }
        }

        #endregion

        #region Priority B — Live Simulation & Interactive Map

        private void simTimer_Tick(object sender, EventArgs e)
        {
            simulationService.Tick(0.3); // 300ms per tick

            DateTime clock = simulationService.CurrentSimulatedTime;
            lblSimClock.Text = $"{clock:HH:mm:ss} · Georgia time";

            if (tabDepartureBoard.Visible && clock.Ticks / TimeSpan.TicksPerMinute != lastDepartureBoardMinute.Ticks / TimeSpan.TicksPerMinute)
            {
                lastDepartureBoardMinute = clock;
                UpdateDepartureBoard();
            }

            if (tabControlMain.SelectedTab != tabSimulation) return;

            // Update active vehicles on map
            var vehicles = simulationService.GetActiveVehicles(
                geoMapControl.ZoomLevel,
                geoMapControl.CenterLatitude,
                geoMapControl.CenterLongitude);

            geoMapControl.SetVehicles(vehicles);
        }

        private void btnSimPlayPause_Click(object sender, EventArgs e)
        {
            simulationService.IsPaused = !simulationService.IsPaused;
            btnSimPlayPause.Text = simulationService.IsPaused ? "▶ Play" : "⏸ Pause";
            btnSimPlayPause.BackColor = SimulationAccent;
        }

        private void btnSimReset_Click(object sender, EventArgs e)
        {
            simulationService.ResetToRealTime();
            btnSimPlayPause.Text = "⏸ Pause";
            btnSimPlayPause.BackColor = SimulationAccent;
            cmbSimSpeed.SelectedIndex = 0;
        }

        private void cmbSimSpeed_SelectedIndexChanged(object sender, EventArgs e)
        {
            simulationService.TimeMultiplier = cmbSimSpeed.SelectedIndex switch
            {
                1 => 5.0,
                2 => 15.0,
                3 => 60.0,
                _ => 1.0
            };
        }

        private void btnZoomIn_Click(object sender, EventArgs e) => geoMapControl.ZoomIn();
        private void btnZoomOut_Click(object sender, EventArgs e) => geoMapControl.ZoomOut();
        private void btnZoomGeorgia_Click(object sender, EventArgs e) => geoMapControl.ZoomToGeorgia();
        private void btnZoomKutaisi_Click(object sender, EventArgs e) => geoMapControl.ZoomToKutaisi();

        private void GeoMapControl_VehicleSelected(VehiclePosition vehicle)
        {
            SetInspectorDetailsVisible(true);
            lblInspectorOperator.Text = vehicle.OperatorName;
            lblInspectorRoute.Text = $"Route: {vehicle.RouteNumber} ({vehicle.Origin} → {vehicle.Destination})";
            lblInspectorDeparture.Text = $"Scheduled Dep: {vehicle.ScheduledDeparture:hh\\:mm}";
            lblInspectorArrival.Text = $"Est. Arrival: {vehicle.EstimatedArrival:hh\\:mm}";
            lblInspectorNextStop.Text = $"Next Stop: {vehicle.NextStopName}";
            lblInspectorStatus.Text = $"{vehicle.StatusDescription} [Simulated]";
        }

        private void GeoMapControl_StopSelected(MunicipalStop stop)
        {
            SetInspectorDetailsVisible(true);
            lblInspectorOperator.Text = "Kutaisi Municipal Stop";
            lblInspectorRoute.Text = $"Stop #{stop.StopCode}: {stop.StopName}";
            lblInspectorDeparture.Text = $"Stop ID: {stop.StopId}";
            lblInspectorArrival.Text = $"Coordinates: {stop.Latitude:F5}, {stop.Longitude:F5}";
            lblInspectorNextStop.Text = $"Mode: {stop.VehicleMode}";
            lblInspectorStatus.Text = "Stop Marker Active";
        }

        #endregion

        #region Priority B — Network Explorer & Departure Board

        private void PopulateMunicipalRoutesExplorer()
        {
            lstMunicipalPatterns.Items.Clear();
            foreach (var p in municipalLoader.Patterns)
            {
                lstMunicipalPatterns.Items.Add($"Bus #{p.RouteNumber} (Dir {p.DirectionId}): {p.StartPoint} → {p.EndPoint}");
            }

            if (municipalLoader.Patterns.Count > 0)
            {
                lstMunicipalPatterns.SelectedIndex = 0;
            }
        }

        private void lstMunicipalPatterns_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = lstMunicipalPatterns.SelectedIndex;
            if (idx >= 0 && idx < municipalLoader.Patterns.Count)
            {
                var pattern = municipalLoader.Patterns[idx];
                dgvMunicipalStops.DataSource = pattern.Stops.Select(s => new
                {
                    Order = s.StopOrder,
                    Code = s.StopCode,
                    Name = s.StopName,
                    Latitude = s.Latitude.ToString("F5"),
                    Longitude = s.Longitude.ToString("F5")
                }).ToList();
            }
        }

        private void PopulateIntercityExplorer()
        {
            dgvIntercityServices.DataSource = intercityLoader.Services.Select(s => new
            {
                s.ServiceId,
                s.OperatorName,
                Category = s.Category.ToString(),
                Origin = s.DepartureCity,
                Destination = s.ArrivalCity,
                Departure = s.DepartureTime.HasValue ? s.DepartureTime.Value.ToString(@"hh\:mm") : (s.SchedulePattern ?? "Frequency window"),
                Arrival = s.ArrivalTime.HasValue ? s.ArrivalTime.Value.ToString(@"hh\:mm") : "N/A",
                Duration = s.DurationMinutes.HasValue ? $"{s.DurationMinutes.Value} min" : "N/A",
                Fare = s.TicketPriceGEL.HasValue ? $"{s.TicketPriceGEL.Value:F2} ₾" : "N/A",
                Evidence = s.EvidenceStatus
            }).ToList();
        }

        private void PopulateDepartureBoardStops()
        {
            cmbDepartureStop.DisplayMember = nameof(MunicipalStop.StopName);
            cmbDepartureStop.DataSource = municipalLoader.UniqueStops.Values
                .OrderBy(stop => stop.StopName)
                .ThenBy(stop => stop.StopCode)
                .Select(stop => new DepartureBoardStop(stop, $"{stop.StopName} ({stop.StopCode})"))
                .ToList();
        }

        private sealed record DepartureBoardStop(MunicipalStop Stop, string StopName);

        private void cmbDepartureStop_SelectedIndexChanged(object sender, EventArgs e) => UpdateDepartureBoard();

        private void UpdateDepartureBoard()
        {
            if (cmbDepartureStop.SelectedItem is not DepartureBoardStop selected) return;

            DateTime clock = simulationService.CurrentSimulatedTime;
            var departures = municipalLoader.GetNextDepartures(selected.Stop.StopId, clock.DayOfWeek.ToString(), clock.TimeOfDay, 10);
            dgvUpcomingDepartures.DataSource = departures.Select(departure => new
            {
                Route = $"Bus #{departure.RouteNumber}",
                Destination = municipalLoader.GetPattern(departure.RouteNumber, departure.DirectionId)?.EndPoint ?? string.Empty,
                Scheduled = departure.DepartureTime.ToString(@"hh\:mm"),
                Countdown = $"{Math.Ceiling((departure.DepartureTime - clock.TimeOfDay).TotalMinutes)} min"
            }).ToList();
            lblDepartureBoardStatus.Text = !municipalLoader.IsTimetableLoaded
                ? "Published timetable unavailable or loading"
                : departures.Count == 0
                    ? "No more published departures today"
                    : $"Published times · Simulation clock {clock:ddd HH:mm}";
        }

        private void PopulateCoverageReport()
        {
            rtbDataCoverage.Text =
@"========================================================================================
GEORGIA TRANSIT INTELLIGENCE — DATASET PROVENANCE & QUALITY SPECIFICATION
========================================================================================

1. KUTAISI MUNICIPAL TRANSPORT NETWORK (Published API Dataset)
----------------------------------------------------------------------------------------
• Municipal Bus Routes:              22 active routes
• Route-Direction Patterns:          44 verified patterns (22 OD pairs)
• Unique Stop Nodes:                 450 stops with verified GPS coordinates
• Ordered Route-Stop Combinations:   847 sequence tuples
• Published Timetable Records:       182,291 departure entries across all 7 weekdays
• Days Covered:                      Monday through Sunday (No synthetic holidays)
• Primary Provenance:                Kutaisi Transport Public API (AzRy / PIS Gateway)

2. GEORGIAN NATIONWIDE INTERCITY NETWORK (Operator & Directory Research Snapshot)
----------------------------------------------------------------------------------------
• Named Transport Operators:         19 operators (Citybus, Metro, Omnibus, Georgian Bus,
                                     LTD Okriba, Imertrans, Sprinter, etc.)
• Nationwide Destination Nodes:      168 cities, towns, and border terminals
• Source-Backed Service Records:     585 services (520 timed departures, 65 frequency windows)
• Primary / Institutional Sources:   194 records (official operator published schedules)
• Community / Directory Records:     391 records (line-detail observations, marked unconfirmed)
• Directional Route Pairs:           74 operator-confirmed OD pairs; 266 directory leads
• Synthetic Trip Expansion:          0 synthetic trips fabricated

3. UNIVERSITY MIDTERM COMPLIANCE & REPRODUCIBILITY (BusTimetableDb)
----------------------------------------------------------------------------------------
• Graded Table:                      dbo.BusRoutes (Exactly 1 table)
• Database Engine:                   SQL Server LocalDB ((localdb)\MSSQLLocalDB)
• SQL Architecture:                  BusRouteRepository (All queries parameterized)
• Seed Row Count:                    18 realistic intercity routes (Requirement: >= 15)
• Script Safety:                     database.sql can be executed repeatedly without duplicates
• Bonus CSV Export:                  UTF-8 with BOM, preserves Georgian Unicode text.
";
        }

        private void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedTab == tabSimulation)
            {
                geoMapControl.Invalidate();
            }
            if (tabDepartureBoard.Visible) UpdateDepartureBoard();
        }

        #endregion
    }
}
