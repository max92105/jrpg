using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class TabGlyphShapeTests
{
    private static readonly TabGlyph[] _glyphs = Enum.GetValues<TabGlyph>();

    [Fact]
    public void Pixels_EachGlyph_AreTwelveByTwelve()
    {
        var sizes = _glyphs.Select(glyph => TabGlyphShape.Pixels(glyph).Length);

        Assert.All(sizes, size => Assert.Equal(12 * 12, size));
    }

    [Fact]
    public void Pixels_EachGlyph_HasSomeOpaquePixels()
    {
        var masks = _glyphs.Select(TabGlyphShape.Pixels);

        Assert.All(masks, pixels => Assert.Contains(Color.White, pixels));
    }

    [Fact]
    public void Pixels_EachGlyph_DiffersFromTheOthers()
    {
        var masks = _glyphs.Select(glyph => string.Concat(TabGlyphShape.Pixels(glyph).Select(pixel => pixel.A > 0 ? '#' : '.')));

        Assert.Equal(_glyphs.Length, masks.Distinct().Count());
    }
}
