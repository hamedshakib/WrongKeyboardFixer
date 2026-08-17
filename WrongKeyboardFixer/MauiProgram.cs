using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.Services.Conversion;
using WrongKeyboardFixer.ViewModels;

namespace WrongKeyboardFixer;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit();

#if WINDOWS
        builder.ConfigureLifecycleEvents(events =>
        {
            events.AddWindows(dsl =>
            {
                dsl.OnWindowCreated((window) =>
                {
                    var services = window.Handler?.MauiContext?.Services;
                    if (services != null)
                    {
                        var trayService = services.GetService<Platforms.Windows.TrayIconService>();
                        trayService?.Initialize(window);
                    }
                });
            });
        });
#endif

        builder.ConfigureFonts(fonts =>
        {
            fonts.AddFont("segoeui.ttf", "OpenSansRegular");
            fonts.AddFont("segoeuib.ttf", "OpenSansSemibold");
        });

        // Platform services
#if WINDOWS
        builder.Services.AddSingleton<IClipboardService, Platforms.Windows.WindowsClipboardService>();
        builder.Services.AddSingleton<IKeyboardSimulator, Platforms.Windows.WindowsKeyboardSimulator>();
        builder.Services.AddSingleton<Platforms.Windows.WindowsHotkeyService>();
        builder.Services.AddSingleton<Platforms.Windows.WindowsRegistryService>();
        builder.Services.AddSingleton<Platforms.Windows.TrayIconService>();
#else
        // Fallback for non-Windows (stub implementations)
        builder.Services.AddSingleton<IClipboardService, StubClipboardService>();
        builder.Services.AddSingleton<IKeyboardSimulator, StubKeyboardSimulator>();
#endif

        // Core services
        builder.Services.AddSingleton<TextConversionService>();

        // ViewModels
        builder.Services.AddTransient<SettingsViewModel>();
        builder.Services.AddTransient<KeyboardMappingsViewModel>();

        // Pages
        builder.Services.AddTransient<Pages.SettingsPage>();
        builder.Services.AddTransient<Pages.KeyboardMappingsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}

#if !WINDOWS
/// <summary>Stub clipboard for non-Windows platforms.</summary>
internal sealed class StubClipboardService : IClipboardService
{
    public string GetText() => string.Empty;
    public Task<string> GetTextWithRetryAsync() => Task.FromResult(string.Empty);
    public void SetText(string text) { }
    public void RestoreText(string text) { }
}

/// <summary>Stub keyboard simulator for non-Windows platforms.</summary>
internal sealed class StubKeyboardSimulator : IKeyboardSimulator
{
    public void SendCtrlC() { }
    public void SendCtrlV() { }
}
#endif
