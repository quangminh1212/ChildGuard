using System.Text.RegularExpressions;

namespace ChildGuard.Core.Protection;

public sealed class UrlSafetyChecker
{
    private readonly List<Regex> _allow;
    private readonly List<Regex> _block;

    public UrlSafetyChecker(IEnumerable<string> allowPatterns, IEnumerable<string> blockPatterns)
    {
        _allow = allowPatterns.Select(p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToList();
        _block = blockPatterns.Select(p => new Regex(p, RegexOptions.IgnoreCase | RegexOptions.Compiled)).ToList();
    }

    public bool IsUnsafe(string url, out string? rule)
    {
        foreach (var r in _block) if (r.IsMatch(url)) { rule = r.ToString(); return true; }
        if (_allow.Count > 0)
        {
            foreach (var r in _allow) if (r.IsMatch(url)) { rule = null; return false; }
            rule = "not-allowed"; return true; // default deny when allowlist present
        }
        rule = null; return false;
    }
}

