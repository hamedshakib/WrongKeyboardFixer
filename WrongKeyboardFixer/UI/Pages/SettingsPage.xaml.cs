using WrongKeyboardFixer.ViewModels;

namespace WrongKeyboardFixer.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;

    public SettingsPage(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.SettingsSaved += OnSettingsSaved;
        _viewModel.RequestClose += OnRequestClose;
        _viewModel.OpenMappingsRequested += OnOpenMappingsRequested;

        LanguagePicker.SelectedIndexChanged += (_, _) => _viewModel.OnLanguageChangedCommand.Execute(null);
        ModifierPicker.SelectedIndexChanged += (_, _) => _viewModel.OnModifierChangedCommand.Execute(null);
        KeyPicker.SelectedIndexChanged += (_, _) => _viewModel.OnKeyChangedCommand.Execute(null);
    }

    private void OnSettingsSaved(object? sender, EventArgs e)
    {
        DisplayAlert(
            Core.Helpers.Localization.Get("Success"),
            Core.Helpers.Localization.Get("SettingsSaved"),
            Core.Helpers.Localization.Get("OK"));
    }

    private void OnRequestClose(object? sender, EventArgs e)
    {
        // In MAUI, we can't close a page like WinForms.
        // The page stays in the navigation stack.
    }

    private async void OnOpenMappingsRequested(object? sender, EventArgs e)
    {
        var vm = new KeyboardMappingsViewModel();
        var page = new KeyboardMappingsPage(vm);
        await Navigation.PushModalAsync(page);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
}
