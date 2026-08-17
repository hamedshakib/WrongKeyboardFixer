using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;
using WrongKeyboardFixer.Core.Services;

namespace WrongKeyboardFixer.ViewModels;

public partial class KeyboardMappingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;
    private Dictionary<char, char> _persianToEnglish;
    private Dictionary<char, char> _englishToPersian;
    private List<string> _wordCorrections;

    [ObservableProperty] private string _title;
    [ObservableProperty] private string _subtitle;
    [ObservableProperty] private string _saveLabel;
    [ObservableProperty] private string _cancelLabel;
    [ObservableProperty] private string _searchPlaceholder;
    [ObservableProperty] private string _addWordPlaceholder;
    [ObservableProperty] private string _addNewMappingLabel;
    [ObservableProperty] private string _resetAllLabel;
    [ObservableProperty] private string _resetWordsLabel;
    [ObservableProperty] private string _addWordLabel;
    [ObservableProperty] private string _persianToEnglishTitle;
    [ObservableProperty] private string _englishToPersianTitle;
    [ObservableProperty] private string _wordCorrectionsTitle;
    [ObservableProperty] private string _wordCorrectionsHint;
    [ObservableProperty] private string _searchText = "";
    [ObservableProperty] private string _newWordText = "";
    [ObservableProperty] private string _persianToEnglishCount;
    [ObservableProperty] private string _englishToPersianCount;
    [ObservableProperty] private string _wordCount;

    public ObservableCollection<MappingDataGrid.MappingItem> PersianToEnglishItems { get; } = new();
    public ObservableCollection<MappingDataGrid.MappingItem> EnglishToPersianItems { get; } = new();
    public ObservableCollection<WordItem> WordItems { get; } = new();

    public KeyboardMappingsViewModel()
    {
        _settings = SettingsManager.Load();
        _persianToEnglish = new Dictionary<char, char>(_settings.PersianToEnglishMap ?? MappingDefaults.GetDefaultPersianToEnglishMap());
        _englishToPersian = new Dictionary<char, char>(_settings.EnglishToPersianMap ?? MappingDefaults.GetDefaultEnglishToPersianMap());
        _wordCorrections = new List<string>(_settings.WordCorrections ?? MappingDefaults.GetDefaultWordCorrections());

        RefreshLocalizedStrings();
        LoadMappings();
        LoadWords();
    }

    private void RefreshLocalizedStrings()
    {
        Title = Localization.Get("KeyboardMappingsTitle");
        Subtitle = Localization.Get("KeyboardMappingsSubtitle");
        SaveLabel = Localization.Get("Save");
        CancelLabel = Localization.Get("Cancel");
        SearchPlaceholder = Localization.Get("Search");
        AddWordPlaceholder = Localization.Get("AddWord");
        AddNewMappingLabel = Localization.Get("AddNewMapping");
        ResetAllLabel = Localization.Get("ResetAll");
        ResetWordsLabel = Localization.Get("ResetWords");
        AddWordLabel = Localization.Get("AddWord");
        PersianToEnglishTitle = Localization.Get("PersianToEnglish");
        EnglishToPersianTitle = Localization.Get("EnglishToPersian");
        WordCorrectionsTitle = Localization.Get("WordCorrections");
        WordCorrectionsHint = Localization.Get("WordCorrectionsHint");
    }

    private void LoadMappings()
    {
        PersianToEnglishItems.Clear();
        foreach (var kv in _persianToEnglish.OrderBy(x => x.Key))
            PersianToEnglishItems.Add(new MappingDataGrid.MappingItem(kv.Key, kv.Value));
        PersianToEnglishCount = Localization.Format("MappingsCount", _persianToEnglish.Count);

        EnglishToPersianItems.Clear();
        foreach (var kv in _englishToPersian.OrderBy(x => x.Key))
            EnglishToPersianItems.Add(new MappingDataGrid.MappingItem(kv.Key, kv.Value));
        EnglishToPersianCount = Localization.Format("MappingsCount", _englishToPersian.Count);
    }

    private void LoadWords()
    {
        WordItems.Clear();
        var words = _wordCorrections.OrderBy(w => w, StringComparer.Ordinal).ToList();

        if (!string.IsNullOrWhiteSpace(SearchText))
            words = words.Where(w => w.Contains(SearchText, StringComparison.Ordinal)).ToList();

        foreach (var word in words)
            WordItems.Add(new WordItem(word));

        WordCount = Localization.Format("WordCount", words.Count);
    }

    // ── Commands ──

    [RelayCommand]
    private void DeletePersianMapping(char persianChar)
    {
        if (_persianToEnglish.Remove(persianChar))
            LoadMappings();
    }

    [RelayCommand]
    private void DeleteEnglishMapping(char englishChar)
    {
        if (_englishToPersian.Remove(englishChar))
            LoadMappings();
    }

    [RelayCommand]
    private void ResetPersianMapping(char persianChar)
    {
        var defaults = MappingDefaults.GetDefaultPersianToEnglishMap();
        if (defaults.TryGetValue(persianChar, out var value))
            _persianToEnglish[persianChar] = value;
        else if (_persianToEnglish.Remove(persianChar))
            { }
        LoadMappings();
    }

    [RelayCommand]
    private void ResetEnglishMapping(char englishChar)
    {
        var defaults = MappingDefaults.GetDefaultEnglishToPersianMap();
        if (defaults.TryGetValue(englishChar, out var value))
            _englishToPersian[englishChar] = value;
        else if (_englishToPersian.Remove(englishChar))
            { }
        LoadMappings();
    }

    [RelayCommand]
    private void ResetAllPersianToEnglish()
    {
        _persianToEnglish = MappingDefaults.GetDefaultPersianToEnglishMap();
        LoadMappings();
    }

    [RelayCommand]
    private void ResetAllEnglishToPersian()
    {
        _englishToPersian = MappingDefaults.GetDefaultEnglishToPersianMap();
        LoadMappings();
    }

    [RelayCommand]
    private void ResetWords()
    {
        _wordCorrections = MappingDefaults.GetDefaultWordCorrections();
        LoadWords();
    }

    [RelayCommand]
    private void DeleteWord(string word)
    {
        _wordCorrections.Remove(word);
        LoadWords();
    }

    [RelayCommand]
    private void AddWord()
    {
        string word = NewWordText.Trim();

        if (string.IsNullOrEmpty(word) || word.Contains(' ') || word.Contains('\u200C'))
            return;

        if (word.Length < 2 || !word.All(char.IsLetter) || (word[0] != 'آ' && word[0] != 'ژ'))
            return;

        if (_wordCorrections.Contains(word))
            return;

        _wordCorrections.Add(word);
        NewWordText = "";
        LoadWords();
    }

    [RelayCommand]
    private void OnSearchTextChanged()
    {
        LoadWords();
    }

    public void CommitPersianMapping(char source, char target)
    {
        _persianToEnglish[source] = target;
        LoadMappings();
    }

    public void CommitEnglishMapping(char source, char target)
    {
        _englishToPersian[source] = target;
        LoadMappings();
    }

    public void AddPersianMapping(char persian, char english)
    {
        _englishToPersian.Remove(english);
        _persianToEnglish[persian] = english;
        LoadMappings();
    }

    public void AddEnglishMapping(char english, char persian)
    {
        _persianToEnglish.Remove(persian);
        _englishToPersian[english] = persian;
        LoadMappings();
    }

    [RelayCommand]
    private void Save()
    {
        _settings.PersianToEnglishMap = new Dictionary<char, char>(_persianToEnglish);
        _settings.EnglishToPersianMap = new Dictionary<char, char>(_englishToPersian);
        _settings.WordCorrections = new List<string>(_wordCorrections);
        SettingsManager.Save(_settings);
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void Cancel()
    {
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    public event EventHandler? RequestClose;

    public class WordItem
    {
        public string Word { get; }
        public string DeleteLabel => Localization.Get("Delete");
        public WordItem(string word) => Word = word;
    }
}
