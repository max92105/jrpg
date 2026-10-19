using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class DiamondCropTests
{
    [Fact]
    public void Bake_Corner_IsTransparent()
    {
        var pixels = DiamondCrop.Bake(Solid(Color.Red, 8, 8), 8, 8, size: 4);

        Assert.Equal(Color.Transparent, pixels[0]);
    }

    [Fact]
    public void Bake_Middle_KeepsTheImage()
    {
        var pixels = DiamondCrop.Bake(Solid(Color.Red, 8, 8), 8, 8, size: 4);

        Assert.Equal(Color.Red, pixels[(1 * 4) + 1]);
    }

    [Fact]
    public void Bake_ShrinkingByTwo_AveragesEachBlock()
    {
        // Columns alternate white and black, so every 2x2 block averages to mid grey.
        var source = new Color[8 * 8];
        for (var index = 0; index < source.Length; index++)
        {
            source[index] = index % 2 == 0 ? Color.White : Color.Black;
        }

        var pixels = DiamondCrop.Bake(source, 8, 8, size: 4);

        Assert.Equal(new Color(127, 127, 127, 255), pixels[(1 * 4) + 1]);
    }

    [Fact]
    public void Bake_WideImage_CropsTheCenteredSquare()
    {
        // 12 x 8: the outer two columns on each side are blue, the centered 8 x 8 square is red.
        var source = new Color[12 * 8];
        for (var index = 0; index < source.Length; index++)
        {
            var x = index % 12;
            source[index] = x < 2 || x >= 10 ? Color.Blue : Color.Red;
        }

        var pixels = DiamondCrop.Bake(source, 12, 8, size: 4);

        // The left-edge pixel would take blue from an uncropped image; the diamond's edge covers half of it.
        Assert.Equal(Color.Red * 0.5f, pixels[(1 * 4) + 0]);
    }

    [Fact]
    public void Bake_SourceSmallerThanSize_RepeatsSourcePixels()
    {
        // 2 x 2: red on the left column, blue on the right. Baked at 4, each source pixel covers two by two.
        var source = new[] { Color.Red, Color.Blue, Color.Red, Color.Blue };

        var pixels = DiamondCrop.Bake(source, 2, 2, size: 4);

        Assert.Equal(Color.Red, pixels[(1 * 4) + 1]);
        Assert.Equal(Color.Blue, pixels[(1 * 4) + 2]);
    }

    private static Color[] Solid(Color color, int width, int height) => Enumerable.Repeat(color, width * height).ToArray();
}
