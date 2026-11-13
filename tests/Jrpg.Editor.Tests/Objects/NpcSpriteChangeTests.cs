using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Objects;

namespace Jrpg.Editor.Tests.Objects;

/// <summary>
/// Change NPC sprite on the Objects panel's chosen object, over the test world: its villager (an NPC on the placeholder
/// sprite) with a second record added so there is a sprite to change to.
/// </summary>
public sealed class NpcSpriteChangeTests
{
    private const string Other = "guard.png";

    private readonly CatalogDraft _draft;
    private readonly ObjectBrowserViewModel _browser;
    private readonly NpcSpriteChange _change;

    public NpcSpriteChangeTests()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
        var sheets = new Dictionary<string, FieldSpriteSheet>(catalog.FieldSprites, StringComparer.Ordinal);
        sheets[Other] = catalog.FieldSprites["npc_placeholder.png"] with { File = Other };
        _draft = new CatalogDraft(catalog with { FieldSprites = sheets });
        _browser = new ObjectBrowserViewModel(_draft, () => []);
        _change = new NpcSpriteChange(_draft, _browser);
    }

    [Fact]
    public void CanChange_ChosenTileObject_IsFalse()
    {
        _browser.SelectedObject = _draft.Current.Objects.Values.First(definition => definition.FieldSpriteFile is null);

        Assert.False(_change.CanChange);
        Assert.Null(_change.Start());
    }

    [Fact]
    public void Apply_OtherSprite_IsOneUndoStep()
    {
        _browser.SelectedObject = _draft.Current.Objects["villager"];
        var dialog = _change.Start()!;
        dialog.SpriteFile = Other;

        Assert.True(_change.Apply(dialog));

        Assert.Equal(Other, _draft.Current.Objects["villager"].FieldSpriteFile);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
        Assert.Equal("npc_placeholder.png", _draft.Current.Objects["villager"].FieldSpriteFile);
    }

    [Fact]
    public void Apply_SameSprite_RecordsNothing()
    {
        _browser.SelectedObject = _draft.Current.Objects["villager"];

        Assert.True(_change.Apply(_change.Start()!));

        Assert.False(_draft.CanUndo);
    }

    // The chosen object is re-checked on every catalog change (the editor rule): an undo may take the NPC away.
    [Fact]
    public void CanChange_AfterUndoRemovesTheNpc_IsFalse()
    {
        var newNpc = new NewNpcViewModel([.. _draft.Current.Objects.Keys], [.. _draft.Current.FieldSprites.Values]);
        newNpc.Id = "guard";
        _draft.SetObject(newNpc.ToDefinition());
        _browser.SelectedObject = _draft.Current.Objects["guard"];
        var raised = new List<string?>();
        _change.PropertyChanged += (_, e) => raised.Add(e.PropertyName);
        Assert.True(_change.CanChange);

        _draft.Undo();

        Assert.False(_change.CanChange);
        Assert.Contains(nameof(NpcSpriteChange.CanChange), raised);
    }
}
