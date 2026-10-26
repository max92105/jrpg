using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// What the Status page shows of one member, as text: built when the shown member changes, so drawing formats nothing.
/// </summary>
/// <param name="Name">The member's name.</param>
/// <param name="ClassName">Their class's name.</param>
/// <param name="PortraitFile">Their portrait.</param>
/// <param name="Progress">Level, experience and experience to the next level, in <see cref="StatusLabels.Progress"/>'s
/// order.</param>
/// <param name="Stats">HP, attack and defense, in <see cref="StatusLabels.Stats"/>'s order.</param>
/// <param name="Worn">What they wear in each of <see cref="EquipmentSheet.Slots"/>, or the empty slot's text.</param>
/// <param name="Experience">How far they are through their level, for the experience bar.</param>
internal sealed record StatusSheet(
    string Name,
    string ClassName,
    string PortraitFile,
    string[] Progress,
    string[] Stats,
    string[] Worn,
    (int Earned, int Needed) Experience)
{
    /// <param name="member">The shown member.</param>
    /// <param name="catalog">Class names and the experience table.</param>
    /// <param name="labels">The highest level's and an empty slot's text.</param>
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
            [shownVitals.Hp.Value.Text, NumberText.Of(stats.Attack), NumberText.Of(stats.Defense)],
            [.. EquipmentSheet.Slots.Select(slot => member.EquippedIn(slot)?.Name ?? labels.Empty)],
            ExperienceProgress(member.Level, member.Experience, table));
    }

    /// <summary>Experience still needed for the next level; <paramref name="maxLevelText"/> at the highest.</summary>
    /// <param name="level">The member's level.</param>
    /// <param name="experience">Earned toward the next level, as <see cref="PartyMember.Experience"/> counts.</param>
    /// <param name="table">How much each level takes.</param>
    /// <param name="maxLevelText">Shown when there is no next level.</param>
    internal static string ToNextLevelText(int level, int experience, ExperienceTable table, string maxLevelText) =>
        table.IsMaxLevel(level) ? maxLevelText : NumberText.Of(table.ExperienceToReachNext(level) - experience);

    /// <summary>How far the member is through their level, for the experience bar; full at the highest level.</summary>
    /// <param name="level">The member's level.</param>
    /// <param name="experience">Earned toward the next level.</param>
    /// <param name="table">How much each level takes.</param>
    internal static (int Earned, int Needed) ExperienceProgress(int level, int experience, ExperienceTable table) =>
        table.IsMaxLevel(level) ? (1, 1) : (experience, table.ExperienceToReachNext(level));
}
