using GeorgiaTransitIntelligence.Models;

namespace GeorgiaTransitIntelligence
{
    /// <summary>
    /// Dialog form for Adding and Editing bus routes.
    /// Fulfills Requirements 6, 7, and 10 of KIU C# Windows Forms Midterm.
    /// </summary>
    public partial class BusRouteForm : Form
    {
        /// <summary>
        /// Gets the route being created or modified.
        /// </summary>
        public BusRoute CurrentRoute { get; private set; }

        /// <summary>
        /// Constructor 1: Used for adding a new route.
        /// </summary>
        public BusRouteForm()
        {
            InitializeComponent();
            Text = "Add Bus Route";
            lblHeaderTitle.Text = "Add New Bus Route";
            lblHeaderSubtitle.Text = "Register a new route pattern in the transit database";
            btnSave.Text = "Add Route";
            dtpDepartureTime.Value = DateTime.Today.AddHours(8); // Default 08:00
            CurrentRoute = new BusRoute();
        }

        /// <summary>
        /// Constructor 2: Used for editing an existing route.
        /// Calls : this() to ensure all controls are created before populating.
        /// </summary>
        /// <param name="route">The existing route to edit.</param>
        public BusRouteForm(BusRoute route) : this()
        {
            ArgumentNullException.ThrowIfNull(route);

            Text = "Edit Bus Route";
            lblHeaderTitle.Text = $"Edit Route {route.RouteNumber}";
            lblHeaderSubtitle.Text = $"Modifying route between {route.Origin} and {route.Destination}";
            btnSave.Text = "Save Changes";

            CurrentRoute = new BusRoute
            {
                Id = route.Id,
                RouteNumber = route.RouteNumber,
                Origin = route.Origin,
                Destination = route.Destination,
                DepartureTime = route.DepartureTime,
                DurationMinutes = route.DurationMinutes,
                Price = route.Price,
                IsExpress = route.IsExpress
            };

            // Pre-fill controls with existing values (Requirement 7 - Edit pre-population)
            txtRouteNumber.Text = route.RouteNumber;
            txtOrigin.Text = route.Origin;
            txtDestination.Text = route.Destination;
            dtpDepartureTime.Value = DateTime.Today.Add(route.DepartureTime);
            numDurationMinutes.Value = Math.Clamp(route.DurationMinutes, numDurationMinutes.Minimum, numDurationMinutes.Maximum);
            numPrice.Value = Math.Clamp(route.Price, numPrice.Minimum, numPrice.Maximum);
            chkIsExpress.Checked = route.IsExpress;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Input checks (Requirement 10)
            if (string.IsNullOrWhiteSpace(txtRouteNumber.Text))
            {
                MessageBox.Show("Route number cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRouteNumber.Focus();
                return; // Dialog remains open
            }

            if (string.IsNullOrWhiteSpace(txtOrigin.Text))
            {
                MessageBox.Show("Origin station cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtOrigin.Focus();
                return; // Dialog remains open
            }

            if (string.IsNullOrWhiteSpace(txtDestination.Text))
            {
                MessageBox.Show("Destination cannot be empty.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDestination.Focus();
                return; // Dialog remains open
            }

            // Custom domain rule: Origin and Destination must not be identical
            if (string.Equals(txtOrigin.Text.Trim(), txtDestination.Text.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("Origin and destination cannot be identical for an intercity or municipal route.",
                    "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDestination.Focus();
                return; // Dialog remains open
            }

            if (numDurationMinutes.Value <= 0)
            {
                MessageBox.Show("Duration must be at least 1 minute.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numDurationMinutes.Focus();
                return; // Dialog remains open
            }

            if (numPrice.Value < 0)
            {
                MessageBox.Show("Price cannot be negative.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                numPrice.Focus();
                return; // Dialog remains open
            }

            // Assign validated values to CurrentRoute
            CurrentRoute.RouteNumber = txtRouteNumber.Text.Trim();
            CurrentRoute.Origin = txtOrigin.Text.Trim();
            CurrentRoute.Destination = txtDestination.Text.Trim();
            CurrentRoute.DepartureTime = dtpDepartureTime.Value.TimeOfDay;
            CurrentRoute.DurationMinutes = (int)numDurationMinutes.Value;
            CurrentRoute.Price = numPrice.Value;
            CurrentRoute.IsExpress = chkIsExpress.Checked;

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
