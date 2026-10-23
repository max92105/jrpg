using Jrpg.Data.Art;

namespace Jrpg.Data.Tests.Art;

// 8 walk frames by 4 facings of 48 px: 384 x 192.
public sealed class FieldSpriteLayoutTests
{
    [Fact]
    public void Holds_ExactlyTheWalkFrames_IsTrue()
    {
        Assert.True(FieldSpriteLayout.Holds((384, 192)));
    }

    [Fact]
    public void Holds_OneFrameShortInWidth_IsFalse()
    {
        Assert.False(FieldSpriteLayout.Holds((336, 192)));
    }

    [Fact]
    public void Holds_OneRowShort_IsFalse()
    {
        Assert.False(FieldSpriteLayout.Holds((384, 144)));
    }

    [Fact]
    public void Holds_Larger_IsTrue()
    {
        Assert.True(FieldSpriteLayout.Holds((384, 288)));
    }
}
