using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.Model;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.UI;

namespace WrongKeyboardFixer.UI.Forms;

public partial class SettingsForm : Form
{
    private uint lastHotkeyModifier;
    private Keys lastHotkeyKey;

    private readonly WrongKeyboardFixer.Core.Model.AppSettings _settings;
    private readonly HotkeyManager _hotkeyManager;
    private bool _isHotkeyRegistered;

    // کنترل‌های فرم
    private CheckBox chkRunOnStartup = null!;
    private ComboBox cmbLanguage = null!;
    private Label lblLanguage = null!;
    private Label titleLabel = null!;
    private GroupBox grpHotkey = null!;
    private Label lblHotkey = null!;
    private ComboBox cmbHotkeyModifier = null!;
    private Label lblPlus = null!;
    private ComboBox cmbHotkeyKey = null!;
    private Button btnRegisterHotkey = null!;
    private Label lblStatus = null!;
    private Label lblVersionTitle = null!;
    private Button btnCheckUpdate = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;
    private Button btnKeyboardMappings = null!;
    private enum StatusState { Checking, Registered, NotRegistered, RegisterSuccess, RegisterFailed }
    private StatusState _statusState = StatusState.Checking;

    public SettingsForm(WrongKeyboardFixer.Core.Model.AppSettings settings, HotkeyManager hotkeyManager)
    {
        _settings = settings ?? new WrongKeyboardFixer.Core.Model.AppSettings();
        _hotkeyManager = hotkeyManager;

        // زبان ذخیره‌شده را فعال کن قبل از ساخت کنترل‌ها
        Localization.SetLanguage(_settings.Language);

        // RightToLeftLayout را ثابت نگه می‌داریم تا در زمان اجرا تغییر نکند
        // (تغییر آن بعد از Show فرم پشتیبانی نمی‌شود)
        this.RightToLeftLayout = true;

        InitializeControls();
        LoadSettings();
    }

    private void InitializeControls()
    {
        this.Text = Localization.Get("Settings");
        this.Size = new System.Drawing.Size(520, 450);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        // When opened from the tray the main form is hidden/minimized, so
        // CenterParent can position the dialog incorrectly. Use CenterScreen
        // to ensure the settings window appears visible to the user.
        this.StartPosition = FormStartPosition.CenterScreen;
        this.Font = new System.Drawing.Font("Tahoma", 9);
        this.Icon = IconLoader.GetIcon();
        this.RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No;

        int marginX = 25; // فاصله استاندارد از لبه‌های چپ و راست فرم
        int currentY = 20; // موقعیت عمودی شروع
        int formWidth = this.ClientSize.Width;
        int controlWidth = formWidth - (2 * marginX); // عرض مفید برای کنترل‌های سرتاسری

        // ۱. عنوان اصلی
        titleLabel = new Label
        {
            Text = Localization.Get("SettingsTitle"),
            Font = new System.Drawing.Font("Tahoma", 11, System.Drawing.FontStyle.Bold),
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 30),
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        Controls.Add(titleLabel);

        // ۲. ایموجی ترنج (Tray Icon) با نام برنامه
        currentY += 40;
        var trayIconLabel = new Label
        {
            Text = "⚙️ " + Localization.Get("TrayText"),
            Font = new System.Drawing.Font("Tahoma", 10),
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 25),
            Anchor = AnchorStyles.Left | AnchorStyles.Right
        };
        Controls.Add(trayIconLabel);

        // ۳. چک‌باکس اجرای خودکار
        currentY += 35;
        chkRunOnStartup = new CheckBox
        {
            Text = Localization.Get("RunOnStartup"),
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 24),
            Checked = _settings.RunOnStartup
        };
        Controls.Add(chkRunOnStartup);

        // ۴. کم‌باکس زبان
        currentY += 35;
        lblLanguage = new Label
        {
            Text = Localization.Get("Language"),
            Location = new Point(marginX, currentY),
            Size = new Size(100, 20),
            Anchor = AnchorStyles.Left
        };
        Controls.Add(lblLanguage);

        cmbLanguage = new ComboBox
        {
            Location = new Point(marginX + 110, currentY - 3),
            Size = new Size(150, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbLanguage.Items.Add(Localization.Get("LanguagePersian"));
        cmbLanguage.Items.Add(Localization.Get("LanguageEnglish"));
        cmbLanguage.Items.Add(Localization.Get("LanguageFrench"));
        
        // Select current language
        int selectedIndex = _settings.Language.ToLowerInvariant() switch
        {
            "fa" => 0,
            "en" => 1,
            _ => 1
        };
        cmbLanguage.SelectedIndex = selectedIndex;
        Controls.Add(cmbLanguage);

        // ۵. گروپ‌باکس تنظیمات کلید میانبر
        currentY += 65;
        grpHotkey = new GroupBox
        {
            Text = Localization.Get("HotkeyGroup"),
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 130)
        };
        Controls.Add(grpHotkey);

        int groupMarginX = 20;
        int groupCurrentY = 25;

        lblHotkey = new Label
        {
            Text = Localization.Get("HotkeyLabel"),
            Location = new Point(groupMarginX, groupCurrentY),
            Size = new Size(100, 20)
        };
        grpHotkey.Controls.Add(lblHotkey);

        cmbHotkeyModifier = new ComboBox
        {
            Location = new Point(groupMarginX + 110, groupCurrentY - 3),
            Size = new Size(150, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbHotkeyModifier.Items.Add(Localization.Get("HotkeyModifierCtrlAlt"));
        cmbHotkeyModifier.Items.Add(Localization.Get("HotkeyModifierCtrlShift"));
        cmbHotkeyModifier.Items.Add(Localization.Get("HotkeyModifierAltShift"));
        cmbHotkeyModifier.Items.Add(Localization.Get("HotkeyModifierCtrl"));
        cmbHotkeyModifier.Items.Add(Localization.Get("HotkeyModifierAlt"));
        cmbHotkeyModifier.Items.Add(Localization.Get("HotkeyModifierShift"));
        
        // Set current modifier
        int modIndex = _settings.HotkeyModifier switch
        {
            (int)(HotkeyModifiers.Control | HotkeyModifiers.Alt) => 0,
            (int)(HotkeyModifiers.Control | HotkeyModifiers.Shift) => 1,
            (int)(HotkeyModifiers.Alt | HotkeyModifiers.Shift) => 2,
            (int)HotkeyModifiers.Control => 3,
            (int)HotkeyModifiers.Alt => 4,
            (int)HotkeyModifiers.Shift => 5,
            _ => 0
        };
        cmbHotkeyModifier.SelectedIndex = modIndex;
        grpHotkey.Controls.Add(cmbHotkeyModifier);

        lblPlus = new Label
        {
            Text = "+",
            Location = new Point(groupMarginX + 265, groupCurrentY + 2),
            Size = new Size(20, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };
        grpHotkey.Controls.Add(lblPlus);

        cmbHotkeyKey = new ComboBox
        {
            Location = new Point(groupMarginX + 285, groupCurrentY - 3),
            Size = new Size(120, 24),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyAdd"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeySubtract"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyMultiply"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF1"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF2"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF3"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF4"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF5"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF6"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF7"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF8"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF9"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF10"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF11"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyF12"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyInsert"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyHome"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyPageUp"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyPageDown"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyEnd"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeyDelete"));
        cmbHotkeyKey.Items.Add(Localization.Get("HotkeyKeySpace"));
        
        // Select current key
        int keyIndex = _settings.HotkeyKey switch
        {
            Keys.Add => 0,
            Keys.Subtract => 1,
            Keys.Multiply => 2,
            Keys.F1 => 3,
            Keys.F2 => 4,
            Keys.F3 => 5,
            Keys.F4 => 6,
            Keys.F5 => 7,
            Keys.F6 => 8,
            Keys.F7 => 9,
            Keys.F8 => 10,
            Keys.F9 => 11,
            Keys.F10 => 12,
            Keys.F11 => 13,
            Keys.F12 => 14,
            Keys.Insert => 15,
            Keys.Home => 16,
            Keys.PageUp => 17,
            Keys.PageDown => 18,
            Keys.End => 19,
            Keys.Delete => 20,
            Keys.Space => 21,
            _ => 0
        };
        cmbHotkeyKey.SelectedIndex = keyIndex;
        grpHotkey.Controls.Add(cmbHotkeyKey);

        btnRegisterHotkey = new Button
        {
            Text = Localization.Get("ApplyHotkey"),
            Location = new Point(groupMarginX, groupCurrentY + 40),
            Size = new Size(150, 30)
        };
        btnRegisterHotkey.Click += BtnRegisterHotkey_Click;
        grpHotkey.Controls.Add(btnRegisterHotkey);

        lblStatus = new Label
        {
            Text = Localization.Get("StatusNotRegistered"),
            Location = new Point(groupMarginX + 165, groupCurrentY + 45),
            Size = new Size(240, 20),
            ForeColor = Color.OrangeRed
        };
        grpHotkey.Controls.Add(lblStatus);

        // ۶. دکمه‌های پایین فرم
        currentY += 155;
        int buttonWidth = 100;
        int buttonHeight = 32;
        int btnMarginX = (formWidth - (buttonWidth * 3 + 20 * 2)) / 2; // Center buttons

        btnKeyboardMappings = new Button
        {
            Text = Localization.Get("KeyboardMappings"),
            Location = new Point(btnMarginX, currentY),
            Size = new Size(buttonWidth, buttonHeight)
        };
        btnKeyboardMappings.Click += (s, e) => ShowKeyboardMappings();
        Controls.Add(btnKeyboardMappings);

        btnCancel = new Button
        {
            Text = Localization.Get("Cancel"),
            Location = new Point(btnMarginX + buttonWidth + 20, currentY),
            Size = new Size(buttonWidth, buttonHeight),
            DialogResult = DialogResult.Cancel
        };
        Controls.Add(btnCancel);

        btnSave = new Button
        {
            Text = Localization.Get("Save"),
            Location = new Point(btnMarginX + (buttonWidth + 20) * 2, currentY),
            Size = new Size(buttonWidth, buttonHeight),
            DialogResult = DialogResult.OK
        };
        btnSave.Click += (s, e) => SaveSettings();
        Controls.Add(btnSave);

        // ۷. اطلاعات نسخه (در پایین فرم)
        currentY += 45;
        lblVersionTitle = new Label
        {
            Text = Localization.Format("CurrentVersion", "1.2.0"),
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 20),
            Anchor = AnchorStyles.Left
        };
        Controls.Add(lblVersionTitle);

        btnCheckUpdate = new Button
        {
            Text = Localization.Get("CheckUpdate"),
            Location = new Point(formWidth - 120 - marginX, currentY - 3),
            Size = new Size(120, 24)
        };
        btnCheckUpdate.Click += (s, e) => CheckForUpdatesAsync();
        Controls.Add(btnCheckUpdate);
    }

    private void LoadSettings()
    {
        UpdateHotkeyStatus();
    }

    private void UpdateHotkeyStatus()
    {
        if (_isHotkeyRegistered)
        {
            lblStatus.Text = Localization.Get("StatusRegistered");
            lblStatus.ForeColor = Color.Green;
        }
        else
        {
            lblStatus.Text = Localization.Get("StatusNotRegistered");
            lblStatus.ForeColor = Color.OrangeRed;
        }
    }

    private void BtnRegisterHotkey_Click(object? sender, EventArgs e)
    {
        // ثبت کلید میانبر جدید
        uint newModifier = cmbHotkeyModifier.SelectedIndex switch
        {
            0 => HotkeyModifier.ControlAlt,
            1 => HotkeyModifier.Control | HotkeyModifier.Shift,
            2 => HotkeyModifier.Alt | HotkeyModifier.Shift,
            3 => HotkeyModifier.Control,
            4 => HotkeyModifier.Alt,
            5 => HotkeyModifier.Shift,
            _ => HotkeyModifier.ControlAlt
        };

        Keys newKey = cmbHotkeyKey.SelectedIndex switch
        {
            0 => Keys.Add,
            1 => Keys.Subtract,
            2 => Keys.Multiply,
            3 => Keys.F1,
            4 => Keys.F2,
            5 => Keys.F3,
            6 => Keys.F4,
            7 => Keys.F5,
            8 => Keys.F6,
            9 => Keys.F7,
            10 => Keys.F8,
            11 => Keys.F9,
            12 => Keys.F10,
            13 => Keys.F11,
            14 => Keys.F12,
            15 => Keys.Insert,
            16 => Keys.Home,
            17 => Keys.PageUp,
            18 => Keys.PageDown,
            19 => Keys.End,
            20 => Keys.Delete,
            21 => Keys.Space,
            _ => Keys.Add
        };

        // اگر همه چیز درست است، کلید میانبر را ثبت کن
        if (_hotkeyManager.Register(newModifier, newKey))
        {
            _isHotkeyRegistered = true;
            lastHotkeyModifier = newModifier;
            lastHotkeyKey = newKey;
            UpdateHotkeyStatus();
            MessageBox.Show(Localization.Get("StatusRegisterSuccess"), Localization.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            _isHotkeyRegistered = false;
            UpdateHotkeyStatus();
            MessageBox.Show(Localization.Get("StatusRegisterFailed"), Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ShowKeyboardMappings()
    {
        var mappingsForm = new WrongKeyboardFixer.Core.UI.KeyboardMappingsForm();
        mappingsForm.ShowDialog();
    }

    private void CheckForUpdatesAsync()
    {
        _ = Core.Services.AutoUpdater.CheckForUpdatesAsync();
    }

    private void SaveSettings()
    {
        try
        {
            // ذخیره تنظیمات
            _settings.RunOnStartup = chkRunOnStartup.Checked;

            // ذخیره زبان
            _settings.Language = cmbLanguage.SelectedIndex switch
            {
                0 => "fa",
                1 => "en",
                2 => "fr",
                _ => "en"
            };

            // ذخیره کلید میانبر
            _settings.HotkeyModifier = cmbHotkeyModifier.SelectedIndex switch
            {
                0 => (int)(HotkeyModifiers.Control | HotkeyModifiers.Alt),
                1 => (int)(HotkeyModifiers.Control | HotkeyModifiers.Shift),
                2 => (int)(HotkeyModifiers.Alt | HotkeyModifiers.Shift),
                3 => (int)HotkeyModifiers.Control,
                4 => (int)HotkeyModifiers.Alt,
                5 => (int)HotkeyModifiers.Shift,
                _ => (int)(HotkeyModifiers.Control | HotkeyModifiers.Alt)
            };

            _settings.HotkeyKey = cmbHotkeyKey.SelectedIndex switch
            {
                0 => Keys.Add,
                1 => Keys.Subtract,
                2 => Keys.Multiply,
                3 => Keys.F1,
                4 => Keys.F2,
                5 => Keys.F3,
                6 => Keys.F4,
                7 => Keys.F5,
                8 => Keys.F6,
                9 => Keys.F7,
                10 => Keys.F8,
                11 => Keys.F9,
                12 => Keys.F10,
                13 => Keys.F11,
                14 => Keys.F12,
                15 => Keys.Insert,
                16 => Keys.Home,
                17 => Keys.PageUp,
                18 => Keys.PageDown,
                19 => Keys.End,
                20 => Keys.Delete,
                21 => Keys.Space,
                _ => Keys.Add
            };

            // ذخیره تنظیمات
            SettingsManager.Save(_settings);

            // اعمال زبان جدید
            Localization.SetLanguage(_settings.Language);

            // اجرای خودکار
            SettingsManager.AddToStartup(_settings.RunOnStartup);

            MessageBox.Show(Localization.Get("SettingsSaved"), Localization.Get("Success"), MessageBoxButtons.OK, MessageBoxIcon.Information);

            // بستن فرم با موفقیت
            this.DialogResult = DialogResult.OK;
        }
        catch (Exception ex)
        {
            MessageBox.Show(Localization.Format("SaveSettingsError", ex.Message), Localization.Get("Error"), MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}