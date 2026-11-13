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
            Thread thread = new Thread(() =>
            {
                string solutionScreenshotsDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "screenshots"));
                Directory.CreateDirectory(solutionScreenshotsDir);

                // 1. Main Window
                using (var mainForm = new MainForm())
                {
                    mainForm.Size = new Size(1184, 712);
                    mainForm.CreateControl();
                    Application.DoEvents();

                    using Bitmap bmp1 = new Bitmap(mainForm.Width, mainForm.Height);
                    mainForm.DrawToBitmap(bmp1, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                    bmp1.Save(Path.Combine(solutionScreenshotsDir, "1_main_window.png"), ImageFormat.Png);

                    // 2. Search Results
                    var txtSearch = mainForm.Controls.Find("txtSearch", true)[0] as TextBox;
                    txtSearch!.Text = "Batumi";
                    Application.DoEvents();

                    using Bitmap bmp2 = new Bitmap(mainForm.Width, mainForm.Height);
                    mainForm.DrawToBitmap(bmp2, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                    bmp2.Save(Path.Combine(solutionScreenshotsDir, "2_search_results.png"), ImageFormat.Png);

                    // 5. Journey Planner
                    var tabs = mainForm.Controls.Find("tabControlMain", true)[0] as TabControl;
                    tabs!.SelectedIndex = 1;
                    Application.DoEvents();

                    var btnPlan = mainForm.Controls.Find("btnPlanJourney", true)[0] as Button;
                    btnPlan!.PerformClick();
                    Application.DoEvents();

                    using Bitmap bmp5 = new Bitmap(mainForm.Width, mainForm.Height);
                    mainForm.DrawToBitmap(bmp5, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                    bmp5.Save(Path.Combine(solutionScreenshotsDir, "5_journey_planner.png"), ImageFormat.Png);

                    // 6. National Live Simulation
                    tabs.SelectedIndex = 2;
                    Application.DoEvents();

                    using Bitmap bmp6 = new Bitmap(mainForm.Width, mainForm.Height);
                    mainForm.DrawToBitmap(bmp6, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                    bmp6.Save(Path.Combine(solutionScreenshotsDir, "6_national_live_simulation.png"), ImageFormat.Png);

                    // 7. Network Explorer
                    tabs.SelectedIndex = 3;
                    Application.DoEvents();

                    using Bitmap bmp7 = new Bitmap(mainForm.Width, mainForm.Height);
                    mainForm.DrawToBitmap(bmp7, new Rectangle(0, 0, mainForm.Width, mainForm.Height));
                    bmp7.Save(Path.Combine(solutionScreenshotsDir, "7_network_explorer.png"), ImageFormat.Png);
                }

                // 3. Add Dialog
                using (var addForm = new BusRouteForm())
                {
                    addForm.CreateControl();
                    Application.DoEvents();

                    using Bitmap bmp3 = new Bitmap(addForm.Width, addForm.Height);
                    addForm.DrawToBitmap(bmp3, new Rectangle(0, 0, addForm.Width, addForm.Height));
                    bmp3.Save(Path.Combine(solutionScreenshotsDir, "3_add_dialog.png"), ImageFormat.Png);
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
                    editForm.CreateControl();
                    Application.DoEvents();

                    using Bitmap bmp4 = new Bitmap(editForm.Width, editForm.Height);
                    editForm.DrawToBitmap(bmp4, new Rectangle(0, 0, editForm.Width, editForm.Height));
                    bmp4.Save(Path.Combine(solutionScreenshotsDir, "4_edit_dialog.png"), ImageFormat.Png);
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            string solutionScreenshotsDir = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "screenshots"));
            Assert.True(File.Exists(Path.Combine(solutionScreenshotsDir, "1_main_window.png")));
            Assert.True(File.Exists(Path.Combine(solutionScreenshotsDir, "2_search_results.png")));
            Assert.True(File.Exists(Path.Combine(solutionScreenshotsDir, "3_add_dialog.png")));
            Assert.True(File.Exists(Path.Combine(solutionScreenshotsDir, "4_edit_dialog.png")));
            Assert.True(File.Exists(Path.Combine(solutionScreenshotsDir, "5_journey_planner.png")));
            Assert.True(File.Exists(Path.Combine(solutionScreenshotsDir, "6_national_live_simulation.png")));
            Assert.True(File.Exists(Path.Combine(solutionScreenshotsDir, "7_network_explorer.png")));
        }
    }
}
