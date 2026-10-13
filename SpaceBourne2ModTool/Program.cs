using System;
using System.Windows.Forms;
using SpaceBourne2ModTool.Services;

namespace SpaceBourne2ModTool
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// specSB2modTool - Unofficial community tool for UE4.27 modding.
        /// Supports: PAK extraction/packaging, Localization editing, Trainer templates,
        /// Loose file mods, Asset replacement workflows.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Initialize logging and configuration early
            LoggerService.Initialize();
            ConfigService.Load();

            try
            {
                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                LoggerService.LogError("Unhandled exception in Main", ex);
                MessageBox.Show(
                    "A critical error occurred:\n\n" + ex.Message +
                    "\n\nSee log file for details.",
                    "specSB2modTool - Fatal Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            finally
            {
                ConfigService.Save();
                LoggerService.Shutdown();
            }
        }
    }
}
