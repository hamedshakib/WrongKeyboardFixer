namespace WrongKeyboardFixer.UI.Controls;

/// <summary>
/// A rounded status label ("chip") that auto-sizes to its content.
/// Matches the WinForms StatusChip appearance.
/// </summary>
public class StatusChip : Border
{
    private readonly Label _label;

    public static readonly BindableProperty StatusTextProperty =
        BindableProperty.Create(nameof(StatusText), typeof(string), typeof(StatusChip), "");

    public string StatusText
    {
        get => (string)GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    public static readonly BindableProperty ChipTextColorProperty =
        BindableProperty.Create(nameof(ChipTextColor), typeof(Color), typeof(StatusChip), Theme.Info);

    public Color ChipTextColor
    {
        get => (Color)GetValue(ChipTextColorProperty);
        set => SetValue(ChipTextColorProperty, value);
    }

    public static readonly BindableProperty ChipBackgroundColorProperty =
        BindableProperty.Create(nameof(ChipBackgroundColor), typeof(Color), typeof(StatusChip), Theme.InfoSoft);

    public Color ChipBackgroundColor
    {
        get => (Color)GetValue(ChipBackgroundColorProperty);
        set => SetValue(ChipBackgroundColorProperty, value);
    }

    public StatusChip()
    {
        CornerRadius = new CornerRadius(8);
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(8) };
        BackgroundColor = Theme.InfoSoft;
        HeightRequest = 36;
        Padding = new Thickness(12, 0);
        StrokeThickness = 0;
        HasShadow = false;
        HorizontalOptions = LayoutOptions.Start;
        VerticalOptions = LayoutOptions.Center;

        _label = new Label
        {
            FontSize = Theme.BodyFontSize,
            FontAttributes = FontAttributes.Bold,
            TextColor = Theme.Info,
            VerticalOptions = LayoutOptions.Center,
            LineBreakMode = LineBreakMode.TailTruncation,
        };

        Content = _label;

        UpdateVisuals();
    }

    private static void OnChipPropertyChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is StatusChip chip)
            chip.UpdateVisuals();
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == StatusTextProperty.PropertyName ||
            propertyName == ChipTextColorProperty.PropertyName ||
            propertyName == ChipBackgroundColorProperty.PropertyName)
        {
            UpdateVisuals();
        }
    }

    private void UpdateVisuals()
    {
        _label.Text = StatusText;
        _label.TextColor = ChipTextColor;
        BackgroundColor = ChipBackgroundColor;
    }

    /// <summary>
    /// Sets the chip to a predefined status state.
    /// </summary>
    public void SetStatus(string text, Color textColor, Color bgColor)
    {
        StatusText = text;
        ChipTextColor = textColor;
        ChipBackgroundColor = bgColor;
    }
}
