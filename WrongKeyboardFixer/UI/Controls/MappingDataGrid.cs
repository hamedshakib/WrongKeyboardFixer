using System.Collections.ObjectModel;

namespace WrongKeyboardFixer.UI.Controls;

/// <summary>
/// Editable two-column mapping table (Persian ↔ English) using MAUI CollectionView.
/// Matches the WinForms MappingGrid appearance and functionality.
/// </summary>
public class MappingDataGrid : ContentView
{
    public enum Direction
    {
        PersianToEnglish,
        EnglishToPersian
    }

    public event Action<char, char>? ValueCommitted;
    public event Action<char>? DeleteRequested;
    public event Action<char>? ResetRequested;

    private readonly Direction _direction;
    private readonly ObservableCollection<MappingItem> _items = new();
    private CollectionView? _collectionView;

    public MappingDataGrid(Direction direction)
    {
        _direction = direction;
        Build();
    }

    private void Build()
    {
        string firstHeader = _direction == Direction.PersianToEnglish
            ? Core.Helpers.Localization.Get("PersianChar")
            : Core.Helpers.Localization.Get("EnglishChar");
        string secondHeader = _direction == Direction.PersianToEnglish
            ? Core.Helpers.Localization.Get("EnglishChar")
            : Core.Helpers.Localization.Get("PersianChar");

        // Header row
        var headerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                new ColumnDefinition(new GridLength(70)),
                new ColumnDefinition(new GridLength(70)),
            },
            BackgroundColor = Theme.SurfaceAlt,
            HeightRequest = 40,
            Padding = new Thickness(8, 0),
        };

        headerGrid.Add(new Label
        {
            Text = firstHeader,
            FontSize = Theme.SmallFontSize,
            TextColor = Theme.TextSecondary,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
        }, 0);

        headerGrid.Add(new Label
        {
            Text = secondHeader,
            FontSize = Theme.SmallFontSize,
            TextColor = Theme.TextSecondary,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
        }, 1);

        headerGrid.Add(new Label
        {
            Text = Core.Helpers.Localization.Get("Delete"),
            FontSize = Theme.SmallFontSize,
            TextColor = Theme.Danger,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
        }, 2);

        headerGrid.Add(new Label
        {
            Text = Core.Helpers.Localization.Get("Reset"),
            FontSize = Theme.SmallFontSize,
            TextColor = Theme.Warning,
            FontAttributes = FontAttributes.Bold,
            VerticalOptions = LayoutOptions.Center,
            HorizontalOptions = LayoutOptions.Center,
        }, 3);

        // Data items
        _collectionView = new CollectionView
        {
            ItemsSource = _items,
            SelectionMode = SelectionMode.None,
            BackgroundColor = Theme.Surface,
        };

        _collectionView.ItemTemplate = new DataTemplate(() =>
        {
            var rowGrid = new Grid
            {
                ColumnDefinitions =
                {
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(1, GridUnitType.Star)),
                    new ColumnDefinition(new GridLength(70)),
                    new ColumnDefinition(new GridLength(70)),
                },
                Padding = new Thickness(8, 0),
                HeightRequest = 40,
                BackgroundColor = Theme.Surface,
            };

            // Source character
            var sourceLabel = new Label
            {
                FontSize = Theme.BodyFontSize,
                TextColor = Theme.TextSecondary,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center,
                FontAttributes = FontAttributes.Bold,
            };
            sourceLabel.SetBinding(Label.TextProperty, "SourceChar");
            rowGrid.Add(sourceLabel, 0);

            // Editable target character
            var targetEntry = new Entry
            {
                FontSize = Theme.BodyFontSize,
                TextColor = Theme.Accent,
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center,
                HorizontalTextAlignment = TextAlignment.Center,
                MaxLength = 1,
                BackgroundColor = Colors.Transparent,
                WidthRequest = 60,
            };
            targetEntry.SetBinding(Entry.TextProperty, "TargetChar");
            targetEntry.TextChanged += (s, e) =>
            {
                if (s is Entry entry && entry.BindingContext is MappingItem item)
                {
                    string? val = e.NewTextValue;
                    if (!string.IsNullOrEmpty(val) && val.Length == 1)
                    {
                        item.TargetChar = val;
                        ValueCommitted?.Invoke(item.SourceChar, val[0]);
                    }
                }
            };
            rowGrid.Add(targetEntry, 1);

            // Delete button
            var deleteBtn = new Button
            {
                Text = Core.Helpers.Localization.Get("Delete"),
                FontSize = Theme.SmallFontSize,
                TextColor = Theme.Danger,
                BackgroundColor = Theme.DangerSoft,
                CornerRadius = 6,
                HeightRequest = 28,
                Padding = new Thickness(8, 0),
                Margin = new Thickness(4),
                FontAttributes = FontAttributes.Bold,
            };
            deleteBtn.Clicked += (s, e) =>
            {
                if (deleteBtn.BindingContext is MappingItem item)
                    DeleteRequested?.Invoke(item.SourceChar);
            };
            rowGrid.Add(deleteBtn, 2);

            // Reset button
            var resetBtn = new Button
            {
                Text = Core.Helpers.Localization.Get("Reset"),
                FontSize = Theme.SmallFontSize,
                TextColor = Theme.Warning,
                BackgroundColor = Theme.WarningSoft,
                CornerRadius = 6,
                HeightRequest = 28,
                Padding = new Thickness(8, 0),
                Margin = new Thickness(4),
                FontAttributes = FontAttributes.Bold,
            };
            resetBtn.Clicked += (s, e) =>
            {
                if (resetBtn.BindingContext is MappingItem item)
                    ResetRequested?.Invoke(item.SourceChar);
            };
            rowGrid.Add(resetBtn, 3);

            return rowGrid;
        });

        var scrollView = new ScrollView
        {
            Content = _collectionView,
        };

        var container = new VerticalStackLayout
        {
            Children = { headerGrid, new BoxView { HeightRequest = 1, BackgroundColor = Theme.Border }, scrollView }
        };

        Content = container;
    }

    public void Load(IEnumerable<KeyValuePair<char, char>> entries)
    {
        _items.Clear();
        foreach (var pair in entries.OrderBy(kv => kv.Key))
            _items.Add(new MappingItem(pair.Key, pair.Value));
    }

    public class MappingItem
    {
        public char SourceChar { get; }
        public string SourceCharStr => SourceChar.ToString();
        public string TargetChar { get; set; }

        public MappingItem(char source, char target)
        {
            SourceChar = source;
            TargetChar = target.ToString();
        }
    }
}
