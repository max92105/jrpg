using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Randomness;
using Jrpg.Core.Session;
using Jrpg.Data.Content;
using static System.FormattableString;

namespace Jrpg.Simulations.Loot;

/// <summary>
/// Rolls the goblin trio's drops once per seed, with no battle fought: drops don't depend on how the fight went (ADR
/// 0030). The summary shows what the placeholder drop chances and rarity weights give before they are tuned; the
/// assertions read the content's own numbers, so tuning them doesn't break the simulation.
/// </summary>
public sealed class LootSimulation(ITestOutputHelper output)
{
    private const int Victories = 2_000;
    private const string EncounterId = "goblin_trio";
    private const string PotionId = "potion";

    // In potions per goblin: about five standard deviations while a potion drops one at a time; a quantity range widens
    // the spread, so a retune that adds one may need a wider bound.
    private const double PotionRateTolerance = 0.03;

    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
    private static readonly List<DropTable> _tables = TablesOf(EncounterId);
    private static readonly List<Drops> _drops = [.. Enumerable.Range(0, Victories).Select(seed => GameSession.DropRollFor(_catalog, new SeededRandom(seed)).Roll(_tables, luckPermille: 0))];

    // Unique has no row in the table, so the equality also says it never rolls (decision 339).
    [Fact]
    public void GoblinTrio_TwoThousandVictories_RollsExactlyTheWeightedRarities()
    {
        var gear = _drops.SelectMany(drops => drops.Gear).ToList();

        WriteSummary(gear);
        var weighted = _catalog.Rules.Loot.Rarities.Where(row => row.Value.Weight > 0).Select(row => row.Key).Order();
        Assert.Equal(weighted, gear.Select(piece => piece.Rarity).Distinct().Order());
    }

    [Fact]
    public void GoblinTrio_TwoThousandVictories_PotionRateNearContent()
    {
        var entries = _tables.SelectMany(table => table.Entries).Where(entry => entry.ItemId == PotionId).ToList();
        Assert.NotEmpty(entries);

        var expected = entries.Sum(entry => entry.Chance / 100.0 * (entry.MinQuantity + entry.MaxQuantity) / 2) / entries.Count;
        var observed = _drops.Average(drops => CountOf(drops, PotionId)) / entries.Count;
        output.WriteLine(Invariant($"{PotionId} per goblin: expected {expected:F3}, rolled {observed:F3}"));
        Assert.InRange(observed, expected - PotionRateTolerance, expected + PotionRateTolerance);
    }

    private static List<DropTable> TablesOf(string encounterId) =>
    [
        .. _catalog.Encounters[encounterId].EnemyIds
            .Select(enemyId => _catalog.Enemies[enemyId].DropTableId)
            .OfType<string>()
            .Select(tableId => _catalog.DropTables[tableId]),
    ];

    private static int CountOf(Drops drops, string itemId) =>
        drops.Stacks.Where(stack => stack.Item.Id == itemId).Sum(stack => stack.Quantity) + drops.Gear.Count(piece => piece.Base.Id == itemId);

    private void WriteSummary(List<GearPiece> gear)
    {
        // Invariant culture, so a baseline copied from one machine compares with another's.
        output.WriteLine($"{EncounterId}: drops over {Victories} victories, no battle fought");
        output.WriteLine(Invariant($"  Items per victory    {_drops.Average(drops => drops.Stacks.Sum(stack => stack.Quantity) + drops.Gear.Count),6:F2}"));
        output.WriteLine(Invariant($"  Gear per victory     {(double)gear.Count / Victories,6:F2}"));
        output.WriteLine(Invariant($"  Victories per piece  {(double)Victories / Math.Max(gear.Count, 1),6:F2}"));
        foreach (var itemId in _tables.SelectMany(table => table.Entries).Select(entry => entry.ItemId).Distinct())
        {
            output.WriteLine(Invariant($"  {itemId,-20} {_drops.Average(drops => CountOf(drops, itemId)),6:F3} per victory"));
        }

        WriteRarityShares(gear);
    }

    private void WriteRarityShares(List<GearPiece> gear)
    {
        output.WriteLine($"  Rarity of {gear.Count} pieces:");
        foreach (var (rarity, rule) in _catalog.Rules.Loot.Rarities.OrderBy(row => row.Key))
        {
            var pieces = gear.Where(piece => piece.Rarity == rarity).ToList();
            var mostAffixes = pieces.Count(piece => piece.Affixes.Count == rule.MaxAffixes);
            output.WriteLine(Invariant(
                $"    {rarity,-8} {100.0 * pieces.Count / Math.Max(gear.Count, 1),5:F1} %  ({100.0 * mostAffixes / Math.Max(pieces.Count, 1),5:F1} % with {rule.MaxAffixes} affixes)"));
        }
    }
}
