namespace GeorgiaTransitIntelligence
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Standard Windows Forms initialization
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
