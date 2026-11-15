using Jrpg.Core.Cinematics;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The cinematics are replaced whole as one undo step (ADR 0045, ADR 0046), on the test world's draft.
public sealed class CatalogDraftCinematicTests
{
    private static readonly CinematicDefinition _intro = new()
    {
        Id = "intro",
        Stills = [new CinematicStill { ImageFile = "intro/1.png", Seconds = 4 }],
    };

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void SetCinematics_AddedCinematic_IsOneUndoStep()
    {
        _draft.SetCinematics([_intro]);

        Assert.Equal(_intro, _draft.Current.Cinematics["intro"]);
        Assert.True(_draft.IsDirty);
        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetCinematics_Undo_RestoresTheCinematics()
    {
        _draft.SetCinematics([_intro]);

        _draft.Undo();

        Assert.False(_draft.Current.Cinematics.ContainsKey("intro"));
    }

    [Fact]
    public void SetCinematics_EqualByContent_RecordsNothing()
    {
        _draft.SetCinematics([_intro]);
        var copy = _intro with { Stills = [.. _intro.Stills] };

        _draft.SetCinematics([copy]);
        _draft.Undo();

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void CinematicsRemovedByUndo_AfterAdding_NamesTheAdded()
    {
        _draft.SetCinematics([_intro]);

        Assert.Equal(["intro"], _draft.CinematicsRemovedByUndo());
    }

    [Fact]
    public void CinematicsRemovedByUndo_AfterEditingOnly_NamesNone()
    {
        _draft.SetCinematics([_intro]);
        _draft.SetCinematics([_intro with { MusicFile = "town/loop.wav" }]);

        Assert.Empty(_draft.CinematicsRemovedByUndo());
    }
}
