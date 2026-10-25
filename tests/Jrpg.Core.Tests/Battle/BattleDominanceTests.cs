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

// A druid whose Growth and Decay (max 10, from 0) each dominate the other for +50 %, against one goblin (100 HP,
// defense 2); the shuffle (1) keeps the druid first. Rot feeds Decay, Rejuvenate feeds Growth, Thorns feeds Decay
// with damage, and Equilibrium spends 3 of each.
public sealed class BattleDominanceTests
{
    private static readonly SkillDefinition _rot = Skill(
        "rot", new Effect(EffectKind.GrantStatus, 0, Targeting.SingleEnemy, new StatusGrant(StatusKind.Poison, Power: 8, Turns: 3)), new ResourceChange("decay", 2));

    private static readonly SkillDefinition _rejuvenate = Skill(
        "rejuvenate", new Effect(EffectKind.GrantStatus, 0, Targeting.SingleAlly, new StatusGrant(StatusKind.Regen, Power: 10, Turns: 3)), new ResourceChange("growth", 2));

    private static readonly SkillDefinition _thorns = Skill("thorns", new Effect(EffectKind.Damage, 20, Targeting.SingleEnemy), new ResourceChange("decay", 1));

    private static readonly SkillDefinition _equilibrium = Skill(
        "equilibrium", new Effect(EffectKind.Damage, 45, Targeting.AllEnemies), new ResourceChange("growth", -3), new ResourceChange("decay", -3));

    [Fact]
    public void Execute_RotWhileDecayDominant_PoisonPowerRaised()
    {
        var (druid, goblin, battle) = Battle(growth: 0, decay: 2);

        battle.Execute(new SkillCommand(_rot, [goblin]));

        Assert.Equal(12, goblin.Statuses.Active.Single(status => status.Kind == StatusKind.Poison).Power);
        Assert.Equal(4, druid.Resources.AmountOf("decay"));
    }

    // Dominance is judged before the skill's own gain, so the cast that first puts Decay ahead isn't empowered.
    [Fact]
    public void Execute_RotWithNeitherDominant_PoisonAtBasePower()
    {
        var (_, goblin, battle) = Battle(growth: 0, decay: 0);

        battle.Execute(new SkillCommand(_rot, [goblin]));

        Assert.Equal(8, goblin.Statuses.Active.Single().Power);
    }

    [Fact]
    public void Execute_RejuvenateWhileDecayDominant_RegenAtBasePower()
    {
        var (druid, _, battle) = Battle(growth: 0, decay: 2);

        battle.Execute(new SkillCommand(_rejuvenate, [druid]));

        Assert.Equal(10, druid.Statuses.Active.Single().Power);
    }

    // Thorns hits for 20, raised to 30 while Decay dominates, less the goblin's defense 2.
    [Fact]
    public void Execute_DamageSkillWhileItsForceDominant_EffectPowerRaised()
    {
        var (_, goblin, battle) = Battle(growth: 0, decay: 2);

        var events = battle.Execute(new SkillCommand(_thorns, [goblin]));

        Assert.Contains(new EffectApplied(goblin, EffectKind.Damage, 28), events);
    }

    // Equilibrium feeds neither force, so it lands at its base 45, less defense 2.
    [Fact]
    public void Execute_Equilibrium_SpendsBothForces()
    {
        var (druid, goblin, battle) = Battle(growth: 3, decay: 4);

        var events = battle.Execute(new SkillCommand(_equilibrium, [goblin]));

        Assert.Equal((0, 1), (druid.Resources.AmountOf("growth"), druid.Resources.AmountOf("decay")));
        Assert.Contains(new EffectApplied(goblin, EffectKind.Damage, 43), events);
    }

    [Fact]
    public void CanUse_EquilibriumOneForceShort_ReturnsFalse()
    {
        var (_, _, battle) = Battle(growth: 3, decay: 2);

        Assert.False(battle.CanUse(_equilibrium));
    }

    private static (Combatant Druid, Combatant Goblin, BattleState Battle) Battle(int growth, int decay)
    {
        var forces = new[]
        {
            new ResourceDefinition("growth", "Growth", Max: 10) { Dominance = new DominanceRule("decay", 50) },
            new ResourceDefinition("decay", "Decay", Max: 10) { Dominance = new DominanceRule("growth", 50) },
        };
        var skills = new[] { _rot, _rejuvenate, _thorns, _equilibrium };
        var druid = Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId("thaali").BuildDefinition(),
            new ClassBuilder().WithId("druid").WithResources(forces).WithSkills([.. skills.Select(skill => skill.Id)]).Build(),
            skills.ToDictionary(skill => skill.Id)));
        druid.ChangeResource("growth", growth, []);
        druid.ChangeResource("decay", decay, []);
        var goblin = Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: 100, attack: 20, defense: 2).Build());
        return (druid, goblin, new BattleState([druid, goblin], new Inventory(), new FixedSequenceRandom(1)));
    }

    private static SkillDefinition Skill(string id, Effect effect, params ResourceChange[] changes) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", ResourceChanges = changes, Effect = effect };
}
