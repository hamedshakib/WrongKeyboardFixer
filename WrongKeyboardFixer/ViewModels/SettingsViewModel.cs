using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.Services.Update;

namespace WrongKeyboardFixer.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly AppSettings _settings;

    [ObservableProperty] private string _settingsTitle;
    [ObservableProperty] private string _settingsSubtitle;
    [ObservableProperty] private string _languageLabel;
    [ObservableProperty] private string _runOnStartupLabel;
    [ObservableProperty] private string _hotkeyGroupLabel;
    [ObservableProperty] private string _applyHotkeyLabel;
    [ObservableProperty] private string _statusText;
    [ObservableProperty] private Color _statusColor;
    [ObservableProperty] private Color _statusBgColor;
    [ObservableProperty] private string _currentVersionLabel;
    [ObservableProperty] private string _checkUpdateLabel;
    [ObservableProperty] private string _keyboardMappingsLabel;
    [ObservableProperty] private string _saveLabel;
    [ObservableProperty] private string _cancelLabel;
    [ObservableProperty] private bool _runOnStartup;
    [ObservableProperty] private int _languageIndex;
    [ObservableProperty] private int _hotkeyModifierIndex;
    [ObservableProperty] private int _hotkeyKeyIndex;
    [ObservableProperty] private bool _isHotkeyApplied;
    [ObservableProperty] private bool _isInitialized;
    [ObservableProperty] private string _title;
    [ObservableProperty] private string _subtitle;

    private uint _lastModifier;
    private int _lastKey;

    public List<string> LanguageItems { get; } = new()
    {
        Localization.Get("LanguageEnglish"),
        Localization.Get("LanguagePersian")
    };

    public List<string> ModifierItems { get; } = Enumerable.Range(0, HotkeyOptions.ModifierCount)
        .Select(HotkeyOptions.ModifierText).ToList();

    public List<string> KeyItems { get; } = Enumerable.Range(0, HotkeyOptions.KeyCount)
        .Select(HotkeyOptions.KeyText).ToList();

    public SettingsViewModel()
    {
        _settings = SettingsManager.Load();
        Localization.SetLanguage(_settings.Language);

        _settingsTitle = Localization.Get("SettingsTitle");
        _settingsSubtitle = Localization.Get("SettingsSubtitle");
        _languageLabel = Localization.Get("Language");
        _runOnStartupLabel = Localization.Get("RunOnStartup");
        _hotkeyGroupLabel = Localization.Get("HotkeyGroup");
        _applyHotkeyLabel = Localization.Get("ApplyHotkey");
        _currentVersionLabel = VersionInfo.CurrentString;
        _checkUpdateLabel = Localization.Get("CheckUpdate");
        _keyboardMappingsLabel = Localization.Get("KeyboardMappings");
        _saveLabel = Localization.Get("Save");
        _cancelLabel = Localization.Get("Cancel");
        _statusColor = Theme.Info;
        _statusBgColor = Theme.InfoSoft;
        _title = Localization.Get("SettingsTitle");
        _subtitle = Localization.Get("SettingsSubtitle");

        LoadSettings();
        _isInitialized = true;
    }

    private void LoadSettings()
    {
        _runOnStartup = _settings.RunOnStartup;
        _languageIndex = _settings.Language == Localization.Languages.Persian ? 1 : 0;

        _hotkeyModifierIndex = HotkeyOptions.IndexOfModifier((uint)_settings.HotkeyModifier);
        _hotkeyKeyIndex = HotkeyOptions.IndexOfKey(_settings.HotkeyKey);
        _lastModifier = (uint)_settings.HotkeyModifier;
        _lastKey = _settings.HotkeyKey;

        UpdateStatusUI(false, Localization.Get("StatusChecking"), Theme.Info, Theme.InfoSoft);
    }

    private void UpdateStatusUI(bool applied, string text, Color color, Color bgColor)
    {
        IsHotkeyApplied = applied;
        StatusText = text;
        StatusColor = color;
        StatusBgColor = bgColor;
    }

    [RelayCommand]
    private void OnLanguageChanged()
    {
        if (!_isInitialized) return;
        string lang = LanguageIndex == 0 ? Localization.Languages.English : Localization.Languages.Persian;
        if (Localization.CurrentLanguage != lang)
        {
            Localization.SetLanguage(lang);
            RefreshLocalizedStrings();
        }
    }

    private void RefreshLocalizedStrings()
    {
        SettingsTitle = Localization.Get("SettingsTitle");
        SettingsSubtitle = Localization.Get("SettingsSubtitle");
        LanguageLabel = Localization.Get("Language");
        RunOnStartupLabel = Localization.Get("RunOnStartup");
        HotkeyGroupLabel = Localization.Get("HotkeyGroup");
        ApplyHotkeyLabel = Localization.Get("ApplyHotkey");
        CheckUpdateLabel = Localization.Get("CheckUpdate");
        KeyboardMappingsLabel = Localization.Get("KeyboardMappings");
        SaveLabel = Localization.Get("Save");
        CancelLabel = Localization.Get("Cancel");
        Title = Localization.Get("SettingsTitle");
        Subtitle = Localization.Get("SettingsSubtitle");
        UpdateCurrentStatus();
    }

    private void UpdateCurrentStatus()
    {
        var (modifier, key) = GetSelectedHotkey();
        bool hasChanged = _lastKey != key || _lastModifier != modifier;
        bool registered = !hasChanged;

        if (registered)
            UpdateStatusUI(true, Localization.Get("StatusRegistered"), Theme.Success, Theme.SuccessSoft);
        else
            UpdateStatusUI(false, Localization.Get("StatusNotRegistered"), Theme.Warning, Theme.WarningSoft);
    }

    [RelayCommand]
    private void OnModifierChanged() => UpdateCurrentStatus();

    [RelayCommand]
    private void OnKeyChanged() => UpdateCurrentStatus();

    private (uint modifier, int key) GetSelectedHotkey()
    {
        uint modifier = HotkeyOptions.ModifierValue(HotkeyModifierIndex);
        int key = HotkeyOptions.KeyValue(HotkeyKeyIndex);
        return (modifier, key);
    }

    public event EventHandler? SettingsSaved;
    public event EventHandler? OpenMappingsRequested;
    public event EventHandler? RequestClose;

    [RelayCommand]
    private void Save()
    {
        try
        {
            _settings.RunOnStartup = RunOnStartup;
            _settings.Language = LanguageIndex == 0 ? Localization.Languages.English : Localization.Languages.Persian;

            if (!IsHotkeyApplied)
            {
                var (modifier, key) = GetSelectedHotkey();
                _settings.HotkeyModifier = (int)modifier;
                _settings.HotkeyKey = key;
            }

            SettingsManager.Save(_settings);
            SettingsSaved?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            Core.Helpers.Localization.Format("SaveSettingsError", ex.Message);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        Localization.SetLanguage(_settings.Language);
        RequestClose?.Invoke(this, EventArgs.Empty);
    }

    [RelayCommand]
    private void OpenKeyboardMappings()
    {
        OpenMappingsRequested?.Invoke(this, EventArgs.Empty);
    }

    public AppSettings GetSettings() => _settings;
}
