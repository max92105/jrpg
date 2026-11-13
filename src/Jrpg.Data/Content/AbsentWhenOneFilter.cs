using System.Text.Json.Serialization.Metadata;
using Jrpg.Core.Loot;

namespace Jrpg.Data.Content;

/// <summary>
/// Leaves a whole-number field out of a file while it is 1, its default, so a table the editor writes reads like a
/// hand-written one (decision 600, decision 586's pattern): a drop entry's quantities, each on its own.
/// </summary>
internal static class AbsentWhenOneFilter
{
    private static readonly (Type Type, string Field)[] _oneByDefault =
    [
        // Decision 343: an entry gives a single copy unless content says otherwise.
        (typeof(DropEntry), "minQuantity"),
        (typeof(DropEntry), "maxQuantity"),
    ];

    public static void SkipOneOptionalFields(JsonTypeInfo typeInfo) =>
        OptionalFieldFilter.SkipWhen(typeInfo, _oneByDefault, value => value is 1);
}
