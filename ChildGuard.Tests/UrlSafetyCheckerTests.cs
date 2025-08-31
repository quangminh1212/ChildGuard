using ChildGuard.Core.Protection;

public class UrlSafetyCheckerTests
{
    [Fact]
    public void Blocklist_Takes_Precedence()
    {
        var checker = new UrlSafetyChecker(Array.Empty<string>(), new[] { "bad\\.com" });
        Assert.True(checker.IsUnsafe("http://bad.com/path", out _));
    }

    [Fact]
    public void Allowlist_Default_Deny()
    {
        var checker = new UrlSafetyChecker(new[] { "^https?://(www\\.)?good\\.com" }, Array.Empty<string>());
        Assert.False(checker.IsUnsafe("https://good.com/page", out _));
        Assert.True(checker.IsUnsafe("https://unknown.com", out var rule));
        Assert.Equal("not-allowed", rule);
    }
}

