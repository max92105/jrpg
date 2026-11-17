using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Stats;
using Jrpg.Editor.Party;

namespace Jrpg.Editor.Tests.Party;

/// <summary>
/// The Classes page tests' own class, so no repository number is pinned: every base and growth number differs from the
/// others and from a new class's, so a box that drops or swaps one shows in a record compared whole; and the boxes'
/// texts read in the page's order.
/// </summary>
internal static class ClassFixtures
{
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
