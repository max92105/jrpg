using Jrpg.Core.Battle;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Core.Skills;
using Jrpg.Core.Statuses;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Battle;

// The enemies' choices as the code made them before the enemy behavior plan, recorded from a first run and asserted for
// the whole plan (decision 860): each line is one Execute's ActionStarted, its actor, then its command and targets. The
// party and the enemies share one seeded random, as in the game: a draw added or lost moves every later line.
public sealed class EnemyTacticsReplayTests
{
    private const int TurnLimit = 60;

    private const string LineSeparator = " | ";

    private static readonly SkillDefinition _cleave = Skill("cleave", new Effect(EffectKind.Damage, 10, Targeting.AllEnemies));
    private static readonly SkillDefinition _dressing =
        Skill("dressing", new Effect(EffectKind.RestoreHp, 25, Targeting.SingleAlly));
    private static readonly SkillDefinition _stunningBlow = Skill(
        "stunning_blow",
        TestStatuses.Granting(new Effect(EffectKind.Damage, 8, Targeting.SingleEnemy), TestStatuses.Stun(50, 1)));
    private static readonly SkillDefinition _warCry = Skill(
        "war_cry", TestStatuses.Granting(new Effect(EffectKind.GrantStatus, 0, Targeting.AllAllies), TestStatuses.Guard(20, 2)));
    private static readonly SkillDefinition _taunt = Skill(
        "taunt", TestStatuses.Granting(new Effect(EffectKind.GrantStatus, 0, Targeting.Self), TestStatuses.Taunt(2)));
    private static readonly Dictionary<string, SkillDefinition> _skills =
        new[] { _cleave, _dressing, _stunningBlow, _warCry, _taunt }.ToDictionary(skill => skill.Id);

    public static TheoryData<int, string[]> ChiefLikeRecordings => new()
    {
        { 1, _chiefLikeSeed1 },
        { 2, _chiefLikeSeed2 },
        { 3, _chiefLikeSeed3 },
    };

    public static TheoryData<int, string[]> ForestLikeRecordings => new()
    {
        { 1, _forestLikeSeed1 },
        { 2, _forestLikeSeed2 },
        { 3, _forestLikeSeed3 },
    };

    [Theory]
    [MemberData(nameof(ChiefLikeRecordings))]
    public void ChooseCommand_ChiefLikeFight_ChoosesAsRecorded(int seed, string[] recording)
    {
        var lines = Replay(ChiefLikeFight(), seed);

        Assert.Equal(recording.SelectMany(row => row.Split(LineSeparator)), lines);
    }

    [Theory]
    [MemberData(nameof(ForestLikeRecordings))]
    public void ChooseCommand_ForestLikeFight_ChoosesAsRecorded(int seed, string[] recording)
    {
        var lines = Replay(ForestLikeFight(), seed);

        Assert.Equal(recording.SelectMany(row => row.Split(LineSeparator)), lines);
    }

    private static List<string> Replay(Combatant[] combatants, int seed)
    {
        var random = new SeededRandom(seed);
        var battle = new BattleState(combatants, new Inventory(), random);
        var tactics = new EnemyTactics(random, _skills);
        var lines = new List<string>();
        for (var turn = 0; turn < TurnLimit && !battle.IsOver; turn++)
        {
            var command = battle.Phase == BattlePhase.EnemyTurn ? tactics.ChooseCommand(battle) : PartyCommand(battle);
            lines.Add(LineOf(battle.Execute(command).OfType<ActionStarted>().Single()));
        }

        return lines;
    }

    // Each member attacks the first living enemy; the one who knows Taunt renews it whenever it has run out.
    private static BattleCommand PartyCommand(BattleState battle)
    {
        var actor = battle.CurrentActor;
        return actor.Knows(_taunt) && actor.Statuses.FirstWith(StatusPartKind.DrawAttacks) is null
            ? new SkillCommand(_taunt, [actor])
            : new AttackCommand(battle.LivingOn(BattleSide.Enemies)[0]);
    }

    private static string LineOf(ActionStarted started) => started.Command switch
    {
        AttackCommand attack => $"{LabelOf(started.Actor)}: attack({LabelOf(attack.Target)})",
        SkillCommand skill => $"{LabelOf(started.Actor)}: {skill.Skill.Id}({string.Join(", ", skill.Targets.Select(LabelOf))})",
        _ => throw new ArgumentOutOfRangeException(nameof(started), started.Command, "The fights give no other command."),
    };

    private static string LabelOf(Combatant combatant) => combatant.Member?.Id ?? combatant.Name;

    // The goblin in front falls first, so the chief sees an ally hurt, then fallen, while the party wears it down; its
    // crowd control resistance makes it roll against the taunt.
    private static Combatant[] ChiefLikeFight()
    {
        var allyBelowHalf = new TacticCondition(TacticConditionKind.AllyHpBelow, 50);
        var allyFallen = new TacticCondition(TacticConditionKind.AllyFallen);
        var chief = new EnemyDefinitionBuilder().WithName("Chief").WithStats(maxHp: 140, damage: 10)
            .WithCrowdControlResistance(50).AsBoss()
            .WithTactics(Attack(weight: 4), Use(_cleave, weight: 2), Use(_dressing, weight: 3, allyBelowHalf))
            .WithPhases(new EnemyPhase
            {
                BelowHpPercent = 50,
                Tactics = [Attack(weight: 2), Use(_stunningBlow, weight: 3), Use(_warCry, weight: 2, allyFallen)],
            })
            .Build();
        return
        [
            Member("sarxian"), Taunter("jazog"), Member("teelle"),
            Goblin("GoblinA", maxHp: 70), Combatant.FromEnemy(chief), Goblin("GoblinB", maxHp: 70),
        ];
    }

    private static Combatant[] ForestLikeFight() =>
        [Member("sarxian"), Member("jazog"), Goblin("GoblinA", maxHp: 40), Goblin("GoblinB", maxHp: 40),
            Goblin("GoblinC", maxHp: 40)];

    private static EnemyAction Attack(int weight) => new() { Weight = weight };

    private static EnemyAction Use(SkillDefinition skill, int weight, TacticCondition? when = null) =>
        new() { SkillId = skill.Id, Weight = weight, When = when };

    private static Combatant Member(string id) => Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).Build());

    private static Combatant Taunter(string id) =>
        Combatant.FromPartyMember(new PartyMemberBuilder().WithId(id).WithSkills(_taunt).Build());

    private static Combatant Goblin(string name, int maxHp) =>
        Combatant.FromEnemy(new EnemyDefinitionBuilder().WithName(name).WithStats(maxHp, damage: 8).Build());

    private static SkillDefinition Skill(string id, Effect effect) =>
        new() { Id = id, Name = id, Description = "", IconFile = "", Effect = effect };

    private static readonly string[] _chiefLikeSeed1 =
    [
        "Chief: attack(jazog) | GoblinA: attack(teelle) | teelle: attack(GoblinA) | GoblinB: attack(sarxian)",
        "sarxian: attack(GoblinA) | jazog: taunt(jazog) | Chief: attack(sarxian) | GoblinA: attack(jazog)",
        "teelle: attack(GoblinA) | GoblinB: attack(jazog) | sarxian: attack(GoblinA) | jazog: attack(GoblinA)",
        "Chief: dressing(GoblinA) | GoblinA: attack(jazog) | teelle: attack(GoblinA) | GoblinB: attack(jazog)",
        "sarxian: attack(GoblinA) | jazog: attack(GoblinA) | Chief: cleave(sarxian, jazog, teelle)",
        "GoblinA: attack(teelle) | teelle: attack(GoblinA) | GoblinB: attack(teelle) | sarxian: attack(GoblinA)",
        "jazog: taunt(jazog) | Chief: cleave(sarxian, jazog, teelle) | teelle: attack(Chief) | GoblinB: attack(jazog)",
        "sarxian: attack(Chief) | jazog: attack(Chief) | Chief: attack(teelle) | teelle: attack(Chief)",
        "GoblinB: attack(jazog) | sarxian: attack(Chief) | jazog: attack(Chief)",
        "Chief: cleave(sarxian, jazog, teelle) | teelle: attack(Chief) | GoblinB: attack(sarxian)",
        "sarxian: attack(Chief) | jazog: taunt(jazog) | Chief: war_cry(Chief, GoblinB) | teelle: attack(Chief)",
        "GoblinB: attack(jazog) | sarxian: attack(Chief) | jazog: attack(Chief) | Chief: stunning_blow(jazog)",
        "teelle: attack(Chief) | GoblinB: attack(sarxian) | sarxian: attack(Chief) | Chief: war_cry(Chief, GoblinB)",
        "teelle: attack(Chief) | GoblinB: attack(teelle) | sarxian: attack(Chief) | Chief: war_cry(Chief, GoblinB)",
        "teelle: attack(Chief) | GoblinB: attack(sarxian) | sarxian: attack(GoblinB) | teelle: attack(GoblinB)",
        "GoblinB: attack(teelle) | sarxian: attack(GoblinB) | teelle: attack(GoblinB)",
    ];

    private static readonly string[] _chiefLikeSeed2 =
    [
        "jazog: taunt(jazog) | GoblinA: attack(jazog) | GoblinB: attack(jazog) | sarxian: attack(GoblinA)",
        "teelle: attack(GoblinA) | Chief: attack(sarxian) | jazog: attack(GoblinA) | GoblinA: attack(jazog)",
        "GoblinB: attack(jazog) | sarxian: attack(GoblinA) | teelle: attack(GoblinA) | Chief: attack(jazog)",
        "jazog: attack(GoblinA) | GoblinA: attack(sarxian) | GoblinB: attack(teelle) | sarxian: attack(GoblinA)",
        "teelle: attack(Chief) | Chief: attack(teelle) | jazog: taunt(jazog) | GoblinB: attack(jazog)",
        "sarxian: attack(Chief) | teelle: attack(Chief) | Chief: attack(sarxian) | jazog: attack(Chief)",
        "GoblinB: attack(jazog) | sarxian: attack(Chief) | teelle: attack(Chief)",
        "Chief: cleave(sarxian, jazog, teelle) | jazog: attack(Chief) | GoblinB: attack(teelle)",
        "sarxian: attack(Chief) | teelle: attack(Chief) | Chief: stunning_blow(teelle) | jazog: taunt(jazog)",
        "GoblinB: attack(jazog) | sarxian: attack(Chief) | Chief: war_cry(Chief, GoblinB) | jazog: attack(Chief)",
        "GoblinB: attack(jazog) | sarxian: attack(Chief) | teelle: attack(Chief) | Chief: attack(jazog)",
        "jazog: attack(Chief) | GoblinB: attack(teelle) | sarxian: attack(Chief) | teelle: attack(GoblinB)",
        "jazog: taunt(jazog) | GoblinB: attack(jazog) | sarxian: attack(GoblinB) | teelle: attack(GoblinB)",
        "GoblinB: attack(teelle) | sarxian: attack(GoblinB) | teelle: attack(GoblinB) | GoblinB: attack(teelle)",
        "sarxian: attack(GoblinB) | teelle: attack(GoblinB)",
    ];

    private static readonly string[] _chiefLikeSeed3 =
    [
        "teelle: attack(GoblinA) | GoblinB: attack(sarxian) | sarxian: attack(GoblinA) | Chief: attack(teelle)",
        "GoblinA: attack(jazog) | jazog: taunt(jazog) | teelle: attack(GoblinA) | GoblinB: attack(jazog)",
        "sarxian: attack(GoblinA) | Chief: cleave(sarxian, jazog, teelle) | GoblinA: attack(jazog)",
        "jazog: attack(GoblinA) | teelle: attack(GoblinA) | GoblinB: attack(jazog) | sarxian: attack(GoblinA)",
        "Chief: attack(teelle) | jazog: attack(Chief) | teelle: attack(Chief) | GoblinB: attack(sarxian)",
        "sarxian: attack(Chief) | Chief: cleave(sarxian, jazog, teelle) | jazog: taunt(jazog) | teelle: attack(Chief)",
        "GoblinB: attack(jazog) | sarxian: attack(Chief) | Chief: attack(jazog) | jazog: attack(Chief)",
        "teelle: attack(Chief) | GoblinB: attack(jazog) | sarxian: attack(Chief) | Chief: attack(jazog)",
        "jazog: attack(Chief) | teelle: attack(Chief) | GoblinB: attack(jazog) | sarxian: attack(Chief)",
        "Chief: stunning_blow(jazog) | teelle: attack(Chief) | GoblinB: attack(sarxian) | sarxian: attack(Chief)",
        "Chief: stunning_blow(sarxian) | teelle: attack(Chief) | GoblinB: attack(sarxian) | sarxian: attack(GoblinB)",
        "teelle: attack(GoblinB) | GoblinB: attack(teelle) | sarxian: attack(GoblinB) | teelle: attack(GoblinB)",
        "GoblinB: attack(teelle) | sarxian: attack(GoblinB) | teelle: attack(GoblinB) | GoblinB: attack(teelle)",
        "sarxian: attack(GoblinB)",
    ];

    private static readonly string[] _forestLikeSeed1 =
    [
        "GoblinB: attack(jazog) | GoblinA: attack(sarxian) | GoblinC: attack(sarxian) | sarxian: attack(GoblinA)",
        "jazog: attack(GoblinA) | GoblinB: attack(jazog) | GoblinA: attack(sarxian) | GoblinC: attack(jazog)",
        "sarxian: attack(GoblinA) | jazog: attack(GoblinA) | GoblinB: attack(sarxian) | GoblinC: attack(sarxian)",
        "sarxian: attack(GoblinB) | jazog: attack(GoblinB) | GoblinB: attack(sarxian) | GoblinC: attack(jazog)",
        "sarxian: attack(GoblinB) | jazog: attack(GoblinB) | GoblinC: attack(jazog) | sarxian: attack(GoblinC)",
        "jazog: attack(GoblinC) | GoblinC: attack(jazog) | sarxian: attack(GoblinC) | jazog: attack(GoblinC)",
    ];

    private static readonly string[] _forestLikeSeed2 =
    [
        "GoblinA: attack(sarxian) | GoblinC: attack(sarxian) | sarxian: attack(GoblinA) | jazog: attack(GoblinA)",
        "GoblinB: attack(jazog) | GoblinA: attack(sarxian) | GoblinC: attack(sarxian) | sarxian: attack(GoblinA)",
        "jazog: attack(GoblinA) | GoblinB: attack(sarxian) | GoblinC: attack(jazog) | sarxian: attack(GoblinB)",
        "jazog: attack(GoblinB) | GoblinB: attack(sarxian) | GoblinC: attack(sarxian) | sarxian: attack(GoblinB)",
        "jazog: attack(GoblinB) | GoblinC: attack(jazog) | sarxian: attack(GoblinC) | jazog: attack(GoblinC)",
        "GoblinC: attack(sarxian) | sarxian: attack(GoblinC) | jazog: attack(GoblinC)",
    ];

    private static readonly string[] _forestLikeSeed3 =
    [
        "GoblinC: attack(jazog) | sarxian: attack(GoblinA) | GoblinB: attack(sarxian) | GoblinA: attack(sarxian)",
        "jazog: attack(GoblinA) | GoblinC: attack(jazog) | sarxian: attack(GoblinA) | GoblinB: attack(sarxian)",
        "GoblinA: attack(sarxian) | jazog: attack(GoblinA) | GoblinC: attack(jazog) | sarxian: attack(GoblinB)",
        "GoblinB: attack(jazog) | jazog: attack(GoblinB) | GoblinC: attack(jazog) | sarxian: attack(GoblinB)",
        "GoblinB: attack(sarxian) | jazog: attack(GoblinB) | GoblinC: attack(sarxian) | sarxian: attack(GoblinC)",
        "jazog: attack(GoblinC) | GoblinC: attack(jazog) | sarxian: attack(GoblinC) | jazog: attack(GoblinC)",
    ];
}
