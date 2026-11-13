using System.Text.Json.Serialization.Metadata;

namespace Jrpg.Data.Content;

/// <summary>
/// The one loop the optional field filters share, whether a field was added after the content was written or its
/// default carries a meaning (decision 586): each filter names its fields and the value that leaves one out of a file.
/// </summary>
internal static class OptionalFieldFilter
{
    /// <param name="typeInfo">The type being set up for the content JSON options.</param>
    /// <param name="fields">The record type and the JSON name of each field.</param>
    /// <param name="isAbsent">Whether a value is left out of the file.</param>
    public static void SkipWhen(JsonTypeInfo typeInfo, IEnumerable<(Type Type, string Field)> fields, Func<object?, bool> isAbsent)
    {
        foreach (var (type, field) in fields)
        {
            if (typeInfo.Type == type)
            {
                typeInfo.Properties.Single(property => property.Name == field).ShouldSerialize = (_, value) => !isAbsent(value);
            }
        }
    }
}
