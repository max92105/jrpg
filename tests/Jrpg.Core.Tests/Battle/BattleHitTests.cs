using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A hit against 50 % dodge, or from 60 accuracy, lands on a draw below its chance; 999 misses. The shuffle's draw comes
// first: (1) keeps the hero first against one goblin, (0) puts the goblin first.
public sealed class BattleHitTests
{
    private const int MissingDraw = 999;

    private static readonly SkillDefinition _doubleBash = new()
    {
        Id = "double_bash",
        Name = "Double Bash",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 13, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 100, Turns: 2), Hits: 2),
    };

    private static readonly SkillDefinition _shieldBash = new()
    {
        Id = "shield_bash",
        Name = "Shield Bash",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 13, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 100, Turns: 2)),
    };

    [Fact]
    public void Execute_AttackRollsAboveChance_Misses()
    {
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
        var goblin = NimbleGoblin();
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, MissingDraw));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Contains(new HitMissed(goblin), events);
        Assert.DoesNotContain(events, battleEvent => battleEvent is EffectApplied);
        Assert.Equal(goblin.Vitals.MaxHp, goblin.Vitals.Hp);
    }

    [Fact]
    public void Execute_Miss_SetsOffNoHitTakenGain()
    {
        var ferocity = new ResourceDefinition("ferocity", "Ferocity", Max: 10, StartsAt: 0) { Gains = new ResourceGains { PerHitTaken = 2 } };
        var barbarian = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("jazog").BuildDefinition(),
            new ClassBuilder().WithId("barbarian").WithResources(ferocity).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral with { BaseDodge = 50 }));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());
        var battle = new BattleState([barbarian, goblin], new Inventory(), new FixedSequenceRandom(0, MissingDraw));

        var events = battle.Execute(new AttackCommand(barbarian));

        Assert.Contains(new HitMissed(barbarian), events);
        Assert.Equal(0, barbarian.Resources.AmountOf("ferocity"));
        Assert.DoesNotContain(events, battleEvent => battleEvent is ResourceChanged);
    }

    [Fact]
    public void Execute_ShieldBashMisses_GrantsNoStun()
    {
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_shieldBash).Build());
        var goblin = NimbleGoblin();
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, MissingDraw));

        var events = battle.Execute(new SkillCommand(_shieldBash, [goblin]));

        Assert.Contains(new HitMissed(goblin), events);
        Assert.DoesNotContain(events, battleEvent => battleEvent is StatusApplied);
        Assert.False(goblin.Statuses.Has(StatusKind.Stun));
    }

    [Fact]
    public void Execute_MemberWithLowAccuracyRollsAboveIt_Misses()
    {
        var hero = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().BuildDefinition(),
            new ClassBuilder().Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral with { BaseAccuracy = 60 }));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, MissingDraw));

        var events = battle.Execute(new AttackCommand(goblin));

        Assert.Contains(new HitMissed(goblin), events);
    }

    [Fact]
    public void Execute_EnemyWithLowAccuracyRollsAboveIt_Misses()
    {
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().Build());
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithAccuracy(60).Build());
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(0, MissingDraw));

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Contains(new HitMissed(hero), events);
    }

    // Decision 326 withholds the status only when every hit missed: one landing hit of two is enough.
    [Fact]
    public void Execute_OneOfTwoHitsLands_GrantsTheStatus()
    {
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(_doubleBash).Build());
        var goblin = NimbleGoblin();
        var battle = new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1, MissingDraw, 0));

        var events = battle.Execute(new SkillCommand(_doubleBash, [goblin]));

        Assert.Contains(new HitMissed(goblin), events);
        Assert.Contains(new StatusApplied(goblin, StatusKind.Stun, 2), events);
    }

    private static Combatant NimbleGoblin() => Combatant.FromEnemy(new EnemyDefinitionBuilder().WithDodge(50).Build());
}
