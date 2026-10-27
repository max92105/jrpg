using Jrpg.Core.Stats;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Menus;

/// <summary>
/// What each primary gives, as the Status page writes it beside the primary (decision 296: a primary is never
/// shown alone): Core's conversions at the member's value against the same at 0, so a rate changed in
/// <c>rules.json</c> shows.
/// </summary>
internal static class AttributeCaptions
{
    /// <returns>One caption per primary, in <see cref="StatKinds.Primaries"/> order.</returns>
    public static string[] Of(PrimaryStats primaries, StatRules rules, UiMessages messages)
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
            messages.Format(MessageIds.StatusWillGives, Percent("resist", resistances.Fire)),
        ];
    }

    // A permille as the whole percent the page writes (ADR 0026), signed.
    private static MessageArgument Percent(string name, int permille) => Signed(name, StatFormula.WholePercentOf(permille));

    // "+5" or "-1": a primary below 0 (gear that takes it away) gives a loss, and reads as one.
    private static MessageArgument Signed(string name, int value) =>
        new(name, value >= 0 ? $"+{NumberText.Of(value)}" : NumberText.Of(value));
}
