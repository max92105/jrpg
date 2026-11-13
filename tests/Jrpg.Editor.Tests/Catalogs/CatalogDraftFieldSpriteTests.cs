using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The sprite records are replaced whole as one undo step (decision 569), on the test world's two records.
public sealed class CatalogDraftFieldSpriteTests
{
    private const string Placeholder = "npc_placeholder.png";

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void SetFieldSprites_ChangedSheet_IsOneUndoStep()
    {
        _draft.SetFieldSprites(WithPlaceholderScale(3));

        Assert.Equal(3, _draft.Current.FieldSprites[Placeholder].Scale);
        Assert.True(_draft.IsDirty);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
    }

    // The window hands back only the records it edited (decision 569): the others stay, none is ever removed.
    [Fact]
    public void SetFieldSprites_OneChangedSheet_KeepsTheOthers()
    {
        var player = _draft.Current.FieldSprites["player.png"];

        _draft.SetFieldSprites([_draft.Current.FieldSprites[Placeholder] with { Scale = 3 }]);

        Assert.Equal(player, _draft.Current.FieldSprites["player.png"]);
        Assert.Equal(3, _draft.Current.FieldSprites[Placeholder].Scale);
    }

    [Fact]
    public void SetFieldSprites_SameSheets_RecordsNothing()
    {
        var copies = _draft.Current.FieldSprites.Values.Select(sheet => sheet with { Walk = sheet.Walk with { } }).ToList();

        _draft.SetFieldSprites(copies);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void Undo_AfterSetFieldSprites_RestoresTheRecord()
    {
        var before = _draft.Current.FieldSprites[Placeholder];
        _draft.SetFieldSprites(WithPlaceholderScale(3));

        _draft.Undo();

        Assert.Equal(before, _draft.Current.FieldSprites[Placeholder]);
        Assert.False(_draft.IsDirty);
    }

    private List<FieldSpriteSheet> WithPlaceholderScale(int scale) =>
        [.. _draft.Current.FieldSprites.Values.Select(sheet => sheet.File == Placeholder ? sheet with { Scale = scale } : sheet)];
}
