using System.Text;

namespace PersianKeyboardFixer;

public static class KeyboardLayoutConverter
{
    private static readonly Dictionary<char, char> EnToFa = new()
    {
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
        ['m'] = 'پ',
        [','] = 'و'
    };

    private static readonly Dictionary<char, char> FaToEn =
        EnToFa.ToDictionary(x => x.Value, x => x.Key);

    public static string Convert(string text)
    {
        int persian = text.Count(IsPersian);
        int english = text.Count(IsEnglish);

        if (persian > english)
            return ConvertFaToEn(text);

        return ConvertEnToFa(text);
    }

    private static string ConvertEnToFa(string text)
    {
        var sb = new StringBuilder();

        foreach (var c in text)
        {
            char lower = char.ToLowerInvariant(c);

            if (EnToFa.TryGetValue(lower, out var fa))
                sb.Append(fa);
            else
                sb.Append(c);
        }

        return sb.ToString();
    }

    private static string ConvertFaToEn(string text)
    {
        var sb = new StringBuilder();

        foreach (var c in text)
        {
            if (FaToEn.TryGetValue(c, out var en))
                sb.Append(en);
            else
                sb.Append(c);
        }

        return sb.ToString();
    }

    private static bool IsPersian(char c)
    {
        return c >= 0x0600 && c <= 0x06FF;
    }

    private static bool IsEnglish(char c)
    {
        return char.IsLetter(c) && c <= 127;
    }
}