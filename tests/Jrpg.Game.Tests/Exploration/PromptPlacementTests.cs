using Jrpg.Game.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Exploration;

public sealed class PromptPlacementTests
{
    private static readonly Point _boxSize = new(60, 30);

    // A two-cell object at column 1, row 3, as ObjectScreenArea gives it.
    private static readonly Rectangle _twoCells = new(48, 144, 96, 48);

    [Fact]
    public void BoxFor_ObjectArea_CentersOnItsWidth()
    {
        var box = PromptPlacement.BoxFor(_twoCells, _boxSize);

        Assert.Equal(48 + 48 - 30, box.X);
        Assert.Equal(_boxSize, box.Size);
    }

    [Fact]
    public void BoxFor_ObjectArea_BottomIs6PixelsAboveItsTop()
    {
        var box = PromptPlacement.BoxFor(_twoCells, _boxSize);

        Assert.Equal(144 - 6, box.Bottom);
    }

    // A tall NPC's area starts above its cell (FS4): the prompt clears the head, not the cell.
    [Fact]
    public void BoxFor_TallNpcArea_SitsAboveTheHead()
    {
        var head = new Rectangle(24, 96, 96, 96);

        var box = PromptPlacement.BoxFor(head, _boxSize);

        Assert.Equal(96 - 6, box.Bottom);
        Assert.Equal(24 + 18, box.X);
    }
}
