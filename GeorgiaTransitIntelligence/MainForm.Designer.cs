using GeorgiaTransitIntelligence.Mapping;

namespace GeorgiaTransitIntelligence
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            DataGridViewCellStyle dataGridViewCellStyle1 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle2 = new DataGridViewCellStyle();
            DataGridViewCellStyle dataGridViewCellStyle3 = new DataGridViewCellStyle();
            pnlTopBanner = new Panel();
            lblAppSubtitle = new Label();
            lblAppTitle = new Label();
            tabControlMain = new TabControl();
            tabCrud = new TabPage();
            pnlCrudControls = new Panel();
            btnRefresh = new Button();
            btnExportCsv = new Button();
            btnDelete = new Button();
            btnEdit = new Button();
            btnAdd = new Button();
            cmbFilter = new ComboBox();
            lblFilter = new Label();
            txtSearch = new TextBox();
            lblSearch = new Label();
            dgvBusRoutes = new DataGridView();
            statusStrip = new StatusStrip();
            lblStatus = new ToolStripStatusLabel();
            lblDbStatus = new ToolStripStatusLabel();
            tabPlanner = new TabPage();
            pnlPlannerLeft = new Panel();
            btnSimulateJourney = new Button();
            btnPlanJourney = new Button();
            grpModes = new GroupBox();
            chkAllowMarshrutka = new CheckBox();
            chkAllowIntercity = new CheckBox();
            chkAllowMunicipal = new CheckBox();
            cmbPlannerOptimization = new ComboBox();
            lblPlannerOptimization = new Label();
            dtpPlannerTime = new DateTimePicker();
            lblPlannerTime = new Label();
            cmbPlannerDestination = new ComboBox();
            lblPlannerDestination = new Label();
            cmbPlannerOrigin = new ComboBox();
            lblPlannerOrigin = new Label();
            pnlPlannerRight = new Panel();
            splitPlanner = new SplitContainer();
            dgvItineraries = new DataGridView();
            pnlItineraryDetails = new Panel();
            lstJourneyLegs = new ListView();
            colLegMode = new ColumnHeader();
            colLegFrom = new ColumnHeader();
            colLegTo = new ColumnHeader();
            colLegTime = new ColumnHeader();
            colLegDuration = new ColumnHeader();
            colLegFare = new ColumnHeader();
            colLegInstructions = new ColumnHeader();
            lblItineraryDetailsHeader = new Label();
            tabSimulation = new TabPage();
            geoMapControl = new GeoMapControl();
            pnlSimControls = new Panel();
            lblSimClock = new Label();
            btnZoomKutaisi = new Button();
            btnZoomGeorgia = new Button();
            btnZoomOut = new Button();
            btnZoomIn = new Button();
            cmbSimSpeed = new ComboBox();
            lblSimSpeed = new Label();
            btnSimReset = new Button();
            btnSimPlayPause = new Button();
            pnlVehicleInspector = new Panel();
            lblInspectorStatus = new Label();
            lblInspectorNextStop = new Label();
            lblInspectorArrival = new Label();
            lblInspectorDeparture = new Label();
            lblInspectorRoute = new Label();
            lblInspectorOperator = new Label();
            lblInspectorHeader = new Label();
            tabExplorer = new TabPage();
            tabSubExplorer = new TabControl();
            tabKutaisiRoutes = new TabPage();
            splitKutaisi = new SplitContainer();
            lstMunicipalPatterns = new ListBox();
            dgvMunicipalStops = new DataGridView();
            tabIntercityServices = new TabPage();
            dgvIntercityServices = new DataGridView();
            tabDepartureBoard = new TabPage();
            pnlDepartureTop = new Panel();
            cmbDepartureStop = new ComboBox();
            lblDepartureStop = new Label();
            lblDepartureBoardStatus = new Label();
            dgvUpcomingDepartures = new DataGridView();
            tabDataCoverage = new TabPage();
            rtbDataCoverage = new RichTextBox();
            simTimer = new System.Windows.Forms.Timer(components);
            pnlTopBanner.SuspendLayout();
            tabControlMain.SuspendLayout();
            tabCrud.SuspendLayout();
            pnlCrudControls.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBusRoutes).BeginInit();
            statusStrip.SuspendLayout();
            tabPlanner.SuspendLayout();
            pnlPlannerLeft.SuspendLayout();
            grpModes.SuspendLayout();
            pnlPlannerRight.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitPlanner).BeginInit();
            splitPlanner.Panel1.SuspendLayout();
            splitPlanner.Panel2.SuspendLayout();
            splitPlanner.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvItineraries).BeginInit();
            pnlItineraryDetails.SuspendLayout();
            tabSimulation.SuspendLayout();
            pnlSimControls.SuspendLayout();
            pnlVehicleInspector.SuspendLayout();
            tabExplorer.SuspendLayout();
            tabSubExplorer.SuspendLayout();
            tabKutaisiRoutes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitKutaisi).BeginInit();
            splitKutaisi.Panel1.SuspendLayout();
            splitKutaisi.Panel2.SuspendLayout();
            splitKutaisi.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvMunicipalStops).BeginInit();
            tabIntercityServices.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvIntercityServices).BeginInit();
            tabDepartureBoard.SuspendLayout();
            pnlDepartureTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUpcomingDepartures).BeginInit();
            tabDataCoverage.SuspendLayout();
            SuspendLayout();
            // 
            // pnlTopBanner
            // 
            pnlTopBanner.BackColor = Color.FromArgb(20, 32, 60);
            pnlTopBanner.Controls.Add(lblAppSubtitle);
            pnlTopBanner.Controls.Add(lblAppTitle);
            pnlTopBanner.Dock = DockStyle.Top;
            pnlTopBanner.Location = new Point(0, 0);
            pnlTopBanner.Name = "pnlTopBanner";
            pnlTopBanner.Size = new Size(1184, 62);
            pnlTopBanner.TabIndex = 0;
            // 
            // lblAppSubtitle
            // 
            lblAppSubtitle.AutoSize = true;
            lblAppSubtitle.Font = new Font("Segoe UI", 9F);
            lblAppSubtitle.ForeColor = Color.FromArgb(180, 205, 235);
            lblAppSubtitle.Location = new Point(18, 36);
            lblAppSubtitle.Name = "lblAppSubtitle";
            lblAppSubtitle.Size = new Size(392, 15);
            lblAppSubtitle.TabIndex = 1;
            lblAppSubtitle.Text = "National Bus Network && Timetable Control Center";
            // 
            // lblAppTitle
            // 
            lblAppTitle.AutoSize = true;
            lblAppTitle.Font = new Font("Segoe UI Semibold", 13.5F, FontStyle.Bold);
            lblAppTitle.ForeColor = Color.White;
            lblAppTitle.Location = new Point(16, 9);
            lblAppTitle.Name = "lblAppTitle";
            lblAppTitle.Size = new Size(307, 25);
            lblAppTitle.TabIndex = 0;
            lblAppTitle.Text = "GEORGIA TRANSIT INTELLIGENCE";
            // 
            // tabControlMain
            // 
            tabControlMain.Controls.Add(tabCrud);
            tabControlMain.Controls.Add(tabPlanner);
            tabControlMain.Controls.Add(tabSimulation);
            tabControlMain.Controls.Add(tabExplorer);
            tabControlMain.Dock = DockStyle.Fill;
            tabControlMain.Font = new Font("Segoe UI", 9.5F);
            tabControlMain.ItemSize = new Size(176, 34);
            tabControlMain.Location = new Point(0, 62);
            tabControlMain.Name = "tabControlMain";
            tabControlMain.SelectedIndex = 0;
            tabControlMain.Size = new Size(1184, 650);
            tabControlMain.SizeMode = TabSizeMode.Normal;
            tabControlMain.Padding = new Point(14, 6);
            tabControlMain.TabIndex = 1;
            tabControlMain.SelectedIndexChanged += tabControlMain_SelectedIndexChanged;
            // 
            // tabCrud
            // 
            tabCrud.BackColor = Color.FromArgb(248, 250, 252);
            tabCrud.Controls.Add(dgvBusRoutes);
            tabCrud.Controls.Add(pnlCrudControls);
            tabCrud.Controls.Add(statusStrip);
            tabCrud.Location = new Point(4, 36);
            tabCrud.Name = "tabCrud";
            tabCrud.Padding = new Padding(12);
            tabCrud.Size = new Size(1176, 610);
            tabCrud.TabIndex = 0;
            tabCrud.Text = "Timetable Manager";
            // 
            // pnlCrudControls
            // 
            pnlCrudControls.BackColor = Color.White;
            pnlCrudControls.BorderStyle = BorderStyle.FixedSingle;
            pnlCrudControls.Controls.Add(btnRefresh);
            pnlCrudControls.Controls.Add(btnExportCsv);
            pnlCrudControls.Controls.Add(btnDelete);
            pnlCrudControls.Controls.Add(btnEdit);
            pnlCrudControls.Controls.Add(btnAdd);
            pnlCrudControls.Controls.Add(cmbFilter);
            pnlCrudControls.Controls.Add(lblFilter);
            pnlCrudControls.Controls.Add(txtSearch);
            pnlCrudControls.Controls.Add(lblSearch);
            pnlCrudControls.Dock = DockStyle.Top;
            pnlCrudControls.Location = new Point(12, 12);
            pnlCrudControls.Name = "pnlCrudControls";
            pnlCrudControls.Size = new Size(1152, 54);
            pnlCrudControls.TabIndex = 0;
            // 
            // btnRefresh
            // 
            btnRefresh.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnRefresh.BackColor = Color.FromArgb(240, 242, 246);
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(200, 204, 212);
            btnRefresh.FlatStyle = FlatStyle.Flat;
            btnRefresh.Location = new Point(1054, 11);
            btnRefresh.Name = "btnRefresh";
            btnRefresh.Size = new Size(82, 30);
            btnRefresh.TabIndex = 8;
            btnRefresh.Text = "Refresh";
            btnRefresh.UseVisualStyleBackColor = false;
            btnRefresh.Click += btnRefresh_Click;
            // 
            // btnExportCsv
            // 
            btnExportCsv.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnExportCsv.BackColor = Color.FromArgb(235, 245, 238);
            btnExportCsv.FlatAppearance.BorderColor = Color.FromArgb(160, 210, 180);
            btnExportCsv.FlatStyle = FlatStyle.Flat;
            btnExportCsv.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnExportCsv.ForeColor = Color.FromArgb(20, 110, 50);
            btnExportCsv.Location = new Point(942, 11);
            btnExportCsv.Name = "btnExportCsv";
            btnExportCsv.Size = new Size(106, 30);
            btnExportCsv.TabIndex = 7;
            btnExportCsv.Text = "Export CSV";
            btnExportCsv.UseVisualStyleBackColor = false;
            btnExportCsv.Click += btnExportCsv_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnDelete.BackColor = Color.FromArgb(254, 242, 242);
            btnDelete.FlatAppearance.BorderColor = Color.FromArgb(240, 180, 180);
            btnDelete.FlatStyle = FlatStyle.Flat;
            btnDelete.ForeColor = Color.FromArgb(180, 40, 40);
            btnDelete.Location = new Point(854, 11);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(82, 30);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "Delete";
            btnDelete.UseVisualStyleBackColor = false;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnEdit.BackColor = Color.FromArgb(240, 242, 246);
            btnEdit.FlatAppearance.BorderColor = Color.FromArgb(200, 204, 212);
            btnEdit.FlatStyle = FlatStyle.Flat;
            btnEdit.Location = new Point(766, 11);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(82, 30);
            btnEdit.TabIndex = 5;
            btnEdit.Text = "Edit";
            btnEdit.UseVisualStyleBackColor = false;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnAdd.BackColor = Color.FromArgb(15, 108, 189);
            btnAdd.FlatAppearance.BorderSize = 0;
            btnAdd.FlatStyle = FlatStyle.Flat;
            btnAdd.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            btnAdd.ForeColor = Color.White;
            btnAdd.Location = new Point(668, 11);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(92, 30);
            btnAdd.TabIndex = 4;
            btnAdd.Text = "+ Add Route";
            btnAdd.UseVisualStyleBackColor = false;
            btnAdd.Click += btnAdd_Click;
            // 
            // cmbFilter
            // 
            cmbFilter.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbFilter.FormattingEnabled = true;
            cmbFilter.Items.AddRange(new object[] { "All", "Express", "Regular" });
            cmbFilter.Location = new Point(386, 14);
            cmbFilter.Name = "cmbFilter";
            cmbFilter.Size = new Size(120, 25);
            cmbFilter.TabIndex = 3;
            cmbFilter.SelectedIndexChanged += cmbFilter_SelectedIndexChanged;
            // 
            // lblFilter
            // 
            lblFilter.AutoSize = true;
            lblFilter.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblFilter.Location = new Point(344, 19);
            lblFilter.Name = "lblFilter";
            lblFilter.Size = new Size(36, 15);
            lblFilter.TabIndex = 2;
            lblFilter.Text = "Filter:";
            // 
            // txtSearch
            // 
            txtSearch.Location = new Point(68, 14);
            txtSearch.Name = "txtSearch";
            txtSearch.PlaceholderText = "Search origin, destination, route...";
            txtSearch.Size = new Size(250, 24);
            txtSearch.TabIndex = 1;
            txtSearch.TextChanged += txtSearch_TextChanged;
            // 
            // lblSearch
            // 
            lblSearch.AutoSize = true;
            lblSearch.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblSearch.Location = new Point(14, 18);
            lblSearch.Name = "lblSearch";
            lblSearch.Size = new Size(46, 15);
            lblSearch.TabIndex = 0;
            lblSearch.Text = "Search:";
            // 
            // dgvBusRoutes
            // 
            dgvBusRoutes.AllowUserToAddRows = false;
            dgvBusRoutes.AllowUserToDeleteRows = false;
            dgvBusRoutes.AllowUserToResizeRows = false;
            dgvBusRoutes.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvBusRoutes.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvBusRoutes.BackgroundColor = Color.White;
            dgvBusRoutes.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewCellStyle1.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = Color.FromArgb(240, 243, 248);
            dataGridViewCellStyle1.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            dataGridViewCellStyle1.ForeColor = Color.FromArgb(30, 30, 30);
            dataGridViewCellStyle1.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = DataGridViewTriState.True;
            dgvBusRoutes.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            dgvBusRoutes.ColumnHeadersHeight = 32;
            dgvBusRoutes.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgvBusRoutes.EnableHeadersVisualStyles = false;
            dgvBusRoutes.Location = new Point(12, 74);
            dgvBusRoutes.MultiSelect = false;
            dgvBusRoutes.Name = "dgvBusRoutes";
            dgvBusRoutes.ReadOnly = true;
            dgvBusRoutes.RowHeadersVisible = false;
            dgvBusRoutes.RowTemplate.Height = 28;
            dgvBusRoutes.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvBusRoutes.Size = new Size(1152, 498);
            dgvBusRoutes.TabIndex = 1;
            dgvBusRoutes.CellDoubleClick += dgvBusRoutes_CellDoubleClick;
            dgvBusRoutes.CellFormatting += dgvBusRoutes_CellFormatting;
            // 
            // statusStrip
            // 
            statusStrip.Items.AddRange(new ToolStripItem[] { lblStatus, lblDbStatus });
            statusStrip.Location = new Point(12, 574);
            statusStrip.Name = "statusStrip";
            statusStrip.Size = new Size(1152, 24);
            statusStrip.TabIndex = 2;
            // 
            // lblStatus
            // 
            lblStatus.Font = new Font("Segoe UI", 9F);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(995, 19);
            lblStatus.Spring = true;
            lblStatus.Text = "Ready. 18 bus routes loaded.";
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // lblDbStatus
            // 
            lblDbStatus.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            lblDbStatus.ForeColor = Color.FromArgb(16, 124, 16);
            lblDbStatus.Name = "lblDbStatus";
            lblDbStatus.Size = new Size(142, 19);
            lblDbStatus.Text = "Database: (localdb)\\MSSQLLocalDB";
            // 
            // tabPlanner
            // 
            tabPlanner.BackColor = Color.FromArgb(248, 250, 252);
            tabPlanner.Controls.Add(pnlPlannerRight);
            tabPlanner.Controls.Add(pnlPlannerLeft);
            tabPlanner.Location = new Point(4, 36);
            tabPlanner.Name = "tabPlanner";
            tabPlanner.Padding = new Padding(10);
            tabPlanner.Size = new Size(1176, 610);
            tabPlanner.TabIndex = 1;
            tabPlanner.Text = "Georgia Journey Planner";
            // 
            // pnlPlannerLeft
            // 
            pnlPlannerLeft.BackColor = Color.White;
            pnlPlannerLeft.BorderStyle = BorderStyle.FixedSingle;
            pnlPlannerLeft.Controls.Add(btnSimulateJourney);
            pnlPlannerLeft.Controls.Add(btnPlanJourney);
            pnlPlannerLeft.Controls.Add(grpModes);
            pnlPlannerLeft.Controls.Add(cmbPlannerOptimization);
            pnlPlannerLeft.Controls.Add(lblPlannerOptimization);
            pnlPlannerLeft.Controls.Add(dtpPlannerTime);
            pnlPlannerLeft.Controls.Add(lblPlannerTime);
            pnlPlannerLeft.Controls.Add(cmbPlannerDestination);
            pnlPlannerLeft.Controls.Add(lblPlannerDestination);
            pnlPlannerLeft.Controls.Add(cmbPlannerOrigin);
            pnlPlannerLeft.Controls.Add(lblPlannerOrigin);
            pnlPlannerLeft.Dock = DockStyle.Left;
            pnlPlannerLeft.Location = new Point(10, 10);
            pnlPlannerLeft.Name = "pnlPlannerLeft";
            pnlPlannerLeft.Padding = new Padding(14);
            pnlPlannerLeft.Size = new Size(330, 590);
            pnlPlannerLeft.TabIndex = 0;
            // 
            // btnSimulateJourney
            // 
            btnSimulateJourney.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnSimulateJourney.BackColor = Color.FromArgb(123, 31, 162);
            btnSimulateJourney.FlatAppearance.BorderSize = 0;
            btnSimulateJourney.FlatStyle = FlatStyle.Flat;
            btnSimulateJourney.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            btnSimulateJourney.ForeColor = Color.White;
            btnSimulateJourney.Location = new Point(14, 538);
            btnSimulateJourney.Name = "btnSimulateJourney";
            btnSimulateJourney.Size = new Size(300, 36);
            btnSimulateJourney.TabIndex = 10;
            btnSimulateJourney.Text = "▶ Simulate My Journey on Map";
            btnSimulateJourney.UseVisualStyleBackColor = false;
            btnSimulateJourney.Click += btnSimulateJourney_Click;
            // 
            // btnPlanJourney
            // 
            btnPlanJourney.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            btnPlanJourney.BackColor = Color.FromArgb(15, 108, 189);
            btnPlanJourney.FlatAppearance.BorderSize = 0;
            btnPlanJourney.FlatStyle = FlatStyle.Flat;
            btnPlanJourney.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            btnPlanJourney.ForeColor = Color.White;
            btnPlanJourney.Location = new Point(14, 386);
            btnPlanJourney.Name = "btnPlanJourney";
            btnPlanJourney.Size = new Size(300, 38);
            btnPlanJourney.TabIndex = 9;
            btnPlanJourney.Text = "Find Multimodal Journeys";
            btnPlanJourney.UseVisualStyleBackColor = false;
            btnPlanJourney.Click += btnPlanJourney_Click;
            // 
            // grpModes
            // 
            grpModes.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            grpModes.Controls.Add(chkAllowMarshrutka);
            grpModes.Controls.Add(chkAllowIntercity);
            grpModes.Controls.Add(chkAllowMunicipal);
            grpModes.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            grpModes.Location = new Point(14, 252);
            grpModes.Name = "grpModes";
            grpModes.Size = new Size(300, 114);
            grpModes.TabIndex = 8;
            grpModes.TabStop = false;
            grpModes.Text = "Included Transport Modes";
            // 
            // chkAllowMarshrutka
            // 
            chkAllowMarshrutka.AutoSize = true;
            chkAllowMarshrutka.Checked = true;
            chkAllowMarshrutka.CheckState = CheckState.Checked;
            chkAllowMarshrutka.Font = new Font("Segoe UI", 9F);
            chkAllowMarshrutka.Location = new Point(16, 80);
            chkAllowMarshrutka.Name = "chkAllowMarshrutka";
            chkAllowMarshrutka.Size = new Size(181, 19);
            chkAllowMarshrutka.TabIndex = 2;
            chkAllowMarshrutka.Text = "Regional Marshrutka / Minibus";
            chkAllowMarshrutka.UseVisualStyleBackColor = true;
            // 
            // chkAllowIntercity
            // 
            chkAllowIntercity.AutoSize = true;
            chkAllowIntercity.Checked = true;
            chkAllowIntercity.CheckState = CheckState.Checked;
            chkAllowIntercity.Font = new Font("Segoe UI", 9F);
            chkAllowIntercity.Location = new Point(16, 52);
            chkAllowIntercity.Name = "chkAllowIntercity";
            chkAllowIntercity.Size = new Size(207, 19);
            chkAllowIntercity.TabIndex = 1;
            chkAllowIntercity.Text = "Intercity Coach (Citybus, Omnibus)";
            chkAllowIntercity.UseVisualStyleBackColor = true;
            // 
            // chkAllowMunicipal
            // 
            chkAllowMunicipal.AutoSize = true;
            chkAllowMunicipal.Checked = true;
            chkAllowMunicipal.CheckState = CheckState.Checked;
            chkAllowMunicipal.Font = new Font("Segoe UI", 9F);
            chkAllowMunicipal.Location = new Point(16, 24);
            chkAllowMunicipal.Name = "chkAllowMunicipal";
            chkAllowMunicipal.Size = new Size(189, 19);
            chkAllowMunicipal.TabIndex = 0;
            chkAllowMunicipal.Text = "Kutaisi Municipal Bus Network";
            chkAllowMunicipal.UseVisualStyleBackColor = true;
            // 
            // cmbPlannerOptimization
            // 
            cmbPlannerOptimization.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbPlannerOptimization.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbPlannerOptimization.FormattingEnabled = true;
            cmbPlannerOptimization.Items.AddRange(new object[] { "Fastest Arrival", "Cheapest Known Fare", "Fewest Transfers" });
            cmbPlannerOptimization.Location = new Point(14, 210);
            cmbPlannerOptimization.Name = "cmbPlannerOptimization";
            cmbPlannerOptimization.Size = new Size(300, 25);
            cmbPlannerOptimization.TabIndex = 7;
            // 
            // lblPlannerOptimization
            // 
            lblPlannerOptimization.AutoSize = true;
            lblPlannerOptimization.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblPlannerOptimization.Location = new Point(14, 190);
            lblPlannerOptimization.Name = "lblPlannerOptimization";
            lblPlannerOptimization.Size = new Size(82, 15);
            lblPlannerOptimization.TabIndex = 6;
            lblPlannerOptimization.Text = "Optimization:";
            // 
            // dtpPlannerTime
            // 
            dtpPlannerTime.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            dtpPlannerTime.CustomFormat = "HH:mm";
            dtpPlannerTime.Format = DateTimePickerFormat.Custom;
            dtpPlannerTime.Location = new Point(14, 150);
            dtpPlannerTime.Name = "dtpPlannerTime";
            dtpPlannerTime.ShowUpDown = true;
            dtpPlannerTime.Size = new Size(300, 24);
            dtpPlannerTime.TabIndex = 5;
            // 
            // lblPlannerTime
            // 
            lblPlannerTime.AutoSize = true;
            lblPlannerTime.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblPlannerTime.Location = new Point(14, 130);
            lblPlannerTime.Name = "lblPlannerTime";
            lblPlannerTime.Size = new Size(130, 15);
            lblPlannerTime.TabIndex = 4;
            lblPlannerTime.Text = "Earliest Departure Time:";
            // 
            // cmbPlannerDestination
            // 
            cmbPlannerDestination.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbPlannerDestination.FormattingEnabled = true;
            cmbPlannerDestination.Items.AddRange(new object[] { "Batumi", "Tbilisi", "Zugdidi", "Poti", "Gori", "Telavi", "Akhaltsikhe", "Mestia", "Kutaisi" });
            cmbPlannerDestination.Location = new Point(14, 90);
            cmbPlannerDestination.Name = "cmbPlannerDestination";
            cmbPlannerDestination.Size = new Size(300, 25);
            cmbPlannerDestination.TabIndex = 3;
            cmbPlannerDestination.Text = "Batumi";
            // 
            // lblPlannerDestination
            // 
            lblPlannerDestination.AutoSize = true;
            lblPlannerDestination.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblPlannerDestination.Location = new Point(14, 70);
            lblPlannerDestination.Name = "lblPlannerDestination";
            lblPlannerDestination.Size = new Size(130, 15);
            lblPlannerDestination.TabIndex = 2;
            lblPlannerDestination.Text = "Destination City / Stop:";
            // 
            // cmbPlannerOrigin
            // 
            cmbPlannerOrigin.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            cmbPlannerOrigin.FormattingEnabled = true;
            cmbPlannerOrigin.Items.AddRange(new object[] { "Tabukashvili Street 217, Kutaisi", "Kutaisi International University (KIU)", "Kutaisi (Chavchavadze Omnibus Terminal)", "Kutaisi (Red Bridge)", "Bagrati Cathedral, Kutaisi", "Kutaisi Airport (KUT)", "Tbilisi (Didube)", "Batumi (Central)" });
            cmbPlannerOrigin.Location = new Point(14, 30);
            cmbPlannerOrigin.Name = "cmbPlannerOrigin";
            cmbPlannerOrigin.Size = new Size(300, 25);
            cmbPlannerOrigin.TabIndex = 1;
            cmbPlannerOrigin.Text = "Tabukashvili Street 217, Kutaisi";
            // 
            // lblPlannerOrigin
            // 
            lblPlannerOrigin.AutoSize = true;
            lblPlannerOrigin.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblPlannerOrigin.Location = new Point(14, 10);
            lblPlannerOrigin.Name = "lblPlannerOrigin";
            lblPlannerOrigin.Size = new Size(101, 15);
            lblPlannerOrigin.TabIndex = 0;
            lblPlannerOrigin.Text = "Origin City / Stop:";
            // 
            // pnlPlannerRight
            // 
            pnlPlannerRight.Controls.Add(splitPlanner);
            pnlPlannerRight.Dock = DockStyle.Fill;
            pnlPlannerRight.Location = new Point(340, 10);
            pnlPlannerRight.Name = "pnlPlannerRight";
            pnlPlannerRight.Padding = new Padding(10, 0, 0, 0);
            pnlPlannerRight.Size = new Size(826, 590);
            pnlPlannerRight.TabIndex = 1;
            // 
            // splitPlanner
            // 
            splitPlanner.Dock = DockStyle.Fill;
            splitPlanner.Location = new Point(10, 0);
            splitPlanner.Name = "splitPlanner";
            splitPlanner.Orientation = Orientation.Horizontal;
            // 
            // splitPlanner.Panel1
            // 
            splitPlanner.Panel1.Controls.Add(dgvItineraries);
            // 
            // splitPlanner.Panel2
            // 
            splitPlanner.Panel2.Controls.Add(pnlItineraryDetails);
            splitPlanner.Size = new Size(816, 590);
            splitPlanner.SplitterDistance = 240;
            splitPlanner.TabIndex = 0;
            // 
            // dgvItineraries
            // 
            dgvItineraries.AllowUserToAddRows = false;
            dgvItineraries.AllowUserToDeleteRows = false;
            dgvItineraries.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvItineraries.BackgroundColor = Color.White;
            dgvItineraries.BorderStyle = BorderStyle.Fixed3D;
            dataGridViewCellStyle2.Alignment = DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = Color.FromArgb(240, 243, 248);
            dataGridViewCellStyle2.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            dataGridViewCellStyle2.ForeColor = SystemColors.WindowText;
            dataGridViewCellStyle2.SelectionBackColor = SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = DataGridViewTriState.True;
            dgvItineraries.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            dgvItineraries.ColumnHeadersHeight = 30;
            dgvItineraries.Dock = DockStyle.Fill;
            dgvItineraries.EnableHeadersVisualStyles = false;
            dgvItineraries.Location = new Point(0, 0);
            dgvItineraries.MultiSelect = false;
            dgvItineraries.Name = "dgvItineraries";
            dgvItineraries.ReadOnly = true;
            dgvItineraries.RowHeadersVisible = false;
            dgvItineraries.RowTemplate.Height = 26;
            dgvItineraries.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvItineraries.Size = new Size(816, 240);
            dgvItineraries.TabIndex = 0;
            dgvItineraries.SelectionChanged += dgvItineraries_SelectionChanged;
            // 
            // pnlItineraryDetails
            // 
            pnlItineraryDetails.BackColor = Color.White;
            pnlItineraryDetails.BorderStyle = BorderStyle.FixedSingle;
            pnlItineraryDetails.Controls.Add(lstJourneyLegs);
            pnlItineraryDetails.Controls.Add(lblItineraryDetailsHeader);
            pnlItineraryDetails.Dock = DockStyle.Fill;
            pnlItineraryDetails.Location = new Point(0, 0);
            pnlItineraryDetails.Name = "pnlItineraryDetails";
            pnlItineraryDetails.Padding = new Padding(10);
            pnlItineraryDetails.Size = new Size(816, 346);
            pnlItineraryDetails.TabIndex = 0;
            // 
            // lstJourneyLegs
            // 
            lstJourneyLegs.Columns.AddRange(new ColumnHeader[] { colLegMode, colLegFrom, colLegTo, colLegTime, colLegDuration, colLegFare, colLegInstructions });
            lstJourneyLegs.Dock = DockStyle.Fill;
            lstJourneyLegs.FullRowSelect = true;
            lstJourneyLegs.GridLines = true;
            lstJourneyLegs.Location = new Point(10, 36);
            lstJourneyLegs.Name = "lstJourneyLegs";
            lstJourneyLegs.Size = new Size(794, 298);
            lstJourneyLegs.TabIndex = 1;
            lstJourneyLegs.UseCompatibleStateImageBehavior = false;
            lstJourneyLegs.View = View.Details;
            // 
            // colLegMode
            // 
            colLegMode.Text = "Mode";
            colLegMode.Width = 120;
            // 
            // colLegFrom
            // 
            colLegFrom.Text = "From";
            colLegFrom.Width = 150;
            // 
            // colLegTo
            // 
            colLegTo.Text = "To";
            colLegTo.Width = 150;
            // 
            // colLegTime
            // 
            colLegTime.Text = "Schedule";
            colLegTime.Width = 100;
            // 
            // colLegDuration
            // 
            colLegDuration.Text = "Duration";
            colLegDuration.Width = 80;
            // 
            // colLegFare
            // 
            colLegFare.Text = "Fare";
            colLegFare.Width = 70;
            // 
            // colLegInstructions
            // 
            colLegInstructions.Text = "Passenger Instructions";
            colLegInstructions.Width = 260;
            // 
            // lblItineraryDetailsHeader
            // 
            lblItineraryDetailsHeader.AutoSize = true;
            lblItineraryDetailsHeader.Dock = DockStyle.Top;
            lblItineraryDetailsHeader.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold);
            lblItineraryDetailsHeader.Location = new Point(10, 10);
            lblItineraryDetailsHeader.Name = "lblItineraryDetailsHeader";
            lblItineraryDetailsHeader.Padding = new Padding(0, 0, 0, 7);
            lblItineraryDetailsHeader.Size = new Size(207, 26);
            lblItineraryDetailsHeader.TabIndex = 0;
            lblItineraryDetailsHeader.Text = "Step-by-Step Multimodal Legs:";
            // 
            // tabSimulation
            // 
            tabSimulation.Controls.Add(geoMapControl);
            tabSimulation.Controls.Add(pnlVehicleInspector);
            tabSimulation.Controls.Add(pnlSimControls);
            tabSimulation.Location = new Point(4, 36);
            tabSimulation.Name = "tabSimulation";
            tabSimulation.Size = new Size(1176, 610);
            tabSimulation.TabIndex = 2;
            tabSimulation.Text = "National Live Simulation";
            tabSimulation.UseVisualStyleBackColor = true;
            // 
            // geoMapControl
            // 
            geoMapControl.BackColor = Color.FromArgb(242, 244, 248);
            geoMapControl.CenterLatitude = 42.15D;
            geoMapControl.CenterLongitude = 43.3D;
            geoMapControl.Cursor = Cursors.Hand;
            geoMapControl.Dock = DockStyle.Fill;
            geoMapControl.Location = new Point(0, 48);
            geoMapControl.Name = "geoMapControl";
            geoMapControl.Size = new Size(916, 562);
            geoMapControl.TabIndex = 1;
            geoMapControl.ZoomLevel = 8D;
            // 
            // pnlSimControls
            // 
            pnlSimControls.BackColor = Color.FromArgb(24, 38, 68);
            pnlSimControls.Controls.Add(lblSimClock);
            pnlSimControls.Controls.Add(btnZoomKutaisi);
            pnlSimControls.Controls.Add(btnZoomGeorgia);
            pnlSimControls.Controls.Add(btnZoomOut);
            pnlSimControls.Controls.Add(btnZoomIn);
            pnlSimControls.Controls.Add(cmbSimSpeed);
            pnlSimControls.Controls.Add(lblSimSpeed);
            pnlSimControls.Controls.Add(btnSimReset);
            pnlSimControls.Controls.Add(btnSimPlayPause);
            pnlSimControls.Dock = DockStyle.Top;
            pnlSimControls.Location = new Point(0, 0);
            pnlSimControls.Name = "pnlSimControls";
            pnlSimControls.Size = new Size(1176, 48);
            pnlSimControls.TabIndex = 0;
            // 
            // lblSimClock
            // 
            lblSimClock.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSimClock.AutoSize = true;
            lblSimClock.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold);
            lblSimClock.ForeColor = Color.Gold;
            lblSimClock.Location = new Point(980, 14);
            lblSimClock.Name = "lblSimClock";
            lblSimClock.Size = new Size(181, 20);
            lblSimClock.TabIndex = 8;
            lblSimClock.Text = "Sim Clock: 12:00 (UTC+4)";
            // 
            // btnZoomKutaisi
            // 
            btnZoomKutaisi.BackColor = Color.FromArgb(30, 136, 229);
            btnZoomKutaisi.FlatAppearance.BorderSize = 0;
            btnZoomKutaisi.FlatStyle = FlatStyle.Flat;
            btnZoomKutaisi.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            btnZoomKutaisi.ForeColor = Color.White;
            btnZoomKutaisi.Location = new Point(540, 10);
            btnZoomKutaisi.Name = "btnZoomKutaisi";
            btnZoomKutaisi.Size = new Size(100, 28);
            btnZoomKutaisi.TabIndex = 7;
            btnZoomKutaisi.Text = "Kutaisi City";
            btnZoomKutaisi.UseVisualStyleBackColor = false;
            btnZoomKutaisi.Click += btnZoomKutaisi_Click;
            // 
            // btnZoomGeorgia
            // 
            btnZoomGeorgia.BackColor = Color.FromArgb(50, 70, 110);
            btnZoomGeorgia.FlatAppearance.BorderSize = 0;
            btnZoomGeorgia.FlatStyle = FlatStyle.Flat;
            btnZoomGeorgia.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            btnZoomGeorgia.ForeColor = Color.White;
            btnZoomGeorgia.Location = new Point(430, 10);
            btnZoomGeorgia.Name = "btnZoomGeorgia";
            btnZoomGeorgia.Size = new Size(104, 28);
            btnZoomGeorgia.TabIndex = 6;
            btnZoomGeorgia.Text = "Georgia View";
            btnZoomGeorgia.UseVisualStyleBackColor = false;
            btnZoomGeorgia.Click += btnZoomGeorgia_Click;
            // 
            // btnZoomOut
            // 
            btnZoomOut.BackColor = Color.FromArgb(40, 55, 90);
            btnZoomOut.FlatAppearance.BorderSize = 0;
            btnZoomOut.FlatStyle = FlatStyle.Flat;
            btnZoomOut.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnZoomOut.ForeColor = Color.White;
            btnZoomOut.Location = new Point(386, 10);
            btnZoomOut.Name = "btnZoomOut";
            btnZoomOut.Size = new Size(32, 28);
            btnZoomOut.TabIndex = 5;
            btnZoomOut.Text = "−";
            btnZoomOut.UseVisualStyleBackColor = false;
            btnZoomOut.Click += btnZoomOut_Click;
            // 
            // btnZoomIn
            // 
            btnZoomIn.BackColor = Color.FromArgb(40, 55, 90);
            btnZoomIn.FlatAppearance.BorderSize = 0;
            btnZoomIn.FlatStyle = FlatStyle.Flat;
            btnZoomIn.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            btnZoomIn.ForeColor = Color.White;
            btnZoomIn.Location = new Point(348, 10);
            btnZoomIn.Name = "btnZoomIn";
            btnZoomIn.Size = new Size(32, 28);
            btnZoomIn.TabIndex = 4;
            btnZoomIn.Text = "+";
            btnZoomIn.UseVisualStyleBackColor = false;
            btnZoomIn.Click += btnZoomIn_Click;
            // 
            // cmbSimSpeed
            // 
            cmbSimSpeed.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbSimSpeed.FormattingEnabled = true;
            cmbSimSpeed.Items.AddRange(new object[] { "1x Real-time", "5x Fast", "15x Rapid", "60x Warp" });
            cmbSimSpeed.Location = new Point(234, 12);
            cmbSimSpeed.Name = "cmbSimSpeed";
            cmbSimSpeed.Size = new Size(100, 25);
            cmbSimSpeed.TabIndex = 3;
            cmbSimSpeed.SelectedIndexChanged += cmbSimSpeed_SelectedIndexChanged;
            // 
            // lblSimSpeed
            // 
            lblSimSpeed.AutoSize = true;
            lblSimSpeed.ForeColor = Color.White;
            lblSimSpeed.Location = new Point(184, 16);
            lblSimSpeed.Name = "lblSimSpeed";
            lblSimSpeed.Size = new Size(48, 17);
            lblSimSpeed.TabIndex = 2;
            lblSimSpeed.Text = "Speed:";
            // 
            // btnSimReset
            // 
            btnSimReset.BackColor = Color.FromArgb(40, 55, 90);
            btnSimReset.FlatAppearance.BorderSize = 0;
            btnSimReset.FlatStyle = FlatStyle.Flat;
            btnSimReset.ForeColor = Color.White;
            btnSimReset.Location = new Point(98, 10);
            btnSimReset.Name = "btnSimReset";
            btnSimReset.Size = new Size(76, 28);
            btnSimReset.TabIndex = 1;
            btnSimReset.Text = "Reset Time";
            btnSimReset.UseVisualStyleBackColor = false;
            btnSimReset.Click += btnSimReset_Click;
            // 
            // btnSimPlayPause
            // 
            btnSimPlayPause.BackColor = Color.FromArgb(15, 108, 189);
            btnSimPlayPause.FlatAppearance.BorderSize = 0;
            btnSimPlayPause.FlatStyle = FlatStyle.Flat;
            btnSimPlayPause.Font = new Font("Segoe UI Semibold", 8.5F, FontStyle.Bold);
            btnSimPlayPause.ForeColor = Color.White;
            btnSimPlayPause.Location = new Point(14, 10);
            btnSimPlayPause.Name = "btnSimPlayPause";
            btnSimPlayPause.Size = new Size(78, 28);
            btnSimPlayPause.TabIndex = 0;
            btnSimPlayPause.Text = "⏸ Pause";
            btnSimPlayPause.UseVisualStyleBackColor = false;
            btnSimPlayPause.Click += btnSimPlayPause_Click;
            // 
            // pnlVehicleInspector
            // 
            pnlVehicleInspector.BackColor = Color.White;
            pnlVehicleInspector.BorderStyle = BorderStyle.FixedSingle;
            pnlVehicleInspector.Controls.Add(lblInspectorStatus);
            pnlVehicleInspector.Controls.Add(lblInspectorNextStop);
            pnlVehicleInspector.Controls.Add(lblInspectorArrival);
            pnlVehicleInspector.Controls.Add(lblInspectorDeparture);
            pnlVehicleInspector.Controls.Add(lblInspectorRoute);
            pnlVehicleInspector.Controls.Add(lblInspectorOperator);
            pnlVehicleInspector.Controls.Add(lblInspectorHeader);
            pnlVehicleInspector.Dock = DockStyle.Right;
            pnlVehicleInspector.Location = new Point(916, 48);
            pnlVehicleInspector.Name = "pnlVehicleInspector";
            pnlVehicleInspector.Padding = new Padding(12);
            pnlVehicleInspector.Size = new Size(260, 562);
            pnlVehicleInspector.TabIndex = 2;
            // 
            // lblInspectorStatus
            // 
            lblInspectorStatus.AutoSize = true;
            lblInspectorStatus.Font = new Font("Segoe UI", 9F);
            lblInspectorStatus.ForeColor = Color.FromArgb(16, 124, 16);
            lblInspectorStatus.Location = new Point(12, 210);
            lblInspectorStatus.Name = "lblInspectorStatus";
            lblInspectorStatus.Size = new Size(137, 15);
            lblInspectorStatus.TabIndex = 6;
            lblInspectorStatus.Text = "Status: Simulated (Active)";
            // 
            // lblInspectorNextStop
            // 
            lblInspectorNextStop.AutoSize = true;
            lblInspectorNextStop.Font = new Font("Segoe UI", 9F);
            lblInspectorNextStop.Location = new Point(12, 175);
            lblInspectorNextStop.Name = "lblInspectorNextStop";
            lblInspectorNextStop.Size = new Size(111, 15);
            lblInspectorNextStop.TabIndex = 5;
            lblInspectorNextStop.Text = "Next Stop: Highway";
            // 
            // lblInspectorArrival
            // 
            lblInspectorArrival.AutoSize = true;
            lblInspectorArrival.Font = new Font("Segoe UI", 9F);
            lblInspectorArrival.Location = new Point(12, 140);
            lblInspectorArrival.Name = "lblInspectorArrival";
            lblInspectorArrival.Size = new Size(106, 15);
            lblInspectorArrival.TabIndex = 4;
            lblInspectorArrival.Text = "Est. Arrival: --:--";
            // 
            // lblInspectorDeparture
            // 
            lblInspectorDeparture.AutoSize = true;
            lblInspectorDeparture.Font = new Font("Segoe UI", 9F);
            lblInspectorDeparture.Location = new Point(12, 105);
            lblInspectorDeparture.Name = "lblInspectorDeparture";
            lblInspectorDeparture.Size = new Size(125, 15);
            lblInspectorDeparture.TabIndex = 3;
            lblInspectorDeparture.Text = "Scheduled Dep: --:--";
            // 
            // lblInspectorRoute
            // 
            lblInspectorRoute.AutoSize = true;
            lblInspectorRoute.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblInspectorRoute.Location = new Point(12, 70);
            lblInspectorRoute.Name = "lblInspectorRoute";
            lblInspectorRoute.Size = new Size(140, 17);
            lblInspectorRoute.TabIndex = 2;
            lblInspectorRoute.Text = "Route: None Selected";
            // 
            // lblInspectorOperator
            // 
            lblInspectorOperator.AutoSize = true;
            lblInspectorOperator.Font = new Font("Segoe UI", 9F);
            lblInspectorOperator.ForeColor = Color.DimGray;
            lblInspectorOperator.Location = new Point(12, 40);
            lblInspectorOperator.Name = "lblInspectorOperator";
            lblInspectorOperator.Size = new Size(157, 15);
            lblInspectorOperator.TabIndex = 1;
            lblInspectorOperator.Text = "Click any vehicle on the map";
            // 
            // lblInspectorHeader
            // 
            lblInspectorHeader.AutoSize = true;
            lblInspectorHeader.Font = new Font("Segoe UI Semibold", 10.5F, FontStyle.Bold);
            lblInspectorHeader.Location = new Point(12, 12);
            lblInspectorHeader.Name = "lblInspectorHeader";
            lblInspectorHeader.Size = new Size(116, 19);
            lblInspectorHeader.TabIndex = 0;
            lblInspectorHeader.Text = "Vehicle Inspector";
            // 
            // tabExplorer
            // 
            tabExplorer.Controls.Add(tabSubExplorer);
            tabExplorer.Location = new Point(4, 36);
            tabExplorer.Name = "tabExplorer";
            tabExplorer.Size = new Size(1176, 610);
            tabExplorer.TabIndex = 3;
            tabExplorer.Text = "Network Explorer";
            tabExplorer.UseVisualStyleBackColor = true;
            // 
            // tabSubExplorer
            // 
            tabSubExplorer.Controls.Add(tabKutaisiRoutes);
            tabSubExplorer.Controls.Add(tabIntercityServices);
            tabSubExplorer.Controls.Add(tabDepartureBoard);
            tabSubExplorer.Controls.Add(tabDataCoverage);
            tabSubExplorer.Dock = DockStyle.Fill;
            tabSubExplorer.Location = new Point(0, 0);
            tabSubExplorer.Name = "tabSubExplorer";
            tabSubExplorer.SelectedIndex = 0;
            tabSubExplorer.Size = new Size(1176, 610);
            tabSubExplorer.TabIndex = 0;
            // 
            // tabKutaisiRoutes
            // 
            tabKutaisiRoutes.Controls.Add(splitKutaisi);
            tabKutaisiRoutes.Location = new Point(4, 26);
            tabKutaisiRoutes.Name = "tabKutaisiRoutes";
            tabKutaisiRoutes.Padding = new Padding(8);
            tabKutaisiRoutes.Size = new Size(1168, 580);
            tabKutaisiRoutes.TabIndex = 0;
            tabKutaisiRoutes.Text = "Kutaisi Municipal Routes (22 Routes / 44 Directions)";
            tabKutaisiRoutes.UseVisualStyleBackColor = true;
            // 
            // splitKutaisi
            // 
            splitKutaisi.Dock = DockStyle.Fill;
            splitKutaisi.Location = new Point(8, 8);
            splitKutaisi.Name = "splitKutaisi";
            // 
            // splitKutaisi.Panel1
            // 
            splitKutaisi.Panel1.Controls.Add(lstMunicipalPatterns);
            // 
            // splitKutaisi.Panel2
            // 
            splitKutaisi.Panel2.Controls.Add(dgvMunicipalStops);
            splitKutaisi.Size = new Size(1152, 564);
            splitKutaisi.SplitterDistance = 340;
            splitKutaisi.TabIndex = 0;
            // 
            // lstMunicipalPatterns
            // 
            lstMunicipalPatterns.Dock = DockStyle.Fill;
            lstMunicipalPatterns.FormattingEnabled = true;
            lstMunicipalPatterns.ItemHeight = 17;
            lstMunicipalPatterns.Location = new Point(0, 0);
            lstMunicipalPatterns.Name = "lstMunicipalPatterns";
            lstMunicipalPatterns.Size = new Size(340, 564);
            lstMunicipalPatterns.TabIndex = 0;
            lstMunicipalPatterns.SelectedIndexChanged += lstMunicipalPatterns_SelectedIndexChanged;
            // 
            // dgvMunicipalStops
            // 
            dgvMunicipalStops.AllowUserToAddRows = false;
            dgvMunicipalStops.AllowUserToDeleteRows = false;
            dgvMunicipalStops.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvMunicipalStops.BackgroundColor = Color.White;
            dgvMunicipalStops.BorderStyle = BorderStyle.Fixed3D;
            dgvMunicipalStops.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvMunicipalStops.Dock = DockStyle.Fill;
            dgvMunicipalStops.Location = new Point(0, 0);
            dgvMunicipalStops.MultiSelect = false;
            dgvMunicipalStops.Name = "dgvMunicipalStops";
            dgvMunicipalStops.ReadOnly = true;
            dgvMunicipalStops.RowHeadersVisible = false;
            dgvMunicipalStops.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvMunicipalStops.Size = new Size(808, 564);
            dgvMunicipalStops.TabIndex = 0;
            // 
            // tabIntercityServices
            // 
            tabIntercityServices.Controls.Add(dgvIntercityServices);
            tabIntercityServices.Location = new Point(4, 26);
            tabIntercityServices.Name = "tabIntercityServices";
            tabIntercityServices.Padding = new Padding(8);
            tabIntercityServices.Size = new Size(1168, 580);
            tabIntercityServices.TabIndex = 1;
            tabIntercityServices.Text = "Intercity Operators & Services (585 Services / 19 Operators)";
            tabIntercityServices.UseVisualStyleBackColor = true;
            // 
            // dgvIntercityServices
            // 
            dgvIntercityServices.AllowUserToAddRows = false;
            dgvIntercityServices.AllowUserToDeleteRows = false;
            dgvIntercityServices.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvIntercityServices.BackgroundColor = Color.White;
            dgvIntercityServices.BorderStyle = BorderStyle.Fixed3D;
            dgvIntercityServices.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvIntercityServices.Dock = DockStyle.Fill;
            dgvIntercityServices.Location = new Point(8, 8);
            dgvIntercityServices.MultiSelect = false;
            dgvIntercityServices.Name = "dgvIntercityServices";
            dgvIntercityServices.ReadOnly = true;
            dgvIntercityServices.RowHeadersVisible = false;
            dgvIntercityServices.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvIntercityServices.Size = new Size(1152, 564);
            dgvIntercityServices.TabIndex = 0;
            // 
            // tabDepartureBoard
            // 
            tabDepartureBoard.Controls.Add(dgvUpcomingDepartures);
            tabDepartureBoard.Controls.Add(pnlDepartureTop);
            tabDepartureBoard.Location = new Point(4, 26);
            tabDepartureBoard.Name = "tabDepartureBoard";
            tabDepartureBoard.Padding = new Padding(8);
            tabDepartureBoard.Size = new Size(1168, 580);
            tabDepartureBoard.TabIndex = 2;
            tabDepartureBoard.Text = "Smart Departure Board";
            tabDepartureBoard.UseVisualStyleBackColor = true;
            // 
            // pnlDepartureTop
            // 
            pnlDepartureTop.Controls.Add(cmbDepartureStop);
            pnlDepartureTop.Controls.Add(lblDepartureStop);
            pnlDepartureTop.Controls.Add(lblDepartureBoardStatus);
            pnlDepartureTop.Dock = DockStyle.Top;
            pnlDepartureTop.Location = new Point(8, 8);
            pnlDepartureTop.Name = "pnlDepartureTop";
            pnlDepartureTop.Size = new Size(1152, 46);
            pnlDepartureTop.TabIndex = 0;
            // 
            // cmbDepartureStop
            // 
            cmbDepartureStop.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbDepartureStop.FormattingEnabled = true;
            cmbDepartureStop.Location = new Point(140, 10);
            cmbDepartureStop.Name = "cmbDepartureStop";
            cmbDepartureStop.Size = new Size(380, 25);
            cmbDepartureStop.TabIndex = 1;
            cmbDepartureStop.SelectedIndexChanged += cmbDepartureStop_SelectedIndexChanged;
            // 
            // lblDepartureStop
            // 
            lblDepartureStop.AutoSize = true;
            lblDepartureStop.Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold);
            lblDepartureStop.Location = new Point(10, 14);
            lblDepartureStop.Name = "lblDepartureStop";
            lblDepartureStop.Size = new Size(122, 15);
            lblDepartureStop.TabIndex = 0;
            lblDepartureStop.Text = "Kutaisi stop";
            lblDepartureBoardStatus.AutoSize = true;
            lblDepartureBoardStatus.Location = new Point(532, 14);
            lblDepartureBoardStatus.Name = "lblDepartureBoardStatus";
            // 
            // dgvUpcomingDepartures
            // 
            dgvUpcomingDepartures.AllowUserToAddRows = false;
            dgvUpcomingDepartures.AllowUserToDeleteRows = false;
            dgvUpcomingDepartures.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUpcomingDepartures.BackgroundColor = Color.White;
            dgvUpcomingDepartures.BorderStyle = BorderStyle.Fixed3D;
            dgvUpcomingDepartures.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvUpcomingDepartures.Dock = DockStyle.Fill;
            dgvUpcomingDepartures.Location = new Point(8, 54);
            dgvUpcomingDepartures.MultiSelect = false;
            dgvUpcomingDepartures.Name = "dgvUpcomingDepartures";
            dgvUpcomingDepartures.ReadOnly = true;
            dgvUpcomingDepartures.RowHeadersVisible = false;
            dgvUpcomingDepartures.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dgvUpcomingDepartures.Size = new Size(1152, 518);
            dgvUpcomingDepartures.TabIndex = 1;
            // 
            // tabDataCoverage
            // 
            tabDataCoverage.Controls.Add(rtbDataCoverage);
            tabDataCoverage.Location = new Point(4, 26);
            tabDataCoverage.Name = "tabDataCoverage";
            tabDataCoverage.Padding = new Padding(12);
            tabDataCoverage.Size = new Size(1168, 580);
            tabDataCoverage.TabIndex = 3;
            tabDataCoverage.Text = "Dataset Provenance & Quality Report";
            tabDataCoverage.UseVisualStyleBackColor = true;
            // 
            // rtbDataCoverage
            // 
            rtbDataCoverage.BackColor = Color.White;
            rtbDataCoverage.BorderStyle = BorderStyle.None;
            rtbDataCoverage.Dock = DockStyle.Fill;
            rtbDataCoverage.Font = new Font("Consolas", 9.5F);
            rtbDataCoverage.Location = new Point(12, 12);
            rtbDataCoverage.Name = "rtbDataCoverage";
            rtbDataCoverage.ReadOnly = true;
            rtbDataCoverage.Size = new Size(1144, 556);
            rtbDataCoverage.TabIndex = 0;
            rtbDataCoverage.Text = "";
            // 
            // simTimer
            // 
            simTimer.Interval = 300;
            simTimer.Tick += simTimer_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 17F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1184, 712);
            Controls.Add(tabControlMain);
            Controls.Add(pnlTopBanner);
            Font = new Font("Segoe UI", 9.5F);
            MinimumSize = new Size(1060, 600);
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Georgia Transit Intelligence — National Bus Network & Timetable Control Center";
            Load += MainForm_Load;
            pnlTopBanner.ResumeLayout(false);
            pnlTopBanner.PerformLayout();
            tabControlMain.ResumeLayout(false);
            tabCrud.ResumeLayout(false);
            tabCrud.PerformLayout();
            pnlCrudControls.ResumeLayout(false);
            pnlCrudControls.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvBusRoutes).EndInit();
            statusStrip.ResumeLayout(false);
            statusStrip.PerformLayout();
            tabPlanner.ResumeLayout(false);
            pnlPlannerLeft.ResumeLayout(false);
            pnlPlannerLeft.PerformLayout();
            grpModes.ResumeLayout(false);
            grpModes.PerformLayout();
            pnlPlannerRight.ResumeLayout(false);
            splitPlanner.Panel1.ResumeLayout(false);
            splitPlanner.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitPlanner).EndInit();
            splitPlanner.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvItineraries).EndInit();
            pnlItineraryDetails.ResumeLayout(false);
            pnlItineraryDetails.PerformLayout();
            tabSimulation.ResumeLayout(false);
            pnlSimControls.ResumeLayout(false);
            pnlSimControls.PerformLayout();
            pnlVehicleInspector.ResumeLayout(false);
            pnlVehicleInspector.PerformLayout();
            tabExplorer.ResumeLayout(false);
            tabSubExplorer.ResumeLayout(false);
            tabKutaisiRoutes.ResumeLayout(false);
            splitKutaisi.Panel1.ResumeLayout(false);
            splitKutaisi.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitKutaisi).EndInit();
            splitKutaisi.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvMunicipalStops).EndInit();
            tabIntercityServices.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)dgvIntercityServices).EndInit();
            tabDepartureBoard.ResumeLayout(false);
            pnlDepartureTop.ResumeLayout(false);
            pnlDepartureTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)dgvUpcomingDepartures).EndInit();
            tabDataCoverage.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Panel pnlTopBanner;
        private Label lblAppTitle;
        private Label lblAppSubtitle;
        private TabControl tabControlMain;
        private TabPage tabCrud;
        private Panel pnlCrudControls;
        private Label lblSearch;
        private TextBox txtSearch;
        private Label lblFilter;
        private ComboBox cmbFilter;
        private Button btnAdd;
        private Button btnEdit;
        private Button btnDelete;
        private Button btnExportCsv;
        private Button btnRefresh;
        private DataGridView dgvBusRoutes;
        private StatusStrip statusStrip;
        private ToolStripStatusLabel lblStatus;
        private ToolStripStatusLabel lblDbStatus;
        private TabPage tabPlanner;
        private Panel pnlPlannerLeft;
        private Label lblPlannerOrigin;
        private ComboBox cmbPlannerOrigin;
        private Label lblPlannerDestination;
        private ComboBox cmbPlannerDestination;
        private Label lblPlannerTime;
        private DateTimePicker dtpPlannerTime;
        private Label lblPlannerOptimization;
        private ComboBox cmbPlannerOptimization;
        private GroupBox grpModes;
        private CheckBox chkAllowMunicipal;
        private CheckBox chkAllowIntercity;
        private CheckBox chkAllowMarshrutka;
        private Button btnPlanJourney;
        private Button btnSimulateJourney;
        private Panel pnlPlannerRight;
        private SplitContainer splitPlanner;
        private DataGridView dgvItineraries;
        private Panel pnlItineraryDetails;
        private Label lblItineraryDetailsHeader;
        private ListView lstJourneyLegs;
        private ColumnHeader colLegMode;
        private ColumnHeader colLegFrom;
        private ColumnHeader colLegTo;
        private ColumnHeader colLegTime;
        private ColumnHeader colLegDuration;
        private ColumnHeader colLegFare;
        private ColumnHeader colLegInstructions;
        private TabPage tabSimulation;
        private Panel pnlSimControls;
        private Button btnSimPlayPause;
        private Button btnSimReset;
        private Label lblSimSpeed;
        private ComboBox cmbSimSpeed;
        private Button btnZoomIn;
        private Button btnZoomOut;
        private Button btnZoomGeorgia;
        private Button btnZoomKutaisi;
        private Label lblSimClock;
        private GeoMapControl geoMapControl;
        private Panel pnlVehicleInspector;
        private Label lblInspectorHeader;
        private Label lblInspectorOperator;
        private Label lblInspectorRoute;
        private Label lblInspectorDeparture;
        private Label lblInspectorArrival;
        private Label lblInspectorNextStop;
        private Label lblInspectorStatus;
        private TabPage tabExplorer;
        private TabControl tabSubExplorer;
        private TabPage tabKutaisiRoutes;
        private SplitContainer splitKutaisi;
        private ListBox lstMunicipalPatterns;
        private DataGridView dgvMunicipalStops;
        private TabPage tabIntercityServices;
        private DataGridView dgvIntercityServices;
        private TabPage tabDepartureBoard;
        private Panel pnlDepartureTop;
        private Label lblDepartureStop;
        private Label lblDepartureBoardStatus;
        private ComboBox cmbDepartureStop;
        private DataGridView dgvUpcomingDepartures;
        private TabPage tabDataCoverage;
        private RichTextBox rtbDataCoverage;
        private System.Windows.Forms.Timer simTimer;
    }
}
