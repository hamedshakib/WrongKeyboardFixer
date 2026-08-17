using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();

        // Load saved language before navigating
        var settings = Core.Services.SettingsManager.Load();
        Localization.SetLanguage(settings.Language);

        MainPage = new AppShell();
    }
}
