using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Randomness;
using Jrpg.Core.World;
using Jrpg.Data.Content;
using static System.FormattableString;

namespace Jrpg.Simulations.Exploration;

/// <summary>
/// Walks the forest until the first random battle, once per seed, to check the configured odds end to end: the player
/// walks straight from the warp arrival cell and turns a random way when blocked. The grace rule itself is unit-tested;
/// missing it would move the average by only 3 of 28 steps, inside the bound.
/// </summary>
public sealed class ForestEncounterRateSimulation(ITestOutputHelper output)
{
    private const int Seeds = 1_000;
    private const int StepLimit = 1_000;
    private const double Tolerance = 0.2;
    private const string MapId = "forest";

    // Turns draw from their own seeds, offset so a turn seed never equals an encounter seed.
    private const int TurnSeedOffset = Seeds;

    // One cell at the content's walk speed (3 tiles a second); each walk call is one step unless blocked.
    private static readonly TimeSpan _stepTime = TimeSpan.FromSeconds(1.0 / 3);
    private static readonly MoveIntent[] _directions =
        [MoveIntent.FromAxes(1, 0), MoveIntent.FromAxes(0, 1), MoveIntent.FromAxes(-1, 0), MoveIntent.FromAxes(0, -1)];

    // The forest's arrival cell from the start map's warp tiles.
    private static readonly Point2D _arrival = new GridCell(1, 1).Center;

    [Fact]
    public void ForestEncounters_StraightWalk_AverageStepsNearConfiguredOdds()
    {
        var catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));
        var table = catalog.Maps[MapId].Settings.RandomEncounters!;

        var stepsToBattle = Enumerable.Range(0, Seeds).Select(seed => WalkUntilBattle(catalog, seed)).ToList();

        var expected = table.GraceSteps + table.OneInSteps;
        var average = stepsToBattle.Average();
        WriteSummary(stepsToBattle, expected);
        Assert.All(stepsToBattle, steps => Assert.True(steps <= StepLimit, $"A seed walked {StepLimit} steps without a battle."));
        Assert.InRange(average, expected * (1 - Tolerance), expected * (1 + Tolerance));
    }

    private static int WalkUntilBattle(ContentCatalog catalog, int seed)
    {
        var context = new ExplorationContext(catalog, new Inventory(), new WorldState(), new SeededRandom(seed));
        var exploration = new ExplorationState(context, new MapLocation(MapId, _arrival, Facing.Right));
        var turnRandom = new SeededRandom(seed + TurnSeedOffset);
        var direction = _directions[0];
        var steps = 0;

        // Each call either enters a new cell, slides to the map edge within the cell, or is blocked; blocked calls turn.
        for (var calls = 0; steps <= StepLimit && calls < StepLimit * 4; calls++)
        {
            var before = exploration.Location.Position;
            exploration.Walk(direction, _stepTime);
            var after = exploration.Location.Position;
            if (GridCell.Containing(after) != GridCell.Containing(before))
            {
                steps++;
            }

            if (exploration.PendingEncounter is not null)
            {
                return steps;
            }

            if (after == before)
            {
                direction = _directions[turnRandom.NextBelow(_directions.Length)];
            }
        }

        return StepLimit + 1;
    }

    private void WriteSummary(List<int> stepsToBattle, int expected)
    {
        // Invariant culture, so a baseline copied from one machine compares with another's.
        var ordered = stepsToBattle.Order().ToList();
        output.WriteLine($"{MapId}: steps to the first random battle, {Seeds} seeds, straight walk");
        output.WriteLine(Invariant($"  Expected average  {expected,6:F1} (grace + one in N)"));
        output.WriteLine(Invariant($"  Average           {stepsToBattle.Average(),6:F1}"));
        output.WriteLine(Invariant($"  Median            {ordered[ordered.Count / 2],6}"));
    }
}
