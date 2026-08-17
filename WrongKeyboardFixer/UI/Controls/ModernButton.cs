namespace WrongKeyboardFixer.UI.Controls;

/// <summary>
/// A flat, rounded MAUI button with Primary/Secondary/Ghost variants.
/// Matches the WinForms ModernButton appearance.
/// </summary>
public class ModernButton : Button
{
    public enum Variant
    {
        Primary,
        Secondary,
        Ghost
    }

    public static readonly BindableProperty ButtonVariantProperty =
        BindableProperty.Create(nameof(ButtonVariant), typeof(Variant), typeof(ModernButton), Variant.Primary,
            propertyChanged: OnVariantChanged);

    public Variant ButtonVariant
    {
        get => (Variant)GetValue(ButtonVariantProperty);
        set => SetValue(ButtonVariantProperty, value);
    }

    public ModernButton()
    {
        CornerRadius = 8;
        HeightRequest = Theme.StdHeight;
        FontSize = Theme.ButtonFontSize;
        FontAttributes = FontAttributes.Bold;
        Padding = new Thickness(16, 0);
        Visual = VisualMarker.Flat;
        UpdateAppearance();
    }

    private static void OnVariantChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernButton btn)
            btn.UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        switch (ButtonVariant)
        {
            case Variant.Primary:
                BackgroundColor = Theme.Accent;
                TextColor = Theme.TextOnAccent;
                BorderColor = Theme.Accent;
                break;
            case Variant.Secondary:
                BackgroundColor = Theme.Surface;
                TextColor = Theme.TextPrimary;
                BorderColor = Theme.BorderStrong;
                break;
            case Variant.Ghost:
                BackgroundColor = Colors.Transparent;
                TextColor = Theme.TextPrimary;
                BorderColor = Colors.Transparent;
                break;
        }
    }

    protected override void ChangeVisualState()
    {
        base.ChangeVisualState();
        if (!IsEnabled)
        {
            BackgroundColor = Theme.SurfaceMuted;
            TextColor = Theme.TextDisabled;
        }
        else
        {
            UpdateAppearance();
        }
    }
}
