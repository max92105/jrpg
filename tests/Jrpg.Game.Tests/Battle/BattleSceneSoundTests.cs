using Jrpg.Core.Content;
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

// The scene passes up what its outcome screens and its turns ask to hear.
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

    // An attack can land, crit or miss, so any of the three proves the turn's sounds reach the scene.
    [Fact]
    public void Update_AttackPlaysOut_AsksItsImpactSound()
    {
        var scene = NewBattle(_ => { });
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        var asked = new List<string>();

        for (var update = 0; update < 120; update++)
        {
            scene.Update(StartMapScenarios.OneUpdate, default);
            asked.AddRange(scene.Sounds);
        }

        Assert.Contains(asked, file => file is UiSoundFiles.Battle.Hit or UiSoundFiles.Battle.Critical or UiSoundFiles.Battle.Miss);
    }

    // One goblin a single certain hit fells, so no balance or seed matters.
    [Fact]
    public void Update_VictoryScreenOpens_AsksTheFanfareOverSilence()
    {
        var catalog = BattleScenarios.WithOneGoblin(StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { MaxHp = 1, Dodge = 0 });
        var scene = NewBattle(catalog with { Rules = catalog.Rules with { Stats = BattleScenarios.CertainRules } }, _ => { });
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        scene.Update(TimeSpan.FromSeconds(10), default);

        Assert.Contains(UiSoundFiles.Battle.Victory, scene.Sounds);
        Assert.Equal(BattleMusicCue.Silence, scene.MusicCue);

        scene.Update(BattleMusic.FanfareHold, default);

        Assert.Equal(BattleMusicCue.Field, scene.MusicCue);
    }

    [Fact]
    public void Update_RunFromTheBattle_AsksTheEscapeAndTheField()
    {
        var scene = NewBattle(_ => { });
        scene.Update(TimeSpan.FromSeconds(10), default);
        ChooseRun(scene);

        scene.Update(TimeSpan.FromSeconds(10), default);

        Assert.Contains(UiSoundFiles.Battle.Escape, scene.Sounds);
        Assert.Equal(BattleMusicCue.Field, scene.MusicCue);
    }

    [Fact]
    public void Update_BattleLost_AsksTheDefeatOverSilence()
    {
        var scene = NewBattle(session => session.Party.Leader.Vitals.TakeDamage(9999));

        scene.Update(TimeSpan.FromSeconds(10), default);

        Assert.Contains(UiSoundFiles.Battle.Defeat, scene.Sounds);
        Assert.Equal(BattleMusicCue.Silence, scene.MusicCue);
    }

    [Fact]
    public void MusicCue_WhileFighting_IsTheTheme()
    {
        Assert.Equal(BattleMusicCue.Theme, NewBattle(_ => { }).MusicCue);
    }

    // Lost at the start (decision 97), so no seed matters; with no save the defeat menu lists one choice. The two long
    // updates let the defeat screen come in and take input.
    private static BattleScene LostBattle()
    {
        var scene = NewBattle(session => session.Party.Leader.Vitals.TakeDamage(9999));
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(TimeSpan.FromSeconds(10), default);
        return scene;
    }

    // Run is the list's last row, so Up wraps to it; the scene doesn't expose its menu.
    private static void ChooseRun(BattleScene scene)
    {
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Up));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
    }

    private static BattleScene NewBattle(Action<GameSession> beforeTheBattle) => NewBattle(StartMapScenarios.NextToForestWarpAlwaysRolling, beforeTheBattle);

    private static BattleScene NewBattle(ContentCatalog catalog, Action<GameSession> beforeTheBattle)
    {
        var session = GameSession.StartNew(catalog, "sarxian", new SeededRandom(1));
        Assert.True(StartMapScenarios.WalkUntilBattleRequested(StartMapScenarios.SceneOf(session.Exploration, catalog)));
        beforeTheBattle(session);
        session.StartPendingBattle();
        return new BattleScene(session, new BattleSceneSources(catalog.Items, UnwrittenSaveSlots.Create()), RepositoryScreenText.Text, new UiTextures());
    }
}
