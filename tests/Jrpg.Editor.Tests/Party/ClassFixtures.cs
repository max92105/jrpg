using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Stats;
using Jrpg.Editor.Party;

namespace Jrpg.Editor.Tests.Party;

/// <summary>
/// The Classes page tests' own classes, so no repository number is pinned: every base and growth number of the scholar
/// differs from the others and from a new class's, so a box that drops or swaps one shows in a record compared whole; a
/// second class for the list; and the boxes' texts read in the page's order.
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
            SkillIds = ["expose"],
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

    /// <summary>The eight boxes' texts in the page's order: max HP, speed, accuracy, then the five primaries.</summary>
    public static string[] TextsOf(ClassStatsBoxes boxes) =>
    [
        boxes.MaxHpText, boxes.SpeedText, boxes.AccuracyText, boxes.PowerText, boxes.AgilityText,
        boxes.IntelligenceText, boxes.ConstitutionText, boxes.WillText,
    ];
}
