using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Data.Content;
using static System.FormattableString;

namespace Jrpg.Simulations.Battle;

/// <summary>
/// The new-game fight: a level 1 leader alone against the goblin trio, attacking only, once for each class, since any
/// class can lead (decision 168). A lone level 1 leader must win the start area's fights (question 184, ADR 0017).
/// </summary>
public sealed class GoblinTrioSimulation(ITestOutputHelper output)
{
    private const int Battles = 1_000;
    private const int TurnLimit = 500;
    private const string EncounterId = "goblin_trio";

    // The member whose skills and art the leader has; only the class's numbers matter when attacking only.
    private const string StandInId = "sarxian";

    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    public static TheoryData<string> ClassIds => [.. _catalog.Classes.Keys];

    [Theory]
    [MemberData(nameof(ClassIds))]
    public void GoblinTrio_LoneLevelOneLeaderAttackingOnly_WinsEveryBattle(string classId)
    {
        var results = Enumerable.Range(0, Battles).Select(seed => RunBattle(_catalog.Classes[classId], seed)).ToList();

        WriteSummary(classId, results);
        var losses = results.Where(result => result.Phase != BattlePhase.Victory).Select(result => result.Seed).ToList();
        Assert.True(losses.Count == 0, $"A level 1 {classId} lost or ran out of turns on {losses.Count} of {Battles} seeds, the first {losses.FirstOrDefault()}.");
    }

    // A fresh leader per battle: battle damage persists on a member (shared Vitals), so reusing one would leak between seeds.
    private static BattleResult RunBattle(ClassDefinition characterClass, int seed)
    {
        var leader = PartyMember.Recruit(_catalog.PartyMembers[StandInId], characterClass, _catalog.Skills, _catalog.Rules.Stats);
        var party = new PartyState([leader], [leader], _catalog.Rules.Party);
        var enemies = _catalog.Encounters[EncounterId].EnemyIds.Select(id => _catalog.Enemies[id]).ToList();
        var random = new SeededRandom(seed);
        var battle = BattleState.Start(party, enemies, new Inventory(), random);
        var tactics = new EnemyTactics(random);

        var turns = 0;
        for (; turns < TurnLimit && !battle.IsOver; turns++)
        {
            var command = battle.Phase == BattlePhase.PartyTurn ? AttackFirstLivingEnemyPolicy.ChooseCommand(battle) : tactics.ChooseCommand(battle);
            battle.Execute(command);
        }

        return new BattleResult(seed, battle.Phase, turns, leader.Vitals.Hp);
    }

    private void WriteSummary(string classId, IReadOnlyList<BattleResult> results)
    {
        // Invariant culture, so a baseline copied from one machine compares with another's.
        var wins = results.Where(result => result.Phase == BattlePhase.Victory).ToList();
        var hpLeft = wins.Count == 0 ? "n/a" : Invariant($"{wins.Average(result => result.LeaderHpLeft):F1}");
        output.WriteLine($"{EncounterId}, a level 1 {classId} alone, attack only, {Battles} seeds");
        output.WriteLine(Invariant($"  Win rate          {100.0 * wins.Count / results.Count,6:F1} %"));
        output.WriteLine(Invariant($"  Average turns     {results.Average(result => result.Turns),6:F1}"));
        output.WriteLine($"  Average HP left   {hpLeft,6} (wins only)");
    }

    private sealed record BattleResult(int Seed, BattlePhase Phase, int Turns, int LeaderHpLeft);
}
