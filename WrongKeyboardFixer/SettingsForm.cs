using System;
using System.Windows.Forms;

namespace WrongKeyboardFixer;

public partial class SettingsForm : Form
{
    private readonly AppSettings _settings;
    private readonly HotkeyManager _hotkeyManager;
    private bool _isHotkeyRegistered;

    // کنترل‌های فرم
    private CheckBox chkRunOnStartup;
    private CheckBox chkStartMinimized;
    private CheckBox chkShowNotifications;
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

        // تنظیمات راست‌چین
        this.RightToLeft = RightToLeft.Yes;
        this.RightToLeftLayout = true;

        InitializeControls();
        LoadSettings();
    }

    private void InitializeControls()
    {
        this.Text = "تنظیمات";
        this.Size = new System.Drawing.Size(450, 400);
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.StartPosition = FormStartPosition.CenterParent;
        this.Font = new System.Drawing.Font("Tahoma", 9);

        // ایجاد پنل اصلی
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(15),
            ColumnCount = 2,
            RowCount = 8,
            AutoSize = true
        };

        // تنظیم ستون‌ها
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        // تنظیم سطرها
        for (int i = 0; i < 8; i++)
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 35));

        int row = 0;

        // عنوان
        var titleLabel = new Label
        {
            Text = "تنظیمات برنامه",
            Font = new System.Drawing.Font("Tahoma", 12, System.Drawing.FontStyle.Bold),
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        panel.Controls.Add(titleLabel, 0, row);
        panel.SetColumnSpan(titleLabel, 2);
        row++;

        // اجرای خودکار
        chkRunOnStartup = new CheckBox
        {
            Text = "اجرای خودکار با ویندوز",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        panel.Controls.Add(chkRunOnStartup, 0, row);
        panel.SetColumnSpan(chkRunOnStartup, 2);
        row++;

        // شروع با مینیمم
        chkStartMinimized = new CheckBox
        {
            Text = "شروع با حالت مینیمم (سینی سیستم)",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        panel.Controls.Add(chkStartMinimized, 0, row);
        panel.SetColumnSpan(chkStartMinimized, 2);
        row++;

        // نمایش نوتیفیکیشن
        chkShowNotifications = new CheckBox
        {
            Text = "نمایش پیام‌های اطلاع‌رسانی",
            AutoSize = true,
            Anchor = AnchorStyles.Left
        };
        panel.Controls.Add(chkShowNotifications, 0, row);
        panel.SetColumnSpan(chkShowNotifications, 2);
        row++;

        // کلید ترکیبی
        var lblHotkey = new Label
        {
            Text = "کلید ترکیبی:",
            AutoSize = true,
            Anchor = AnchorStyles.Right
        };
        panel.Controls.Add(lblHotkey, 0, row);

        var hotkeyPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true
        };

        cmbHotkeyModifier = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 100
        };
        cmbHotkeyModifier.Items.AddRange(new object[]
        {
            "Ctrl + Alt",
            "Ctrl + Shift",
            "Alt + Shift",
            "Ctrl",
            "Alt",
            "Shift"
        });
        cmbHotkeyModifier.SelectedIndex = 0;

        cmbHotkeyKey = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Width = 80
        };
        cmbHotkeyKey.Items.AddRange(new object[]
        {
            "Add (+)",
            "F1", "F2", "F3", "F4", "F5", "F6",
            "F7", "F8", "F9", "F10", "F11", "F12",
            "Insert", "Home", "PageUp", "PageDown",
            "End", "Delete", "Space"
        });
        cmbHotkeyKey.SelectedIndex = 0;

        hotkeyPanel.Controls.Add(cmbHotkeyModifier);
        hotkeyPanel.Controls.Add(new Label { Text = "+", AutoSize = true });
        hotkeyPanel.Controls.Add(cmbHotkeyKey);

        panel.Controls.Add(hotkeyPanel, 1, row);
        row++;

        // وضعیت ثبت کلید
        lblStatus = new Label
        {
            Text = "وضعیت: بررسی...",
            AutoSize = true,
            ForeColor = System.Drawing.Color.Blue,
            Anchor = AnchorStyles.Left
        };
        panel.Controls.Add(lblStatus, 0, row);
        panel.SetColumnSpan(lblStatus, 2);
        row++;

        // دکمه ثبت کلید
        btnRegisterHotkey = new Button
        {
            Text = "ثبت کلید ترکیبی",
            Size = new System.Drawing.Size(150, 30),
            Anchor = AnchorStyles.Left,
            BackColor = System.Drawing.Color.LightGreen,
            FlatStyle = FlatStyle.Flat
        };
        btnRegisterHotkey.Click += BtnRegisterHotkey_Click;
        panel.Controls.Add(btnRegisterHotkey, 0, row);
        panel.SetColumnSpan(btnRegisterHotkey, 2);
        row++;

        // دکمه‌های ذخیره و انصراف
        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };

        btnSave = new Button
        {
            Text = "ذخیره",
            Size = new System.Drawing.Size(100, 30),
            BackColor = System.Drawing.Color.LightBlue,
            FlatStyle = FlatStyle.Flat
        };
        btnSave.Click += BtnSave_Click;

        btnCancel = new Button
        {
            Text = "انصراف",
            Size = new System.Drawing.Size(100, 30),
            FlatStyle = FlatStyle.Flat
        };
        btnCancel.Click += (_, _) => DialogResult = DialogResult.Cancel;

        buttonPanel.Controls.Add(btnSave);
        buttonPanel.Controls.Add(btnCancel);
        panel.Controls.Add(buttonPanel, 0, row);
        panel.SetColumnSpan(buttonPanel, 2);

        this.Controls.Add(panel);
    }

    private void LoadSettings()
    {
        chkRunOnStartup.Checked = _settings.RunOnStartup;
        chkStartMinimized.Checked = _settings.StartMinimized;
        chkShowNotifications.Checked = _settings.ShowNotifications;

        // بارگذاری کلید ترکیبی
        LoadHotkeyFromSettings();

        UpdateStatus();
    }

    private void LoadHotkeyFromSettings()
    {
        // تنظیم modifier
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

        // تنظیم کلید
        var key = _settings.HotkeyKey;
        string keyName = key.ToString();

        // تبدیل نام کلید به فرمت نمایشی
        for (int i = 0; i < cmbHotkeyKey.Items.Count; i++)
        {
            string item = cmbHotkeyKey.Items[i].ToString()!;
            if (item.StartsWith(keyName) || item.Contains(keyName))
            {
                cmbHotkeyKey.SelectedIndex = i;
                break;
            }
        }
    }

    private void BtnRegisterHotkey_Click(object? sender, EventArgs e)
    {
        try
        {
            var (modifier, key) = GetSelectedHotkey();

            // لغو ثبت قبلی
            _hotkeyManager.Unregister();

            // ثبت جدید
            bool success = _hotkeyManager.Register(modifier, key);

            if (success)
            {
                _isHotkeyRegistered = true;
                lblStatus.Text = "✅ کلید ترکیبی با موفقیت ثبت شد";
                lblStatus.ForeColor = System.Drawing.Color.Green;

                // به‌روزرسانی تنظیمات
                _settings.HotkeyModifier = (int)modifier;
                _settings.HotkeyKey = key;

                MessageBox.Show(
                    "کلید ترکیبی جدید با موفقیت ثبت شد.",
                    "موفقیت",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            else
            {
                lblStatus.Text = "❌ ثبت کلید ترکیبی ناموفق بود. کلید قبلاً ثبت شده است.";
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
            // ذخیره تنظیمات
            _settings.RunOnStartup = chkRunOnStartup.Checked;
            _settings.StartMinimized = chkStartMinimized.Checked;
            _settings.ShowNotifications = chkShowNotifications.Checked;

            // ذخیره کلید ترکیبی
            if (!_isHotkeyRegistered)
            {
                var (modifier, key) = GetSelectedHotkey();
                _settings.HotkeyModifier = (int)modifier;
                _settings.HotkeyKey = key;
            }

            // اعمال اجرای خودکار
            SettingsManager.AddToStartup(_settings.RunOnStartup);

            // ذخیره در فایل
            SettingsManager.Save(_settings);

            DialogResult = DialogResult.OK;
            Close();

            MessageBox.Show(
                "تنظیمات با موفقیت ذخیره شد.",
                "موفقیت",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطا در ذخیره تنظیمات: {ex.Message}",
                "خطا",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
    }
}