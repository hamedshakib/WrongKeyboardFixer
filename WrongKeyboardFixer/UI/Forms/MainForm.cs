using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Model;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.UI;

namespace WrongKeyboardFixer.UI.Forms;

public class MainForm : Form
{
    private HotkeyManager? _hotkeyManager;
    private ClipboardManager _clipboardManager = null!;
    private AppSettings _settings = null!;
    private NotifyIcon? _trayIcon;
    private ContextMenuStrip? _trayMenu;
    private bool _isInitialized;

    public MainForm()
    {
        try
        {
            _settings = SettingsManager.Load();

            // اعمال زبان ذخیره‌شده قبل از ایجاد هر کنترل
            Localization.SetLanguage(_settings.Language);

            InitializeForm();
            InitializeComponents();
            _isInitialized = true;

            ApplyStartupSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                Localization.Format("StartupError", ex.Message),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            Environment.Exit(1);
        }
    }

    private void InitializeComponents()
    {
        _clipboardManager = new ClipboardManager();
        _hotkeyManager = new HotkeyManager(this.Handle);

        // ثبت کلید ترکیبی از تنظیمات
        RegisterHotkeyFromSettings();

        CheckForUpdatesAsync();
        Task.Run(Theme.WarmUpFonts);
    }

    private void RegisterHotkeyFromSettings()
    {
        if (_hotkeyManager == null)
            return;

        uint modifier = (uint)_settings.HotkeyModifier;
        Keys key = _settings.HotkeyKey;

        if (!_hotkeyManager.Register(modifier, key))
        {
            // اگر ثبت ناموفق بود، با کلید پیش‌فرض امتحان کن
            if (!_hotkeyManager.Register(HotkeyModifier.ControlAlt, Keys.Add))
            {
                MessageBox.Show(
                    Localization.Get("HotkeyRegisterFailed"),
                    Localization.Get("Warning"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    private void InitializeForm()
    {
        Text = Localization.Get("TrayText");
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        Visible = false;
        FormBorderStyle = FormBorderStyle.None;
        Size = new System.Drawing.Size(1, 1);
        Icon = IconLoader.GetIcon();
        CreateTrayIcon();
    }

    private void CreateTrayIcon()
    {
        _trayMenu = BuildTrayMenu();

        _trayIcon = new NotifyIcon
        {
            Icon = IconLoader.GetIcon(),
            Visible = true,
            ContextMenuStrip = _trayMenu,
            Text = Localization.Get("TrayText")
        };

        // دابل کلیک برای باز کردن تنظیمات
        _trayIcon.DoubleClick += (_, _) => OpenSettings();
    }

    private ContextMenuStrip BuildTrayMenu()
    {
        var menu = new ContextMenuStrip
        {
            Renderer = Theme.CreateMenuRenderer(),
            Font = Theme.BodyFont,
            BackColor = Theme.Surface,
            ForeColor = Theme.TextPrimary,
            ShowImageMargin = true
        };

        var settingsItem = new ToolStripMenuItem(Localization.Get("TraySettings"));
        settingsItem.Image = CreateEmojiIcon("⚙️");
        settingsItem.Click += (_, _) => OpenSettings();
        menu.Items.Add(settingsItem);

        //menu.Items.Add(Localization.Get("TrayCheckUpdate"), null, (_, _) => _ = CheckForUpdatesManualAsync());

        menu.Items.Add("-"); // جداکننده

        var exitItem = new ToolStripMenuItem(Localization.Get("TrayExit"));
        exitItem.Image = CreateEmojiIcon("⏻");
        exitItem.Click += (_, _) => Application.Exit();
        menu.Items.Add(exitItem);

        return menu;
    }

    /// <summary>
    /// تبدیل یک ایموجی به تصویر برای استفاده به عنوان آیکون آیتم منو
    /// </summary>
    private static Bitmap CreateEmojiIcon(string emoji)
    {
        var bitmap = new Bitmap(16, 16);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.SmoothingMode = SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = TextRenderingHint.AntiAlias;

        using var font = new Font("Segoe UI Emoji", 11f, FontStyle.Regular, GraphicsUnit.Pixel);
        using var brush = new SolidBrush(Color.Black);

        var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center
        };

        var rect = new RectangleF(0, 0, 16, 16);
        graphics.DrawString(emoji, font, brush, rect, format);

        return bitmap;
    }

    private void OpenSettings()
    {
        if (_hotkeyManager == null)
            return;

        if (FormSingleton.ShowDialog(() => new SettingsForm(_settings, _hotkeyManager)) == DialogResult.OK)
        {
            // بارگذاری مجدد تنظیمات
            _settings = SettingsManager.Load();

            // اعمال زبان جدید و بازسازی منوی tray
            Localization.SetLanguage(_settings.Language);
            RefreshTrayMenu();

            // ثبت مجدد کلید ترکیبی
            RegisterHotkeyFromSettings();
        }
    }

    private void RefreshTrayMenu()
    {
        if (_trayMenu == null || _trayIcon == null)
            return;

        _trayMenu.Items.Clear();
        _trayMenu.Items.AddRange(BuildTrayMenu().Items);
        _trayIcon.Text = Localization.Get("TrayText");
    }

    private void ApplyStartupSettings()
    {
        // اجرای خودکار با ویندوز
        SettingsManager.AddToStartup(_settings.RunOnStartup);
    }

    protected override void WndProc(ref Message message)
    {
        if (_hotkeyManager != null && _hotkeyManager.HandleHotkeyMessage(ref message))
        {
            Debug.WriteLine("🔥 Hotkey detected!");
            _ = ProcessSelectedTextAsync();
        }

        base.WndProc(ref message);
    }

    private async Task ProcessSelectedTextAsync()
    {
        if (_clipboardManager == null || !_isInitialized)
            return;

        string previousClipboard = _clipboardManager.GetText();

        try
        {
            Debug.WriteLine("🔄 Starting text conversion...");

            KeyboardSimulator.SendCtrlC();
            await Task.Delay(300);

            string originalText = await _clipboardManager.GetTextWithRetryAsync();
            if (string.IsNullOrWhiteSpace(originalText))
            {
                _clipboardManager.RestoreText(previousClipboard);
                return;
            }

            bool toPersian = KeyboardConverter.ShouldConvertToPersian(originalText);

            string convertedText;
            if (toPersian)
            {
                // Convert English to Persian
                convertedText = KeyboardConverter.ConvertEnglishToPersian(originalText, _settings.EnglishToPersianMap);
            }
            else
            {
                // Convert Persian to English
                convertedText = KeyboardConverter.ConvertPersianToEnglish(originalText, _settings.PersianToEnglishMap);
            }

            _clipboardManager.SetText(convertedText);
            await Task.Delay(200);
            KeyboardSimulator.SendCtrlV();
            await Task.Delay(200);

            _clipboardManager.RestoreText(previousClipboard);
            Debug.WriteLine("✅ Conversion completed successfully");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Error: {ex.Message}");
            _clipboardManager?.RestoreText(previousClipboard);
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        _hotkeyManager?.Unregister();
        _hotkeyManager?.Dispose();
        _trayIcon?.Dispose();
        base.OnFormClosing(e);
    }

    /// <summary>
    /// بررسی خودکار بروزرسانی در استارتاپ (silent - بدون نمایش پیام "بروزرسانی موجود نیست")
    /// </summary>
    private async void CheckForUpdatesAsync()
    {
        try
        {
            await AutoUpdater.CheckForUpdatesAsync(silent: true);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"❌ Update check error: {ex.Message}");
        }
    }

    /// <summary>
    /// بررسی دستی بروزرسانی با نمایش Progress Dialog
    /// </summary>
    private async Task CheckForUpdatesManualAsync()
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
            ControlBox = false,
            AutoScaleDimensions = new SizeF(96F, 96F),
            AutoScaleMode = AutoScaleMode.Dpi
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
        progressForm.Show();

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
            Debug.WriteLine($"❌ Update check error: {ex.Message}");
            MessageBox.Show(
                Localization.Format("UpdCheckError", ex.Message),
                Localization.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
