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

// A monk whose Chakra (max 10) gains 1 per hit landed, against one goblin (defense 2); the shuffle (1) keeps the monk
// first. Flurry hits 4 times for 8 - 2 = 6 and spends 3 chakra; Palm hits twice for 10 - 2 = 8.
public sealed class BattleMultiHitTests
{
    private static readonly ResourceDefinition _chakra =
        new("chakra", "Chakra", Max: 10) { Gains = new ResourceGains { PerHitLanded = 1 } };

    private static readonly SkillDefinition _flurry =
        Skill("flurry", new Effect(EffectKind.Damage, 8, Targeting.SingleEnemy, Hits: 4), new ResourceChange("chakra", -3));

    private static readonly SkillDefinition _palm = Skill("palm", new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy, Hits: 2));

    [Fact]
    public void Execute_FourHits_ReportsFourEffectApplied()
    {
        var (_, goblin, battle) = Battle(goblinHp: 100, chakra: 3);

        var events = battle.Execute(new SkillCommand(_flurry, [goblin]));

        Assert.Equal(4, events.Count(battleEvent => battleEvent == new EffectApplied(goblin, EffectKind.Damage, 6)));
        Assert.Equal(76, goblin.Vitals.Hp);
    }

    [Fact]
    public void Execute_TargetFallsMidway_StopsHitting()
    {
        var (_, goblin, battle) = Battle(goblinHp: 10, chakra: 3);

        var events = battle.Execute(new SkillCommand(_flurry, [goblin]));

        Assert.Equal(
            [new EffectApplied(goblin, EffectKind.Damage, 6), new EffectApplied(goblin, EffectKind.Damage, 4)],
            events.OfType<EffectApplied>());
        Assert.Single(events, battleEvent => battleEvent == new CombatantDefeated(goblin));
    }

    [Fact]
    public void Execute_MultiHitWithStatus_GrantsItOnceAfterTheHits()
    {
        var stunningPalm = Skill(
            "stunning_palm", new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy, new StatusGrant(StatusKind.Stun, Power: 0, Turns: 1), Hits: 2));
        var (_, goblin, battle) = Battle(goblinHp: 100, chakra: 0, stunningPalm);

        var events = battle.Execute(new SkillCommand(stunningPalm, [goblin])).ToList();

        Assert.Single(events, battleEvent => battleEvent is StatusApplied);
        Assert.True(events.FindIndex(battleEvent => battleEvent is StatusApplied) > events.FindLastIndex(battleEvent => battleEvent is EffectApplied));
    }

    [Fact]
    public void Execute_MonkHitsLand_GainsChakraPerHit()
    {
        var (monk, goblin, battle) = Battle(goblinHp: 100, chakra: 0);

        battle.Execute(new SkillCommand(_palm, [goblin]));

        Assert.Equal(2, monk.Resources.AmountOf("chakra"));
    }

    [Fact]
    public void Execute_AttackLands_GainsOneChakra()
    {
        var (monk, goblin, battle) = Battle(goblinHp: 100, chakra: 0);

        battle.Execute(new AttackCommand(goblin));

        Assert.Equal(1, monk.Resources.AmountOf("chakra"));
    }

    // Ferocity gains 2 per hit taken, so the two hits of a strike aimed at the barbarian give 4.
    [Fact]
    public void Execute_MultiHitOnBarbarian_FerocityPerHit()
    {
        var ferocity = new ResourceDefinition("ferocity", "Ferocity", Max: 10) { Gains = new ResourceGains { PerHitTaken = 2 } };
        var spar = Skill("spar", new Effect(EffectKind.Damage, 10, Targeting.SingleAlly, Hits: 2));
        var (monk, _, _) = Battle(goblinHp: 100, chakra: 0, spar);
        var barbarian = Member("jazog", "barbarian", ferocity);
        var battle = new BattleState([monk, barbarian, Goblin(hp: 100)], new Inventory(), new FixedSequenceRandom(2, 1));

        battle.Execute(new SkillCommand(spar, [barbarian]));

        Assert.Equal(4, barbarian.Resources.AmountOf("ferocity"));
    }

    private static (Combatant Monk, Combatant Goblin, BattleState Battle) Battle(int goblinHp, int chakra, params SkillDefinition[] extraSkills)
    {
        var monk = Member("zaruk", "monk", _chakra, [_flurry, _palm, .. extraSkills]);
        monk.ChangeResource("chakra", chakra, []);
        var goblin = Goblin(goblinHp);
        return (monk, goblin, new BattleState([monk, goblin], new Inventory(), new FixedSequenceRandom(1)));
    }

    private static Combatant Member(string id, string classId, ResourceDefinition resource, params SkillDefinition[] skills) =>
        Combatant.FromPartyMember(PartyMember.Recruit(
            new PartyMemberBuilder().WithId(id).BuildDefinition(),
            new ClassBuilder().WithId(classId).WithResources(resource).WithSkills([.. skills.Select(skill => skill.Id)]).Build(),
            skills.ToDictionary(skill => skill.Id)));

    private static Combatant Goblin(int hp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithStats(maxHp: hp, attack: 20, defense: 2).Build());

    private static SkillDefinition Skill(string id, Effect effect, params ResourceChange[] changes) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", ResourceChanges = changes, Effect = effect };
}
