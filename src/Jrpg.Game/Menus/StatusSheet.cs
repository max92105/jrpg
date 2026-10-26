using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// What the Status page shows of one member, as text, built when the shown member changes so drawing formats nothing.
/// The arrays follow <see cref="StatusLabels"/>' order.
/// </summary>
/// <param name="Name">The member's name.</param>
/// <param name="ClassName">Their class's name.</param>
/// <param name="PortraitFile">Their portrait's file name.</param>
/// <param name="Progress">Level, experience, and experience to the next level.</param>
/// <param name="Experience">How far they are through their level, for the experience bar.</param>
/// <param name="Stats">HP (as "value / max"), Attack, Defense.</param>
/// <param name="Worn">What they wear in each slot, or the empty label.</param>
internal sealed record StatusSheet(
    string Name,
    string ClassName,
    string PortraitFile,
    string[] Progress,
    (int Earned, int Needed) Experience,
    string[] Stats,
    string[] Worn)
{
    /// <param name="member">The shown member.</param>
    /// <param name="catalog">Class names and the experience table.</param>
    /// <param name="labels">The max-level and empty-slot texts.</param>
    /// <param name="shownVitals">The HP meter, already showing the member: the HP text comes from it.</param>
    public static StatusSheet Of(PartyMember member, ContentCatalog catalog, StatusLabels labels, MemberVitalBars shownVitals)
    {
        var table = catalog.Rules.Experience;
        var stats = member.TotalStats;
        return new StatusSheet(
            member.Definition.Name,
            catalog.ClassOf(member.Definition).Name,
            member.Definition.PortraitFile,
            [NumberText.Of(member.Level), NumberText.Of(member.Experience), ToNextLevelText(member.Level, member.Experience, table, labels.MaxLevel)],
            ExperienceProgress(member.Level, member.Experience, table),
            [shownVitals.Hp.Value.Text, NumberText.Of(stats.Attack), NumberText.Of(stats.Defense)],
            [member.Weapon?.Name ?? labels.Empty, member.Armor?.Name ?? labels.Empty]);
    }

    /// <summary>
    /// The experience still needed for the next level, or <paramref name="maxLevelText"/> at the highest level.
    /// </summary>
    /// <param name="level">The member's level.</param>
    /// <param name="experience">
    /// Earned toward the next level, as <see cref="PartyMember.Experience"/> counts it.
    /// </param>
    /// <param name="table">How much each level takes.</param>
    /// <param name="maxLevelText">Shown when there is no next level.</param>
    internal static string ToNextLevelText(int level, int experience, ExperienceTable table, string maxLevelText) =>
        table.IsMaxLevel(level) ? maxLevelText : NumberText.Of(table.ExperienceToReachNext(level) - experience);

    /// <summary>
    /// How far the member is through their level, for the experience bar; all the way at the highest level.
    /// </summary>
    /// <param name="level">The member's level.</param>
    /// <param name="experience">Earned toward the next level.</param>
    /// <param name="table">How much each level takes.</param>
    internal static (int Earned, int Needed) ExperienceProgress(int level, int experience, ExperienceTable table) =>
        table.IsMaxLevel(level) ? (1, 1) : (experience, table.ExperienceToReachNext(level));
}
