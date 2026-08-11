using System;
using System.Threading;
using System.Windows.Forms;

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
            // زبان ذخیره‌شده را بارگذاری کن تا پیام به زبان درست نمایش داده شود
            Core.Helpers.Localization.SetLanguage(Core.Services.SettingsManager.Load().Language);

            MessageBox.Show(
                Core.Helpers.Localization.Get("AlreadyRunning"),
                Core.Helpers.Localization.Get("Attention"),
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