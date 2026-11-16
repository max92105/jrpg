using Jrpg.Core.Content;

namespace Jrpg.Editor.Skills;

/// <summary>One status the Skills page's grant boxes offer (decision 981).</summary>
/// <param name="Id">The status's id, which a grant row names.</param>
/// <param name="Line">"poison_1: Poison": the id, then the name the game shows.</param>
internal sealed record StatusChoice(string Id, string Line)
{
    /// <returns>
    /// The content's statuses by id (the catalog keeps no file order). A grant names only a status the content holds,
    /// Data resolving each at load, so a box over these never misses its value and shows no other skill's.
    /// </returns>
    public static IReadOnlyList<StatusChoice> AllIn(ContentCatalog content) =>
    [
        .. content.Statuses.Values
            .OrderBy(status => status.Id, StringComparer.Ordinal)
            .Select(status => new StatusChoice(status.Id, $"{status.Id}: {status.Name}")),
    ];
}
