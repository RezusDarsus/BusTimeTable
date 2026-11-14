using System.Drawing;
using System.Drawing.Imaging;
using GeorgiaTransitIntelligence.Models;
using Xunit;

namespace GeorgiaTransitIntelligence.Tests
{
    public class ScreenshotGenerator
    {
        [Fact]
        public void CaptureRealApplicationScreenshots()
        {
            Exception? captureError = null;
            Thread thread = new Thread(() =>
            {
                try
                {
                    Application.EnableVisualStyles();
                    CaptureScreenshots();
                }
                catch (Exception ex)
                {
                    captureError = ex;
                }
            });

            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            Assert.True(thread.Join(TimeSpan.FromSeconds(90)), "Screenshot capture timed out.");
            Assert.Null(captureError);
        }

        private static void CaptureScreenshots()
        {
            string solutionScreenshotsDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "screenshots"));
            Directory.CreateDirectory(solutionScreenshotsDir);

            // 1. Main Window
            using (var mainForm = new MainForm())
            {
                mainForm.Size = new Size(1184, 712);
                mainForm.StartPosition = FormStartPosition.Manual;
                mainForm.Location = new Point(-2000, 0);
                mainForm.Show();
                Application.DoEvents();
                var grid = Assert.IsType<DataGridView>(mainForm.Controls.Find("dgvBusRoutes", true).Single());
                Assert.True(grid.Rows.Count >= 15);
                int unfilteredCount = grid.Rows.Count;

                using Bitmap bmp1 = new Bitmap(mainForm.Width, mainForm.Height);
                mainForm.DrawToBitmap(bmp1, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                SaveRenderedScreenshot(bmp1, Path.Combine(solutionScreenshotsDir, "1_main_window.png"));

                // 2. Search Results
                var txtSearch = mainForm.Controls.Find("txtSearch", true)[0] as TextBox;
                txtSearch!.Text = "Zugdidi";
                Application.DoEvents();
                Assert.InRange(grid.Rows.Count, 1, unfilteredCount - 1);

                using Bitmap bmp2 = new Bitmap(mainForm.Width, mainForm.Height);
                mainForm.DrawToBitmap(bmp2, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                SaveRenderedScreenshot(bmp2, Path.Combine(solutionScreenshotsDir, "2_search_results.png"));

                // 5. Journey Planner
                var tabs = mainForm.Controls.Find("tabControlMain", true)[0] as TabControl;
                tabs!.SelectedIndex = 1;
                Application.DoEvents();

                var origin = Assert.IsType<ComboBox>(mainForm.Controls.Find("cmbPlannerOrigin", true).Single());
                var destination = Assert.IsType<ComboBox>(mainForm.Controls.Find("cmbPlannerDestination", true).Single());
                var departure = Assert.IsType<DateTimePicker>(mainForm.Controls.Find("dtpPlannerTime", true).Single());
                origin.Text = "Kutaisi International University (KIU)";
                destination.Text = "Tbilisi";
                departure.Value = DateTime.Today.AddHours(8);
                var btnPlan = mainForm.Controls.Find("btnPlanJourney", true)[0] as Button;
                btnPlan!.PerformClick();
                Application.DoEvents();
                var itineraries = Assert.IsType<DataGridView>(mainForm.Controls.Find("dgvItineraries", true).Single());
                Assert.NotEmpty(itineraries.Rows.Cast<DataGridViewRow>());

                using Bitmap bmp5 = new Bitmap(mainForm.Width, mainForm.Height);
                mainForm.DrawToBitmap(bmp5, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                SaveRenderedScreenshot(bmp5, Path.Combine(solutionScreenshotsDir, "5_journey_planner.png"));

                // 6. National Live Simulation
                var simulate = Assert.IsType<Button>(mainForm.Controls.Find("btnSimulateJourney", true).Single());
                simulate.PerformClick();
                Application.DoEvents();
                Assert.Equal(2, tabs.SelectedIndex);

                using Bitmap bmp6 = new Bitmap(mainForm.Width, mainForm.Height);
                mainForm.DrawToBitmap(bmp6, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                // DrawToBitmap paints overlapping WinForms child controls in reverse order.
                var inspector = Assert.IsType<Panel>(mainForm.Controls.Find("pnlVehicleInspector", true).Single());
                Assert.True(inspector.Visible);
                Assert.True(inspector.Parent!.ClientRectangle.Contains(inspector.Bounds));
                using var inspectorBitmap = new Bitmap(inspector.Width, inspector.Height);
                inspector.DrawToBitmap(inspectorBitmap, inspector.ClientRectangle);
                Point inspectorLocation = mainForm.PointToClient(inspector.PointToScreen(Point.Empty));
                Point clientInset = mainForm.PointToScreen(Point.Empty) - (Size)mainForm.Location;
                using (Graphics composition = Graphics.FromImage(bmp6))
                {
                    composition.DrawImageUnscaled(inspectorBitmap, inspectorLocation + (Size)clientInset);
                }
                SaveRenderedScreenshot(bmp6, Path.Combine(solutionScreenshotsDir, "6_national_live_simulation.png"));

                // 7. Network Explorer
                tabs.SelectedIndex = 3;
                Application.DoEvents();

                using Bitmap bmp7 = new Bitmap(mainForm.Width, mainForm.Height);
                mainForm.DrawToBitmap(bmp7, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                SaveRenderedScreenshot(bmp7, Path.Combine(solutionScreenshotsDir, "7_network_explorer.png"));
            }

            // 3. Add Dialog
            using (var addForm = new BusRouteForm())
            {
                addForm.StartPosition = FormStartPosition.Manual;
                addForm.Location = new Point(-2000, 0);
                addForm.Show();
                Application.DoEvents();

                using Bitmap bmp3 = new Bitmap(addForm.Width, addForm.Height);
                addForm.DrawToBitmap(bmp3, new Rectangle(0, 0, addForm.Width, addForm.Height));
                SaveRenderedScreenshot(bmp3, Path.Combine(solutionScreenshotsDir, "3_add_dialog.png"));
            }

            // 4. Edit Dialog
            var sampleRoute = new BusRoute
            {
                Id = 1,
                RouteNumber = "GEO-101",
                Origin = "Tbilisi (Ortachala)",
                Destination = "Batumi (New Station)",
                DepartureTime = new TimeSpan(7, 0, 0),
                DurationMinutes = 330,
                Price = 40.00m,
                IsExpress = true
            };

            using (var editForm = new BusRouteForm(sampleRoute))
            {
                editForm.StartPosition = FormStartPosition.Manual;
                editForm.Location = new Point(-2000, 0);
                editForm.Show();
                Application.DoEvents();

                using Bitmap bmp4 = new Bitmap(editForm.Width, editForm.Height);
                editForm.DrawToBitmap(bmp4, new Rectangle(0, 0, editForm.Width, editForm.Height));
                SaveRenderedScreenshot(bmp4, Path.Combine(solutionScreenshotsDir, "4_edit_dialog.png"));
            }
        }

        private static void SaveRenderedScreenshot(Bitmap bitmap, string path)
        {
            // A blank client area has only one color. Ignore the window chrome.
            var colors = new HashSet<int>();
            for (int y = 50; y < bitmap.Height - 20 && colors.Count < 12; y += 8)
            {
                for (int x = 20; x < bitmap.Width - 20; x += 8)
                {
                    colors.Add(bitmap.GetPixel(x, y).ToArgb());
                }
            }
            Assert.True(colors.Count >= 12, $"The client area did not render for {Path.GetFileName(path)}.");
            bitmap.Save(path, ImageFormat.Png);
        }
    }
}
