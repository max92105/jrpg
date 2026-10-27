using Jrpg.Core.Resources;
using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// What each primary gives, as the Status page writes it beside the primary (decision 296: a primary is never shown
/// alone): Core's conversions at the member's value against the same at 0, so a rate changed in <c>rules.json</c>
/// shows. Will's caption also names the points it adds to each resource that follows it.
/// </summary>
internal static class AttributeCaptions
{
    /// <returns>One caption per primary, in <see cref="StatKinds.Primaries"/> order.</returns>
    /// <param name="primaries">The member's primaries, gear included.</param>
    /// <param name="rules">The conversions' rates.</param>
    /// <param name="resources">The member's class resources, for the ones Will raises.</param>
    /// <param name="messages">How each caption is written.</param>
    public static string[] Of(PrimaryStats primaries, StatRules rules, IReadOnlyList<ResourceDefinition> resources, UiMessages messages)
    {
        var resistances = StatFormula.ResistancesFrom(primaries, Resistances.None, rules);
        return
        [
            messages.Format(MessageIds.StatusPowerGives, Percent("damage", StatFormula.DamagePermille(primaries.Power, rules) - StatFormula.DamagePermille(0, rules))),
            messages.Format(
                MessageIds.StatusAgilityGives,
                Percent("crit", StatFormula.CritChancePermille(primaries.Agility, rules) - StatFormula.CritChancePermille(0, rules)),
                Percent("dodge", StatFormula.DodgePermille(primaries.Agility, 0, rules) - StatFormula.DodgePermille(0, 0, rules))),
            messages.Format(MessageIds.StatusIntelligenceGives, Percent("status", StatFormula.StatusPermille(primaries.Intelligence, rules) - StatFormula.StatusPermille(0, rules))),
            messages.Format(
                MessageIds.StatusConstitutionGives,
                Signed("hp", StatFormula.MaxHp(100, primaries.Constitution, rules) - 100),
                Percent("resist", resistances.Blunt)),
            WillCaption(primaries.Will, resistances.Fire, resources, messages),
        ];
    }

    // The magical resistance first, then "+1 mana" for each resource Will raises, in the class's order.
    private static string WillCaption(int will, int magicalResistancePermille, IReadOnlyList<ResourceDefinition> resources, UiMessages messages)
    {
        IEnumerable<string> parts =
        [
            messages.Format(MessageIds.StatusWillGives, Percent("resist", magicalResistancePermille)),
            .. resources.Select(resource => ResourcePart(will, resource, messages)).OfType<string>(),
        ];
        return string.Join(messages.Text(MessageIds.StatusCaptionSeparator), parts);
    }

    private static string? ResourcePart(int will, ResourceDefinition resource, UiMessages messages) =>
        resource.WillScaling is { } scaling
            ? messages.Format(
                MessageIds.StatusWillGivesResource,
                Signed("points", WillFormula.BonusPoints(will, scaling.WillPerPoint)),
                new MessageArgument("resource", resource.Name.ToLowerInvariant()))
            : null;

    // A permille as the whole percent the page writes (ADR 0026), signed.
    private static MessageArgument Percent(string name, int permille) => Signed(name, StatFormula.WholePercentOf(permille));

    // "+5" or "-1": a primary below 0 (gear that takes it away) gives a loss, and reads as one.
    private static MessageArgument Signed(string name, int value) =>
        new(name, value >= 0 ? $"+{NumberText.Of(value)}" : NumberText.Of(value));
}
