using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// The scene passes up what its command menu, its turns and its outcome screens ask to hear.
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

    // Sarxian knows the warlock's spells and the new game's bag holds potions, so Items is two rows down; whichever
    // item comes first, on Sarxian, opens with the item-use sound (decision 647).
    [Fact]
    public void Update_ItemConfirmed_AsksTheItemSoundAtOnce()
    {
        var scene = NewBattle(_ => { });
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Down));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Down));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.Contains(UiSoundFiles.Battle.ItemUse, scene.Sounds);
    }

    // A certain hit on one goblin: nothing sounds while the attack winds up; the hit sounds in the update that ends the
    // wind-up, the confirming update being its first.
    [Fact]
    public void Update_AttackConfirmed_AsksTheHitOnlyAtTheImpact()
    {
        var oneGoblin = BattleScenarios.WithOneGoblin(
            StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { Dodge = 0 });
        var scene = NewBattle(oneGoblin with { Rules = oneGoblin.Rules with { Stats = BattleScenarios.CertainRules } }, _ => { });
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        var heardWhileWindingUp = new List<string>(scene.Sounds);
        var step = StartMapScenarios.OneUpdate;
        for (var woundUp = step; woundUp + step < BattlePacing.WindUp; woundUp += step)
        {
            scene.Update(step, default);
            heardWhileWindingUp.AddRange(scene.Sounds);
        }

        scene.Update(step, default);

        Assert.DoesNotContain(heardWhileWindingUp, file => file is UiSoundFiles.Battle.Hit or UiSoundFiles.Battle.Critical);
        Assert.Contains(scene.Sounds, file => file is UiSoundFiles.Battle.Hit or UiSoundFiles.Battle.Critical);
    }

    // Sarxian's strip given a swing here: the whoosh is asked in the update that reaches the swing's start, before the
    // hit at the impact.
    [Fact]
    public void Update_SwingingAttack_WhooshesBeforeTheHit()
    {
        var oneGoblin = BattleScenarios.WithOneGoblin(
            StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { Dodge = 0 });
        var strip = oneGoblin.PartyMembers["sarxian"].AttackEffectFile;
        var swinging = new EffectStripDefinition { File = strip, FrameMilliseconds = 80, SwingFile = "Swing.png" };
        var catalog = oneGoblin with
        {
            Rules = oneGoblin.Rules with { Stats = BattleScenarios.CertainRules },
            EffectStrips = new Dictionary<string, EffectStripDefinition> { [strip] = swinging },
        };
        var scene = NewBattle(catalog, _ => { });
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        var step = StartMapScenarios.OneUpdate;
        var (whooshAt, hitAt) = (TimeSpan.MinValue, TimeSpan.MinValue);
        // The confirming update is the wind-up's first, so the next one ends two steps in.
        for (var woundUp = step * 2; woundUp < BattlePacing.WindUp + step; woundUp += step)
        {
            scene.Update(step, default);
            whooshAt = scene.Sounds.Contains(UiSoundFiles.Battle.Swing) ? woundUp : whooshAt;
            hitAt = scene.Sounds.Any(file => file is UiSoundFiles.Battle.Hit or UiSoundFiles.Battle.Critical) ? woundUp : hitAt;
        }

        Assert.InRange(whooshAt, BattlePacing.SwingStart, BattlePacing.SwingStart + step);
        Assert.InRange(hitAt, BattlePacing.WindUp, BattlePacing.WindUp + step);
    }

    // The victory screen's own sounds reach the flow through the scene; leaving needs no level-up or drop, so it holds
    // whatever the fixture's experience.
    [Fact]
    public void Update_VictoryConfirmLeaves_SceneAsksTheConfirmSound()
    {
        var catalog = BattleScenarios.WithOneGoblin(StartMapScenarios.NextToForestWarpAlwaysRolling, goblin => goblin with { MaxHp = 1, Dodge = 0, DropTableId = null });
        var scene = NewBattle(catalog with { Rules = catalog.Rules with { Stats = BattleScenarios.CertainRules } }, _ => { });
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        scene.Update(TimeSpan.FromSeconds(10), default);
        scene.Update(TimeSpan.FromSeconds(10), default);

        var request = scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Confirm));

        Assert.IsType<BattleFinished>(request);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], scene.Sounds);
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
    public void Update_DownOnTheCommands_AsksTheMoveSound()
    {
        var scene = NewBattle(_ => { });
        scene.Update(TimeSpan.FromSeconds(10), default);

        scene.Update(StartMapScenarios.OneUpdate, InputFrames.Pressing(GameButton.Down));

        Assert.Contains(UiSoundFiles.Effects.MenuMove, scene.Sounds);
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
