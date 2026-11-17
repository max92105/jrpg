using System.Windows.Media;
using System.Windows.Media.Imaging;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// A page's decoded images over a decoder the test counts: A.png decodes to a one-pixel image, any other file to null.
public sealed class DecodedArtImagesTests
{
    private readonly List<string> _decodedFiles = [];
    private readonly DecodedArtImages _images;

    public DecodedArtImagesTests() => _images = new DecodedArtImages(Decode);

    [Fact]
    public void Of_SameFileTwice_DecodesOnce()
    {
        var first = _images.Of("A.png");

        var second = _images.Of("A.png");

        Assert.Equal((1, first), (_decodedFiles.Count, second));
    }

    [Fact]
    public void Of_UnreadableFileTwice_DecodesOnceAndIsNull()
    {
        _images.Of("Broken.png");

        var image = _images.Of("Broken.png");

        Assert.Equal((1, (BitmapSource?)null), (_decodedFiles.Count, image));
    }

    [Fact]
    public void Forget_DecodedFile_DecodesItAgain()
    {
        _images.Of("A.png");
        _images.Forget(["A.png"]);

        _images.Of("A.png");

        Assert.Equal(["A.png", "A.png"], _decodedFiles);
    }

    [Fact]
    public void Forget_AnotherFile_KeepsTheDecodedOne()
    {
        _images.Of("A.png");
        _images.Forget(["B.png"]);

        _images.Of("A.png");

        Assert.Equal(["A.png"], _decodedFiles);
    }

    private BitmapSource? Decode(string file)
    {
        _decodedFiles.Add(file);
        return file == "A.png" ? BitmapSource.Create(1, 1, 96, 96, PixelFormats.Bgra32, null, new byte[4], 4) : null;
    }
}
