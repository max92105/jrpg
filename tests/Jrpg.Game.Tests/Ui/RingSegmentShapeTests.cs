using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Ui;

// One slice of a ring 16 px out and 3 px wide, of 48 (decision 626): a short bar at the ring's top.
public sealed class RingSegmentShapeTests
{
    private static readonly RingSegmentShape _shape = new(OuterRadius: 16, Thickness: 3, Count: 48);

    [Fact]
    public void Pixels_SizeOfTheMask_IsWidthByHeight()
    {
        Assert.Equal(_shape.Width * _shape.Height, _shape.Pixels().Length);
    }

    // The ring's middle line, at the slice's middle column: 1.5 px in from the outer edge.
    [Fact]
    public void Pixels_MiddleOfTheBar_IsOpaque()
    {
        var pixels = _shape.Pixels();
        var middleRow = (int)(_shape.Center.Y - 16 + 1.5f);
        var middleColumn = _shape.Width / 2;

        Assert.Equal(Color.White, pixels[(middleRow * _shape.Width) + middleColumn]);
    }

    [Fact]
    public void Pixels_Corner_IsTransparent()
    {
        Assert.Equal(Color.Transparent, _shape.Pixels()[0]);
    }

    // Between the ring's edges but past the slice's side: the next segment draws there.
    [Fact]
    public void Pixels_BesideTheSlice_IsTransparent()
    {
        var pixels = _shape.Pixels();
        var middleRow = (int)(_shape.Center.Y - 16 + 1.5f);

        Assert.Equal(Color.Transparent, pixels[middleRow * _shape.Width]);
    }

    // Past the ring's inner edge, toward its center.
    [Fact]
    public void Pixels_BelowTheInnerEdge_IsTransparent()
    {
        var pixels = _shape.Pixels();
        var lastRow = _shape.Height - 1;

        Assert.Equal(Color.Transparent, pixels[(lastRow * _shape.Width) + (_shape.Width / 2)]);
    }

    [Fact]
    public void Center_IsBelowTheTexture_AtTheRingsCenter()
    {
        Assert.Equal(new Vector2(_shape.Width / 2f, 16 + 1), _shape.Center);
    }
}
