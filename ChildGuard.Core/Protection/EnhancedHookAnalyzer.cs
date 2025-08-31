using System.Text;
using System.Text.RegularExpressions;
using ChildGuard.Core.Protection;

namespace ChildGuard.Core.Protection;

public sealed class EnhancedHookAnalyzer
{
    private readonly StringBuilder _buffer = new();
    private readonly int _maxLen = 256;
    private readonly BadWordsDetector _badWords;
    private readonly Regex _urlRegex = new("https?://[^\s]+", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public event Action<string>? OnBadWord;
    public event Action<string>? OnUrlDetected;

    public EnhancedHookAnalyzer(BadWordsDetector badWords)
    {
        _badWords = badWords;
    }

    public void OnKey(string key, bool down)
    {
        if (!down) return;
        if (key.Length == 1)
        {
            _buffer.Append(key);
        }
        else if (key.Equals("Space", StringComparison.OrdinalIgnoreCase))
        {
            _buffer.Append(' ');
        }
        else if (key.Equals("Enter", StringComparison.OrdinalIgnoreCase))
        {
            _buffer.Append(' ');
        }
        else if (key.Equals("Backspace", StringComparison.OrdinalIgnoreCase))
        {
            if (_buffer.Length > 0) _buffer.Length -= 1;
        }

        if (_buffer.Length > _maxLen)
        {
            _buffer.Remove(0, _buffer.Length - _maxLen);
        }

        var text = _buffer.ToString();
        if (_badWords.ContainsBadWord(text, out var matched))
        {
            OnBadWord?.Invoke(matched!);
        }

        var m = _urlRegex.Match(text);
        if (m.Success)
        {
            OnUrlDetected?.Invoke(m.Value);
        }
    }
}

