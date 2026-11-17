namespace Jrpg.Editor.Party;

/// <summary>
/// What a rule's editable id combo lists (strain's resource, dominance's rival): the class's other resources' ids, and
/// the id as typed when it isn't one of them. A combo whose text matches an item that leaves its list clears the text
/// and writes the empty id back, so the typed id is always listed: an id renamed or removed stays until retyped.
/// </summary>
internal static class ResourceIdChoices
{
    /// <param name="classmateIds">The class's other resources' ids as typed, none empty.</param>
    /// <param name="typedId">The rule's id as typed.</param>
    /// <returns>
    /// <paramref name="classmateIds"/> itself while it lists the typed id or that id is empty, so the combo keeps its
    /// list; otherwise a new list with the typed id last.
    /// </returns>
    public static IReadOnlyList<string> Of(IReadOnlyList<string> classmateIds, string typedId) =>
        typedId.Length == 0 || classmateIds.Contains(typedId) ? classmateIds : [.. classmateIds, typedId];
}
