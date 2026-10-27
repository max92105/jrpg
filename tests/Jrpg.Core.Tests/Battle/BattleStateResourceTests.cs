using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// A mage with 10 max mana, starting at 3 and gaining 2 at the end of each of the mage's turns, against one goblin.
// Blast costs 2 mana and Meteor 5.
// The shuffle (1) keeps the listed order: the mage, then the goblin.
public sealed class BattleStateResourceTests
{
    private static readonly ResourceDefinition _mana =
        new("mana", "Mana", Max: 10, StartsAt: 3) { Gains = new ResourceGains { PerTurn = 2 } };

    private static readonly SkillDefinition _blast = Spell("blast", manaCost: 2);
    private static readonly SkillDefinition _meteor = Spell("meteor", manaCost: 5);

    private static readonly SkillDefinition _bolt = Spell("bolt", manaCost: 0) with
    {
        ResourceChanges = [new ResourceChange("instability", 2)],
    };

    private static readonly SkillDefinition _voidRift = Spell("void_rift", manaCost: 0) with
    {
        ResourceChanges = [new ResourceChange("instability", 3)],
        Requirements = [new ResourceRequirement("instability", 4)],
    };

    private readonly PartyMember _mage = PartyMember.Recruit(
        new PartyMemberBuilder().WithId("omer").BuildDefinition(),
        new ClassBuilder().WithId("mage").WithResources(_mana).WithSkills("blast", "meteor").Build(),
        new Dictionary<string, SkillDefinition> { ["blast"] = _blast, ["meteor"] = _meteor },
        TestStatRules.Neutral);

    [Fact]
    public void Start_MemberWithResource_StartsAtStartsAt()
    {
        var battle = StartBattle();

        Assert.Equal(3, MageIn(battle).Resources.AmountOf("mana"));
    }

    [Fact]
    public void Execute_EndOfMemberTurn_GainsPerTurnAndReportsIt()
    {
        var battle = StartBattle();
        var mage = MageIn(battle);

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(5, mage.Resources.AmountOf("mana"));
        Assert.Contains(new ResourceChanged(mage, "mana", 2), events);
    }

    [Fact]
    public void Execute_AtMax_ReportsNoChange()
    {
        var battle = StartBattle();
        var mage = MageIn(battle);
        mage.Resources.Change("mana", 10);

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(10, mage.Resources.AmountOf("mana"));
        Assert.DoesNotContain(events, battleEvent => battleEvent is ResourceChanged);
    }

    [Fact]
    public void Execute_EnemyTurn_MemberGainsNothing()
    {
        var battle = StartBattle();
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new DefendCommand());

        Assert.Equal(5, MageIn(battle).Resources.AmountOf("mana"));
        Assert.DoesNotContain(events, battleEvent => battleEvent is ResourceChanged);
    }

    [Fact]
    public void Start_SecondBattle_StartsAtStartsAgain()
    {
        var first = StartBattle();
        first.Execute(new DefendCommand());

        var second = StartBattle();

        Assert.Equal((5, 3), (MageIn(first).Resources.AmountOf("mana"), MageIn(second).Resources.AmountOf("mana")));
    }

    [Fact]
    public void Execute_SkillCostingResource_SpendsBeforeEffect()
    {
        var battle = StartBattle();
        var mage = MageIn(battle);
        var goblin = battle.LivingOn(BattleSide.Enemies)[0];

        var events = battle.Execute(new SkillCommand(_blast, [goblin]));

        Assert.Equal(new ResourceChanged(mage, "mana", -2), events[1]);
        Assert.IsType<EffectApplied>(events[2]);
    }

    [Fact]
    public void Execute_UnaffordableResourceSkill_Throws()
    {
        var battle = StartBattle();
        var goblin = battle.LivingOn(BattleSide.Enemies)[0];

        Assert.Throws<InvalidOperationException>(() => battle.Execute(new SkillCommand(_meteor, [goblin])));
        Assert.Equal(3, MageIn(battle).Resources.AmountOf("mana"));
    }

    [Fact]
    public void LackingResourceFor_Unaffordable_NeverNull()
    {
        var battle = StartBattle();

        var lacking = battle.LackingResourceFor(_meteor);

        Assert.False(battle.CanAfford(_meteor));
        Assert.Same(_mana, lacking);
    }

    [Fact]
    public void CanUse_AfterGainingTurns_CostlierSkillBecomesUsable()
    {
        var battle = StartBattle();
        var usableAtStart = battle.CanUse(_meteor);

        battle.Execute(new DefendCommand());
        battle.Execute(new DefendCommand());

        Assert.False(usableAtStart);
        Assert.True(battle.CanUse(_meteor));
    }

    // The shuffle (2, 1) keeps the listed order of three, so the goblin acts first.
    [Fact]
    public void Execute_AllyFallsToAnAttack_ClericLosesFaith()
    {
        var faith = new ResourceDefinition("faith", "Faith", Max: 5, StartsAt: 5) { Gains = new ResourceGains { PerAllyFallen = -1 } };
        var cleric = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("zanyr").BuildDefinition(),
            new ClassBuilder().WithId("cleric").WithResources(faith).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral));
        var ally = Combatant.FromPartyMember(new PartyMemberBuilder().WithId("ragnir").WithVitals(hp: 1, maxHp: 100).Build());
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, damage: 20).Build());
        var battle = new BattleState([goblin, cleric, ally], new Inventory(), new FixedSequenceRandom(2, 1));

        var events = battle.Execute(new AttackCommand(ally));

        Assert.Equal(4, cleric.Resources.AmountOf("faith"));
        Assert.Contains(new ResourceChanged(cleric, "faith", -1), events);
    }

    // The shuffle (0) puts the goblin first; its attack costs the barbarian HP, so ferocity rises by 2.
    [Fact]
    public void Execute_EnemyHitsBarbarian_GainsFerocity()
    {
        var ferocity = new ResourceDefinition("ferocity", "Ferocity", Max: 10, StartsAt: 0) { Gains = new ResourceGains { PerHitTaken = 2 } };
        var barbarian = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("jazog").BuildDefinition(),
            new ClassBuilder().WithId("barbarian").WithResources(ferocity).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, damage: 20).Build());
        var battle = new BattleState([barbarian, goblin], new Inventory(), new FixedSequenceRandom(0));

        var events = battle.Execute(new AttackCommand(barbarian));

        Assert.Equal(2, barbarian.Resources.AmountOf("ferocity"));
        Assert.Contains(new ResourceChanged(barbarian, "ferocity", 2), events);
    }

    // The barbarian is down to 5 HP; the goblin's 15-damage hit fells him, and still adds its ferocity before the fall.
    [Fact]
    public void Execute_HitFellsBarbarian_GainsFerocityBeforeTheFall()
    {
        var ferocity = new ResourceDefinition("ferocity", "Ferocity", Max: 10, StartsAt: 0) { Gains = new ResourceGains { PerHitTaken = 2 } };
        var barbarian = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("jazog").BuildDefinition(),
            new ClassBuilder().WithId("barbarian").WithResources(ferocity).Build(),
            new Dictionary<string, SkillDefinition>(),
            TestStatRules.Neutral));
        barbarian.Vitals.TakeDamage(barbarian.Vitals.Hp - 5);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, damage: 20).Build());
        var battle = new BattleState([barbarian, goblin], new Inventory(), new FixedSequenceRandom(0));

        var events = battle.Execute(new AttackCommand(barbarian));

        Assert.IsType<EffectApplied>(events[1]);
        Assert.Equal(new ResourceChanged(barbarian, "ferocity", 2), events[2]);
        Assert.Equal(new CombatantDefeated(barbarian), events[3]);
    }

    [Fact]
    public void Execute_WarlockTurnEnds_InstabilityFallsByOne()
    {
        var (warlock, battle) = WarlockAgainstGoblin();
        warlock.Resources.Change("instability", 5);

        battle.Execute(new DefendCommand());

        Assert.Equal(4, warlock.Resources.AmountOf("instability"));
    }

    // Void Rift needs 4 and adds 3: at 3 it stays unusable, though casting it would leave 6.
    [Fact]
    public void CanUse_RequirementJudgedBeforeOwnGain_StillShortAtThree()
    {
        var (warlock, battle) = WarlockAgainstGoblin();
        warlock.Resources.Change("instability", 3);

        var canUse = battle.CanUse(_voidRift);

        Assert.False(canUse);
    }

    // At 9 of 10, a Bolt (+2) adds only 1; the end of the turn then takes 1 off.
    [Fact]
    public void Execute_GainPastMax_StopsAtMax()
    {
        var (warlock, battle) = WarlockAgainstGoblin();
        warlock.Resources.Change("instability", 9);
        var goblin = battle.LivingOn(BattleSide.Enemies)[0];

        var events = battle.Execute(new SkillCommand(_bolt, [goblin]));

        Assert.Equal(new ResourceChanged(warlock, "instability", 1), events[1]);
        Assert.Equal(9, warlock.Resources.AmountOf("instability"));
    }

    // A warlock with Instability (0 of 10, −1 a turn), Eldritch Bolt (+2) and Void Rift (+3, needs 4), acting first.
    private static (Combatant Warlock, BattleState Battle) WarlockAgainstGoblin()
    {
        var instability = new ResourceDefinition("instability", "Instability", Max: 10, StartsAt: 0) { Gains = new ResourceGains { PerTurn = -1 } };
        var warlock = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("sarxian").BuildDefinition(),
            new ClassBuilder().WithId("warlock").WithResources(instability).WithSkills("bolt", "void_rift").Build(),
            new Dictionary<string, SkillDefinition> { ["bolt"] = _bolt, ["void_rift"] = _voidRift },
            TestStatRules.Neutral));
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, damage: 20).Build());
        return (warlock, new BattleState([warlock, goblin], new Inventory(), new FixedSequenceRandom(1)));
    }

    private BattleState StartBattle()
    {
        var party = new PartyState([_mage], [_mage], new PartyRules(MaxActiveMembers: 4));
        var goblin = new EnemyDefinitionBuilder().WithStats(maxHp: 40, damage: 20).Build();
        return BattleState.Start(party, [goblin], new Inventory(), new FixedSequenceRandom(1));
    }

    private static Combatant MageIn(BattleState battle) => battle.LivingOn(BattleSide.Party)[0];

    private static SkillDefinition Spell(string id, int manaCost) => new()
    {
        Id = id,
        Name = id,
        Description = "",
        IconFile = "",
        ResourceChanges = [new ResourceChange("mana", -manaCost)],
        Effect = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy),
    };
}
