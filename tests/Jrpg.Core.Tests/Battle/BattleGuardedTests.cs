using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Battle;

// The hero knows Guard (+25 to every resistance, self) and acts first against a goblin that hits for 20; the shuffle
// (1) keeps the hero first. Guarded raises resistances (decision 331), so the goblin's hit lands as 15 while it lasts.
public sealed class BattleGuardedTests
{
    [Fact]
    public void Execute_AttackOnGuardedTarget_DealsLess()
    {
        var (hero, battle) = GuardingHero(turns: 2);
        battle.Execute(new SkillCommand(hero.Skills[0], [hero]));

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Equal(15, events.OfType<EffectApplied>().Single().Amount);
    }

    [Fact]
    public void Execute_GuardedEnds_ResistancesBackToBase()
    {
        var (hero, battle) = GuardingHero(turns: 1);
        battle.Execute(new SkillCommand(hero.Skills[0], [hero]));
        battle.Execute(new DefendCommand());
        battle.Execute(new DefendCommand());

        var events = battle.Execute(new AttackCommand(hero));

        Assert.Equal(20, events.OfType<EffectApplied>().Single().Amount);
        Assert.Equal(Resistances.None, hero.CurrentStats.Resistances);
    }

    private static (Combatant Hero, BattleState Battle) GuardingHero(int turns)
    {
        var guard = new SkillDefinition
        {
            Id = "guard",
            Name = "Guard",
            Description = "",
            IconFile = "",
            Effect = new Effect(EffectKind.GrantStatus, 0, Targeting.Self, new StatusGrant(StatusKind.Guarded, Power: 25, Turns: turns)),
        };
        var hero = Combatant.FromPartyMember(new PartyMemberBuilder().WithSkills(guard).Build());
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 25, damage: 20).Build());
        return (hero, new BattleState([hero, goblin], new Inventory(), new FixedSequenceRandom(1)));
    }
}
