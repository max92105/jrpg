using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class TabGlyphShapeTests
{
    private static readonly TabGlyph[] _glyphs = Enum.GetValues<TabGlyph>();

    [Fact]
    public void Pixels_EachGlyph_AreTwelveByTwelve()
    {
        Assert.All(_glyphs, glyph => Assert.Equal(TabGlyphShape.Size * TabGlyphShape.Size, TabGlyphShape.Pixels(glyph).Length));
    }

    [Fact]
    public void Pixels_EachGlyph_HasSomeOpaquePixels()
    {
        Assert.All(_glyphs, glyph => Assert.Contains(Color.White, TabGlyphShape.Pixels(glyph)));
    }

    [Fact]
    public void Pixels_EachGlyph_DiffersFromTheOthers()
    {
        var shapes = _glyphs.Select(glyph => string.Concat(TabGlyphShape.Pixels(glyph).Select(pixel => pixel.A > 0 ? '#' : '.')));

        Assert.Equal(_glyphs.Length, shapes.Distinct().Count());
    }
}
