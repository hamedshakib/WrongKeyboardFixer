using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Model;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.UI;

namespace WrongKeyboardFixer.UI.Forms;

public partial class SettingsForm : ModernForm, ICloseRequestHandler
{
    private uint lastHotkeyModifier;
    private Keys lastHotkeyKey;

    private readonly AppSettings _settings;
    private readonly HotkeyManager _hotkeyManager;
    private bool _isHotkeyRegistered;

    // Controls
    private ToggleSwitch tglRunOnStartup = null!;
    private ComboBox cmbLanguage = null!;
    private ComboBox cmbHotkeyModifier = null!;
    private ComboBox cmbHotkeyKey = null!;
    private ModernButton btnRegisterHotkey = null!;
    private StatusChip lblStatus = null!;
    private Label lblVersionValue = null!;
    private ModernButton btnCheckUpdate = null!;
    private ModernButton btnSave = null!;
    private ModernButton btnCancel = null!;
    private ModernButton btnKeyboardMappings = null!;

    private enum StatusState { Checking, Registered, NotRegistered, RegisterSuccess, RegisterFailed }
    private StatusState _statusState = StatusState.Checking;

    public SettingsForm(AppSettings settings, HotkeyManager hotkeyManager)
    {
        _settings = settings ?? new AppSettings();
        _hotkeyManager = hotkeyManager;
        Localization.SetLanguage(_settings.Language);
        this.RightToLeftLayout = true;

        this.SuspendLayout();                // ← جدید
        InitializeForm();
        InitializeControls();
        this.ResumeLayout(false);            // ← جدید

        LoadSettings();
    }

    private void InitializeForm()
    {
        this.Text = Localization.Get("Settings");
        this.FormBorderStyle = FormBorderStyle.None;
        this.AutoScaleDimensions = new SizeF(96F, 96F);
        this.AutoScaleMode = AutoScaleMode.Dpi;
        this.ClientSize = new Size(560, 600);
        this.MinimumSize = new Size(560, 600);
        this.StartPosition = FormStartPosition.CenterScreen;
        this.BackColor = Theme.Background;
        this.Font = Theme.BodyFont;
        this.Icon = IconLoader.GetIcon();
        this.RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No;

        AddTitleBar("SettingsTitle", "SettingsSubtitle");
    }

    private const int StdHeight = 36;
    private const int RowGap = 14;
    private const int SectionGap = 22;
    private TableLayoutPanel _layout = null!;
    private RoundedPanel _panel = null!;

    private void InitializeControls()
    {
        _panel = new RoundedPanel
        {
            CornerRadius = 14,
            BackColor = Theme.Surface,
            Padding = new Padding(24),
            Location = new Point(14, ModernTitleBar.TitleBarHeight + 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoScroll = true
        };
        _panel.Size = new Size(this.ClientSize.Width - 28,
            this.ClientSize.Height - ModernTitleBar.TitleBarHeight - 24);
        this.Controls.Add(_panel);

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
        AddHeaderRow(Localization.Get("Language"), topGap: 0);

        cmbLanguage = Theme.CreateCombo(
            Localization.Get("LanguageEnglish"),
            Localization.Get("LanguagePersian"));
        cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
        AddControlRow(cmbLanguage, width: 170, topGap: 10);

        tglRunOnStartup = new ToggleSwitch
        {
            Text = Localization.Get("RunOnStartup"),
            Height = 30,
            AutoSize = false,
            Width = 220,
        };

        AddControlRow(tglRunOnStartup, fillWidth: false, topGap: RowGap);

        AddDividerRow();

        // ── Hotkey section ────────────────────────────────
        AddHeaderRow(Localization.Get("HotkeyGroup"));

        var modifierItems = new string[HotkeyOptions.ModifierCount];
        for (int i = 0; i < HotkeyOptions.ModifierCount; i++)
            modifierItems[i] = HotkeyOptions.ModifierText(i);
        cmbHotkeyModifier = Theme.CreateCombo(modifierItems);
        cmbHotkeyModifier.Width = 160;
        cmbHotkeyModifier.SelectedIndex = 0;
        cmbHotkeyModifier.SelectedIndexChanged += CmbHotkeyModifier_SelectedIndexChanged;

        var lblPlus = Theme.BodyLabel("+", Theme.TextSecondary, Theme.BodyBoldFont);
        lblPlus.TextAlign = ContentAlignment.MiddleCenter;
        lblPlus.Width = 24;

        var keyItems = new string[HotkeyOptions.KeyCount];
        for (int i = 0; i < HotkeyOptions.KeyCount; i++)
            keyItems[i] = HotkeyOptions.KeyText(i);
        cmbHotkeyKey = Theme.CreateCombo(keyItems);
        cmbHotkeyKey.Width = 130;
        cmbHotkeyKey.SelectedIndex = 0;
        cmbHotkeyKey.SelectedIndexChanged += CmbHotkeyKey_SelectedIndexChanged;
        AddColumnsRow(topGap: 10, (cmbHotkeyModifier, 160), (lblPlus, 24), (cmbHotkeyKey, 130));

        btnRegisterHotkey = new ModernButton
        {
            Text = Localization.Get("ApplyHotkey"),
            ButtonVariant = ModernButton.Variant.Primary,
            Width = 150
        };
        btnRegisterHotkey.Click += BtnRegisterHotkey_Click;
        btnRegisterHotkey.Enabled = false;

        lblStatus = new StatusChip(Localization.Get("StatusChecking"), Theme.Info, Theme.InfoSoft);
        AddColumnsRow(topGap: RowGap, (btnRegisterHotkey, 150), (lblStatus, null));

        AddDividerRow();

        // ── Keyboard section ──────────────────────────────
        AddHeaderRow(Localization.Get("KeyboardMappings"));

        btnKeyboardMappings = new ModernButton
        {
            Text = Localization.Get("KeyboardMappings"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Width = 240
        };
        btnKeyboardMappings.Click += BtnKeyboardMappings_Click;
        AddControlRow(btnKeyboardMappings, width: 240, topGap: 10);

        AddDividerRow();

        // ── Current version section ───────────────────────
        AddHeaderRow(Localization.Get("CurrentVersion"));

        lblVersionValue = Theme.BodyLabel(AutoUpdater.GetCurrentVersionString(), Theme.Accent, Theme.BodyBoldFont);
        lblVersionValue.Width = 140;
        btnCheckUpdate = new ModernButton
        {
            Text = Localization.Get("CheckUpdate"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Width = 170
        };
        btnCheckUpdate.Click += async (_, _) => await BtnCheckUpdate_Click();
        AddColumnsRow(topGap: 10, (lblVersionValue, 140), (btnCheckUpdate, 170));

        AddDividerRow();

        // ── Footer ────────────────────────────────────────
        AddFooterRow();

        // Size the form to the actual content height (structural, no hardcoded Y).
        var prefSize = _layout.GetPreferredSize(new Size(_panel.ClientSize.Width, 0));
        int panelHeight = _panel.Padding.Top + prefSize.Height + _panel.Padding.Bottom;
        _panel.Height = panelHeight;
        this.ClientSize = new Size(this.ClientSize.Width, _panel.Location.Y + panelHeight + 14);
        this.MinimumSize = this.ClientSize;
    }

    private void FitFormToContent()
    {
        _layout.PerformLayout();
        int contentH = _layout.Height;                      // ارتفاع واقعی پس از چیدمان نهایی
        int panelHeight = _panel.Padding.Vertical + contentH;
        _panel.Height = panelHeight;
        this.ClientSize = new Size(this.ClientSize.Width,
            _panel.Location.Y + panelHeight + 14);
        this.MinimumSize = this.ClientSize;
    }

    protected override void OnShown(EventArgs e)
    {
        base.OnShown(e);
        FitFormToContent();   // تصحیح نهایی پس از layout واقعی → حذف کادر خالی پایین
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
            Margin = new Padding(0, 12, 0, 0),   // قبلاً SectionGap(22) → فاصلهٔ مضاعف و حس «کادر خالی»
            Padding = Padding.Empty,
            BackColor = Theme.Surface
        };
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        footer.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 112));
        footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        btnCancel = new ModernButton
        {
            Text = Localization.Get("Cancel"),
            ButtonVariant = ModernButton.Variant.Secondary,
            Dock = DockStyle.Fill
        };
        btnCancel.Click += BtnCancel_Click;
        footer.Controls.Add(btnCancel, 2, 0);

        btnSave = new ModernButton
        {
            Text = Localization.Get("Save"),
            ButtonVariant = ModernButton.Variant.Primary,
            Dock = DockStyle.Fill
        };
        btnSave.Click += BtnSave_Click;
        footer.Controls.Add(btnSave, 1, 0);

        AddRow(footer);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        RedrawLock.Suspend(this);
        base.OnHandleCreated(e);
        lblStatus?.UpdateSize();
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        lblStatus?.UpdateSize();
    }

    private void CmbLanguage_SelectedIndexChanged(object? sender, EventArgs e)
    {
        string lang = GetSelectedLanguage();
        if (Localization.CurrentLanguage != lang)
            Localization.SetLanguage(lang);
    }

    private string GetSelectedLanguage()
    {
        return cmbLanguage.SelectedIndex switch
        {
            0 => Localization.Languages.English,
            1 => Localization.Languages.Persian,
            _ => Localization.Languages.English
        };
    }

    private void CmbHotkeyModifier_SelectedIndexChanged(object? sender, EventArgs e) => UpdateStatus();
    private void CmbHotkeyKey_SelectedIndexChanged(object? sender, EventArgs e) => UpdateStatus();

    private void LoadSettings()
    {
        tglRunOnStartup.Checked = _settings.RunOnStartup;

        cmbLanguage.SelectedIndex = _settings.Language switch
        {
            Localization.Languages.English => 0,
            _ => 1
        };

        LoadHotkeyFromSettings();
        UpdateStatus();
    }

    private void LoadHotkeyFromSettings()
    {
        cmbHotkeyModifier.SelectedIndex = HotkeyOptions.IndexOfModifier((uint)_settings.HotkeyModifier);
        cmbHotkeyKey.SelectedIndex = HotkeyOptions.IndexOfKey(_settings.HotkeyKey);

        lastHotkeyModifier = (uint)_settings.HotkeyModifier;
        lastHotkeyKey = _settings.HotkeyKey;
    }

    private void BtnRegisterHotkey_Click(object? sender, EventArgs e)
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
                lastHotkeyKey = key;
                lastHotkeyModifier = modifier;
                btnRegisterHotkey.Enabled = false;
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
        uint modifier = HotkeyOptions.ModifierValue(cmbHotkeyModifier.SelectedIndex);
        Keys key = HotkeyOptions.KeyValue(cmbHotkeyKey.SelectedIndex);
        return (modifier, key);
    }

    private void UpdateStatus()
    {
        var hotkey = GetSelectedHotkey();
        var hasChanged = (lastHotkeyKey != hotkey.key || lastHotkeyModifier != hotkey.modifier);

        _isHotkeyRegistered = !hasChanged;
        _statusState = _isHotkeyRegistered ? StatusState.Registered : StatusState.NotRegistered;

        UpdateStatusLabel();
        btnRegisterHotkey.Enabled = !_isHotkeyRegistered;
    }

    private void UpdateStatusLabel()
    {
        lblStatus.Text = _statusState switch
        {
            StatusState.Registered => Localization.Get("StatusRegistered"),
            StatusState.NotRegistered => Localization.Get("StatusNotRegistered"),
            StatusState.RegisterSuccess => Localization.Get("StatusRegisterSuccess"),
            StatusState.RegisterFailed => Localization.Get("StatusRegisterFailed"),
            _ => Localization.Get("StatusChecking")
        };

        // ← به‌جای BackColor، در Tag ذخیره می‌شود
        (lblStatus.ForeColor, lblStatus.Tag) = _statusState switch
        {
            StatusState.Registered or StatusState.RegisterSuccess => (Theme.Success, (object)Theme.SuccessSoft),
            StatusState.NotRegistered => (Theme.Warning, (object)Theme.WarningSoft),
            StatusState.RegisterFailed => (Theme.Danger, (object)Theme.DangerSoft),
            _ => (Theme.Info, (object)Theme.InfoSoft)
        };

        lblStatus.UpdateSize();
        lblStatus.Invalidate();
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        Localization.SetLanguage(_settings.Language);
        DialogResult = DialogResult.Cancel;
        Close();
    }

    private async Task BtnCheckUpdate_Click()
    {
        btnCheckUpdate.Enabled = false;

        using var progress = new UpdateProgressDialog(this);
        try
        {
            var status = await AutoUpdater.CheckForUpdatesAsync(progress.Progress, silent: true);
            if (status == AutoUpdater.UpdateStatus.NoUpdate)
                MessageBox.Show(Localization.Format("UpdNoUpdate", AutoUpdater.GetCurrentVersionString()),
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
            btnCheckUpdate.Enabled = true;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            _settings.RunOnStartup = tglRunOnStartup.Checked;
            _settings.Language = GetSelectedLanguage();

            if (!_isHotkeyRegistered)
            {
                var (modifier, key) = GetSelectedHotkey();
                _settings.HotkeyModifier = (int)modifier;
                _settings.HotkeyKey = key;
            }

            SettingsManager.AddToStartup(_settings.RunOnStartup);
            SettingsManager.Save(_settings);

            DialogResult = DialogResult.OK;
            Close();

            MessageBox.Show(Localization.Get("SettingsSaved"), Localization.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(Localization.Format("SaveSettingsError", ex.Message), Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void BtnKeyboardMappings_Click(object? sender, EventArgs e)
    {
        FormSingleton.ShowDialog(() => new KeyboardMappingsForm(_settings), this);
    }

    public void RequestClose()
    {
        BtnCancel_Click(this, EventArgs.Empty);
    }
}
