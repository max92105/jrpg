using Jrpg.Core.Battle;
using Jrpg.Data.Content;
using Jrpg.Editor.Backgrounds;
using Jrpg.Editor.Catalogs;
using static Jrpg.Editor.Tests.Backgrounds.BackgroundFixtures;

namespace Jrpg.Editor.Tests.Backgrounds;

// The Battle backgrounds page's session (decision 901) over the test world's draft with one record built here, Pulse at
// 200 ms, its folder that one sheet.
public sealed class BattleBackgroundEditingViewModelTests
{
    private readonly CatalogDraft _draft;
    private readonly BattleBackgroundEditingViewModel _editing;

    public BattleBackgroundEditingViewModelTests()
    {
        var testWorld = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));
        _draft = new CatalogDraft(testWorld with
        {
            AnimatedBackgrounds = new Dictionary<string, AnimatedBackgroundDefinition> { [Pulse.File] = Pulse },
        });
        _editing = new BattleBackgroundEditingViewModel(
            _draft, () => [Pulse.File], file => file == Pulse.File ? PulseSheet : null);
    }

    [Fact]
    public void Apply_FrameTimeChanged_WritesTheTypedTime()
    {
        _editing.Apply(SessionTyping("300"));

        Assert.Equal(300, _draft.Current.AnimatedBackgrounds[Pulse.File].Animation.FrameMilliseconds);
    }

    [Fact]
    public void Apply_FrameTimeChanged_RecordsOneStep()
    {
        _editing.Apply(SessionTyping("300"));

        _draft.Undo();

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void Apply_Undo_RestoresTheOldTime()
    {
        _editing.Apply(SessionTyping("300"));

        _draft.Undo();

        Assert.Equal(200, _draft.Current.AnimatedBackgrounds[Pulse.File].Animation.FrameMilliseconds);
    }

    [Fact]
    public void Apply_NothingChanged_RecordsNoStep()
    {
        _editing.Apply(_editing.Start());

        Assert.False(_draft.CanUndo);
    }

    [Fact]
    public void Apply_StillImportedOverTheAnimated_DropsItsRecord()
    {
        var edited = _editing.Start();
        edited.RefreshAfterStillImport([Pulse.File]);

        _editing.Apply(edited);

        Assert.Empty(_draft.Current.AnimatedBackgrounds);
    }

    private BattleBackgroundCatalogViewModel SessionTyping(string frameTime)
    {
        var edited = _editing.Start();
        edited.Backgrounds.Single(row => row.File == Pulse.File).FrameMillisecondsText = frameTime;
        return edited;
    }
}
