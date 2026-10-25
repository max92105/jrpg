using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// A cleric whose faith (5 of 5) drops by 1 when an ally falls, beside a fighter, against a goblin; and a barbarian
// whose ferocity (0 of 10) rises by 2 for each hit that costs HP.
public sealed class ResourceTriggersTests
{
    private static readonly ResourceDefinition _faith =
        new("faith", "Faith", Max: 5, StartsAt: 5) { Gains = new ResourceGains { PerAllyFallen = -1 } };

    private readonly Combatant _cleric = Combatant.FromPartyMember(PartyMember.Recruit(
        new PartyMemberBuilder().WithId("zanyr").BuildDefinition(),
        new ClassBuilder().WithId("cleric").WithResources(_faith).Build(),
        new Dictionary<string, SkillDefinition>()));

    private static readonly ResourceDefinition _ferocity =
        new("ferocity", "Ferocity", Max: 10, StartsAt: 0) { Gains = new ResourceGains { PerHitTaken = 2 } };

    private readonly Combatant _barbarian = Combatant.FromPartyMember(PartyMember.Recruit(
        new PartyMemberBuilder().WithId("jazog").BuildDefinition(),
        new ClassBuilder().WithId("barbarian").WithResources(_ferocity).Build(),
        new Dictionary<string, SkillDefinition>()));

    // Conviction 0 of 10: +3 when an enemy falls, +2 for a hit of at least 20 % of max HP (100 here, so 20 HP).
    private static readonly ResourceDefinition _conviction = new("conviction", "Conviction", Max: 10, StartsAt: 0)
    {
        Gains = new ResourceGains { PerHeavyHit = 2, HeavyHitPercent = 20, PerEnemyFallen = 3 },
    };

    private readonly Combatant _paladin = Combatant.FromPartyMember(PartyMember.Recruit(
        new PartyMemberBuilder().WithId("teelle").BuildDefinition(),
        new ClassBuilder().WithId("paladin").WithResources(_conviction).Build(),
        new Dictionary<string, SkillDefinition>()));

    private readonly Combatant _fighter = Combatant.FromPartyMember(new PartyMemberBuilder().WithId("ragnir").Build());

    private readonly Combatant _goblin =
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 40, attack: 20, defense: 2).Build());

    [Fact]
    public void OnCombatantFallen_AllyFalls_LivingClericLosesFaith()
    {
        var events = new List<BattleEvent>();
        Fell(_fighter);

        ResourceTriggers.OnCombatantFallen(_fighter, [_cleric, _fighter, _goblin], events);

        Assert.Equal(4, _cleric.Resources.AmountOf("faith"));
        Assert.Equal([new ResourceChanged(_cleric, "faith", -1)], events);
    }

    [Fact]
    public void OnCombatantFallen_EnemyFalls_ClericUnchanged()
    {
        var events = new List<BattleEvent>();
        Fell(_goblin);

        ResourceTriggers.OnCombatantFallen(_goblin, [_cleric, _fighter, _goblin], events);

        Assert.Equal(5, _cleric.Resources.AmountOf("faith"));
        Assert.Empty(events);
    }

    [Fact]
    public void OnCombatantFallen_ClericHerselfFalls_NoChangeToHer()
    {
        var events = new List<BattleEvent>();
        Fell(_cleric);

        ResourceTriggers.OnCombatantFallen(_cleric, [_cleric, _fighter, _goblin], events);

        Assert.Equal(5, _cleric.Resources.AmountOf("faith"));
        Assert.Empty(events);
    }

    [Fact]
    public void OnHitTaken_MemberWithPerHitGain_GainsAndReports()
    {
        var events = new List<BattleEvent>();

        ResourceTriggers.OnHitTaken(_barbarian, hpLost: 12, events);

        Assert.Equal(2, _barbarian.Resources.AmountOf("ferocity"));
        Assert.Equal([new ResourceChanged(_barbarian, "ferocity", 2)], events);
    }

    [Fact]
    public void OnHitTaken_NoHpLost_GainsNothing()
    {
        var events = new List<BattleEvent>();

        ResourceTriggers.OnHitTaken(_barbarian, hpLost: 0, events);

        Assert.Equal(0, _barbarian.Resources.AmountOf("ferocity"));
        Assert.Empty(events);
    }

    [Fact]
    public void OnHitTaken_Enemy_GainsNothing()
    {
        var events = new List<BattleEvent>();

        ResourceTriggers.OnHitTaken(_goblin, hpLost: 12, events);

        Assert.Empty(events);
    }

    [Fact]
    public void OnHitTaken_HeavyHit_GainsBothHitAndHeavyGains()
    {
        var ferocity = _ferocity with { Gains = _ferocity.Gains with { PerHeavyHit = 3, HeavyHitPercent = 20 } };
        var barbarian = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("jazog").BuildDefinition(),
            new ClassBuilder().WithId("barbarian").WithResources(ferocity).Build(),
            new Dictionary<string, SkillDefinition>()));
        var events = new List<BattleEvent>();

        ResourceTriggers.OnHitTaken(barbarian, hpLost: 20, events);

        Assert.Equal([new ResourceChanged(barbarian, "ferocity", 5)], events);
    }

    [Fact]
    public void OnHitTaken_HeavyHitOnCleric_LosesFaith()
    {
        var faith = _faith with { Gains = _faith.Gains with { PerHeavyHit = -1, HeavyHitPercent = 20 } };
        var cleric = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("zanyr").BuildDefinition(),
            new ClassBuilder().WithId("cleric").WithResources(faith).Build(),
            new Dictionary<string, SkillDefinition>()));
        var events = new List<BattleEvent>();

        ResourceTriggers.OnHitTaken(cleric, hpLost: 25, events);

        Assert.Equal(4, cleric.Resources.AmountOf("faith"));
        Assert.Equal([new ResourceChanged(cleric, "faith", -1)], events);
    }

    [Fact]
    public void OnHitTaken_LightHitOnPaladin_GainsNothing()
    {
        var events = new List<BattleEvent>();

        ResourceTriggers.OnHitTaken(_paladin, hpLost: 19, events);

        Assert.Equal(0, _paladin.Resources.AmountOf("conviction"));
        Assert.Empty(events);
    }

    [Fact]
    public void OnCombatantFallen_EnemyFalls_LivingPaladinGains()
    {
        var events = new List<BattleEvent>();
        Fell(_goblin);

        ResourceTriggers.OnCombatantFallen(_goblin, [_paladin, _fighter, _goblin], events);

        Assert.Equal(3, _paladin.Resources.AmountOf("conviction"));
        Assert.Equal([new ResourceChanged(_paladin, "conviction", 3)], events);
    }

    [Fact]
    public void OnCombatantFallen_EnemyFalls_FallenPaladinGainsNothing()
    {
        var events = new List<BattleEvent>();
        Fell(_paladin);
        Fell(_goblin);

        ResourceTriggers.OnCombatantFallen(_goblin, [_paladin, _fighter, _goblin], events);

        Assert.Equal(0, _paladin.Resources.AmountOf("conviction"));
        Assert.Empty(events);
    }

    private static void Fell(Combatant combatant) => combatant.Vitals.TakeDamage(combatant.Vitals.Hp);
}
