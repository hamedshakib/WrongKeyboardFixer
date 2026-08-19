using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.Services.Update;
using WrongKeyboardFixer.UI.Components;

namespace WrongKeyboardFixer.UI.Forms;

public class SettingsForm : ModernForm, ICloseRequestHandler
{
    private const int StdHeight = Constants.UI.StdButtonHeight;
    private const int RowGap = Constants.UI.RowGapMedium;
    private const int SectionGap = Constants.UI.SectionGap;
    private readonly HotkeyManager _hotkeyManager;

    private readonly AppSettings _settings;
    private ModernButton _cancelButton = null!;
    private ModernButton _checkUpdateButton = null!;
    private ComboBox _hotkeyKeyComboBox = null!;
    private ComboBox _hotkeyModifierComboBox = null!;
    private bool _isHotkeyRegistered;
    private ModernButton _keyboardMappingsButton = null!;
    private ComboBox _languageComboBox = null!;
    private Keys _lastHotkeyKey;
    private uint _lastHotkeyModifier;
    private TableLayoutPanel _layout = null!;
    private RoundedPanel _panel = null!;
    private ModernButton _registerHotkeyButton = null!;

    // Controls
    private ToggleSwitch _runOnStartupToggle = null!;
    private ModernButton _saveButton = null!;
    private StatusChip _statusChip = null!;
    private StatusState _statusState = StatusState.Checking;
    private Label _versionLabel = null!;

    public SettingsForm(AppSettings settings, HotkeyManager hotkeyManager)
    {
        _settings = settings ?? new AppSettings();
        _hotkeyManager = hotkeyManager;
        Localization.SetLanguage(_settings.Language);
        RightToLeftLayout = Localization.IsRtl;

        SuspendLayout();
        InitializeForm();
        InitializeControls();
        ResumeLayout(false);

        LoadSettings();
    }

    public void RequestClose()
    {
        CancelButton_Click(this, EventArgs.Empty);
    }

    private void InitializeForm()
    {
        Text = Localization.Get("Settings");
        FormBorderStyle = FormBorderStyle.None;
        AutoScaleDimensions = new SizeF(96F, 96F);
        AutoScaleMode = AutoScaleMode.Dpi;
        ClientSize = new Size(Constants.Forms.SettingsFormWidth, Constants.Forms.SettingsFormHeight);
        MinimumSize = new Size(Constants.Forms.SettingsFormWidth, Constants.Forms.SettingsFormHeight);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.Background;
        Font = Theme.BodyFont;
        Icon = IconLoader.GetIcon();
        RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No;

        AddTitleBar("SettingsTitle", "SettingsSubtitle");
    }

    private void InitializeControls()
    {
        _panel = new RoundedPanel
        {
            CornerRadius = Constants.UI.CardCornerRadiusLarge,
            BackColor = Theme.Surface,
            Padding = new Padding(Constants.UI.PanelPadding),
            Location = new Point(Constants.UI.CardPadding, ModernTitleBar.TitleBarHeight + 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoScroll = true
        };
        _panel.Size = new Size(ClientSize.Width - Constants.UI.CardPadding * 2,
            ClientSize.Height - ModernTitleBar.TitleBarHeight - Constants.UI.PanelPadding);
        Controls.Add(_panel);

        // Single vertical TableLayoutPanel that owns the whole layout.
        // No absolute coordinates anywhere below: rows auto-size, spacing is
        // controlled by row margins, and RTL mirroring is handled by the form.
        _layout = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 0,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
            BackColor = Theme.Surface
        };
        _layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        _panel.Controls.Add(_layout);

        // ── Language section ──────────────────────────────
        AddHeaderRow(Localization.Get("Language"), 0);

        _languageComboBox = Theme.CreateCombo(
            Localization.Get("LanguageEnglish"),
            Localization.Get("LanguagePersian"));
        _languageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
        AddControlRow(_languageComboBox, width: Constants.UI.ButtonWidthMedium, topGap: 10);

        _runOnStartupToggle = new ToggleSwitch
        {
            Text = Localization.Get("RunOnStartup"),
            Height = 30,
            AutoSize = false,
            Width = 220
        };

        AddControlRow(_runOnStartupToggle, fillWidth: false, topGap: RowGap);

        AddDividerRow();

        // ── Hotkey section ────────────────────────────────
        AddHeaderRow(Localization.Get("HotkeyGroup"));

        var modifierItems = new string[HotkeyOptions.ModifierCount];
        for (int i = 0; i < HotkeyOptions.ModifierCount; i++)
            modifierItems[i] = HotkeyOptions.ModifierText(i);
        _hotkeyModifierComboBox = Theme.CreateCombo(modifierItems);
        _hotkeyModifierComboBox.Width = Constants.UI.ButtonWidthMedium;
        _hotkeyModifierComboBox.SelectedIndex = 0;
        _hotkeyModifierComboBox.SelectedIndexChanged += HotkeyModifierComboBox_SelectedIndexChanged;

        var lblPlus = Theme.BodyLabel("+", Theme.TextSecondary, Theme.BodyBoldFont);
        lblPlus.TextAlign = ContentAlignment.MiddleCenter;
        lblPlus.Width = 24;

        var keyItems = new string[HotkeyOptions.KeyCount];
        for (int i = 0; i < HotkeyOptions.KeyCount; i++)
            keyItems[i] = HotkeyOptions.KeyText(i);
        _hotkeyKeyComboBox = Theme.CreateCombo(keyItems);
        _hotkeyKeyComboBox.Width = Constants.UI.LabelCountWidth;
        _hotkeyKeyComboBox.SelectedIndex = 0;
        _hotkeyKeyComboBox.SelectedIndexChanged += HotkeyKeyComboBox_SelectedIndexChanged;
        AddColumnsRow(10, (_hotkeyModifierComboBox, Constants.UI.ButtonWidthMedium), (lblPlus, 24), (_hotkeyKeyComboBox, Constants.UI.LabelCountWidth));

        _registerHotkeyButton = new ModernButton
        {
            Text = Localization.Get("ApplyHotkey"),
            ButtonVariant = ModernButton.Variant.Primary,
            Width = Constants.UI.ButtonWidthSmall
        };
        _registerHotkeyButton.Click += RegisterHotkeyButton_Click;
        _registerHotkeyButton.Enabled = false;

        _statusChip = new StatusChip(Localization.Get("StatusChecking"), Theme.Info, Theme.InfoSoft);
        AddColumnsRow(RowGap, (_registerHotkeyButton, Constants.UI.ButtonWidthSmall), (_statusChip, null));

        AddDividerRow();

        // ── Keyboard section ──────────────────────────────
        AddHeaderRow(Localization.Get("KeyboardMappings"));

        _keyboardMappingsButton = new ModernButton
        {
            Text = Localization.Get("KeyboardMappings"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Width = 240
        };
        _keyboardMappingsButton.Click += KeyboardMappingsButton_Click;
        AddControlRow(_keyboardMappingsButton, width: 240, topGap: 10);

        AddDividerRow();

        // ── Current version section ───────────────────────
        AddHeaderRow(Localization.Get("CurrentVersion"));

        _versionLabel = Theme.BodyLabel(VersionInfo.CurrentString, Theme.Accent, Theme.BodyBoldFont);
        _versionLabel.Width = Constants.UI.LabelCountWidth;
        _checkUpdateButton = new ModernButton
        {
            Text = Localization.Get("CheckUpdate"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Width = Constants.UI.ButtonWidthMedium
        };
        _checkUpdateButton.Click += async (_, _) => await CheckUpdateButton_Click();
        AddColumnsRow(10, (_versionLabel, Constants.UI.LabelCountWidth), (_checkUpdateButton, Constants.UI.ButtonWidthMedium));

        AddDividerRow();

        // ── Footer ────────────────────────────────────────
        AddFooterRow();

        // Size the form to the actual content height (structural, no hardcoded Y).
        var prefSize = _layout.GetPreferredSize(new Size(_panel.ClientSize.Width, 0));
        int panelHeight = _panel.Padding.Top + prefSize.Height + _panel.Padding.Bottom;
        _panel.Height = panelHeight;
        ClientSize = new Size(ClientSize.Width, _panel.Location.Y + panelHeight + Constants.UI.CardPadding);
        MinimumSize = ClientSize;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        RedrawLock.Resume(this);
    }

    private void AddRow(Control control)
    {
        _layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        _layout.Controls.Add(control, 0, _layout.RowCount);
        _layout.RowCount++;
    }

    private void AddHeaderRow(string text, int topGap = 0)
    {
        var label = Theme.SectionLabel(text);
        label.Margin = new Padding(0, topGap, 0, 0);
        label.Anchor = AnchorStyles.Left | AnchorStyles.Right;
        AddRow(label);
    }

    private void AddControlRow(Control control, int topGap, int? width = null, bool fillWidth = false)
    {
        control.Margin = new Padding(0, topGap, 0, 0);
        control.Anchor = fillWidth ? AnchorStyles.Left | AnchorStyles.Right : AnchorStyles.Left;
        if (width.HasValue && !fillWidth)
            control.Width = width.Value;
        AddRow(control);
    }

    private void AddColumnsRow(int topGap, params (Control control, int? width)[] columns)
    {
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = columns.Length,
            RowCount = 1,
            Margin = new Padding(0, topGap, 0, 0),
            Padding = Padding.Empty,
            BackColor = Theme.Surface,
            Anchor = AnchorStyles.Left
        };
        row.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        foreach (var (control, width) in columns)
        {
            row.ColumnStyles.Add(width is int w
                ? new ColumnStyle(SizeType.Absolute, w)
                : new ColumnStyle(SizeType.AutoSize));
            control.Margin = Padding.Empty;
            control.Anchor = AnchorStyles.None;
            row.Controls.Add(control);
        }

        AddRow(row);
    }

    private void AddDividerRow()
    {
        var divider = new RoundedPanel
        {
            Height = 1,
            CornerRadius = 0,
            BorderWidth = 0,
            BackColor = Theme.Border,
            Padding = Padding.Empty,
            TabStop = false,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, SectionGap, 0, 0)
        };
        AddRow(divider);
    }

    private void AddFooterRow()
    {
        var footer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Height = StdHeight + 2,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 12, 0, 0),
            Padding = Padding.Empty,
            BackColor = Theme.Surface
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Constants.UI.ButtonWidthSmall));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, Constants.UI.ButtonWidthSmall));
        footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _cancelButton = new ModernButton
        {
            Text = Localization.Get("Cancel"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Dock = DockStyle.Fill
        };
        _cancelButton.Click += CancelButton_Click;
        footer.Controls.Add(_cancelButton, 2, 0);

        _saveButton = new ModernButton
        {
            Text = Localization.Get("Save"),
            ButtonVariant = ModernButton.Variant.Primary,
            Dock = DockStyle.Fill
        };
        _saveButton.Click += SaveButton_Click;
        footer.Controls.Add(_saveButton, 1, 0);

        AddRow(footer);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        RedrawLock.Suspend(this);
        base.OnHandleCreated(e);
        _statusChip?.UpdateSize();
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        _statusChip?.UpdateSize();
    }

    private void LanguageComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        string lang = GetSelectedLanguage();
        if (Localization.CurrentLanguage != lang)
            Localization.SetLanguage(lang);
    }

    private string GetSelectedLanguage()
    {
        return _languageComboBox.SelectedIndex switch
        {
            0 => Localization.Languages.English,
            1 => Localization.Languages.Persian,
            _ => Localization.Languages.English
        };
    }

    private void HotkeyModifierComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateStatus();
    }

    private void HotkeyKeyComboBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateStatus();
    }

    private void LoadSettings()
    {
        _runOnStartupToggle.Checked = _settings.RunOnStartup;

        _languageComboBox.SelectedIndex = _settings.Language switch
        {
            Localization.Languages.English => 0,
            _ => 1
        };

        LoadHotkeyFromSettings();
        UpdateStatus();
    }

    private void LoadHotkeyFromSettings()
    {
        _hotkeyModifierComboBox.SelectedIndex = HotkeyOptions.IndexOfModifier((uint)_settings.HotkeyModifier);
        _hotkeyKeyComboBox.SelectedIndex = HotkeyOptions.IndexOfKey(_settings.HotkeyKey);

        _lastHotkeyModifier = (uint)_settings.HotkeyModifier;
        _lastHotkeyKey = _settings.HotkeyKey;
    }

    private void RegisterHotkeyButton_Click(object? sender, EventArgs e)
    {
        try
        {
            var (modifier, key) = GetSelectedHotkey();

            _hotkeyManager.Unregister();
            bool success = _hotkeyManager.Register(modifier, key);

            if (success)
            {
                _isHotkeyRegistered = true;
                _statusState = StatusState.RegisterSuccess;
                UpdateStatusLabel();

                _settings.HotkeyModifier = (int)modifier;
                _settings.HotkeyKey = key;
                _lastHotkeyKey = key;
                _lastHotkeyModifier = modifier;
                _registerHotkeyButton.Enabled = false;
                MessageBox.Show(Localization.Get("HotkeyRegisterSuccess"), Localization.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _statusState = StatusState.RegisterFailed;
                UpdateStatusLabel();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(Localization.Format("StartupError", ex.Message), Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private (uint modifier, Keys key) GetSelectedHotkey()
    {
        uint modifier = HotkeyOptions.ModifierValue(_hotkeyModifierComboBox.SelectedIndex);
        Keys key = HotkeyOptions.KeyValue(_hotkeyKeyComboBox.SelectedIndex);
        return (modifier, key);
    }

    private void UpdateStatus()
    {
        var hotkey = GetSelectedHotkey();
        var hasChanged = _lastHotkeyKey != hotkey.key || _lastHotkeyModifier != hotkey.modifier;

        _isHotkeyRegistered = !hasChanged;
        _statusState = _isHotkeyRegistered ? StatusState.Registered : StatusState.NotRegistered;

        UpdateStatusLabel();
        _registerHotkeyButton.Enabled = !_isHotkeyRegistered;
    }

    private void UpdateStatusLabel()
    {
        _statusChip.Text = _statusState switch
        {
            StatusState.Registered => Localization.Get("StatusRegistered"),
            StatusState.NotRegistered => Localization.Get("StatusNotRegistered"),
            StatusState.RegisterSuccess => Localization.Get("StatusRegisterSuccess"),
            StatusState.RegisterFailed => Localization.Get("StatusRegisterFailed"),
            _ => Localization.Get("StatusChecking")
        };

        // The chip reads its background from Tag when painting (BackColor is unused).
        (_statusChip.ForeColor, _statusChip.Tag) = _statusState switch
        {
            StatusState.Registered or StatusState.RegisterSuccess => (Theme.Success, Theme.SuccessSoft),
            StatusState.NotRegistered => (Theme.Warning, Theme.WarningSoft),
            StatusState.RegisterFailed => (Theme.Danger, (object)Theme.DangerSoft),
            _ => (Theme.Info, (object)Theme.InfoSoft)
        };

        _statusChip.UpdateSize();
        _statusChip.Invalidate();
    }

    private void CancelButton_Click(object? sender, EventArgs e)
    {
        Localization.SetLanguage(_settings.Language);
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private async Task CheckUpdateButton_Click()
    {
        _checkUpdateButton.Enabled = false;

        using var progress = new UpdateProgressDialog(this);
        try
        {
            var status = await AutoUpdater.CheckForUpdatesAsync(progress.Progress, true);
            if (status == AutoUpdater.UpdateStatus.NoUpdate)
                MessageBox.Show(Localization.Format("UpdNoUpdate", VersionInfo.CurrentString),
                    Localization.Get("Update"), MessageBoxButtons.OK, MessageBoxIcon.Information);
            else if (status == AutoUpdater.UpdateStatus.Error)
                MessageBox.Show(Localization.Get("UpdCheckErrorInternet"),
                    Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Localization.Format("UpdCheckError", ex.Message),
                Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            progress.Close();
            _checkUpdateButton.Enabled = true;
        }
    }

    private void SaveButton_Click(object? sender, EventArgs e)
    {
        try
        {
            _settings.RunOnStartup = _runOnStartupToggle.Checked;
            _settings.Language = GetSelectedLanguage();

            if (!_isHotkeyRegistered)
            {
                var (modifier, key) = GetSelectedHotkey();
                _settings.HotkeyModifier = (int)modifier;
                _settings.HotkeyKey = key;
            }

            SettingsManager.AddToStartup(_settings.RunOnStartup);
            SettingsManager.Save(_settings);

            MessageBox.Show(Localization.Get("SettingsSaved"), Localization.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(Localization.Format("SaveSettingsError", ex.Message), Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void KeyboardMappingsButton_Click(object? sender, EventArgs e)
    {
        FormSingleton.ShowDialog(() => new KeyboardMappingsForm(_settings), this);
    }

    private enum StatusState
    {
        Checking,
        Registered,
        NotRegistered,
        RegisterSuccess,
        RegisterFailed
    }
}