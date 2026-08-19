using System;
using System.Threading;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.Services.Conversion;
using WrongKeyboardFixer.UI.Forms;

namespace WrongKeyboardFixer;

/// <summary>
/// Application entry point with dependency injection setup.
/// All services are registered and resolved through static factories.
/// </summary>
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    private const string MutexName = "Global\\WrongKeyboardFixer_Mutex";
    private static Mutex? _mutex;

    // ──────────────────────────────────────────────────────────────────────────
    // DI Container - Simple service locator pattern
    // ──────────────────────────────────────────────────────────────────────────
    private static AppSettings? _appSettings;
    private static ILogger? _logger;
    private static IKeyboardConverter? _converter;

    private static ILogger Logger => _logger ??= new ConsoleLogger();

    private static AppSettings AppSettings => _appSettings ??= SettingsManager.Load();

    private static IKeyboardConverter Converter => _converter ??= new KeyboardConverter();

    private static ClipboardManager CreateClipboardManager() =>
        new ClipboardManager(Logger);

    private static IClipboardManager ClipboardManager => CreateClipboardManager();

    private static TextConversionService CreateTextConversionService() =>
        new TextConversionService(CreateClipboardManager(), AppSettings, Converter, Logger);

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

            // Initialize services before running forms
            SettingsManager.Load(); // Load settings
            Logger.Info("Application started");

            Application.Run(new MainForm());
        }
        finally
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }
}