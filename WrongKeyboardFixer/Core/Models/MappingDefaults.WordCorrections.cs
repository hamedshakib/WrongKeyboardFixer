using System;
using System.Collections.Generic;
using System.IO;

namespace WrongKeyboardFixer.Core.Models;

public static partial class MappingDefaults
{
    private static readonly string[] _defaultWordCorrections = LoadDefaultWordCorrections();

    /// <summary>
    ///     Returns a fresh copy of the default words whose Persian spelling must begin
    ///     with «آ» or «ژ». When keyboard conversion produces a word whose corrected
    ///     form appears here (e.g. «اسان» → «آسان», «زله» → «ژله»), it is fixed.
    ///     The list is loaded once from the embedded resource
    ///     "WrongKeyboardFixer.Core.Models.word-corrections.txt".
    /// </summary>
    public static List<string> GetDefaultWordCorrections()
    {
        return new List<string>(_defaultWordCorrections);
    }

    private static string[] LoadDefaultWordCorrections()
    {
        const string resourceName = "WrongKeyboardFixer.Core.Models.word-corrections.txt";

        var assembly = typeof(MappingDefaults).Assembly;
        using var stream = assembly.GetManifestResourceStream(resourceName)
                           ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found.");

        using var reader = new StreamReader(stream);
        var text = reader.ReadToEnd();

        var lines = text.Split('\n');
        var result = new List<string>(lines.Length);
        foreach (var raw in lines)
        {
            var word = raw.Trim('\r', ' ');
            if (word.Length > 0)
                result.Add(word);
        }

        return result.ToArray();
    }
}