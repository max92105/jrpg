using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Data.Content;
using static System.FormattableString;

namespace Jrpg.Simulations.Battle;

/// <summary>
/// The Goblin Chief's fight, for tuning its placeholder numbers (decision 410): a lone level 1 leader of each class,
/// then a party of four at a few levels, attacking only, every enemy choosing through its tactics. Win rates are
/// printed, not asserted: no map rolls the Chief (decision 399), so ADR 0017's "a lone leader must win" doesn't bind
/// it.
/// </summary>
public sealed class BossFightSimulation(ITestOutputHelper output)
{
    private const int Battles = 500;
    private const int TurnLimit = 500;
    private const int PartySize = 4;
    private const string EncounterId = "goblin_chief";
    private const string ChiefId = "goblin_chief";
    private const string AttackName = "attack";

    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    // Placeholders: enough to show how the fight moves with levels.
    private static readonly int[] _partyLevels = [1, 3, 5];

    [Fact]
    public void Run_ChiefFights_EveryFightEnds()
    {
        var sets = FightSets().ToList();

        foreach (var (label, fights) in sets)
        {
            WriteRow(label, fights);
        }

        Assert.All(sets, set => Assert.All(set.Fights, fight => Assert.True(fight.IsOver, $"{set.Label}, seed {fight.Seed}: no end in {TurnLimit} turns.")));
    }

    [Fact]
    public void Run_ChiefFights_TheChiefUsesEveryActionOfItsKit()
    {
        var used = FightSets().SelectMany(set => set.Fights).SelectMany(fight => fight.ChiefActions).ToHashSet();

        var chief = _catalog.Enemies[ChiefId];
        var kit = chief.Tactics.Concat(chief.Phases.SelectMany(phase => phase.Tactics)).Select(action => action.SkillId ?? AttackName);
        Assert.All(kit, action => Assert.Contains(action, used));
    }

    [Fact]
    public void Run_ChiefFights_ThePhaseIsReachedInSomeFights()
    {
        Assert.Contains(FightSets().SelectMany(set => set.Fights), fight => fight.IsPhaseEntered);
    }

    private static IEnumerable<(string Label, List<Fight> Fights)> FightSets()
    {
        foreach (var characterClass in _catalog.Classes.Values)
        {
            yield return ($"level 1 {characterClass.Id} alone", Play(() => [StandInLeader.Of(_catalog, characterClass)]));
        }

        foreach (var level in _partyLevels)
        {
            yield return ($"party of {PartySize} at level {level}", Play(() => PartyAt(level)));
        }
    }

    private static List<Fight> Play(Func<IReadOnlyList<PartyMember>> newParty) =>
        [.. Enumerable.Range(0, Battles).Select(seed => PlayFight(newParty(), seed))];

    private static List<PartyMember> PartyAt(int level)
    {
        var definitions = _catalog.PartyMembers.Values.OrderBy(member => member.Id, StringComparer.Ordinal).Take(PartySize);
        var members = definitions.Select(definition => PartyMember.Recruit(definition, _catalog.ClassOf(definition), _catalog.Skills, _catalog.Rules.Stats)).ToList();
        foreach (var member in members)
        {
            while (member.Level < level)
            {
                member.GainExperience(_catalog.Rules.Experience.ExperienceToReachNext(member.Level), _catalog.Rules.Experience);
            }

            member.Rest();
        }

        return members;
    }

    private static Fight PlayFight(IReadOnlyList<PartyMember> members, int seed)
    {
        var party = new PartyState(members, members, _catalog.Rules.Party);
        var enemies = _catalog.Encounters[EncounterId].EnemyIds.Select(id => _catalog.Enemies[id]).ToList();
        var random = new SeededRandom(seed);
        var battle = BattleState.Start(party, enemies, new Inventory(), random);
        var tactics = new EnemyTactics(random, _catalog.Skills);
        var chiefActions = new List<string>();
        var isPhaseEntered = false;

        var turns = 0;
        for (; turns < TurnLimit && !battle.IsOver; turns++)
        {
            var command = battle.Phase == BattlePhase.PartyTurn ? AttackFirstLivingEnemyPolicy.ChooseCommand(battle) : tactics.ChooseCommand(battle);
            var events = battle.Execute(command);
            chiefActions.AddRange(ChiefActionsIn(events));
            isPhaseEntered |= events.OfType<EnemyPhaseEntered>().Any();
        }

        return new Fight(seed, battle.IsOver, battle.Phase == BattlePhase.Victory, turns, members.Sum(member => member.Vitals.Hp), chiefActions, isPhaseEntered);
    }

    private static IEnumerable<string> ChiefActionsIn(IReadOnlyList<BattleEvent> events) =>
        events.OfType<ActionStarted>().Where(started => started.Actor.Enemy?.Id == ChiefId).Select(started => NameOf(started.Command));

    private static string NameOf(BattleCommand command) => command is SkillCommand skill ? skill.Skill.Id : AttackName;

    private void WriteRow(string label, List<Fight> fights)
    {
        var wins = fights.Where(fight => fight.IsWon).ToList();
        // Invariant culture, so a baseline copied from one machine compares with another's.
        var hpLeft = wins.Count == 0 ? "n/a" : Invariant($"{wins.Average(fight => fight.HpLeft):F1}");
        var actions = fights.SelectMany(fight => fight.ChiefActions).ToList();
        var shares = actions.GroupBy(action => action).OrderBy(group => group.Key, StringComparer.Ordinal)
            .Select(group => Invariant($"{group.Key} {100.0 * group.Count() / actions.Count:F0} %"));
        output.WriteLine(Invariant($"{label,-28} {fights.Count} fights, win {100.0 * wins.Count / fights.Count,5:F1} %, turns {fights.Average(fight => fight.Turns),5:F1}, HP left {hpLeft,6}, phase in {100.0 * fights.Count(fight => fight.IsPhaseEntered) / fights.Count,5:F1} %; {string.Join(", ", shares)}"));
    }

    private sealed record Fight(int Seed, bool IsOver, bool IsWon, int Turns, int HpLeft, IReadOnlyList<string> ChiefActions, bool IsPhaseEntered);
}
