using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using WrongKeyboardFixer.Core.Helpers;
using WrongKeyboardFixer.Core.Models;
using WrongKeyboardFixer.Core.Services;
using WrongKeyboardFixer.Core.Services.Conversion;
using WrongKeyboardFixer.UI.Components;

namespace WrongKeyboardFixer.UI.Forms;

/// <summary>
/// Main application form (tray-only application).
/// Manages hotkey registration, clipboard monitoring, and update checking.
/// </summary>
public class MainForm : Form
{
    private HotkeyManager? _hotkeyManager;
    private ClipboardManager _clipboardManager = null!;
    private TextConversionService? _textConversionService;
    private AppSettings _settings = null!;
    private TrayIconManager? _trayIcon;
    private ILogger _logger = null!;
    private bool _isInitialized;

    public MainForm(ILogger? logger = null)
    {
        try
        {
            _logger = logger ?? new ConsoleLogger();
            _settings = SettingsManager.Load();

            // اعمال زبان ذخیره‌شده قبل از ایجاد هر کنترل
            Localization.SetLanguage(_settings.Language);

            InitializeForm();
            InitializeComponents();
            _isInitialized = true;

            ApplyStartupSettings();
            _logger.Info("MainForm initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.Error("Failed to initialize MainForm", ex);
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
        _clipboardManager = new ClipboardManager(_logger);
        _textConversionService = new TextConversionService(_clipboardManager, _settings, null, _logger);
        _hotkeyManager = new HotkeyManager(this.Handle);

        // ثبت کلید ترکیبی از تنظیمات
        RegisterHotkeyFromSettings();

        _ = CheckForUpdatesAsync();
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
            if (!_hotkeyManager.Register((uint)(HotkeyModifiers.Control | HotkeyModifiers.Alt), Keys.Add))
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
        Size = new Size(1, 1);
        Icon = IconLoader.GetIcon();

        _trayIcon = new TrayIconManager();
        _trayIcon.SettingsRequested += (_, _) => OpenSettings();
        _trayIcon.ExitRequested += (_, _) => Application.Exit();
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
            _trayIcon?.RebuildMenu();

            // ثبت مجدد کلید ترکیبی
            RegisterHotkeyFromSettings();
        }
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
            _ = ConvertSelectedTextAsync();
        }

        base.WndProc(ref message);
    }

    private async Task ConvertSelectedTextAsync()
    {
        if (_textConversionService == null || !_isInitialized)
            return;

        await _textConversionService.ConvertSelectedTextAsync();
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
    private async Task CheckForUpdatesAsync()
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
}
