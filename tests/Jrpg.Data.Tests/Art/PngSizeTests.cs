using Jrpg.Data.Art;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Art;

public sealed class PngSizeTests : IDisposable
{
    private readonly string _path = Path.Combine(Path.GetTempPath(), $"jrpg-png-{Guid.NewGuid():N}.png");

    public void Dispose() => File.Delete(_path);

    [Fact]
    public void Read_RepositoryFieldSprite_ReturnsItsSize()
    {
        // The player sheet: 8 frames of 48 pixels across, 6 rows down (the game uses the first 4, one per facing).
        var size = PngSize.Read(Path.Combine(AppContext.BaseDirectory, "content", "art", "field-sprites", "player.png"));

        Assert.Equal((384, 288), size);
    }

    [Fact]
    public void Read_NotAPng_ThrowsNamingTheFile()
    {
        File.WriteAllText(_path, "GIF89a, not a PNG at all");

        var exception = Assert.Throws<ContentValidationException>(() => PngSize.Read(_path));

        Assert.Equal($"{Path.GetFileName(_path)}: not a PNG image.", exception.Message);
    }

    [Fact]
    public void Read_FileShorterThanHeader_ThrowsNamingTheFile()
    {
        File.WriteAllBytes(_path, [0x89, (byte)'P', (byte)'N', (byte)'G']);

        var exception = Assert.Throws<ContentValidationException>(() => PngSize.Read(_path));

        Assert.Equal($"{Path.GetFileName(_path)}: not a PNG image.", exception.Message);
    }
}
