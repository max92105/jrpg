using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// Legacy VictoryScreen: the title over 0.45 s, the reward bar after 0.25 s, the first card after 0.45 s and the next
// 0.1 s apart, each fading in over 0.3 s; the bars fill 0.5 s after one more stagger past the last card's start.
public sealed class VictoryEntranceTests
{
    private readonly VictoryEntrance _entrance = new(cardCount: 2);

    [Fact]
    public void Update_BeforeFirstCard_CardHidden()
    {
        _entrance.Update(TimeSpan.FromSeconds(0.4));

        Assert.Equal(0, _entrance.CardOpacity(0));
        Assert.True(_entrance.RewardOpacity > 0);
    }

    [Fact]
    public void Update_HalfwayThroughSecondCardFade_IsHalfVisible()
    {
        _entrance.Update(TimeSpan.FromSeconds(0.55 + 0.15));

        Assert.Equal(0.25f / 0.3f, _entrance.CardOpacity(0), 3);
        Assert.Equal(0.5f, _entrance.CardOpacity(1), 3);
    }

    [Fact]
    public void Update_PastPause_BarsStartFilling()
    {
        _entrance.Update(TimeSpan.FromSeconds(0.45 + 0.2 + 0.5 + 0.1));

        Assert.True(_entrance.IsComplete);
        Assert.Equal(TimeSpan.FromSeconds(0.1), _entrance.BarTime);
    }

    [Fact]
    public void Skip_WhileComingIn_ShowsEverything()
    {
        _entrance.Update(TimeSpan.FromSeconds(0.1));

        _entrance.Skip();

        Assert.True(_entrance.IsComplete);
        Assert.Equal((1f, 0f, 1f, 1f), (_entrance.TitleOpacity, _entrance.TitleOffset, _entrance.RewardOpacity, _entrance.CardOpacity(1)));
    }
}
