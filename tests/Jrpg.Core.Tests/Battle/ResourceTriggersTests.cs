using Jrpg.Core.Battle;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// A cleric whose faith (5 of 5) drops by 1 when an ally falls, beside a fighter, against a goblin.
public sealed class ResourceTriggersTests
{
    private static readonly ResourceDefinition _faith =
        new("faith", "Faith", Max: 5, StartsAt: 5) { Gains = new ResourceGains { PerAllyFallen = -1 } };

    private readonly Combatant _cleric = Combatant.FromPartyMember(PartyMember.Recruit(
        new PartyMemberBuilder().WithId("zanyr").BuildDefinition(),
        new ClassBuilder().WithId("cleric").WithResources(_faith).Build(),
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

    private static void Fell(Combatant combatant) => combatant.Vitals.TakeDamage(combatant.Vitals.Hp);
}
