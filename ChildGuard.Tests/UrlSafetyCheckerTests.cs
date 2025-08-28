using ChildGuard.Core.Detection;

namespace ChildGuard.Tests;

public class UrlSafetyCheckerTests
{
    [Fact]
    public async Task SafeUrl_ReturnsSafe()
    {
        var checker = new UrlSafetyChecker();
        var result = await checker.CheckUrlAsync("https://example.com");
        Assert.True(result.IsSafe);
    }

    [Fact]
    public async Task SuspiciousUrl_ReturnsUnsafe()
    {
        var checker = new UrlSafetyChecker();
        var result = await checker.CheckUrlAsync("http://bad-domain-with-malware.test/path");
        Assert.False(result.IsSafe);
    }

    [Fact]
    public async Task UrlWithKeywords_ReturnsUnsafe()
    {
        var checker = new UrlSafetyChecker();
        var result = await checker.CheckUrlAsync("http://example.com/porn-video");
        Assert.False(result.IsSafe);
    }
}

