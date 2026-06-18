using System.Diagnostics;

namespace WrongKeyboardFixer;

public class MainForm : Form
{
    private HotkeyManager? _hotkeyManager;
    private ClipboardManager _clipboardManager;
    private NotifyIcon? _trayIcon;
    private bool _isInitialized;

    public MainForm()
    {
        try
        {
            InitializeForm();
            InitializeComponents();
            _isInitialized = true;
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

        if (!_hotkeyManager.Register(HotkeyModifier.ControlAlt, Keys.Add))
        {
            MessageBox.Show(
                "ثبت Hotkey ناموفق بود. ممکن است کلید ترکیبی توسط برنامه دیگری گرفته شده باشد.",
                "اخطار",
                MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
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
        contextMenu.Items.Add("خروج", null, (_, _) =>
        {
            Application.Exit();
        });
        _trayIcon.ContextMenuStrip = contextMenu;
    }

    protected override void WndProc(ref Message message)
    {
        // بررسی null بودن قبل از استفاده
        if (_hotkeyManager != null && _hotkeyManager.HandleHotkeyMessage(ref message))
        {
            Debug.WriteLine("🔥 Hotkey detected!");
            _ = ProcessSelectedTextAsync();
        }

        base.WndProc(ref message);
    }

    private async Task ProcessSelectedTextAsync()
    {
        // اگر مقداردهی نشده، خارج شو
        if (_clipboardManager == null || !_isInitialized)
            return;

        string previousClipboard = _clipboardManager.GetText();

        try
        {
            Debug.WriteLine("🔄 Starting text conversion...");

            // Copy selected text
            KeyboardSimulator.SendCtrlC();
            await Task.Delay(300);

            // Get clipboard content
            string originalText = await _clipboardManager.GetTextWithRetryAsync();
            if (string.IsNullOrWhiteSpace(originalText))
            {
                _clipboardManager.RestoreText(previousClipboard);
                return;
            }

            // Convert text
            bool toPersian = KeyboardConverter.ShouldConvertToPersian(originalText);
            string convertedText = KeyboardConverter.Convert(originalText, toPersian);

            // Paste converted text
            _clipboardManager.SetText(convertedText);
            await Task.Delay(200);
            KeyboardSimulator.SendCtrlV();
            await Task.Delay(200);

            // Restore original clipboard
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