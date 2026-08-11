using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Model;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.UI;

namespace WrongKeyboardFixer.UI.Forms;

public partial class SettingsForm : Form, ICloseRequestHandler
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
    private Label lblStatus = null!;
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
        this.DoubleBuffered = true;          // ← جدید

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

        var titleBar = new ModernTitleBar
        {
            Dock = DockStyle.Top,
            Text = Localization.Get("SettingsTitle"),
            Subtitle = Localization.Get("SettingsSubtitle")
        };
        this.Controls.Add(titleBar);
    }

    private const int StdHeight = 36;
    private const int RowGap = 14;
    private const int SectionGap = 22;
    private TableLayoutPanel _layout = null!;

    private void InitializeControls()
    {
        var panel = new RoundedPanel
        {
            CornerRadius = 14,
            BackColor = Theme.Surface,
            Padding = new Padding(24),
            Location = new Point(14, ModernTitleBar.TitleBarHeight + 10),
            Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            AutoScroll = true
        };
        panel.Size = new Size(this.ClientSize.Width - 28,
            this.ClientSize.Height - ModernTitleBar.TitleBarHeight - 24);
        this.Controls.Add(panel);

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
        panel.Controls.Add(_layout);

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
            Height = 30
        };
        AddControlRow(tglRunOnStartup, fillWidth: true, topGap: RowGap);

        AddDividerRow();

        // ── Hotkey section ────────────────────────────────
        AddHeaderRow(Localization.Get("HotkeyGroup"));

        cmbHotkeyModifier = Theme.CreateCombo(
            Localization.Get("HotkeyModifierCtrlAlt"),
            Localization.Get("HotkeyModifierCtrlShift"),
            Localization.Get("HotkeyModifierAltShift"),
            Localization.Get("HotkeyModifierCtrl"),
            Localization.Get("HotkeyModifierAlt"),
            Localization.Get("HotkeyModifierShift"));
        cmbHotkeyModifier.Width = 160;
        cmbHotkeyModifier.SelectedIndex = 0;
        cmbHotkeyModifier.SelectedIndexChanged += CmbHotkeyModifier_SelectedIndexChanged;

        var lblPlus = Theme.BodyLabel("+", Theme.TextSecondary, Theme.BodyBoldFont);
        lblPlus.TextAlign = ContentAlignment.MiddleCenter;
        lblPlus.Width = 24;

        cmbHotkeyKey = Theme.CreateCombo(
            Localization.Get("HotkeyKeyAdd"),
            Localization.Get("HotkeyKeySubtract"),
            Localization.Get("HotkeyKeyMultiply"),
            Localization.Get("HotkeyKeyF1"),
            Localization.Get("HotkeyKeyF2"),
            Localization.Get("HotkeyKeyF3"),
            Localization.Get("HotkeyKeyF4"),
            Localization.Get("HotkeyKeyF5"),
            Localization.Get("HotkeyKeyF6"),
            Localization.Get("HotkeyKeyF7"),
            Localization.Get("HotkeyKeyF8"),
            Localization.Get("HotkeyKeyF9"),
            Localization.Get("HotkeyKeyF10"),
            Localization.Get("HotkeyKeyF11"),
            Localization.Get("HotkeyKeyF12"),
            Localization.Get("HotkeyKeyInsert"),
            Localization.Get("HotkeyKeyHome"),
            Localization.Get("HotkeyKeyPageUp"),
            Localization.Get("HotkeyKeyPageDown"),
            Localization.Get("HotkeyKeyEnd"),
            Localization.Get("HotkeyKeyDelete"),
            Localization.Get("HotkeyKeySpace"));
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

        lblStatus = CreateStatusChip(Localization.Get("StatusChecking"), Theme.Info, Theme.InfoSoft);
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
        var prefSize = _layout.GetPreferredSize(new Size(panel.ClientSize.Width, 0));
        int panelHeight = panel.Padding.Top + prefSize.Height + panel.Padding.Bottom;
        panel.Height = panelHeight;
        this.ClientSize = new Size(this.ClientSize.Width, panel.Location.Y + panelHeight + 14);
        this.MinimumSize = this.ClientSize;
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
            Margin = new Padding(0, SectionGap, 0, 0),
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

    private Label CreateStatusChip(string text, Color fore, Color back)
    {
        var chip = new Label
        {
            Text = text,
            Font = Theme.BodyBoldFont,
            ForeColor = fore,
            AutoSize = false,
            Height = StdHeight,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(12, 0, 12, 0),
            BackColor = Theme.Surface,   // همیشه هم‌رنگ پنل → گوشه‌های مربعی نامرئی
            Tag = back                   // رنگ وضعیت در Tag
        };
        chip.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var bounds = new Rectangle(0, 0, chip.Width - 1, chip.Height - 1);
            using var path = Theme.RoundRect(bounds, Theme.DpiScale(8, chip.DeviceDpi));
            using var brush = new SolidBrush(chip.Tag is Color c ? c : Theme.Surface);
            g.FillPath(brush, path);

            int pad = Theme.DpiScale(12, chip.DeviceDpi);
            var textRect = new Rectangle(pad, 0, chip.Width - pad * 2, chip.Height);
            var flags = TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix;
            if (Localization.IsRtl) flags |= TextFormatFlags.RightToLeft;
            TextRenderer.DrawText(g, chip.Text, chip.Font, textRect, chip.ForeColor, flags);
        };
        UpdateChipSize(chip);
        return chip;
    }

    /// <summary>
    /// Sizes the status chip to its content so it never stretches across the row.
    /// </summary>
    private static void UpdateChipSize(Label chip)
    {
        if (chip is null) return;
        int textWidth = TextRenderer.MeasureText(chip.Text, chip.Font).Width;
        chip.Width = textWidth + Theme.DpiScale(24, chip.DeviceDpi);
    }

    protected override void OnHandleCreated(EventArgs e)
    {
        base.OnHandleCreated(e);
        UpdateChipSize(lblStatus);
    }

    protected override void OnDpiChanged(DpiChangedEventArgs e)
    {
        base.OnDpiChanged(e);
        UpdateChipSize(lblStatus);
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
        var modifier = (HotkeyModifiers)_settings.HotkeyModifier;
        if (modifier.HasFlag(HotkeyModifiers.Control) && modifier.HasFlag(HotkeyModifiers.Alt))
            cmbHotkeyModifier.SelectedIndex = 0;
        else if (modifier.HasFlag(HotkeyModifiers.Control) && modifier.HasFlag(HotkeyModifiers.Shift))
            cmbHotkeyModifier.SelectedIndex = 1;
        else if (modifier.HasFlag(HotkeyModifiers.Alt) && modifier.HasFlag(HotkeyModifiers.Shift))
            cmbHotkeyModifier.SelectedIndex = 2;
        else if (modifier.HasFlag(HotkeyModifiers.Control))
            cmbHotkeyModifier.SelectedIndex = 3;
        else if (modifier.HasFlag(HotkeyModifiers.Alt))
            cmbHotkeyModifier.SelectedIndex = 4;
        else if (modifier.HasFlag(HotkeyModifiers.Shift))
            cmbHotkeyModifier.SelectedIndex = 5;

        var key = _settings.HotkeyKey;
        string keyName = key.ToString();
        for (int i = 0; i < cmbHotkeyKey.Items.Count; i++)
        {
            string? item = cmbHotkeyKey.Items[i]?.ToString();
            if (item is not null && (item.StartsWith(keyName) || item.Contains(keyName)))
            {
                cmbHotkeyKey.SelectedIndex = i;
                break;
            }
        }

        lastHotkeyModifier = (uint)_settings.HotkeyModifier;
        lastHotkeyKey = key;
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
        uint modifier = 0;

        switch (cmbHotkeyModifier.SelectedIndex)
        {
            case 0: modifier = (uint)(HotkeyModifiers.Control | HotkeyModifiers.Alt); break;
            case 1: modifier = (uint)(HotkeyModifiers.Control | HotkeyModifiers.Shift); break;
            case 2: modifier = (uint)(HotkeyModifiers.Alt | HotkeyModifiers.Shift); break;
            case 3: modifier = (uint)HotkeyModifiers.Control; break;
            case 4: modifier = (uint)HotkeyModifiers.Alt; break;
            case 5: modifier = (uint)HotkeyModifiers.Shift; break;
            default: modifier = (uint)(HotkeyModifiers.Control | HotkeyModifiers.Alt); break;
        }

        string keyText = cmbHotkeyKey.SelectedItem?.ToString() ?? "Add";
        Keys key = ParseKeyText(keyText);

        return (modifier, key);
    }

    private Keys ParseKeyText(string keyText)
    {
        if (keyText == Localization.Get("HotkeyKeyAdd")) return Keys.Add;
        if (keyText == Localization.Get("HotkeyKeySubtract")) return Keys.Subtract;
        if (keyText == Localization.Get("HotkeyKeyMultiply")) return Keys.Multiply;
        if (keyText == Localization.Get("HotkeyKeyInsert")) return Keys.Insert;
        if (keyText == Localization.Get("HotkeyKeyHome")) return Keys.Home;
        if (keyText == Localization.Get("HotkeyKeyPageUp")) return Keys.PageUp;
        if (keyText == Localization.Get("HotkeyKeyPageDown")) return Keys.PageDown;
        if (keyText == Localization.Get("HotkeyKeyEnd")) return Keys.End;
        if (keyText == Localization.Get("HotkeyKeyDelete")) return Keys.Delete;
        if (keyText == Localization.Get("HotkeyKeySpace")) return Keys.Space;

        if (keyText.StartsWith("F") && int.TryParse(keyText[1..], out int fNum))
            return (Keys)((int)Keys.F1 + fNum - 1);

        return Keys.Add;
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

        UpdateChipSize(lblStatus);
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
        var progressForm = new Form { /* ... همان تنظیمات قبلی ... */ };
        var lblMessage = new Label { /* ... */ };
        var progressBar = new ProgressBar { /* ... */ };
        progressForm.Controls.Add(lblMessage);
        progressForm.Controls.Add(progressBar);
        progressForm.Show(this);

        var progress = new Progress<(int percent, string message)>(update =>
        {
            progressBar.Value = Math.Min(update.percent, 100);
            lblMessage.Text = update.message;
        });
        try
        {
            var status = await AutoUpdater.CheckForUpdatesAsync(progress, silent: true);
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
            progressForm.Close();
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
        using var mappingsForm = new KeyboardMappingsForm(_settings);
        mappingsForm.ShowDialog(this);
    }

    public void RequestClose()
    {
        BtnCancel_Click(this, EventArgs.Empty);
    }
}
