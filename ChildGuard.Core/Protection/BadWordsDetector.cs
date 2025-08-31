using System.Text;
using System.Text.RegularExpressions;

namespace ChildGuard.Core.Protection;

public sealed class BadWordsDetector
{
    private readonly HashSet<string> _words;

    public BadWordsDetector(IEnumerable<string> words)
    {
        _words = new HashSet<string>(words.Select(Normalize), StringComparer.OrdinalIgnoreCase);
    }

    public bool ContainsBadWord(string text, out string? matched)
    {
        var norm = Normalize(text);
        foreach (var w in _words)
        {
            if (norm.Contains(w, StringComparison.OrdinalIgnoreCase))
            {
                matched = w;
                return true;
            }
        }
        matched = null;
        return false;
    }

    private static string Normalize(string input)
    {
        // remove diacritics and spaces/punct between letters to resist obfuscation
        string formD = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder();
        foreach (var ch in formD)
        {
            var uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(ch);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                if (char.IsLetterOrDigit(ch)) sb.Append(char.ToLowerInvariant(ch));
            }
        }
        return sb.ToString();
    }
}

