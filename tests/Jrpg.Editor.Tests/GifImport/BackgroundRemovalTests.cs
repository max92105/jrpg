using Jrpg.Editor.GifImport;

namespace Jrpg.Editor.Tests.GifImport;

// A solid background made transparent (decision 638): the first frame's top-left color, in every frame.
public sealed class BackgroundRemovalTests
{
    private const uint NearWhite = 0xFFFEFEFE;
    private const uint Red = 0xFFFF0000;

    [Fact]
    public void Apply_TopLeftColor_BecomesTransparentInEveryFrame()
    {
        var cleared = BackgroundRemoval.Apply([[NearWhite, Red], [Red, NearWhite]]);

        Assert.Equal([[0u, Red], [Red, 0u]], cleared);
    }

    [Fact]
    public void Apply_OtherColors_AreKept()
    {
        var cleared = BackgroundRemoval.Apply([[NearWhite, 0xFFFFFFFE, Red]]);

        Assert.Equal([0u, 0xFFFFFFFE, Red], Assert.Single(cleared));
    }

    [Fact]
    public void Apply_TopLeftAlreadyTransparent_ChangesNothing()
    {
        IReadOnlyList<uint[]> frames = [[0x00FEFEFE, NearWhite]];

        Assert.Same(frames, BackgroundRemoval.Apply(frames));
    }
}
