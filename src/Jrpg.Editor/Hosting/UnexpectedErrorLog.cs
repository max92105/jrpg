using System.Globalization;
using System.IO;

namespace Jrpg.Editor.Hosting;

/// <summary>
/// Writes each unexpected error's report (ADR 0050): to the error output first, then to a new file in its folder. A
/// report that can't be written never raises an error of its own: the editor is already handling one.
/// </summary>
/// <param name="folder">Where the reports go; made when missing.</param>
/// <param name="contentRoot">The content folder the editor works on, named in each report.</param>
/// <param name="errorOutput">The console's error output, read by Visual Studio and the smoke script.</param>
internal sealed class UnexpectedErrorLog(string folder, string contentRoot, TextWriter errorOutput)
{
    /// <param name="exception">What nothing caught.</param>
    /// <param name="at">When, which names the file.</param>
    /// <returns>The report's path; null when the file couldn't be written.</returns>
    public string? Write(Exception exception, DateTimeOffset at)
    {
        var report = string.Join(
            Environment.NewLine,
            $"Editor error at {at:O}",
            $"Content: {contentRoot}",
            "",
            exception.ToString(),
            "");
        errorOutput.Write(report);
        try
        {
            Directory.CreateDirectory(folder);
            var path = FreePath(at);
            File.WriteAllText(path, report);
            return path;
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            return null;
        }
    }

    /// <summary>Adds to a report how many more times its error came back, as the editor closes.</summary>
    /// <param name="file">The report, from <see cref="Write"/>.</param>
    /// <param name="count">How many times more, from 1.</param>
    public static void AddRepeatCount(string file, int count)
    {
        var line = count == 1 ? "Came back once more." : $"Came back {count} more times.";
        try
        {
            File.AppendAllText(file, line + Environment.NewLine);
        }
        catch (Exception e) when (e is IOException or UnauthorizedAccessException)
        {
            // The count is a note on a report already written: losing it loses no work.
        }
    }

    // Two errors in one millisecond get "-2", "-3" after the name.
    private string FreePath(DateTimeOffset at)
    {
        var name = $"editor-error-{at.ToString("yyyy-MM-dd-HHmmss-fff", CultureInfo.InvariantCulture)}";
        var path = Path.Combine(folder, $"{name}.txt");
        for (var copy = 2; File.Exists(path); copy++)
        {
            path = Path.Combine(folder, $"{name}-{copy}.txt");
        }

        return path;
    }
}
