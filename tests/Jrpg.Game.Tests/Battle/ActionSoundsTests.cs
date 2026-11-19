using Jrpg.Core.Battle;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// The sound each kind of action opens with, as its wind-up starts (decision 647); BattleSoundsTests holds the impact's.
public sealed class ActionSoundsTests
{
    private readonly Combatant _goblin = Combatant.FromEnemy(StartMapScenarios.TestWorld.Enemies["gnoll"]);

    [Fact]
    public void OpeningOf_SkillWithASound_IsItsSound()
    {
        Assert.Equal("Own.wav", ActionSounds.OpeningOf(new SkillCommand(SoundingSkills.WithSound("Own.wav"), [_goblin])));
    }

    [Fact]
    public void OpeningOf_SkillWithoutASound_IsNone()
    {
        Assert.Null(ActionSounds.OpeningOf(new SkillCommand(SoundingSkills.WithSound(""), [_goblin])));
    }

    [Fact]
    public void OpeningOf_Item_IsTheItemUseSound()
    {
        var potion = StartMapScenarios.TestWorld.Items["potion"];

        Assert.Equal(UiSoundFiles.Battle.ItemUse, ActionSounds.OpeningOf(new ItemCommand(potion, [_goblin])));
    }

    [Fact]
    public void OpeningOf_Defend_IsTheDefendSound()
    {
        Assert.Equal(UiSoundFiles.Battle.Defend, ActionSounds.OpeningOf(new DefendCommand()));
    }

    [Fact]
    public void OpeningOf_Attack_IsNone()
    {
        Assert.Null(ActionSounds.OpeningOf(new AttackCommand(_goblin)));
    }
}
