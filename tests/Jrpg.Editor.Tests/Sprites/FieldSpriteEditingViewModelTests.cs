using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

// The Character sprites window starts from the draft and its whole session is one undo step (decision 569).
public sealed class FieldSpriteEditingViewModelTests
{
    private const string Player = "player.png";

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly FieldSpriteEditingViewModel _editing;

    public FieldSpriteEditingViewModelTests() =>
        _editing = new FieldSpriteEditingViewModel(_draft, () => [Player, "npc_placeholder.png"], _ => (384, 288));

    [Fact]
    public void Apply_EditedSetup_IsOneUndoStep()
    {
        var window = _editing.Start();
        window.Chosen = window.Choices.Single(choice => choice.File == Player);
        window.Setup!.Scale = 2;

        _editing.Apply(window);

        Assert.Equal(2, _draft.Current.FieldSprites[Player].Scale);
        _draft.Undo();
        Assert.False(_draft.CanUndo);
        Assert.Equal(1, _draft.Current.FieldSprites[Player].Scale);
    }

    [Fact]
    public void Apply_Unchanged_RecordsNoStep()
    {
        var window = _editing.Start();
        window.Chosen = window.Choices.Single(choice => choice.File == Player);

        _editing.Apply(window);

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void Start_AfterUndo_ReadsTheDraftAsItIs()
    {
        var first = _editing.Start();
        first.Chosen = first.Choices.Single(choice => choice.File == Player);
        first.Setup!.Scale = 2;
        _editing.Apply(first);
        _draft.Undo();

        var second = _editing.Start();
        second.Chosen = second.Choices.Single(choice => choice.File == Player);

        Assert.Equal(1, second.Setup!.Scale);
    }
}
