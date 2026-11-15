using Jrpg.Core.Effects;

namespace Jrpg.Data.Effects;

/// <summary>
/// The rules for the effects strip a skill or item names for itself (<c>effectFile</c>, decisions 706, 707): a name
/// when present, and only on an effect that damages, the only kind that plays a strip so far. Whether the image is in
/// <c>effects/</c> is the startup art check's (decision 709).
/// </summary>
internal static class OwnEffectStripValidator
{
    /// <param name="effectFile">The strip the skill or item names; null when it plays its user's.</param>
    /// <param name="effect">The skill's or item's effect; null for an item without one.</param>
    public static string? FindProblem(string? effectFile, Effect? effect)
    {
        if (effectFile is null)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(effectFile))
        {
            return "effectFile must name a strip in effects/ when present.";
        }

        return effect is { Kind: EffectKind.Damage } ? null : "effectFile is only for a damaging effect (decision 707).";
    }
}
