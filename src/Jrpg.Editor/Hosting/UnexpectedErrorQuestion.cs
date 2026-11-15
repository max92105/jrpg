namespace Jrpg.Editor.Hosting;

/// <summary>
/// The unexpected-error box's words (ADR 0050): the error, where its report is, what is unsaved and what each answer
/// does. The smoke script finds the box by <see cref="Title"/>.
/// </summary>
internal static class UnexpectedErrorQuestion
{
    public const string Title = "Unexpected error";

    /// <param name="report">The error and its report's file.</param>
    /// <param name="unsavedChanges">The close question's sentence naming what is unsaved; null when nothing is.</param>
    public static string Text(UnexpectedErrorReport report, string? unsavedChanges) =>
        string.Join(
            Environment.NewLine + Environment.NewLine,
            $"The editor hit an error it didn't expect: {report.Exception.GetType().Name}: {report.Exception.Message}",
            report.File is { } file ? $"Report: {file}" : "The report couldn't be written.",
            $"{unsavedChanges ?? "Nothing is unsaved."} A window that was open may lose what wasn't applied.",
            "Yes: keep working, and save with Save all (Ctrl+Shift+S); if something looks wrong after, save and restart. "
                + "No: close the editor (it asks about unsaved work first).");
}
