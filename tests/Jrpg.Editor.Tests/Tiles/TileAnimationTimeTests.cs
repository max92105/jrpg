using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Tests.Tiles;

public sealed class TileAnimationTimeTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void Advance_WithinAFrame_RaisesNothing()
    {
        var (time, raised) = TimeWithA140MillisecondTile();

        time.Advance(TimeSpan.FromMilliseconds(139));

        Assert.Equal(0, raised());
    }

    [Fact]
    public void Advance_PastAFrameTime_RaisesFramesChangedOnce()
    {
        var (time, raised) = TimeWithA140MillisecondTile();

        time.Advance(TimeSpan.FromMilliseconds(100));
        time.Advance(TimeSpan.FromMilliseconds(100));

        Assert.Equal(1, raised());
        Assert.Equal(TimeSpan.FromMilliseconds(200), time.Elapsed);
    }

    [Fact]
    public void Advance_NoAnimatedTile_RaisesNothing()
    {
        var time = new TileAnimationTime(_draft);
        var raised = 0;
        time.FramesChanged += (_, _) => raised++;

        time.Advance(TimeSpan.FromSeconds(10));

        Assert.Equal(0, raised);
    }

    // The time made before any tile moves: it follows the catalog's later change.
    [Fact]
    public void Advance_TileAnimatedAfterTheTimeWasMade_RaisesFramesChanged()
    {
        var time = new TileAnimationTime(_draft);
        var raised = 0;
        time.FramesChanged += (_, _) => raised++;
        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [_draft.Current.Tiles.Keys.Min()] = new TileAnimation { FrameCount = 2, FrameMilliseconds = 100 } });

        time.Advance(TimeSpan.FromMilliseconds(100));

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Advance_NegativeDelta_Throws() =>
        Assert.Throws<ArgumentOutOfRangeException>(() => new TileAnimationTime(_draft).Advance(TimeSpan.FromMilliseconds(-1)));

    private (TileAnimationTime Time, Func<int> Raised) TimeWithA140MillisecondTile()
    {
        var tileId = _draft.Current.Tiles.Keys.Min();
        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [tileId] = new TileAnimation { FrameCount = 4, FrameMilliseconds = 140 } });
        var time = new TileAnimationTime(_draft);
        var raised = 0;
        time.FramesChanged += (_, _) => raised++;
        return (time, () => raised);
    }
}
