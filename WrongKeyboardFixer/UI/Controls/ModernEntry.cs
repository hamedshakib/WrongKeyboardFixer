namespace WrongKeyboardFixer.UI.Controls;

/// <summary>
/// A styled MAUI Entry with rounded border, focus highlighting, and placeholder text.
/// Matches the WinForms ModernTextBox appearance.
/// </summary>
public class ModernEntry : Border
{
    private readonly Entry _entry;
    private bool _isFocused;

    public static readonly BindableProperty PlaceholderTextProperty =
        BindableProperty.Create(nameof(PlaceholderText), typeof(string), typeof(ModernEntry), "");

    public string PlaceholderText
    {
        get => (string)GetValue(PlaceholderTextProperty);
        set => SetValue(PlaceholderTextProperty, value);
    }

    public static readonly BindableProperty EntryTextProperty =
        BindableProperty.Create(nameof(EntryText), typeof(string), typeof(ModernEntry), "",
            defaultBindingMode: BindingMode.TwoWay);

    public string EntryText
    {
        get => (string)GetValue(EntryTextProperty);
        set => SetValue(EntryTextProperty, value);
    }

    public new event EventHandler? TextChanged;

    public ModernEntry()
    {
        CornerRadius = new CornerRadius(8);
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) };
        BackgroundColor = Theme.Surface;
        Stroke = Theme.Border;
        StrokeThickness = 1;
        Padding = new Thickness(12, 0);
        HeightRequest = 36;

        _entry = new Entry
        {
            Placeholder = PlaceholderText,
            PlaceholderColor = Theme.TextSecondary,
            TextColor = Theme.TextPrimary,
            BackgroundColor = Colors.Transparent,
            FontSize = Theme.BodyFontSize,
            VerticalOptions = LayoutOptions.Center,
        };

        _entry.Focused += (_, _) =>
        {
            _isFocused = true;
            Stroke = Theme.Accent;
            StrokeThickness = 1.5;
        };

        _entry.Unfocused += (_, _) =>
        {
            _isFocused = false;
            Stroke = Theme.Border;
            StrokeThickness = 1;
        };

        _entry.TextChanged += (_, e) =>
        {
            EntryText = e.NewTextValue ?? "";
            TextChanged?.Invoke(this, e);
        };

        Content = _entry;
    }

    protected override void OnBindingContextChanged()
    {
        base.OnBindingContextChanged();
        _entry.Placeholder = PlaceholderText;
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == PlaceholderTextProperty.PropertyName)
            _entry.Placeholder = PlaceholderText;
        if (propertyName == EntryTextProperty.PropertyName && _entry.Text != EntryText)
            _entry.Text = EntryText;
    }

    public void Clear()
    {
        _entry.Text = "";
        EntryText = "";
    }

    public new void Focus()
    {
        _entry.Focus();
    }
}
