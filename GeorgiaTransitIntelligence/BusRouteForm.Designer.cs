namespace GeorgiaTransitIntelligence
{
    partial class BusRouteForm
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
            lblRouteNumber = new Label();
            txtRouteNumber = new TextBox();
            lblOrigin = new Label();
            txtOrigin = new TextBox();
            lblDestination = new Label();
            txtDestination = new TextBox();
            lblDepartureTime = new Label();
            dtpDepartureTime = new DateTimePicker();
            lblDurationMinutes = new Label();
            numDurationMinutes = new NumericUpDown();
            lblPrice = new Label();
            numPrice = new NumericUpDown();
            lblIsExpress = new Label();
            chkIsExpress = new CheckBox();
            btnSave = new Button();
            btnCancel = new Button();
            pnlHeader = new Panel();
            lblHeaderTitle = new Label();
            lblHeaderSubtitle = new Label();
            ((System.ComponentModel.ISupportInitialize)numDurationMinutes).BeginInit();
            ((System.ComponentModel.ISupportInitialize)numPrice).BeginInit();
            pnlHeader.SuspendLayout();
            SuspendLayout();
            // 
            // pnlHeader
            // 
            pnlHeader.BackColor = Color.FromArgb(24, 38, 68);
            pnlHeader.Controls.Add(lblHeaderSubtitle);
            pnlHeader.Controls.Add(lblHeaderTitle);
            pnlHeader.Dock = DockStyle.Top;
            pnlHeader.Location = new Point(0, 0);
            pnlHeader.Name = "pnlHeader";
            pnlHeader.Size = new Size(440, 68);
            pnlHeader.TabIndex = 14;
            // 
            // lblHeaderTitle
            // 
            lblHeaderTitle.AutoSize = true;
            lblHeaderTitle.Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold);
            lblHeaderTitle.ForeColor = Color.White;
            lblHeaderTitle.Location = new Point(16, 12);
            lblHeaderTitle.Name = "lblHeaderTitle";
            lblHeaderTitle.Size = new Size(137, 25);
            lblHeaderTitle.TabIndex = 0;
            lblHeaderTitle.Text = "Bus Route Entry";
            // 
            // lblHeaderSubtitle
            // 
            lblHeaderSubtitle.AutoSize = true;
            lblHeaderSubtitle.Font = new Font("Segoe UI", 9F);
            lblHeaderSubtitle.ForeColor = Color.FromArgb(200, 215, 240);
            lblHeaderSubtitle.Location = new Point(18, 38);
            lblHeaderSubtitle.Name = "lblHeaderSubtitle";
            lblHeaderSubtitle.Size = new Size(260, 15);
            lblHeaderSubtitle.TabIndex = 1;
            lblHeaderSubtitle.Text = "Enter timetable details for Georgian transit service";
            // 
            // lblRouteNumber
            // 
            lblRouteNumber.AutoSize = true;
            lblRouteNumber.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblRouteNumber.Location = new Point(24, 88);
            lblRouteNumber.Name = "lblRouteNumber";
            lblRouteNumber.Size = new Size(102, 17);
            lblRouteNumber.TabIndex = 0;
            lblRouteNumber.Text = "Route Number:";
            // 
            // txtRouteNumber
            // 
            txtRouteNumber.Font = new Font("Segoe UI", 10F);
            txtRouteNumber.Location = new Point(150, 85);
            txtRouteNumber.MaxLength = 20;
            txtRouteNumber.Name = "txtRouteNumber";
            txtRouteNumber.PlaceholderText = "e.g. GEO-101 or 34";
            txtRouteNumber.Size = new Size(260, 25);
            txtRouteNumber.TabIndex = 1;
            // 
            // lblOrigin
            // 
            lblOrigin.AutoSize = true;
            lblOrigin.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblOrigin.Location = new Point(24, 126);
            lblOrigin.Name = "lblOrigin";
            lblOrigin.Size = new Size(91, 17);
            lblOrigin.TabIndex = 2;
            lblOrigin.Text = "Origin Station:";
            // 
            // txtOrigin
            // 
            txtOrigin.Font = new Font("Segoe UI", 10F);
            txtOrigin.Location = new Point(150, 123);
            txtOrigin.MaxLength = 100;
            txtOrigin.Name = "txtOrigin";
            txtOrigin.PlaceholderText = "e.g. Kutaisi (Chavchavadze)";
            txtOrigin.Size = new Size(260, 25);
            txtOrigin.TabIndex = 3;
            // 
            // lblDestination
            // 
            lblDestination.AutoSize = true;
            lblDestination.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblDestination.Location = new Point(24, 164);
            lblDestination.Name = "lblDestination";
            lblDestination.Size = new Size(80, 17);
            lblDestination.TabIndex = 4;
            lblDestination.Text = "Destination:";
            // 
            // txtDestination
            // 
            txtDestination.Font = new Font("Segoe UI", 10F);
            txtDestination.Location = new Point(150, 161);
            txtDestination.MaxLength = 100;
            txtDestination.Name = "txtDestination";
            txtDestination.PlaceholderText = "e.g. Batumi (Central)";
            txtDestination.Size = new Size(260, 25);
            txtDestination.TabIndex = 5;
            // 
            // lblDepartureTime
            // 
            lblDepartureTime.AutoSize = true;
            lblDepartureTime.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblDepartureTime.Location = new Point(24, 202);
            lblDepartureTime.Name = "lblDepartureTime";
            lblDepartureTime.Size = new Size(106, 17);
            lblDepartureTime.TabIndex = 6;
            lblDepartureTime.Text = "Departure Time:";
            // 
            // dtpDepartureTime
            // 
            dtpDepartureTime.CustomFormat = "HH:mm";
            dtpDepartureTime.Font = new Font("Segoe UI", 10F);
            dtpDepartureTime.Format = DateTimePickerFormat.Custom;
            dtpDepartureTime.Location = new Point(150, 199);
            dtpDepartureTime.Name = "dtpDepartureTime";
            dtpDepartureTime.ShowUpDown = true;
            dtpDepartureTime.Size = new Size(130, 25);
            dtpDepartureTime.TabIndex = 7;
            // 
            // lblDurationMinutes
            // 
            lblDurationMinutes.AutoSize = true;
            lblDurationMinutes.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblDurationMinutes.Location = new Point(24, 240);
            lblDurationMinutes.Name = "lblDurationMinutes";
            lblDurationMinutes.Size = new Size(119, 17);
            lblDurationMinutes.TabIndex = 8;
            lblDurationMinutes.Text = "Duration (Minutes):";
            // 
            // numDurationMinutes
            // 
            numDurationMinutes.Font = new Font("Segoe UI", 10F);
            numDurationMinutes.Location = new Point(150, 238);
            numDurationMinutes.Maximum = new decimal(new int[] { 1440, 0, 0, 0 });
            numDurationMinutes.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numDurationMinutes.Name = "numDurationMinutes";
            numDurationMinutes.Size = new Size(130, 25);
            numDurationMinutes.TabIndex = 9;
            numDurationMinutes.Value = new decimal(new int[] { 120, 0, 0, 0 });
            // 
            // lblPrice
            // 
            lblPrice.AutoSize = true;
            lblPrice.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblPrice.Location = new Point(24, 278);
            lblPrice.Name = "lblPrice";
            lblPrice.Size = new Size(73, 17);
            lblPrice.TabIndex = 10;
            lblPrice.Text = "Price (GEL):";
            // 
            // numPrice
            // 
            numPrice.DecimalPlaces = 2;
            numPrice.Font = new Font("Segoe UI", 10F);
            numPrice.Increment = new decimal(new int[] { 50, 0, 0, 131072 });
            numPrice.Location = new Point(150, 276);
            numPrice.Maximum = new decimal(new int[] { 1000, 0, 0, 0 });
            numPrice.Name = "numPrice";
            numPrice.Size = new Size(130, 25);
            numPrice.TabIndex = 11;
            numPrice.Value = new decimal(new int[] { 1500, 0, 0, 131072 });
            // 
            // lblIsExpress
            // 
            lblIsExpress.AutoSize = true;
            lblIsExpress.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            lblIsExpress.Location = new Point(24, 316);
            lblIsExpress.Name = "lblIsExpress";
            lblIsExpress.Size = new Size(88, 17);
            lblIsExpress.TabIndex = 12;
            lblIsExpress.Text = "Service Type:";
            // 
            // chkIsExpress
            // 
            chkIsExpress.AutoSize = true;
            chkIsExpress.Font = new Font("Segoe UI", 9.5F);
            chkIsExpress.Location = new Point(150, 315);
            chkIsExpress.Name = "chkIsExpress";
            chkIsExpress.Size = new Size(133, 21);
            chkIsExpress.TabIndex = 13;
            chkIsExpress.Text = "Express Service";
            chkIsExpress.UseVisualStyleBackColor = true;
            // 
            // btnSave
            // 
            btnSave.BackColor = Color.FromArgb(15, 108, 189);
            btnSave.Cursor = Cursors.Hand;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.Font = new Font("Segoe UI Semibold", 9.5F, FontStyle.Bold);
            btnSave.ForeColor = Color.White;
            btnSave.Location = new Point(216, 362);
            btnSave.Name = "btnSave";
            btnSave.Size = new Size(96, 32);
            btnSave.TabIndex = 14;
            btnSave.Text = "Save";
            btnSave.UseVisualStyleBackColor = false;
            btnSave.Click += btnSave_Click;
            // 
            // btnCancel
            // 
            btnCancel.BackColor = Color.FromArgb(235, 237, 240);
            btnCancel.Cursor = Cursors.Hand;
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(200, 204, 210);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Font = new Font("Segoe UI", 9.5F);
            btnCancel.ForeColor = Color.FromArgb(30, 30, 30);
            btnCancel.Location = new Point(318, 362);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(92, 32);
            btnCancel.TabIndex = 15;
            btnCancel.Text = "Cancel";
            btnCancel.UseVisualStyleBackColor = false;
            btnCancel.Click += btnCancel_Click;
            // 
            // BusRouteForm
            // 
            AcceptButton = btnSave;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(250, 251, 253);
            CancelButton = btnCancel;
            ClientSize = new Size(440, 412);
            Controls.Add(btnCancel);
            Controls.Add(btnSave);
            Controls.Add(chkIsExpress);
            Controls.Add(lblIsExpress);
            Controls.Add(numPrice);
            Controls.Add(lblPrice);
            Controls.Add(numDurationMinutes);
            Controls.Add(lblDurationMinutes);
            Controls.Add(dtpDepartureTime);
            Controls.Add(lblDepartureTime);
            Controls.Add(txtDestination);
            Controls.Add(lblDestination);
            Controls.Add(txtOrigin);
            Controls.Add(lblOrigin);
            Controls.Add(txtRouteNumber);
            Controls.Add(lblRouteNumber);
            Controls.Add(pnlHeader);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BusRouteForm";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Bus Route Form";
            ((System.ComponentModel.ISupportInitialize)numDurationMinutes).EndInit();
            ((System.ComponentModel.ISupportInitialize)numPrice).EndInit();
            pnlHeader.ResumeLayout(false);
            pnlHeader.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel pnlHeader;
        private Label lblHeaderTitle;
        private Label lblHeaderSubtitle;
        private Label lblRouteNumber;
        private TextBox txtRouteNumber;
        private Label lblOrigin;
        private TextBox txtOrigin;
        private Label lblDestination;
        private TextBox txtDestination;
        private Label lblDepartureTime;
        private DateTimePicker dtpDepartureTime;
        private Label lblDurationMinutes;
        private NumericUpDown numDurationMinutes;
        private Label lblPrice;
        private NumericUpDown numPrice;
        private Label lblIsExpress;
        private CheckBox chkIsExpress;
        private Button btnSave;
        private Button btnCancel;
    }
}
