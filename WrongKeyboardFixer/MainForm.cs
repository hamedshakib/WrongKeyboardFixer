using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Repositories;

namespace WrongKeyboardFixer;

public class MainForm : Form
{
    private readonly ISettingsRepository _settingsRepository;
    private HotkeyManager? _hotkeyManager;
    private ClipboardManager _clipboardManager;
    private AppSettings _settings;
    private NotifyIcon? _trayIcon;
    private bool _isInitialized;

    public MainForm(ISettingsRepository settingsRepository)
    {
        _settingsRepository = settingsRepository ?? throw new ArgumentNullException(nameof(settingsRepository));

        try
        {
            _settings = _settingsRepository.Load();

            InitializeForm();
            InitializeComponents();
            _isInitialized = true;

            ApplyStartupSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"خطا در راه‌اندازی برنامه: {ex.Message}",
                "خطا",
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
                    "ثبت Hotkey ناموفق بود. ممکن است کلید ترکیبی توسط برنامه دیگری گرفته شده باشد.",
                    "اخطار",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }

    private void InitializeForm()
    {
        Text = "Wrong Keyboard Fixer";
        WindowState = FormWindowState.Minimized;
        ShowInTaskbar = false;
        Visible = false;
        FormBorderStyle = FormBorderStyle.None;
        Size = new System.Drawing.Size(1, 1);
        Icon = Properties.Resources.WrongKeyboardFixerIcon;
        CreateTrayIcon();
    }

    private void CreateTrayIcon()
    {
        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "Wrong Keyboard Fixer"
        };

        var contextMenu = new ContextMenuStrip();

        // آیتم تنظیمات
        contextMenu.Items.Add("تنظیمات ⚙️", null, (_, _) => OpenSettings());
        contextMenu.Items.Add("-"); // جداکننده
        contextMenu.Items.Add("خروج ❌", null, (_, _) => Application.Exit());

        _trayIcon.ContextMenuStrip = contextMenu;
        _trayIcon.Icon = Properties.Resources.WrongKeyboardFixerIcon;
        // دابل کلیک برای باز کردن تنظیمات
        _trayIcon.DoubleClick += (_, _) => OpenSettings();
    }

    private void OpenSettings()
    {
        if (_hotkeyManager == null)
            return;

        using var settingsForm = new SettingsForm(_settings, _hotkeyManager, _settingsRepository);
        if (settingsForm.ShowDialog() == DialogResult.OK)
        {
            // بارگذاری مجدد تنظیمات
            _settings = _settingsRepository.Load();

            // ثبت مجدد کلید ترکیبی
            RegisterHotkeyFromSettings();
        }
    }

    private void ApplyStartupSettings()
    {
        // اجرای خودکار با ویندوز
        _settingsRepository.AddToStartup(_settings.RunOnStartup);
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
            string convertedText = KeyboardConverter.Convert(originalText, toPersian);

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
}