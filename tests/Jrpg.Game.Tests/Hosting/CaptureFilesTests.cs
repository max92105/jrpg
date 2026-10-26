using Jrpg.Game.Hosting;

namespace Jrpg.Game.Tests.Hosting;

public sealed class CaptureFilesTests : IDisposable
{
    private static readonly DateTime _stamp = new(2026, 10, 25, 21, 7, 3);

    private readonly string _directory = Path.Combine(Path.GetTempPath(), $"jrpg-captures-{Guid.NewGuid():N}");

    public CaptureFilesTests() => Directory.CreateDirectory(_directory);

    public void Dispose() => Directory.Delete(_directory, recursive: true);

    [Fact]
    public void NextPath_EmptyFolder_EndsWithOne()
    {
        var path = new CaptureFiles(_directory).NextPath(_stamp);

        Assert.Equal(Path.Combine(_directory, "capture-20261025-210703-1.png"), path);
    }

    [Fact]
    public void NextPath_FileOfThatNameExists_TakesTheNextNumber()
    {
        var captures = new CaptureFiles(_directory);
        File.WriteAllBytes(captures.NextPath(_stamp), []);

        var path = captures.NextPath(_stamp);

        Assert.Equal(Path.Combine(_directory, "capture-20261025-210703-2.png"), path);
    }
}
