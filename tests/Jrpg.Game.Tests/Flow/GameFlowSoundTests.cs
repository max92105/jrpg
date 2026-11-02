using Jrpg.Game.Flow;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Flow.FlowStarts;

namespace Jrpg.Game.Tests.Flow;

// The flow copies the top scene's sounds right after its update, before the request is applied (ADR 0035).
[Collection(RepositoryScreenText.CollectionName)]
public sealed class GameFlowSoundTests
{
    private readonly GameFlow _flow = new(StartMapScenarios.AfterTheIntro, RepositoryScreenText.Text, UnwrittenSaveSlots.Create(), new PlayerSettings());

    // New Game is the first entry with no save; its request goes behind the fade, and the sound still plays, once.
    [Fact]
    public void Update_TitleConfirmsNewGame_PlaysTheConfirmSound()
    {
        _flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], _flow.Audio.Effects.Played);
    }

    [Fact]
    public void Update_AfterASoundingUpdate_ChannelIsEmpty()
    {
        _flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Down));

        _flow.Update(OneUpdate, default);

        Assert.Empty(_flow.Audio.Effects.Played);
    }
}
