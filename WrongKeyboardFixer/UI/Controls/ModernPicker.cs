namespace WrongKeyboardFixer.UI.Controls;

/// <summary>
/// A styled MAUI Picker with rounded border and modern appearance.
/// Matches the WinForms ModernComboBox appearance.
/// </summary>
public class ModernPicker : Border
{
    private readonly Picker _picker;

    public static readonly BindableProperty ItemsProperty =
        BindableProperty.Create(nameof(Items), typeof(IList<string>), typeof(ModernPicker),
            new List<string>(), propertyChanged: OnItemsChanged);

    public IList<string> Items
    {
        get => (IList<string>)GetValue(ItemsProperty);
        set => SetValue(ItemsProperty, value);
    }

    public static readonly BindableProperty SelectedIndexProperty =
        BindableProperty.Create(nameof(SelectedIndex), typeof(int), typeof(ModernPicker), -1,
            defaultBindingMode: BindingMode.TwoWay);

    public int SelectedIndex
    {
        get => (int)GetValue(SelectedIndexProperty);
        set => SetValue(SelectedIndexProperty, value);
    }

    public static readonly BindableProperty SelectedItemProperty =
        BindableProperty.Create(nameof(SelectedItem), typeof(string), typeof(ModernPicker), null,
            defaultBindingMode: BindingMode.TwoWay);

    public string? SelectedItem
    {
        get => (string?)GetValue(SelectedItemProperty);
        set => SetValue(SelectedItemProperty, value);
    }

    public event EventHandler? SelectedIndexChanged;

    public ModernPicker()
    {
        CornerRadius = new CornerRadius(6);
        StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = new CornerRadius(6) };
        BackgroundColor = Theme.Surface;
        Stroke = Theme.BorderStrong;
        StrokeThickness = 1;
        Padding = new Thickness(12, 0);
        HeightRequest = Theme.StdHeight;

        _picker = new Picker
        {
            BackgroundColor = Colors.Transparent,
            TextColor = Theme.TextPrimary,
            FontSize = Theme.BodyFontSize,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Fill,
        };

        _picker.SelectedIndexChanged += (_, _) =>
        {
            SelectedIndex = _picker.SelectedIndex;
            SelectedItem = _picker.SelectedItem as string;
            SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
        };

        Content = _picker;
    }

    private static void OnItemsChanged(BindableObject bindable, object oldValue, object newValue)
    {
        if (bindable is ModernPicker picker && newValue is IList<string> items)
        {
            picker._picker.Items.Clear();
            foreach (var item in items)
                picker._picker.Items.Add(item);
        }
    }

    protected override void OnPropertyChanged(string? propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        if (propertyName == SelectedIndexProperty.PropertyName)
            _picker.SelectedIndex = SelectedIndex;
        if (propertyName == ItemsProperty.PropertyName)
            _picker.ItemsSource = Items;
    }

    /// <summary>
    /// Convenience: set items from string array.
    /// </summary>
    public void SetItems(params string[] items)
    {
        Items = new List<string>(items);
        _picker.ItemsSource = Items;
    }
}
