using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Randomness;
using Jrpg.Data.Content;
using static System.FormattableString;

namespace Jrpg.Simulations.Battle;

/// <summary>
/// The new-game fight: the leader alone against the goblin trio, attacking only.
/// No balance target yet (developer decision 18); the summary is a baseline to compare later changes against.
/// </summary>
public sealed class GoblinTrioSimulation(ITestOutputHelper output)
{
    private const int Battles = 1_000;
    private const int TurnLimit = 500;
    private const string LeaderId = "sarxian";
    private const string EncounterId = "goblin_trio";

    [Fact]
    public void GoblinTrio_AttackFirstLivingEnemy_EveryBattleEndsWithinTurnLimit()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        var results = Enumerable.Range(0, Battles).Select(seed => RunBattle(catalog, seed)).ToList();

        WriteSummary(results);
        Assert.All(results, result => Assert.True(result.IsOver, $"Seed {result.Seed} did not end within {TurnLimit} turns."));
    }

    // A fresh leader per battle: battle damage persists on a member (shared Vitals), so reusing one would leak between seeds.
    private static BattleResult RunBattle(ContentCatalog catalog, int seed)
    {
        var definition = catalog.PartyMembers[LeaderId];
        var leader = PartyMember.Recruit(definition, catalog.Skills);
        var party = new PartyState([leader], [leader], catalog.Rules.Party);
        var enemies = catalog.Encounters[EncounterId].EnemyIds.Select(id => catalog.Enemies[id]).ToList();
        var random = new SeededRandom(seed);
        var battle = BattleState.Start(party, enemies, new Inventory(), random);
        var tactics = new EnemyTactics(random);

        var turns = 0;
        for (; turns < TurnLimit && !battle.IsOver; turns++)
        {
            var command = battle.Phase == BattlePhase.PartyTurn ? AttackFirstLivingEnemyPolicy.ChooseCommand(battle) : tactics.ChooseCommand(battle);
            battle.Execute(command);
        }

        return new BattleResult(seed, battle.IsOver, battle.Phase, turns, leader.Vitals.Hp);
    }

    private void WriteSummary(IReadOnlyList<BattleResult> results)
    {
        // Invariant culture, so a baseline copied from one machine compares with another's.
        var wins = results.Where(result => result.Phase == BattlePhase.Victory).ToList();
        var hpLeft = wins.Count == 0 ? "n/a" : Invariant($"{wins.Average(result => result.LeaderHpLeft):F1}");
        output.WriteLine($"{EncounterId}, {LeaderId} alone, attack only, {Battles} seeds");
        output.WriteLine(Invariant($"  Win rate          {100.0 * wins.Count / results.Count,6:F1} %"));
        output.WriteLine(Invariant($"  Average turns     {results.Average(result => result.Turns),6:F1}"));
        output.WriteLine($"  Average HP left   {hpLeft,6} (wins only)");
    }

    private sealed record BattleResult(int Seed, bool IsOver, BattlePhase Phase, int Turns, int LeaderHpLeft);
}
