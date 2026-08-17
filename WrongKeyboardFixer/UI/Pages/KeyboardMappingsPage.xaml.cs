using WrongKeyboardFixer.ViewModels;

namespace WrongKeyboardFixer.Pages;

public partial class KeyboardMappingsPage : ContentPage
{
    private readonly KeyboardMappingsViewModel _viewModel;

    public KeyboardMappingsPage(KeyboardMappingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.RequestClose += OnRequestClose;
    }

    private async void OnRequestClose(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
