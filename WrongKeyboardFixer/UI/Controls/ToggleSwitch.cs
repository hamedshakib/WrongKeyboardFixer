namespace WrongKeyboardFixer.UI.Controls;

/// <summary>
/// A custom toggle/switch control with track + knob, matching the WinForms ToggleSwitch.
/// </summary>
public class ToggleSwitch : TemplatedView
{
    private readonly Microsoft.Maui.Controls.Shapes.Path _trackPath;
    private readonly Microsoft.Maui.Controls.Shapes.Path _knobPath;
    private readonly Label _label;

    public static readonly BindableProperty IsToggledProperty =
        BindableProperty.Create(nameof(IsToggled), typeof(bool), typeof(ToggleSwitch), false,
            propertyChanged: OnToggledChanged);

    public bool IsToggled
    {
        get => (bool)GetValue(IsToggledProperty);
        set => SetValue(IsToggledProperty, value);
    }

    public static readonly BindableProperty ToggleLabelProperty =
        BindableProperty.Create(nameof(ToggleLabel), typeof(string), typeof(ToggleSwitch), "");

    public string ToggleLabel
    {
        get => (string)GetValue(ToggleLabelProperty);
        set => SetValue(ToggleLabelProperty, value);
    }

    public event EventHandler? Toggled;

    public ToggleSwitch()
    {
        var trackGeometry = new Microsoft.Maui.Controls.Shapes.RoundRectangle
        {
            CornerRadius = new CornerRadius(12),
            WidthRequest = 44,
            HeightRequest = 24,
        };

        _trackPath = new Microsoft.Maui.Controls.Shapes.Path
        {
            Fill = new SolidColorBrush(Theme.BorderStrong),
            WidthRequest = 44,
            HeightRequest = 24,
        };

        _knobPath = new Microsoft.Maui.Controls.Path
        {
            Fill = new SolidColorBrush(Colors.White),
            WidthRequest = 18,
            HeightRequest = 18,
        };

        _label = new Label
        {
            FontSize = Theme.BodyFontSize,
            TextColor = Theme.TextPrimary,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(10, 0, 0, 0),
        };

        var grid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Auto),
                new ColumnDefinition(GridLength.Star),
            },
            ColumnSpacing = 0,
            HeightRequest = 30,
            VerticalOptions = LayoutOptions.Center,
        };

        // Build the track as a Border with rounded corners
        var trackBorder = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(12),
            },
            Stroke = Theme.BorderStrong,
            StrokeThickness = 1,
            BackgroundColor = Theme.BorderStrong,
            WidthRequest = 44,
            HeightRequest = 24,
            Padding = 0,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            GestureRecognizers =
            {
                new TapGestureRecognizer
                {
                    Command = new Command(() => IsToggled = !IsToggled)
                }
            }
        };

        var knob = new Border
        {
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle
            {
                CornerRadius = new CornerRadius(9),
            },
            BackgroundColor = Colors.White,
            WidthRequest = 18,
            HeightRequest = 18,
            Padding = 0,
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Center,
            Margin = new Thickness(3, 0, 0, 0),
            InputTransparent = true,
        };

        var trackGrid = new Grid
        {
            WidthRequest = 44,
            HeightRequest = 24,
            Children = { trackBorder, knob },
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Start,
        };

        grid.Add(trackGrid, 0);
        grid.Add(_label, 1);

        Content = grid;

        UpdateVisuals();
    }

    private static void OnToggledChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ToggleSwitch toggle)
        {
            toggle.UpdateVisuals();
            toggle.Toggled?.Invoke(toggle, EventArgs.Empty);
        }
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == ToggleLabelProperty.PropertyName)
            _label.Text = ToggleLabel;
        if (propertyName == IsToggledProperty.PropertyName)
            UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (Content is not Grid grid) return;

        var trackBorder = grid.Children[0] as Border;
        var knob = grid.Children[1] as Border;

        if (trackBorder != null)
        {
            trackBorder.BackgroundColor = IsToggled ? Theme.Accent : Theme.BorderStrong;
            trackBorder.Stroke = IsToggled ? Theme.Accent : Theme.BorderStrong;
        }

        if (knob != null)
        {
            // Move knob: on = right side (26px from left), off = left side (3px)
            knob.Margin = IsToggled ? new Thickness(23, 0, 0, 0) : new Thickness(3, 0, 0, 0);
        }

        _label.Text = ToggleLabel;
    }
}
