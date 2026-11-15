using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Core.Stats;
using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// Which strip and swing an action plays (decisions 678, 708), on Sarxian against the goblin trio: his attack strip is
// the repository's, a skill's or item's own strip is "Own.png", and the records are built here.
public sealed class ActionStripTests
{
    private const string Own = "Own.png";

    private static readonly BattleState _battle =
        BattleScenarios.SarxianAgainstGoblinTrio(new SeededRandom(BattleScenarios.SarxianFirstSeed));

    private static readonly Combatant _sarxian = _battle.Combatants[0];
    private static readonly Combatant _goblin = _battle.Combatants[1];
    private static readonly string _attackStrip = BattlerArt.Of(_sarxian).AttackEffectFile;

    private static readonly SkillCommand _skill = new(SoundingSkills.WithSound(""), [_goblin]);
    private static readonly SkillCommand _skillWithAStrip =
        new(SoundingSkills.WithSound("") with { EffectFile = Own }, [_goblin]);

    private static readonly ItemDefinition _bomb = new()
    {
        Id = "bomb",
        Name = "Bomb",
        Description = "",
        IconFile = "",
        MaxStack = 99,
        IsUsableFromMenu = false,
        Effect = new Effect(EffectKind.Damage, 5, Targeting.SingleEnemy, DamageType: DamageType.Fire),
    };

    // Both strips swing: only the rule decides which swing an action gets.
    private static readonly Dictionary<string, EffectStripDefinition> _swinging = new()
    {
        [Own] = new EffectStripDefinition { File = Own, FrameMilliseconds = 80, SwingFile = "OwnSwing.png" },
        [_attackStrip] = new EffectStripDefinition { File = _attackStrip, FrameMilliseconds = 80, SwingFile = "Swing.png" },
    };

    [Fact]
    public void FileOf_Attack_IsTheActorsAttackStrip()
    {
        Assert.Equal(_attackStrip, ActionStrip.FileOf(_sarxian, new AttackCommand(_goblin)));
    }

    [Fact]
    public void FileOf_SkillNamingAStrip_IsTheSkillsStrip()
    {
        Assert.Equal(Own, ActionStrip.FileOf(_sarxian, _skillWithAStrip));
    }

    [Fact]
    public void FileOf_SkillWithoutAStrip_IsTheActorsAttackStrip()
    {
        Assert.Equal(_attackStrip, ActionStrip.FileOf(_sarxian, _skill));
    }

    [Fact]
    public void FileOf_ItemNamingAStrip_IsTheItemsStrip()
    {
        Assert.Equal(Own, ActionStrip.FileOf(_sarxian, new ItemCommand(_bomb with { EffectFile = Own }, [_goblin])));
    }

    [Fact]
    public void FileOf_ItemWithoutAStrip_IsTheActorsAttackStrip()
    {
        Assert.Equal(_attackStrip, ActionStrip.FileOf(_sarxian, new ItemCommand(_bomb, [_goblin])));
    }

    [Fact]
    public void SwingFileOf_AttackOnASwingingStrip_IsItsSwing()
    {
        Assert.Equal("Swing.png", ActionStrip.SwingFileOf(_sarxian, new AttackCommand(_goblin), _swinging));
    }

    [Fact]
    public void SwingFileOf_SkillNamingASwingingStrip_IsItsSwing()
    {
        Assert.Equal("OwnSwing.png", ActionStrip.SwingFileOf(_sarxian, _skillWithAStrip, _swinging));
    }

    // Decision 657 kept: the actor's strip swings for an Attack, never for a skill playing on it.
    [Fact]
    public void SwingFileOf_SkillOnTheActorsSwingingStrip_IsNone()
    {
        Assert.Null(ActionStrip.SwingFileOf(_sarxian, _skill, _swinging));
    }

    [Fact]
    public void SwingFileOf_ItemNamingASwingingStrip_IsItsSwing()
    {
        var bomb = new ItemCommand(_bomb with { EffectFile = Own }, [_goblin]);

        Assert.Equal("OwnSwing.png", ActionStrip.SwingFileOf(_sarxian, bomb, _swinging));
    }

    [Fact]
    public void SwingFileOf_OwnStripUnlisted_IsNone()
    {
        Assert.Null(ActionStrip.SwingFileOf(_sarxian, _skillWithAStrip, BattleScenarios.NoEffectStrips));
    }
}
