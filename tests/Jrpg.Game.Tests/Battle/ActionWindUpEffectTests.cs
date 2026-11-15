using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Randomness;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Battle;

// Where a damaging action's impact strip plays its lead while it winds up (decisions 656, 657): on Sarxian against the
// goblin trio, the skills built in the test so their effects are known.
[Collection(RepositoryScreenText.CollectionName)]
public sealed class ActionWindUpEffectTests
{
    private static readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    private static readonly Combatant _sarxian = _battle.Combatants[0];
    private static readonly Combatant _goblin = _battle.Combatants[1];
    private static readonly Combatant _secondGoblin = _battle.Combatants[2];

    [Fact]
    public void Of_Attack_AimsAtItsTarget()
    {
        Assert.Equal([_goblin], WindUpOf(_sarxian, new AttackCommand(_goblin)).AimedTargets);
    }

    [Fact]
    public void Of_DamagingSkill_AimsAtItsTargets()
    {
        var sweep = new SkillCommand(SoundingSkills.WithSound(""), [_goblin, _secondGoblin]);

        Assert.Equal([_goblin, _secondGoblin], WindUpOf(_sarxian, sweep).AimedTargets);
    }

    [Fact]
    public void Of_HealingSkill_AimsAtNoOne()
    {
        var mend = SoundingSkills.WithSound("") with { Effect = new Effect(EffectKind.RestoreHp, 5, Targeting.SingleAlly) };

        Assert.Empty(WindUpOf(_sarxian, new SkillCommand(mend, [_sarxian])).AimedTargets);
    }

    [Fact]
    public void Of_Defend_AimsAtNoOne()
    {
        Assert.Empty(WindUpOf(_sarxian, new DefendCommand()).AimedTargets);
    }

    [Fact]
    public void ImpactFile_Attack_IsTheAttackersStrip()
    {
        var windUp = WindUpOf(_sarxian, new AttackCommand(_goblin));

        Assert.Equal(_sarxian.Member!.Definition.AttackEffectFile, windUp.ImpactFile);
    }

    [Fact]
    public void ImpactFile_Defend_IsNone()
    {
        Assert.Null(WindUpOf(_sarxian, new DefendCommand()).ImpactFile);
    }

    [Fact]
    public void IsMirrored_PartyActor_IsTrue()
    {
        Assert.True(WindUpOf(_sarxian, new AttackCommand(_goblin)).IsMirrored);
    }

    [Fact]
    public void IsMirrored_EnemyActor_IsFalse()
    {
        Assert.False(WindUpOf(_goblin, new AttackCommand(_sarxian)).IsMirrored);
    }

    [Fact]
    public void SinceImpact_HalfwayThroughTheWindUp_IsNegative()
    {
        var windUp = WindUpOf(_sarxian, new AttackCommand(_goblin));

        windUp.Update(BattlePacing.WindUp / 2);

        Assert.Equal(-(BattlePacing.WindUp / 2), windUp.SinceImpact);
    }

    [Fact]
    public void SinceImpact_AtTheEnd_IsZero()
    {
        var windUp = WindUpOf(_sarxian, new AttackCommand(_goblin));

        windUp.Update(BattlePacing.WindUp);

        Assert.Equal(TimeSpan.Zero, windUp.SinceImpact);
    }

    [Fact]
    public void SwingFile_AttackWithASwingingStrip_IsItsSwing()
    {
        var attack = new AttackCommand(_goblin);

        var windUp = ActionWindUp.Of(_sarxian, attack, RepositoryScreenText.Text.Messages, SarxiansStripSwinging());

        Assert.Equal("Swing.png", windUp.SwingFile);
    }

    [Fact]
    public void SwingFile_AttackWithAnUnlistedStrip_IsNone()
    {
        Assert.Null(WindUpOf(_sarxian, new AttackCommand(_goblin)).SwingFile);
    }

    // A damaging skill by an attacker whose strip swings: skills play the impact strip only (decision 657's Q2).
    [Fact]
    public void SwingFile_Skill_IsNone()
    {
        var sweep = new SkillCommand(SoundingSkills.WithSound(""), [_goblin]);

        Assert.Null(ActionWindUp.Of(_sarxian, sweep, RepositoryScreenText.Text.Messages, SarxiansStripSwinging()).SwingFile);
    }

    [Fact]
    public void SinceSwing_AtTheSwingStart_IsZero()
    {
        var windUp = WindUpOf(_sarxian, new AttackCommand(_goblin));

        windUp.Update(BattlePacing.SwingStart);

        Assert.Equal(TimeSpan.Zero, windUp.SinceSwing);
    }

    [Fact]
    public void Sounds_AttackWithASwing_WhooshesAtTheSwingStart()
    {
        var attack = new AttackCommand(_goblin);

        var windUp = ActionWindUp.Of(_sarxian, attack, RepositoryScreenText.Text.Messages, SarxiansStripSwinging());

        Assert.Equal([new TimedSound(UiSoundFiles.Battle.Swing, BattlePacing.SwingStart)], windUp.Sounds);
    }

    [Fact]
    public void Sounds_AttackWithoutASwing_IsSilent()
    {
        Assert.Empty(WindUpOf(_sarxian, new AttackCommand(_goblin)).Sounds);
    }

    // A skill by an attacker whose strip swings: its own sound, no whoosh, as skills don't swing (decision 657's Q2).
    [Fact]
    public void Sounds_SkillWithASoundAndNoSwing_IsItsSoundOnly()
    {
        var skill = new SkillCommand(SoundingSkills.WithSound("Own.wav"), [_goblin]);

        var windUp = ActionWindUp.Of(_sarxian, skill, RepositoryScreenText.Text.Messages, SarxiansStripSwinging());

        Assert.Equal([new TimedSound("Own.wav", TimeSpan.Zero)], windUp.Sounds);
    }

    [Fact]
    public void ImpactFile_SkillNamingAStrip_IsTheSkillsStrip()
    {
        var skill = new SkillCommand(SoundingSkills.WithSound("") with { EffectFile = "Own.png" }, [_goblin]);

        Assert.Equal("Own.png", WindUpOf(_sarxian, skill).ImpactFile);
    }

    [Fact]
    public void SwingFile_SkillNamingASwingingStrip_IsItsSwing()
    {
        var skill = new SkillCommand(SoundingSkills.WithSound("") with { EffectFile = "Own.png" }, [_goblin]);

        var windUp = ActionWindUp.Of(_sarxian, skill, RepositoryScreenText.Text.Messages, OwnStripSwinging());

        Assert.Equal("OwnSwing.png", windUp.SwingFile);
    }

    [Fact]
    public void Sounds_SkillNamingASwingingStrip_IsItsSoundThenTheWhoosh()
    {
        var skill = new SkillCommand(SoundingSkills.WithSound("Own.wav") with { EffectFile = "Own.png" }, [_goblin]);

        var windUp = ActionWindUp.Of(_sarxian, skill, RepositoryScreenText.Text.Messages, OwnStripSwinging());

        Assert.Equal(
            [new TimedSound("Own.wav", TimeSpan.Zero), new TimedSound(UiSoundFiles.Battle.Swing, BattlePacing.SwingStart)],
            windUp.Sounds);
    }

    // The strip a skill names for itself, with a swing in its record (decision 708).
    private static Dictionary<string, EffectStripDefinition> OwnStripSwinging() =>
        new()
        {
            ["Own.png"] = new EffectStripDefinition { File = "Own.png", FrameMilliseconds = 80, SwingFile = "OwnSwing.png" },
        };

    private static Dictionary<string, EffectStripDefinition> SarxiansStripSwinging()
    {
        var strip = _sarxian.Member!.Definition.AttackEffectFile;
        return new() { [strip] = new EffectStripDefinition { File = strip, FrameMilliseconds = 80, SwingFile = "Swing.png" } };
    }

    private static ActionWindUp WindUpOf(Combatant actor, BattleCommand command) =>
        ActionWindUp.Of(actor, command, RepositoryScreenText.Text.Messages, BattleScenarios.NoEffectStrips);
}
