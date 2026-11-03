using Jrpg.Core.Maps;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// Tile animations set as one catalog step (ADR 0039): the import's and the Animate panel's.
public sealed class CatalogDraftAnimationTests
{
    private static readonly TileAnimation _flowing = new() { FrameCount = 4, FrameMilliseconds = 140 };

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void SetTileAnimations_TwoTiles_IsOneUndoStep()
    {
        var (first, second) = TwoTileIds();
        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [first] = _flowing, [second] = _flowing });
        Assert.Equal(_flowing, _draft.Current.Tiles[second].Animation);

        _draft.Undo();

        Assert.Null(_draft.Current.Tiles[first].Animation);
        Assert.Null(_draft.Current.Tiles[second].Animation);
        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetTileAnimations_SameAnimations_RecordsNothing()
    {
        var (first, _) = TwoTileIds();
        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [first] = _flowing });
        var steps = 0;
        _draft.StepRecorded += (_, _) => steps++;

        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [first] = _flowing with { } });

        Assert.Equal(0, steps);
    }

    [Fact]
    public void SetTileAnimations_Null_MakesTheTileStill()
    {
        var (first, _) = TwoTileIds();
        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [first] = _flowing });

        _draft.SetTileAnimations(new Dictionary<int, TileAnimation?> { [first] = null });

        Assert.Null(_draft.Current.Tiles[first].Animation);
    }

    private (int First, int Second) TwoTileIds()
    {
        var ids = _draft.Current.Tiles.Keys.Order().Take(2).ToList();
        return (ids[0], ids[1]);
    }
}
