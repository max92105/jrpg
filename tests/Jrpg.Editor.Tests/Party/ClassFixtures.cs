using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Editor.Party;

namespace Jrpg.Editor.Tests.Party;

/// <summary>
/// The Classes page tests' own classes, so no repository number is pinned: every base and growth number of the scholar
/// differs from the others and from a new class's, so a box that drops or swaps one shows in a record compared whole; a
/// second class for the list; the test world holding the two; the Skills tab's draft of three skills; a resource
/// gaining on every trigger; and the stat and gain boxes' texts read in the form's order.
/// </summary>
internal static class ClassFixtures
{
    /// <summary>
    /// A class with one skill that costs nothing (the repository's <c>expose</c>, which a class may name) and no
    /// resources; its id sorts before the scholar's.
    /// </summary>
    public static ClassDefinition Archer { get; } =
        new("archer", "Archer", "Shoots from behind the line.", new ClassStats(120, 12), new ClassStats(10, 1))
        {
            Skills = [new("expose")],
        };

    /// <summary>A class with one resource that isn't lasting, and no skills.</summary>
    public static ClassDefinition Scholar { get; } = new(
        "scholar",
        "Scholar",
        "A reader of old books who fights with what they teach.",
        new ClassStats(MaxHp: 140, Speed: 9) { Accuracy = 3, Primaries = new PrimaryStats(11, 7, 17, 5, 13) },
        new ClassStats(MaxHp: 12, Speed: 2) { Accuracy = 4, Primaries = new PrimaryStats(6, 8, 10, 14, 16) })
    {
        Resources = [new ResourceDefinition("focus", "Focus", Max: 6, StartsAt: 2) { Color = "#5B8CFF" }],
    };

    /// <summary>
    /// The test world with the archer and the scholar as its only classes, the scholar first so a list in id order is
    /// sorted, not read in the dictionary's order.
    /// </summary>
    public static ContentCatalog World { get; } =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world")) with
        {
            Classes = new Dictionary<string, ClassDefinition> { [Scholar.Id] = Scholar, [Archer.Id] = Archer },
        };

    /// <summary>
    /// A skill built for the Skills tab's tests: it spends mana, which the scholar lacks, and its name, Zap, sorts
    /// after taunt while its id sorts before it, so a list by name differs from one by id.
    /// </summary>
    public static SkillDefinition Spark { get; } = World.Skills["expose"] with
    {
        Id = "spark",
        Name = "Zap",
        ResourceChanges = [new ResourceChange("mana", -1)],
    };

    /// <summary>The archer with two skills opposite to id order, taunt at level 1 and expose at 4.</summary>
    public static ClassDefinition SkilledArcher { get; } =
        Archer with { Skills = [new("taunt"), new("expose") { Level = 4 }] };

    /// <summary>
    /// The test world with only three skills, the repository's expose and taunt (neither spends a resource) and
    /// <see cref="Spark"/>, in an order neither by id nor by name, and the skilled archer and the scholar as classes.
    /// </summary>
    public static ContentCatalog SkillsDraft { get; } = World with
    {
        Skills = new Dictionary<string, SkillDefinition>
        {
            [Spark.Id] = Spark,
            ["expose"] = World.Skills["expose"],
            ["taunt"] = World.Skills["taunt"],
        },
        Classes = new Dictionary<string, ClassDefinition> { [SkilledArcher.Id] = SkilledArcher, [Scholar.Id] = Scholar },
    };

    /// <summary>
    /// A resource gaining on every trigger, each by a different number than the others and than 0, one of them a drain,
    /// so a box that drops or swaps a gain shows in a record compared whole.
    /// </summary>
    public static ResourceDefinition Fury { get; } = new("fury", "Fury", Max: 9, StartsAt: 1)
    {
        Color = "#C83232",
        Gains = new ResourceGains
        {
            PerTurn = 1,
            PerHitTaken = 2,
            PerHeavyHit = 3,
            HeavyHitPercent = 40,
            PerHitLanded = 5,
            PerAllyFallen = 6,
            PerEnemyFallen = 7,
            PerBattleFought = -8,
        },
    };

    /// <summary>The eight boxes' texts in the page's order: max HP, speed, accuracy, then the five primaries.</summary>
    public static string[] TextsOf(ClassStatsBoxes boxes) =>
    [
        boxes.MaxHpText, boxes.SpeedText, boxes.AccuracyText, boxes.PowerText, boxes.AgilityText,
        boxes.IntelligenceText, boxes.ConstitutionText, boxes.WillText,
    ];

    /// <summary>
    /// A resource's gain boxes' texts in the form's order: per turn, per hit taken, per heavy hit and its share, per
    /// hit landed, per ally fallen, per enemy fallen, per battle fought.
    /// </summary>
    public static string[] TextsOf(ResourceGainsBoxes boxes) =>
    [
        boxes.PerTurnText, boxes.PerHitTakenText, boxes.PerHeavyHitText, boxes.HeavyHitPercentText,
        boxes.PerHitLandedText, boxes.PerAllyFallenText, boxes.PerEnemyFallenText, boxes.PerBattleFoughtText,
    ];
}
