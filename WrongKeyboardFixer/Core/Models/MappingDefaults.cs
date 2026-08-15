using System.Collections.Generic;

namespace WrongKeyboardFixer.Core.Models;

/// <summary>
/// Provides the default Persian &lt;-&gt; English character mappings.
/// Each method returns a fresh dictionary so callers may mutate it freely.
/// </summary>
public static partial class MappingDefaults
{
    /// <summary>Returns a fresh copy of the default English → Persian mapping.</summary>
    public static Dictionary<char, char> GetDefaultEnglishToPersianMap()
    {
        return new Dictionary<char, char>
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
            ['\u2019'] = 'گ',
            ['\u2018'] = 'گ',

            ['z'] = 'ظ',
            ['x'] = 'ط',
            ['c'] = 'ز',
            ['v'] = 'ر',
            ['b'] = 'ذ',
            ['n'] = 'د',
            ['m'] = 'ئ',
            [','] = 'و',

            ['?'] = '؟',
            ['`'] = '÷',

            ['0'] = '۰',
            ['1'] = '۱',
            ['2'] = '۲',
            ['3'] = '۳',
            ['4'] = '۴',
            ['5'] = '۵',
            ['6'] = '۶',
            ['7'] = '۷',
            ['8'] = '۸',
            ['9'] = '۹',

            ['Q'] = 'ً',
            ['W'] = 'ٌ',
            ['E'] = 'ٍ',
            ['T'] = '،',
            ['Y'] = '؛',
            ['U'] = ',',
            ['I'] = ']',
            ['O'] = '[',
            ['P'] = '\\',

            ['A'] = 'َ',
            ['S'] = 'ُ',
            ['D'] = 'ِ',
            ['F'] = 'ّ',
            ['G'] = 'ۀ',
            ['H'] = 'آ',
            ['J'] = 'ـ',
            ['K'] = '»',
            ['L'] = '«',

            ['Z'] = 'ة',
            ['X'] = 'ي',
            ['C'] = 'ژ',
            ['V'] = 'ؤ',
            ['B'] = 'إ',
            ['N'] = 'أ',
            ['M'] = 'ء'
        };
    }

    /// <summary>Returns a fresh copy of the default Persian → English mapping.</summary>
    public static Dictionary<char, char> GetDefaultPersianToEnglishMap()
    {
        return new Dictionary<char, char>
        {
            ['ض'] = 'q',
            ['ص'] = 'w',
            ['ث'] = 'e',
            ['ق'] = 'r',
            ['ف'] = 't',
            ['غ'] = 'y',
            ['ع'] = 'u',
            ['ه'] = 'i',
            ['خ'] = 'o',
            ['ح'] = 'p',
            ['ج'] = '[',
            ['چ'] = ']',
            ['پ'] = '\\',

            ['ش'] = 'a',
            ['س'] = 's',
            ['ی'] = 'd',
            ['ب'] = 'f',
            ['ل'] = 'g',
            ['ا'] = 'h',
            ['ت'] = 'j',
            ['ن'] = 'k',
            ['م'] = 'l',
            ['ک'] = ';',
            ['گ'] = '\'',

            ['ظ'] = 'z',
            ['ط'] = 'x',
            ['ز'] = 'c',
            ['ر'] = 'v',
            ['ذ'] = 'b',
            ['د'] = 'n',
            ['ئ'] = 'm',
            ['و'] = ',',

            ['۰'] = '0',
            ['۱'] = '1',
            ['۲'] = '2',
            ['۳'] = '3',
            ['۴'] = '4',
            ['۵'] = '5',
            ['۶'] = '6',
            ['۷'] = '7',
            ['۸'] = '8',
            ['۹'] = '9',
            ['÷'] = '`',

            ['ً'] = 'Q',
            ['ٌ'] = 'W',
            ['ٍ'] = 'E',
            ['،'] = 'T',
            ['؛'] = 'Y',
            [','] = 'U',
            [']'] = 'I',
            ['['] = 'O',
            ['\\'] = 'P',
            ['}'] = '{',
            ['{'] = '}',

            ['َ'] = 'A',
            ['ُ'] = 'S',
            ['ِ'] = 'D',
            ['ّ'] = 'F',
            ['ۀ'] = 'G',
            ['آ'] = 'H',
            ['ـ'] = 'J',
            ['»'] = 'K',
            ['«'] = 'L',

            ['ة'] = 'Z',
            ['ي'] = 'X',
            ['ژ'] = 'C',
            ['ؤ'] = 'V',
            ['إ'] = 'B',
            ['أ'] = 'N',
            ['ء'] = 'M',
            ['؟'] = '?'
        };
    }
}
