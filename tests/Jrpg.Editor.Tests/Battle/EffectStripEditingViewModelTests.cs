using Jrpg.Data.Content;
using Jrpg.Editor.Battle;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Battle;

// The Effect strips window's session over the test world's draft (decision 659), its folder one image of four frames.
public sealed class EffectStripEditingViewModelTests
{
    private readonly CatalogDraft _draft = new(ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")));
    private readonly EffectStripEditingViewModel _editing;

    public EffectStripEditingViewModelTests() =>
        _editing = new EffectStripEditingViewModel(_draft, () => ["Burst.png"], file => file == "Burst.png" ? (32, 8) : null);

    [Fact]
    public void Apply_EditedSession_WritesTheRecord()
    {
        _editing.Apply(EditedSession());

        var burst = _draft.Current.EffectStrips["Burst.png"];
        Assert.Equal((50, 2), (burst.FrameMilliseconds, burst.HitFrame));
    }

    [Fact]
    public void Apply_EditedSession_IsOneUndoStep()
    {
        _editing.Apply(EditedSession());

        _draft.Undo();

        Assert.Equal((false, false), (_draft.CanUndo, _draft.Current.EffectStrips.ContainsKey("Burst.png")));
    }

    [Fact]
    public void Apply_UntouchedSession_AddsNoStep()
    {
        _editing.Apply(_editing.Start());

        Assert.False(_draft.CanUndo);
    }

    // Two boxes edited in one session: the frame time and the hit frame.
    private EffectStripCatalogViewModel EditedSession()
    {
        var edited = _editing.Start();
        edited.Strips[0].FrameMillisecondsText = "50";
        edited.Strips[0].HitFrameText = "2";
        return edited;
    }
}
