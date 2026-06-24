using System;
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
    private CheckBox chkRunOnStartup;
    //private CheckBox chkShowNotifications;
    private ComboBox cmbHotkeyModifier;
    private ComboBox cmbHotkeyKey;
    private Button btnSave;
    private Button btnCancel;
    private Button btnRegisterHotkey;
    private Label lblStatus;

    public SettingsForm(AppSettings settings, HotkeyManager hotkeyManager)
    {
        _settings = settings ?? new AppSettings();
        _hotkeyManager = hotkeyManager;

        // تنظیمات پایه راست‌چین ویندوز
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        InitializeControls();
        LoadSettings();
    }

    private void InitializeControls()
    {
        this.Text = "تنظیمات";
        this.Size = new System.Drawing.Size(460, 330);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Font = new System.Drawing.Font("Tahoma", 9);
        this.Icon = Properties.Resources.WrongKeyboardFixerIcon;

        int marginX = 25; // فاصله استاندارد از لبه‌های چپ و راست فرم
        int currentY = 20; // موقعیت عمودی شروع
        int formWidth = this.ClientSize.Width;
        int controlWidth = formWidth - (2 * marginX); // عرض مفید برای کنترل‌های سرتاسری

        // ۱. عنوان اصلی
        var titleLabel = new Label
        {
            Text = "تنظیمات برنامه",
            Font = new System.Drawing.Font("Tahoma", 11, System.Drawing.FontStyle.Bold),
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 30),
            TextAlign = ContentAlignment.MiddleLeft
        };
        this.Controls.Add(titleLabel);
        currentY += 40;

        // ۲. اجرای خودکار با ویندوز
        chkRunOnStartup = new CheckBox
        {
            Text = "اجرای خودکار با ویندوز",
            CheckAlign = ContentAlignment.MiddleLeft,
            TextAlign = ContentAlignment.MiddleLeft,
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 25)
        };
        this.Controls.Add(chkRunOnStartup);
        currentY += 32;

        // ۵. کادر میانبر (GroupBox)
        var grpHotkey = new GroupBox
        {
            Text = "تنظیمات کلید میانبر",
            Location = new Point(marginX, currentY),
            Size = new Size(controlWidth, 130),
            RightToLeft = RightToLeft.Yes
        };

        // المان‌های داخل کادر میانبر با موقعیت‌دهی محلی (نسبت به لبه‌های GroupBox)
        var lblHotkey = new Label
        {
            Text = "کلید ترکیبی:",
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
        cmbHotkeyKey.Items.AddRange(new object[] { "Add (+)", "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12", "Insert", "Home", "PageUp", "PageDown", "End", "Delete", "Space" });
        cmbHotkeyKey.SelectedIndex = 0;
        cmbHotkeyKey.SelectedIndexChanged += CmbHotkeyKey_SelectedIndexChanged;
        grpHotkey.Controls.Add(cmbHotkeyKey);

        // دکمه اعمال کلید ترکیبی
        btnRegisterHotkey = new Button
        {
            Text = "اعمال کلید ترکیبی",
            Location = new Point(grpHotkey.Width - 145, 80),
            Size = new Size(130, 30),
            BackColor = System.Drawing.Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        btnRegisterHotkey.Click += BtnRegisterHotkey_Click;
        btnRegisterHotkey.Enabled = false;
        grpHotkey.Controls.Add(btnRegisterHotkey);

        // برچسب وضعیت ثبت
        lblStatus = new Label
        {
            Text = "وضعیت: بررسی...",
            Location = new Point(15, 85),
            Size = new Size(grpHotkey.Width - 170, 20),
            ForeColor = System.Drawing.Color.Blue,
            TextAlign = ContentAlignment.MiddleLeft // وضعیت در سمت چپ دکمه ثبت قرار بگیرد
        };
        grpHotkey.Controls.Add(lblStatus);

        this.Controls.Add(grpHotkey);
        currentY += grpHotkey.Height + 25;

        int buttonY = currentY;
        int buttonWidth = 95;

        // دکمه ذخیره (حالا این دکمه کاملاً به لبه چپ فرم می‌چسبد)
        btnSave = new Button
        {
            Text = "ذخیره",
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
            Text = "انصراف",
            Location = new Point(this.ClientSize.Width - marginX - (buttonWidth * 2) - 10, buttonY),
            Size = new Size(buttonWidth, 32),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;
        this.Controls.Add(btnCancel);

    }

    private void CmbHotkeyModifier_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var hotkey = GetSelectedHotkey();
        if (lastHotkeyKey != hotkey.key || lastHotkeyModifier != hotkey.modifier)
            btnRegisterHotkey.Enabled = true;
        else
            btnRegisterHotkey.Enabled = false;
    }

    private void CmbHotkeyKey_SelectedIndexChanged(object? sender, EventArgs e)
    {
        var hotkey = GetSelectedHotkey();
        if (lastHotkeyKey != hotkey.key || lastHotkeyModifier != hotkey.modifier)
            btnRegisterHotkey.Enabled = true;
        else
            btnRegisterHotkey.Enabled = false;
    }

    private void LoadSettings()
    {
        chkRunOnStartup.Checked = _settings.RunOnStartup;

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
            string item = cmbHotkeyKey.Items[i].ToString()!;
            if (item.StartsWith(keyName) || item.Contains(keyName))
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
                lblStatus.Text = "✅ کلید ترکیبی با موفقیت ثبت شد";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                _settings.HotkeyModifier = (int)modifier;
                _settings.HotkeyKey = key;
                btnRegisterHotkey.Enabled = false;
                MessageBox.Show("کلید ترکیبی جدید با موفقیت ثبت شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                lblStatus.Text = "❌ ثبت ناموفق بود. کلید قبلاً ثبت شده است.";
                lblStatus.ForeColor = System.Drawing.Color.Red;
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
        if (_isHotkeyRegistered)
        {
            lblStatus.Text = "✅ کلید ترکیبی ثبت شده است";
            lblStatus.ForeColor = System.Drawing.Color.Green;
        }
        else
        {
            lblStatus.Text = "⚠️ کلید ترکیبی ثبت نشده است";
            lblStatus.ForeColor = System.Drawing.Color.Orange;
        }
    }

    private void BtnSave_Click(object? sender, EventArgs e)
    {
        try
        {
            _settings.RunOnStartup = chkRunOnStartup.Checked;

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

            MessageBox.Show("تنظیمات با موفقیت ذخیره شد.", "موفقیت", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"خطا در ذخیره تنظیمات: {ex.Message}", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}