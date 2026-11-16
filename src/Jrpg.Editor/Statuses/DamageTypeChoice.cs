using System.Text.Json;
using Jrpg.Core.Stats;

namespace Jrpg.Editor.Statuses;

/// <summary>
/// One entry of a damage tick's Damage type box: a type, or none, which ignores resistances (decision 803), so a
/// typeless tick such as Poison's can be chosen back after a type was picked.
/// </summary>
/// <param name="Type">The type the tick meets resistance as; null for none.</param>
/// <param name="Line">"fire", spelled as <c>statuses.json</c> spells it, or "(none: ignores resistances)".</param>
internal sealed record DamageTypeChoice(DamageType? Type, string Line)
{
    /// <summary>None first, then Core's types in their order.</summary>
    public static IReadOnlyList<DamageTypeChoice> All { get; } =
    [
        new(null, "(none: ignores resistances)"),
        .. Enum.GetValues<DamageType>().Select(type => new DamageTypeChoice(type, Spelled(type))),
    ];

    private static string Spelled(DamageType type) => JsonNamingPolicy.CamelCase.ConvertName(type.ToString());
}
