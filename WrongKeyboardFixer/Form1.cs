using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PersianKeyboardFix
{
    public partial class Form1 : Form
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 1;
        private NotifyIcon? _tray;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);


        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, UIntPtr dwExtraInfo);

        private const int KEYEVENTF_KEYUP = 0x0002;
        private const int KEYEVENTF_KEYDOWN = 0x0000;
        private const byte VK_CONTROL = 0x11;
        private const byte VK_C = 0x43;
        private const byte VK_V = 0x56;
        private const byte VK_MENU = 0x12; // Alt

        public Form1()
        {
            InitializeFormManually();

            bool success = RegisterHotKey(this.Handle, HOTKEY_ID,
                0x0002 | 0x0001,      // Ctrl + Alt
                (uint)Keys.Add);

            if (!success)
            {
                MessageBox.Show("ثبت Hotkey ناموفق بود. ممکن است hotkey قبلاً توسط برنامه دیگری گرفته شده باشد.",
                    "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeFormManually()
        {
            this.Text = "Wrong Keyboard Fixer";
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new System.Drawing.Size(1, 1);

            CreateTray();
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
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                Debug.WriteLine("Hotkey detected!");
                _ = ProcessSelectedTextAsync();
            }
            base.WndProc(ref m);
        }

        private async Task ProcessSelectedTextAsync()
        {
            string previousClipboard = "";

            try
            {
                Debug.WriteLine("🔄 شروع فرآیند...");

                // ۱. ذخیره متن قبلی
                try
                {
                    previousClipboard = Clipboard.GetText(TextDataFormat.UnicodeText) ?? "";
                }
                catch { }

                // ۲. کپی
                SendCtrlC();
                await Task.Delay(250);

                // ۳. دریافت متن
                string original = "";
                for (int i = 0; i < 8; i++)
                {
                    await Task.Delay(150);
                    try
                    {
                        original = Clipboard.GetText(TextDataFormat.UnicodeText) ?? "";
                        if (!string.IsNullOrWhiteSpace(original)) break;
                    }
                    catch { }
                }

                if (string.IsNullOrWhiteSpace(original))
                {
                    // بازیابی
                    if (!string.IsNullOrWhiteSpace(previousClipboard))
                        Clipboard.SetText(previousClipboard);
                    return;
                }

                // ۴. تبدیل
                bool toPersian = KeyboardConverter.ShouldConvertToPersian(original);
                string converted = KeyboardConverter.Convert(original, toPersian);

                // ۵. قرار دادن در کلیپ‌بورد
                Clipboard.SetText(converted);
                await Task.Delay(200);

                // ۶. Paste
                SendCtrlV();
                await Task.Delay(200);

                // ۷. بازیابی متن قبلی
                if (!string.IsNullOrWhiteSpace(previousClipboard))
                {
                    Clipboard.SetText(previousClipboard);
                    Debug.WriteLine("✅ متن قبلی بازیابی شد");
                }
                else
                {
                    // پاک کردن بدون PowerShell
                    Clipboard.Clear();
                    var empty = new DataObject();
                    empty.SetData(DataFormats.Text, "");
                    Clipboard.SetDataObject(empty, true);
                    Debug.WriteLine("🧹 کلیپ‌بورد پاک شد");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Error: {ex.Message}");
                try
                {
                    if (!string.IsNullOrWhiteSpace(previousClipboard))
                        Clipboard.SetText(previousClipboard);
                }
                catch { }
            }
        }

        private void SendCtrlC()
        {
            // آزاد کردن کلیدهای Alt و Ctrl (چون ممکن است هنوز فشرده باشند)
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            // کمی صبر
            System.Threading.Thread.Sleep(50);

            // ارسال Ctrl+C
            keybd_event(VK_CONTROL, 0, 0x0000, UIntPtr.Zero); // Ctrl Down
            keybd_event(VK_C, 0, 0x0000, UIntPtr.Zero);       // C Down
            keybd_event(VK_C, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // C Up
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero); // Ctrl Up
        }

        private void SendCtrlV()
        {
            // 1. آزاد کردن کلیدهای Alt و Ctrl (مهم!)
            keybd_event(VK_MENU, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);

            // 2. صبر کوتاه
            System.Threading.Thread.Sleep(30);

            // 3. ارسال Ctrl+V
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_V, 0, KEYEVENTF_KEYDOWN, UIntPtr.Zero);
            keybd_event(VK_V, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
            keybd_event(VK_CONTROL, 0, KEYEVENTF_KEYUP, UIntPtr.Zero);
        }

        private void ClearClipboardSecurely()
        {
            try
            {
                Clipboard.Clear();
                var empty = new DataObject();
                empty.SetData(DataFormats.Text, "");
                Clipboard.SetDataObject(empty, true, 3, 100);
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
            _tray?.Dispose();
            base.OnFormClosing(e);
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}