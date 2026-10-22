using Jrpg.Core.Geometry;
using Jrpg.Game.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Exploration;

public sealed class FieldSpriteFrameTests
{
    [Fact]
    public void SourceOf_FacingDownFirstFrame_IsTheDownRowsFirstCell()
    {
        Assert.Equal(new Rectangle(0, 0, 48, 48), FieldSpriteFrame.SourceOf(Facing.Down, 0));
    }

    [Fact]
    public void SourceOf_ThirdFrame_MovesTwoFramesRight()
    {
        // The up row is the fourth (WalkAnimation.RowOf).
        Assert.Equal(new Rectangle(96, 144, 48, 48), FieldSpriteFrame.SourceOf(Facing.Up, 2));
    }
}
