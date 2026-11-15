using Jrpg.Editor.Hosting;

namespace Jrpg.Editor.Tests.Hosting;

// The unexpected-error reports (ADR 0050), over a temporary folder: one file per error, the error output always.
public sealed class UnexpectedErrorLogTests : IDisposable
{
    private static readonly DateTimeOffset _at = new(2026, 11, 15, 10, 30, 5, 123, TimeSpan.Zero);

    private readonly string _folder = Path.Combine(Path.GetTempPath(), $"jrpg-editor-log-{Guid.NewGuid():N}");
    private readonly StringWriter _output = new();

    public void Dispose()
    {
        _output.Dispose();
        if (Directory.Exists(_folder))
        {
            Directory.Delete(_folder, recursive: true);
        }
        else if (File.Exists(_folder))
        {
            File.Delete(_folder);
        }
    }

    [Fact]
    public void Write_AnError_WritesItsTypeMessageAndStack()
    {
        var file = Log().Write(Thrown(), _at);

        var report = File.ReadAllText(file!);
        Assert.Contains("System.InvalidOperationException: The window failed.", report, StringComparison.Ordinal);
        Assert.Contains(nameof(Thrown), report, StringComparison.Ordinal);
    }

    [Fact]
    public void Write_AnError_NamesTheContentRootAndTime()
    {
        var file = Log().Write(Thrown(), _at);

        Assert.Equal(Path.Combine(_folder, "editor-error-2026-11-15-103005-123.txt"), file);
        Assert.StartsWith(
            $"Editor error at 2026-11-15T10:30:05.1230000+00:00{Environment.NewLine}Content: the-content-root",
            File.ReadAllText(file!),
            StringComparison.Ordinal);
    }

    [Fact]
    public void Write_SameMillisecondTwice_WritesTwoFiles()
    {
        var log = Log();
        log.Write(Thrown(), _at);

        var second = log.Write(Thrown(), _at);

        Assert.Equal(Path.Combine(_folder, "editor-error-2026-11-15-103005-123-2.txt"), second);
    }

    [Fact]
    public void Write_FolderMissing_CreatesIt()
    {
        Log().Write(Thrown(), _at);

        Assert.Single(Directory.GetFiles(_folder));
    }

    [Fact]
    public void Write_FolderIsAFile_ReturnsNullAndStillWritesTheOutput()
    {
        File.WriteAllText(_folder, "in the way");

        var file = Log().Write(Thrown(), _at);

        Assert.Null(file);
        Assert.Contains("The window failed.", _output.ToString(), StringComparison.Ordinal);
    }

    [Fact]
    public void AddRepeatCount_ReportGone_RaisesNothing()
    {
        var file = Log().Write(Thrown(), _at)!;
        Directory.Delete(_folder, recursive: true);

        UnexpectedErrorLog.AddRepeatCount(file, 3);

        Assert.False(File.Exists(file));
    }

    private UnexpectedErrorLog Log() => new(_folder, "the-content-root", _output);

    private static InvalidOperationException Thrown()
    {
        try
        {
            throw new InvalidOperationException("The window failed.");
        }
        catch (InvalidOperationException e)
        {
            return e;
        }
    }
}
