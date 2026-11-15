using Jrpg.Core.Battle;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// The effect strips' records are replaced whole as one undo step (ADR 0045, decision 659), on the test world's draft.
public sealed class CatalogDraftEffectStripTests
{
    private static readonly EffectStripDefinition _slash = new() { File = "Slash.png", FrameMilliseconds = 60, HitFrame = 3 };

    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));

    [Fact]
    public void SetEffectStrips_Changed_AddsOneStep()
    {
        _draft.SetEffectStrips([_slash]);

        Assert.Equal(_slash, _draft.Current.EffectStrips["Slash.png"]);
        Assert.True(_draft.CanUndo);
    }

    [Fact]
    public void SetEffectStrips_SameByContent_AddsNoStep()
    {
        _draft.SetEffectStrips([_slash]);

        _draft.SetEffectStrips([_slash with { }]);
        _draft.Undo();

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void SetEffectStrips_Undo_RestoresTheOldRecords()
    {
        _draft.SetEffectStrips([_slash]);

        _draft.Undo();

        Assert.False(_draft.Current.EffectStrips.ContainsKey("Slash.png"));
    }
}
