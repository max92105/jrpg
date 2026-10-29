using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Resources;
using Jrpg.Core.Stats;
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
/// <param name="Attributes">Each primary, gear included, with what it gives, in <see cref="StatKind"/> order.</param>
/// <param name="Combat">
/// HP (as "value / max"), Weapon damage (decision 337), speed, accuracy, crit chance with crit damage, dodge, crowd
/// control resistance: each with what the member wears.
/// </param>
/// <param name="Resistances">One per damage type, in <see cref="DamageType"/> order.</param>
/// <param name="ResistanceTones">How each resistance is tinted, in the same order: weak, capped or plain.</param>
/// <param name="Affixes">
/// Each stat only affixes give, at 0 when nothing gives it, in <see cref="StatKinds.AffixOnly"/>' order.
/// </param>
/// <param name="Resources">
/// Each class resource by name, in the class's order, with its max, or a lasting one's amount out of its max.
/// </param>
/// <param name="Skills">Each skill the member has, in order, with its description.</param>
internal sealed record StatusSheet(
    string Name,
    string ClassName,
    string PortraitFile,
    string[] Progress,
    (int Earned, int Needed) Experience,
    IReadOnlyList<StatusSheet.Attribute> Attributes,
    string[] Combat,
    string[] Resistances,
    ResistanceTone[] ResistanceTones,
    string[] Affixes,
    IReadOnlyList<StatusSheet.Line> Resources,
    IReadOnlyList<StatusSheet.Line> Skills)
{
    /// <summary>Where HP is in <see cref="Combat"/>: its bar runs under it.</summary>
    public const int HpLine = 0;

    /// <param name="member">The shown member.</param>
    /// <param name="catalog">Class names and the experience table.</param>
    /// <param name="labels">The max-level text, and the messages the values are written with.</param>
    /// <param name="shownVitals">The HP meter, already showing the member: the HP text comes from it.</param>
    public static StatusSheet Of(PartyMember member, ContentCatalog catalog, StatusLabels labels, MemberVitalBars shownVitals)
    {
        var table = catalog.Rules.Experience;
        var sheet = member.Sheet;
        var messages = labels.Messages;
        string Value(StatKind kind) => StatText.ValueOf(kind, sheet.ValueOf(kind), messages);
        var crit = messages.Format(
            MessageIds.StatusCritValue, new MessageArgument("chance", Value(StatKind.CritChance)), new MessageArgument("damage", Value(StatKind.CritDamage)));
        return new StatusSheet(
            member.Definition.Name,
            catalog.ClassOf(member.Definition).Name,
            member.Definition.PortraitFile,
            [NumberText.Of(member.Level), NumberText.Of(member.Experience), ToNextLevelText(member.Level, member.Experience, table, labels.MaxLevel)],
            ExperienceProgress(member.Level, member.Experience, table),
            AttributesOf(sheet.Primaries, catalog.Rules.Stats, member.Class.Resources, messages),
            [
                shownVitals.Hp.Value.Text, NumberText.Of(StatFormula.WeaponDamage(sheet)), Value(StatKind.Speed),
                Value(StatKind.Accuracy), crit, Value(StatKind.Dodge), Value(StatKind.CrowdControlResistance),
            ],
            [.. Enum.GetValues<DamageType>().Select(type => Value(StatKinds.ResistanceTo(type)))],
            [.. Enum.GetValues<DamageType>().Select(type => ToneOf(sheet.Resistances.Of(type)))],
            [.. StatKinds.AffixOnly.Select(Value)],
            [.. ResourceStarts.For(member).Select(start => ResourceLine(start, member, labels))],
            [.. member.Skills.Select(skill => new Line(skill.Name, skill.Description))]);
    }

    // The max as a battle starts it (a speed-scaled one from the member's speed); a lasting resource's amount is what
    // the member holds now.
    private static Line ResourceLine(ResourceStart start, PartyMember member, StatusLabels labels)
    {
        var resource = start.Definition;
        var text = resource.IsLasting
            ? labels.LastingAmountText(member.LastingAmounts[resource.Id], start.Max)
            : NumberText.Of(start.Max);
        return new Line(resource.Name.ToUpperInvariant(), text);
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

    private static Attribute[] AttributesOf(PrimaryStats primaries, StatRules rules, IReadOnlyList<ResourceDefinition> resources, UiMessages messages)
    {
        var captions = AttributeCaptions.Of(primaries, rules, resources, messages);
        return [.. StatKinds.Primaries.Select((kind, index) => new Attribute(NumberText.Of(primaries.ValueOf(kind)), captions[index]))];
    }

    /// <summary>A weakness below 0, at the cap or above it capped, anything else plain.</summary>
    internal static ResistanceTone ToneOf(int permille) =>
        permille < 0 ? ResistanceTone.Weak
        : permille >= StatFormula.PermilleOf(Core.Stats.Resistances.CapPercent) ? ResistanceTone.Capped
        : ResistanceTone.Plain;

    /// <summary>A primary's value and what it gives.</summary>
    public sealed record Attribute(string Value, string Caption);

    /// <summary>A name, and the text beside or under it.</summary>
    public sealed record Line(string Name, string Text);
}
