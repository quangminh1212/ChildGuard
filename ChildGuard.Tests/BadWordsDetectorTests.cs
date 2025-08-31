using ChildGuard.Core.Protection;

public class BadWordsDetectorTests
{
    [Fact]
    public void Detects_VN_Obfuscation()
    {
        var det = new BadWordsDetector(new[] { "xấu" });
        Assert.True(det.ContainsBadWord("x a ́ u", out _));
    }
}

