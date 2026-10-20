using Jrpg.Data.Content;
using Jrpg.Game.Rendering;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Rendering;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class FontLibraryTests : IDisposable
{
    private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), $"jrpg-fonts-{Guid.NewGuid():N}");

    public void Dispose()
    {
        if (Directory.Exists(_contentRoot))
        {
            Directory.Delete(_contentRoot, recursive: true);
        }
    }

    [Fact]
    public void Load_RepositoryContent_GivesAFont()
    {
        using var fonts = FontLibrary.Load(Path.Combine(AppContext.BaseDirectory, "content"));

        Assert.NotNull(fonts.FontAt(40));
    }

    [Fact]
    public void Load_FolderWithoutFont_ThrowsNamingTheFolder()
    {
        Directory.CreateDirectory(Path.Combine(_contentRoot, "art", "fonts"));

        var exception = Assert.Throws<ContentValidationException>(() => FontLibrary.Load(_contentRoot));

        Assert.Equal("art/fonts: there's no .ttf font.", exception.Message);
    }

    [Fact]
    public void Load_FileThatIsNotAFont_ThrowsNamingTheFile()
    {
        var fontsFolder = Path.Combine(_contentRoot, "art", "fonts");
        Directory.CreateDirectory(fontsFolder);
        File.WriteAllText(Path.Combine(fontsFolder, "bad.ttf"), "<html>not a font</html>");

        var exception = Assert.Throws<ContentValidationException>(() => FontLibrary.Load(_contentRoot));

        Assert.Equal("art/fonts/bad.ttf: not a readable font.", exception.Message);
    }

    [Fact]
    public void Load_TruncatedFont_ThrowsNamingTheFile()
    {
        var fontsFolder = Path.Combine(_contentRoot, "art", "fonts");
        Directory.CreateDirectory(fontsFolder);
        var font = File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "content", "art", "fonts", "Lato-Regular.ttf"));
        File.WriteAllBytes(Path.Combine(fontsFolder, "cut.ttf"), font[..1024]);

        var exception = Assert.Throws<ContentValidationException>(() => FontLibrary.Load(_contentRoot));

        Assert.Equal("art/fonts/cut.ttf: not a readable font.", exception.Message);
    }
}
