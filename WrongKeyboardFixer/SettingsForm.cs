using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace WrongKeyboardFixer;

public partial class SettingsForm : Form
{
    private uint lastHotkeyModifier;
    private Keys lastHotkeyKey;

    private readonly AppSettings _settings;
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
    private ComboBox cmbHotkeyKey = null!;
    private Button btnRegisterHotkey = null!;
    private Label lblStatus = null!;
    private Label lblVersionTitle = null!;
    private Button btnCheckUpdate = null!;
    private Button btnSave = null!;
    private Button btnCancel = null!;

    private enum StatusState { Checking, Registered, NotRegistered, RegisterSuccess, RegisterFailed }
    private StatusState _statusState = StatusState.Checking;

    public SettingsForm(AppSettings settings, HotkeyManager hotkeyManager)
    {
        _settings = settings ?? new AppSettings();
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
        this.Size = new System.Drawing.Size(520, 440);
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
            TextAlign = ContentAlignment.MiddleLeft
        };
        this.Controls.Add(titleLabel);
        currentY += 40;

        // ۲. انتخاب زبان
        lblLanguage = new Label
        {
            Text = Localization.Get("Language"),
            Location = new Point(marginX, currentY),
            Size = new Size(100, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };
        this.Controls.Add(lblLanguage);

        cmbLanguage = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(marginX + 110, currentY),
            Size = new Size(120, 25)
        };
        // نام زبان‌ها به زبان خودشان نمایش داده می‌شود (endonym)
        cmbLanguage.Items.AddRange(new object[]
        {
            Localization.Get("LanguageEnglish"),
            Localization.Get("LanguagePersian"),
        });
        cmbLanguage.SelectedIndexChanged += CmbLanguage_SelectedIndexChanged;
        this.Controls.Add(cmbLanguage);
        currentY += 35;

        // ۳. اجرای خودکار با ویندوز
        chkRunOnStartup = new CheckBox
        {
            Text = Localization.Get("RunOnStartup"),
            CheckAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 25)
        };
        this.Controls.Add(chkRunOnStartup);
        currentY += 32;

        // ۴. کادر میانبر (GroupBox)
        grpHotkey = new GroupBox
        {
            Text = Localization.Get("HotkeyGroup"),
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 130),
            RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No
        };

        // المان‌های داخل کادر میانبر با موقعیت‌دهی محلی (نسبت به لبه‌های GroupBox)
        lblHotkey = new Label
        {
            Text = Localization.Get("HotkeyLabel"),
            Location = new Point(grpHotkey.Width - 95, 33),
            Size = new Size(80, 20),
            TextAlign = ContentAlignment.MiddleRight
        };
        grpHotkey.Controls.Add(lblHotkey);

        // انتخاب‌گر Modifier
        cmbHotkeyModifier = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(grpHotkey.Width - 215, 30),
            Size = new Size(110, 25)
        };
        cmbHotkeyModifier.Items.AddRange(new object[] { "Ctrl + Alt", "Ctrl + Shift", "Alt + Shift", "Ctrl", "Alt", "Shift" });
        cmbHotkeyModifier.SelectedIndex = 0;
        cmbHotkeyModifier.SelectedIndexChanged += CmbHotkeyModifier_SelectedIndexChanged;
        grpHotkey.Controls.Add(cmbHotkeyModifier);

        // علامت مثبت بین دو کمبواباکس
        var lblPlus = new Label
        {
            Text = "+",
            Location = new Point(grpHotkey.Width - 235, 33),
            Size = new Size(15, 20),
            TextAlign = ContentAlignment.MiddleCenter
        };
        grpHotkey.Controls.Add(lblPlus);

        // انتخاب‌گر کلید اصلی
        cmbHotkeyKey = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Location = new Point(grpHotkey.Width - 330, 30),
            Size = new Size(90, 25)
        };
        cmbHotkeyKey.Items.AddRange(new object[] { "Add (+)", "Subtract (-)", "Multiply (*)", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Insert", "Home", "PageUp", "PageDown", "End", "Delete", "Space" });
        cmbHotkeyKey.SelectedIndex = 0;
        cmbHotkeyKey.SelectedIndexChanged += CmbHotkeyKey_SelectedIndexChanged;
        grpHotkey.Controls.Add(cmbHotkeyKey);

        // دکمه اعمال کلید ترکیبی
        btnRegisterHotkey = new Button
        {
            Text = Localization.Get("ApplyHotkey"),
            Location = new Point(grpHotkey.Width - 175, 80),
            Size = new Size(160, 30),
            BackColor = System.Drawing.Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        btnRegisterHotkey.Click += BtnRegisterHotkey_Click;
        btnRegisterHotkey.Enabled = false;
        grpHotkey.Controls.Add(btnRegisterHotkey);

        // برچسب وضعیت ثبت
        lblStatus = new Label
        {
            Text = Localization.Get("StatusChecking"),
            Location = new Point(15, 85),
            Size = new Size(grpHotkey.Width - 190, 20),
            ForeColor = System.Drawing.Color.Blue,
            TextAlign = ContentAlignment.MiddleLeft // وضعیت در سمت چپ دکمه ثبت قرار بگیرد
        };
        grpHotkey.Controls.Add(lblStatus);

        this.Controls.Add(grpHotkey);
        currentY += grpHotkey.Height + 25;

        // ۵. بخش نسخه و بروزرسانی
        lblVersionTitle = new Label
        {
            Text = Localization.Get("CurrentVersion"),
            Font = new System.Drawing.Font("Tahoma", 9, System.Drawing.FontStyle.Bold),
            Location = new Point(marginX, currentY),
            Size = new Size(130, 25),
            TextAlign = ContentAlignment.MiddleLeft
        };
        this.Controls.Add(lblVersionTitle);

        var lblVersionValue = new Label
        {
            Text = AutoUpdater.GetCurrentVersionString(),
            Font = new System.Drawing.Font("Tahoma", 9),
            Location = new Point(marginX + 130, currentY),
            Size = new Size(100, 25),
            ForeColor = System.Drawing.Color.Blue,
            TextAlign = ContentAlignment.MiddleLeft
        };
        this.Controls.Add(lblVersionValue);

        // دکمه بررسی بروزرسانی
        btnCheckUpdate = new Button
        {
            Text = Localization.Get("CheckUpdate"),
            Location = new Point(this.ClientSize.Width - marginX - 185, currentY - 3),
            Size = new Size(185, 32),
            FlatStyle = FlatStyle.Flat
        };
        btnCheckUpdate.Click += async (_, _) => await BtnCheckUpdate_Click();
        this.Controls.Add(btnCheckUpdate);

        currentY += 40;

        int buttonY = currentY;
        int buttonWidth = 95;

        // دکمه ذخیره (حالا این دکمه کاملاً به لبه چپ فرم می‌چسبد)
        btnSave = new Button
        {
            Text = Localization.Get("Save"),
            Location = new Point(this.ClientSize.Width - marginX - buttonWidth, buttonY),
            Size = new Size(buttonWidth, 32),
            BackColor = System.Drawing.Color.LightBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.Click += BtnSave_Click;
        this.Controls.Add(btnSave);

        // دکمه انصراف (۱۰ پیکسل فاصله گرفته و در سمت راستِ دکمه ذخیره قرار می‌گیرد)
        btnCancel = new Button
        {
            Text = Localization.Get("Cancel"),
            Location = new Point(this.ClientSize.Width - marginX - (buttonWidth * 2) - 10, buttonY),
            Size = new Size(buttonWidth, 32),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += BtnCancel_Click;
        this.Controls.Add(btnCancel);
    }

    private void CmbLanguage_SelectedIndexChanged(object? sender, EventArgs e)
    {
        string lang = GetSelectedLanguage();

        if (Localization.CurrentLanguage != lang)
        {
            Localization.SetLanguage(lang);
            ApplyLanguage();
        }
    }

    private string GetSelectedLanguage()
    {
        return cmbLanguage.SelectedIndex switch
        {
            0 => Localization.Languages.English,
            1 => Localization.Languages.Persian
        };
    }

    /// <summary>
    /// تمام متن‌های فرم و جهت چیدمان را بر اساس زبان فعال به‌روزرسانی می‌کند.
    /// </summary>
    private void ApplyLanguage()
    {
        bool isRtl = Localization.IsRtl;

        // تغییر جهت چیدمان (فارسی: راست‌چین، انگلیسی/فرانسوی: چپ‌چین)
        this.RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;
        grpHotkey.RightToLeft = isRtl ? RightToLeft.Yes : RightToLeft.No;

        this.Text = Localization.Get("Settings");
        titleLabel.Text = Localization.Get("SettingsTitle");
        lblLanguage.Text = Localization.Get("Language");
        chkRunOnStartup.Text = Localization.Get("RunOnStartup");
        grpHotkey.Text = Localization.Get("HotkeyGroup");
        lblHotkey.Text = Localization.Get("HotkeyLabel");
        btnRegisterHotkey.Text = Localization.Get("ApplyHotkey");
        lblVersionTitle.Text = Localization.Get("CurrentVersion");
        btnCheckUpdate.Text = Localization.Get("CheckUpdate");
        btnSave.Text = Localization.Get("Save");
        btnCancel.Text = Localization.Get("Cancel");

        UpdateStatusLabel();
    }

    private void CmbHotkeyModifier_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateStatus();
    }

    private void CmbHotkeyKey_SelectedIndexChanged(object? sender, EventArgs e)
    {
        UpdateStatus();
    }

    private void LoadSettings()
    {
        chkRunOnStartup.Checked = _settings.RunOnStartup;

        // انتخاب زبان ذخیره‌شده (بدون فعال کردن رویداد تغییر زبان)
        cmbLanguage.SelectedIndex = _settings.Language switch
        {
            Localization.Languages.English => 0,
            _ => 1
        };

        LoadHotkeyFromSettings();
        UpdateStatus();

        // اعمال زبان فعلی روی تمام کنترل‌ها
        ApplyLanguage();
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
        return keyText switch
        {
            "Add (+)" => Keys.Add,
            "Subtract (-)" => Keys.Subtract,
            "Multiply (*)" => Keys.Multiply,
            "Insert" => Keys.Insert,
            "Home" => Keys.Home,
            "PageUp" => Keys.PageUp,
            "PageDown" => Keys.PageDown,
            "End" => Keys.End,
            "Delete" => Keys.Delete,
            "Space" => Keys.Space,
            _ when keyText.StartsWith("F") && int.TryParse(keyText[1..], out int fNum)
                => (Keys)((int)Keys.F1 + fNum - 1),
            _ => Keys.Add
        };
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

        lblStatus.ForeColor = _statusState switch
        {
            StatusState.Registered or StatusState.RegisterSuccess => System.Drawing.Color.Green,
            StatusState.NotRegistered => System.Drawing.Color.Orange,
            StatusState.RegisterFailed => System.Drawing.Color.Red,
            _ => System.Drawing.Color.Blue
        };
    }

    private void BtnCancel_Click(object? sender, EventArgs e)
    {
        // اگر کاربر زبان را تغییر داده و انصراف بدهد، زبان ذخیره‌شده قبلی برگردانده می‌شود
        Localization.SetLanguage(_settings.Language);
        DialogResult = DialogResult.Cancel;
    }

    /// <summary>
    /// بررسی دستی بروزرسانی با نمایش Progress Dialog
    /// </summary>
    private async Task BtnCheckUpdate_Click()
    {
        // ایجاد فرم Progress
        var progressForm = new Form
        {
            Text = Localization.Get("CheckingUpdate"),
            Size = new Size(400, 120),
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterScreen,
            MaximizeBox = false,
            MinimizeBox = false,
            RightToLeft = Localization.IsRtl ? RightToLeft.Yes : RightToLeft.No,
            RightToLeftLayout = true,
            ControlBox = false
        };

        var lblMessage = new Label
        {
            Text = Localization.Get("CheckingProgress"),
            Dock = DockStyle.Top,
            Height = 30,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = new Font("Tahoma", 9)
        };

        var progressBar = new ProgressBar
        {
            Dock = DockStyle.Bottom,
            Height = 25,
            Minimum = 0,
            Maximum = 100,
            Value = 0
        };

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
            progressForm.Close();

            // اگر آپدیتی نبود، پیام بده
            if (status == AutoUpdater.UpdateStatus.NoUpdate)
            {
                MessageBox.Show(
                    Localization.Format("UpdNoUpdate", AutoUpdater.GetCurrentVersionString()),
                    Localization.Get("Update"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else if (status == AutoUpdater.UpdateStatus.Error)
            {
                MessageBox.Show(
                    Localization.Get("UpdCheckErrorInternet"),
                    Localization.Get("Error"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
            // UpdateAvailable, DownloadedAndInstalling, UserDeclined - پیام‌های مربوطه در AutoUpdater نمایش داده می‌شوند
        }
        catch (Exception ex)
        {
            progressForm.Close();
            MessageBox.Show(
                Localization.Format("UpdCheckError", ex.Message),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            _settings.RunOnStartup = chkRunOnStartup.Checked;
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
}