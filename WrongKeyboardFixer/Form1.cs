using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace PersianKeyboardFix
{
    public partial class Form1 : Form
    {
        private const int WM_HOTKEY = 0x0312;
        private const int HOTKEY_ID = 1;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public Form1()
        {
            InitializeFormManually();

            // ثبت Hotkey: Ctrl + Shift + F
            bool success = RegisterHotKey(this.Handle, HOTKEY_ID,
                0x0002 | 0x0004, // MOD_CONTROL | MOD_SHIFT
                (uint)Keys.F);

            if (!success)
            {
                MessageBox.Show("ثبت Hotkey ناموفق بود. ممکن است hotkey قبلاً توسط برنامه دیگری گرفته شده باشد.", "خطا", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitializeFormManually()
        {
            this.Text = "Persian Keyboard Converter";
            this.WindowState = FormWindowState.Minimized;
            this.ShowInTaskbar = false;
            this.Visible = false;
            this.FormBorderStyle = FormBorderStyle.None;
            this.Size = new System.Drawing.Size(1, 1);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_HOTKEY && m.WParam.ToInt32() == HOTKEY_ID)
            {
                _ = ProcessSelectedTextAsync();
            }
            base.WndProc(ref m);
        }

        private async Task ProcessSelectedTextAsync()
        {
            try
            {
                // ۱. کپی متن انتخاب شده
                SendKeys.SendWait("^c");
                await Task.Delay(200);

                string original = Clipboard.GetText(TextDataFormat.UnicodeText) ?? "";
                if (string.IsNullOrWhiteSpace(original)) return;

                // ۲. تشخیص جهت تبدیل
                bool toPersian = KeyboardConverter.ShouldConvertToPersian(original);
                string converted = KeyboardConverter.Convert(original, toPersian);

                // ۳. قرار دادن متن جدید در کلیپ‌بورد
                Clipboard.SetText(converted);

                // ۴. Paste کردن
                await Task.Delay(100);
                SendKeys.SendWait("^v");

                // ۵. پاک کردن امن کلیپ‌بورد
                ClearClipboardSecurely();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error: " + ex.Message);
            }
        }

        private void ClearClipboardSecurely()
        {
            try
            {
                Clipboard.Clear();

                // روش قوی‌تر برای پاک کردن تاریخچه کلیپ‌بورد
                var empty = new DataObject();
                empty.SetData(DataFormats.Text, "");
                Clipboard.SetDataObject(empty, true, 3, 100);
            }
            catch { }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_ID);
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

    // ==================== کلاس تبدیل متن ====================
    public static class KeyboardConverter
    {
        private static readonly Dictionary<char, char> EnToFa = new()
        {
            ['`'] = '‍',
            ['1'] = '۱',
            ['2'] = '۲',
            ['3'] = '۳',
            ['4'] = '۴',
            ['5'] = '۵',
            ['6'] = '۶',
            ['7'] = '۷',
            ['8'] = '۸',
            ['9'] = '۹',
            ['0'] = '۰',
            ['q'] = 'ض',
            ['w'] = 'ص',
            ['e'] = 'ث',
            ['r'] = 'ق',
            ['t'] = 'ف',
            ['y'] = 'غ',
            ['u'] = 'ع',
            ['i'] = 'ه',
            ['o'] = 'خ',
            ['p'] = 'ح',
            ['['] = 'ج',
            [']'] = 'چ',
            ['\\'] = 'پ',
            ['a'] = 'ش',
            ['s'] = 'س',
            ['d'] = 'ی',
            ['f'] = 'ب',
            ['g'] = 'ل',
            ['h'] = 'ا',
            ['j'] = 'ت',
            ['k'] = 'ن',
            ['l'] = 'م',
            [';'] = 'ک',
            ['\''] = 'گ',
            ['z'] = 'ظ',
            ['x'] = 'ط',
            ['c'] = 'ز',
            ['v'] = 'ر',
            ['b'] = 'ذ',
            ['n'] = 'د',
            ['m'] = 'ئ',
            [','] = 'و',
            ['.'] = '.',
            ['/'] = '/',
            // علائم Shift
            ['!'] = '!',
            ['@'] = '٬',
            //['#'] = '٫',
            //['$'] = '﷼',
            ['%'] = '٪',
            ['^'] = '×',
            //['&'] = '*',
            //['*'] = '(',
            //['('] = ')',
            //['_'] = 'ـ',
        };

        private static readonly Dictionary<char, char> FaToEn = new();

        static KeyboardConverter()
        {
            foreach (var pair in EnToFa)
                if (!FaToEn.ContainsKey(pair.Value))
                    FaToEn[pair.Value] = pair.Key;
        }

        public static string Convert(string text, bool toPersian)
        {
            if (string.IsNullOrEmpty(text)) return text;

            var map = toPersian ? EnToFa : FaToEn;
            var sb = new StringBuilder(text.Length);

            foreach (char c in text)
            {
                char lower = char.ToLowerInvariant(c);
                if (map.TryGetValue(lower, out char mapped))
                {
                    char result = mapped;
                    if (!toPersian && char.IsUpper(c))
                        result = char.ToUpperInvariant(result);
                    sb.Append(result);
                }
                else
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static bool ShouldConvertToPersian(string text)
        {
            if (string.IsNullOrEmpty(text)) return false;

            int persianCount = 0;
            foreach (char c in text)
            {
                if (c >= 0x0600 && c <= 0x06FF) persianCount++;
            }

            return persianCount < text.Length * 0.35; // بیشتر انگلیسی → تبدیل به فارسی
        }
    }
}