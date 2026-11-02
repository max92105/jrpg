using Jrpg.Core.Battle;
using Jrpg.Core.Loot;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Session;
using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// A level filled sounds as its "Level Up!" shows, by the bar or by the skip; the drops showing sound the field's found
// item; the Confirm that leaves sounds the confirm (decisions 430, 432). The skip has no sound of its own.
public sealed class VictoryScreenSoundTests
{
    private static readonly InputFrame _confirm = InputFrames.Pressing(GameButton.Confirm);

    // Long enough for the entrance and every bar, in one update.
    private static readonly TimeSpan _wholeFill = TimeSpan.FromSeconds(5);

    [Fact]
    public void Update_BarFillsALevel_AsksTheLevelUpSound()
    {
        var screen = new VictoryScreen(Spoils(LevelUp(), Drops.None));

        screen.Update(_wholeFill, default);

        Assert.Equal([UiSoundFiles.Battle.LevelUp], screen.Sounds);
    }

    [Fact]
    public void Update_UpdateAfterTheLevelUp_AsksNothing()
    {
        var screen = new VictoryScreen(Spoils(LevelUp(), Drops.None));
        screen.Update(_wholeFill, default);

        screen.Update(_wholeFill, default);

        Assert.Empty(screen.Sounds);
    }

    [Fact]
    public void Update_BarFillsWithoutALevel_AsksNothing()
    {
        var screen = new VictoryScreen(Spoils(HalfALevel(), Drops.None));

        screen.Update(_wholeFill, default);

        Assert.True(screen.IsComplete);
        Assert.Empty(screen.Sounds);
    }

    [Fact]
    public void Update_SkipLandsALevel_AsksTheLevelUpSound()
    {
        var screen = new VictoryScreen(Spoils(LevelUp(), Drops.None));

        screen.Update(TimeSpan.Zero, _confirm);

        Assert.True(screen.IsComplete);
        Assert.Equal([UiSoundFiles.Battle.LevelUp], screen.Sounds);
    }

    [Fact]
    public void Update_SkipWithoutALevel_AsksNothing()
    {
        var screen = new VictoryScreen(Spoils(HalfALevel(), Drops.None));

        screen.Update(TimeSpan.Zero, _confirm);

        Assert.True(screen.IsComplete);
        Assert.Empty(screen.Sounds);
    }

    [Fact]
    public void Update_ConfirmShowsTheDrops_AsksTheItemSound()
    {
        var screen = new VictoryScreen(Spoils(HalfALevel(), APotion()));
        screen.Update(_wholeFill, default);

        screen.Update(TimeSpan.Zero, _confirm);

        Assert.True(screen.IsShowingDrops);
        Assert.Equal([UiSoundFiles.Field.ItemGot], screen.Sounds);
    }

    [Fact]
    public void Update_ConfirmLeavesWithoutDrops_AsksTheConfirmSound()
    {
        var screen = new VictoryScreen(Spoils(HalfALevel(), Drops.None));
        screen.Update(_wholeFill, default);

        var request = screen.Update(TimeSpan.Zero, _confirm);

        Assert.NotNull(request);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], screen.Sounds);
    }

    [Fact]
    public void Update_ConfirmLeavesFromTheDrops_AsksTheConfirmSound()
    {
        var screen = new VictoryScreen(Spoils(HalfALevel(), APotion()));
        screen.Update(_wholeFill, default);
        screen.Update(TimeSpan.Zero, _confirm);

        var request = screen.Update(TimeSpan.Zero, _confirm);

        Assert.NotNull(request);
        Assert.Equal([UiSoundFiles.Effects.MenuConfirm], screen.Sounds);
    }

    private static BattleSpoils Spoils(ExperienceGain gain, Drops drops) => new(new VictoryRewards(gain.Amount, [gain]), drops, Gold: 0);

    // 60 experience from 60 of a 120-point level fill it, then 10 into the next.
    private static ExperienceGain LevelUp() =>
        new(Sarxian(), 70, [new LevelProgressStep(1, 60, 120, 120), new LevelProgressStep(2, 0, 10, 150)], IsAtMaxLevel: false);

    private static ExperienceGain HalfALevel() =>
        new(Sarxian(), 60, [new LevelProgressStep(1, 0, 60, 120)], IsAtMaxLevel: false);

    private static Drops APotion() => new([new ItemDrop(BattleScenarios.Catalog.Items["potion"], 1)], []);

    private static PartyMember Sarxian() => BattleScenarios.Recruit("sarxian");
}
