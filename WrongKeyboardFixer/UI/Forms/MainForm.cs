using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.Model;
using WrongKeyboardFixer.Core.Helpers;

namespace WrongKeyboardFixer.UI.Forms;

public class MainForm : Form
{
    private HotkeyManager? _hotkeyManager;
    private ClipboardManager _clipboardManager = null!;
    private WrongKeyboardFixer.Core.Model.AppSettings _settings = null!;
    private NotifyIcon? _trayIcon;
    private ContextMenuStrip? _trayMenu;
    private bool _isInitialized; // Used for potential future initialization checks

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
    }

    private ContextMenuStrip BuildTrayMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add(Localization.Get("TraySettings"), null, (s, e) => ShowSettings());
        menu.Items.Add(Localization.Get("TrayCheckUpdate"), null, (s, e) => CheckForUpdatesAsync());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(Localization.Get("TrayExit"), null, (s, e) => Application.Exit());
        return menu;
    }

    private void ShowSettings()
    {
        if (_settings == null) return;
        
        var settingsForm = new SettingsForm(_settings, _hotkeyManager!);
        settingsForm.ShowDialog();
    }

    private async void CheckForUpdatesAsync()
    {
        await AutoUpdater.CheckForUpdatesAsync();
    }

    private void ApplyStartupSettings()
    {
        if (_settings.RunOnStartup)
        {
            SettingsManager.AddToStartup(true);
        }
        else
        {
            SettingsManager.AddToStartup(false);
        }
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);

        if (_hotkeyManager?.HandleHotkeyMessage(ref m) == true)
        {
            ToggleClipboardConversion();
        }
    }

    private void ToggleClipboardConversion()
    {
        try
        {
            // ارسال Ctrl+C برای کپی متن انتخاب شده
            KeyboardSimulator.SendCtrlC();
            Thread.Sleep(100);

            // دریافت متن کلیپ‌بورد
            string originalText = _clipboardManager.GetText();

            if (string.IsNullOrWhiteSpace(originalText))
                return;

            // تشخیص جهت تبدیل و تبدیل متن
            string convertedText;
            if (KeyboardConverter.ShouldConvertToPersian(originalText))
            {
                convertedText = KeyboardConverter.ConvertEnglishToPersian(originalText);
            }
            else
            {
                convertedText = KeyboardConverter.ConvertPersianToEnglish(originalText);
            }

            // بازگرداندن متن تبدیل شده به کلیپ‌بورد
            _clipboardManager.RestoreText(convertedText);

            Debug.WriteLine($"Converted: {originalText} -> {convertedText}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error in clipboard conversion: {ex.Message}");
        }
    }
}