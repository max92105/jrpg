using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Resources;
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
/// <param name="Speed">Their speed.</param>
/// <param name="Resources">Each of the class's resources: its name in caps, and its max, or for a lasting one its
/// amount out of its max (ADR 0024).</param>
/// <param name="Skills">Each skill they have, in menu order: its name, and its description.</param>
internal sealed record StatusSheet(
    string Name,
    string ClassName,
    string PortraitFile,
    string[] Progress,
    string[] Stats,
    string[] Worn,
    (int Earned, int Needed) Experience,
    string Speed,
    StatusLine[] Resources,
    StatusLine[] Skills)
{
    /// <param name="member">The shown member.</param>
    /// <param name="catalog">Class names and the experience table.</param>
    /// <param name="labels">The highest level's and an empty slot's text, and how a lasting amount is written.</param>
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
            ExperienceProgress(member.Level, member.Experience, table),
            NumberText.Of(member.Speed),
            [.. member.Class.Resources.Select(resource => new StatusLine(resource.Name.ToUpperInvariant(), AmountText(member, resource, labels)))],
            [.. member.Skills.Select(skill => new StatusLine(skill.Name, skill.Description))]);
    }

    // A resource whose max comes from speed has as many points as the member's speed gives charges.
    private static string AmountText(PartyMember member, ResourceDefinition resource, StatusLabels labels)
    {
        var max = resource.MaxFromSpeed is { } scaling ? ReadinessFormula.Charges(member.Speed, scaling.SpeedPerCharge) : resource.Max;
        return resource.IsLasting ? labels.ValueOfMax(member.LastingAmounts[resource.Id], max) : NumberText.Of(max);
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
