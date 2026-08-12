using System;
using System.Threading;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Services;

namespace WrongKeyboardFixer;

internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private const string MutexName = "Global\\WrongKeyboardFixer_Mutex";
    private static Mutex? _mutex;

    [STAThread]
    private static void Main()
    {
        bool createdNew;
        _mutex = new Mutex(true, MutexName, out createdNew);

        if (!createdNew)
        {
            // Load the saved language so the message displays in the correct locale
            Localization.SetLanguage(SettingsManager.Load().Language);

            MessageBox.Show(
                Localization.Get("AlreadyRunning"),
                Localization.Get("Attention"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning
            );
            return;
        }

        try
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UI.Forms.MainForm());
        }
        finally
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }
}