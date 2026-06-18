using System.Text;

namespace PersianKeyboardFix;

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
        ['?'] = '؟',
        //['&'] = '*',
        [')'] = '(',
        ['('] = ')',
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