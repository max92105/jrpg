using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

public sealed class SquareShrinkTests
{
    [Fact]
    public void Bake_HalfBlackHalfWhite_AveragesEachBlock()
    {
        // 4 x 4: the left two columns black, the right two white; shrunk to 2 x 2, each pixel stands for a 2 x 2 block.
        var source = Enumerable.Range(0, 16).Select(index => index % 4 < 2 ? Color.Black : Color.White).ToArray();

        var pixels = SquareShrink.Bake(source, 4, 4, 2);

        Assert.Equal([Color.Black, Color.White, Color.Black, Color.White], pixels);
    }

    [Fact]
    public void Bake_MixedBlock_TakesTheMean()
    {
        Color[] source = [Color.Black, Color.White, Color.White, Color.Black];

        var pixel = Assert.Single(SquareShrink.Bake(source, 2, 2, 1));

        Assert.Equal(new Color(127, 127, 127, 255), pixel);
    }

    [Fact]
    public void Bake_WiderImage_KeepsTheCenteredSquare()
    {
        // 3 x 1: red, green, blue; the centered 1 x 1 square is the green.
        Color[] source = [Color.Red, Color.Lime, Color.Blue];

        var pixel = Assert.Single(SquareShrink.Bake(source, 3, 1, 1));

        Assert.Equal(Color.Lime, pixel);
    }
}
