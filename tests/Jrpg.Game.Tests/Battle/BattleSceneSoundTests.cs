using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// A battle lost at the start (decision 97), so no seed matters; with no save the defeat menu lists one choice.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class BattleSceneSoundTests
{
    [Fact]
    public void Update_DefeatMenuConfirmed_SceneAsksTheConfirmSound()
    {
        var scene = LostBattle();

        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], scene.Sounds);
    }

    [Fact]
    public void Update_AfterASoundingUpdate_AsksNothing()
    {
        var scene = LostBattle();
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        scene.Update(StartMapScenarios.OneUpdate, default);

        Assert.Empty(scene.Sounds);
    }

    // Long enough for the defeat screen to come in and take input.
    private static BattleScene LostBattle()
    {
        var catalog = StartMapScenarios.NextToForestWarpAlwaysRolling;
        var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(1));
        Assert.True(StartMapScenarios.WalkUntilBattleRequested(StartMapScenarios.SceneOf(session.Exploration, catalog)));
        session.Party.Leader.Vitals.TakeDamage(9999);
        session.StartPendingBattle();
        var scene = new BattleScene(session, new BattleSceneSources(catalog.Items, UnwrittenSaveSlots.Create()), RepositoryScreenText.Text, new UiTextures());
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(TimeSpan.FromSeconds(10), default);
        return scene;
    }
}
