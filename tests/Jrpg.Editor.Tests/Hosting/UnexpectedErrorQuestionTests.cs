using Jrpg.Editor.Hosting;

namespace Jrpg.Editor.Tests.Hosting;

// The unexpected-error box's words (ADR 0050).
public sealed class UnexpectedErrorQuestionTests
{
    private static readonly InvalidOperationException _error = new("The window failed.");

    [Fact]
    public void Text_ReportWrittenAndChangesOpen_NamesTheErrorFileAndChanges()
    {
        var report = new UnexpectedErrorReport(_error, "one.txt");

        var text = UnexpectedErrorQuestion.Text(report, "Unsaved changes in map 'forest'.");

        Assert.Contains("InvalidOperationException: The window failed.", text, StringComparison.Ordinal);
        Assert.Contains("Report: one.txt", text, StringComparison.Ordinal);
        Assert.Contains("Unsaved changes in map 'forest'.", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Text_ReportNotWritten_SaysSo()
    {
        var text = UnexpectedErrorQuestion.Text(new UnexpectedErrorReport(_error, null), null);

        Assert.Contains("The report couldn't be written.", text, StringComparison.Ordinal);
    }

    [Fact]
    public void Text_NothingUnsaved_SaysSo()
    {
        var text = UnexpectedErrorQuestion.Text(new UnexpectedErrorReport(_error, null), null);

        Assert.Contains("Nothing is unsaved.", text, StringComparison.Ordinal);
    }
}
