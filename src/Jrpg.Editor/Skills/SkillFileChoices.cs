namespace Jrpg.Editor.Skills;

/// <summary>
/// What one file box of the Skills page offers (decision 442): the empty entry, the folder's files, then the files the
/// skills name that the folder lacks, as the Effect strips page lists the records whose image is missing. A box over a
/// value its list lacks keeps showing the previously chosen skill's file, so every value a skill can hold is listed.
/// </summary>
internal static class SkillFileChoices
{
    /// <param name="inFolder">The folder's files in the order its box lists them.</param>
    /// <param name="named">The files the draft's skills name in this box, empty for none.</param>
    /// <returns>
    /// The empty entry (no icon, the actor's attack strip, or a new skill's sound before one is picked), the folder's
    /// files, then each named file the folder lacks, once, in ordinal order.
    /// </returns>
    public static IReadOnlyList<string> Of(IReadOnlyList<string> inFolder, IEnumerable<string> named) =>
        ["", .. inFolder, .. named
            .Where(file => file.Length > 0 && !inFolder.Contains(file, StringComparer.Ordinal))
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)];
}
