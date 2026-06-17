using System.Windows.Forms;

namespace PersianKeyboardFixer;

public partial class MainForm : Form
{
    private NotifyIcon? _tray;

    public MainForm()
    {
        InitializeComponent();

        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        Visible = false;

        CreateTray();

        GlobalHotKey.RegisterHotKey(
            Handle,
            1,
            GlobalHotKey.MOD_CONTROL |
            GlobalHotKey.MOD_SHIFT,
            (uint)Keys.Space);
    }

    private void CreateTray()
    {
        _tray = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Visible = true,
            Text = "Wrong Keyboard Fixer"
        };

        var menu = new ContextMenuStrip();

        menu.Items.Add("Exit", null, (_, _) =>
        {
            Application.Exit();
        });

        _tray.ContextMenuStrip = menu;
    }

    protected override void WndProc(ref Message m)
    {
        if (m.Msg == GlobalHotKey.WM_HOTKEY)
        {
            _ = ConvertSelectedText();
        }

        base.WndProc(ref m);
    }

    private async Task ConvertSelectedText()
    {
        IDataObject? backup = Clipboard.GetDataObject();

        try
        {
            SendKeys.SendWait("^c");

            await Task.Delay(100);

            string text = Clipboard.GetText();

            if (string.IsNullOrWhiteSpace(text))
                return;

            string converted =
                KeyboardLayoutConverter.Convert(text);

            Clipboard.SetText(converted);

            SendKeys.SendWait("^v");

            await Task.Delay(100);
        }
        finally
        {
            if (backup != null)
                Clipboard.SetDataObject(backup);
        }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        GlobalHotKey.UnregisterHotKey(Handle, 1);

        if (_tray != null)
        {
            _tray.Visible = false;
            _tray.Dispose();
        }

        base.OnFormClosed(e);
    }
}