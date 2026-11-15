using Jrpg.Core.Cinematics;
using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

// A cinematic's pictures asked one per frame, in order (decision 628).
public sealed class StillPreloadTests
{
    private readonly StillPreload _preload = new([Still("intro/1.png"), Still("intro/2.png")]);

    [Fact]
    public void Next_AtStart_IsTheFirstPicture()
    {
        Assert.Equal("intro/1.png", _preload.Next);
        Assert.False(_preload.IsDone);
    }

    [Fact]
    public void MarkLoaded_EachInTurn_AsksTheNextInOrder()
    {
        _preload.MarkLoaded();

        Assert.Equal("intro/2.png", _preload.Next);
    }

    [Fact]
    public void IsDone_AfterTheLast_IsTrueAndNextIsNull()
    {
        _preload.MarkLoaded();
        _preload.MarkLoaded();

        Assert.True(_preload.IsDone);
        Assert.Null(_preload.Next);
    }

    private static CinematicStill Still(string imageFile) => new() { ImageFile = imageFile, Seconds = 4 };
}
