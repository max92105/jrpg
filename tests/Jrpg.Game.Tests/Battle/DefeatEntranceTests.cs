using Jrpg.Game.Battle;

namespace Jrpg.Game.Tests.Battle;

// Legacy DefeatScreen: the title fades in over 0.6 s; the choices fade in over 0.3 s from 0.7 s.
public sealed class DefeatEntranceTests
{
    private readonly DefeatEntrance _entrance = new();

    [Fact]
    public void Update_BeforeChoices_ChoicesHidden()
    {
        _entrance.Update(TimeSpan.FromSeconds(0.6));

        Assert.Equal(1, _entrance.TitleOpacity);
        Assert.Equal(0, _entrance.ChoicesOpacity);
        Assert.False(_entrance.IsComplete);
    }

    [Fact]
    public void Update_PastDelay_ChoicesShown()
    {
        _entrance.Update(TimeSpan.FromSeconds(1.0));

        Assert.Equal(1, _entrance.ChoicesOpacity);
        Assert.True(_entrance.IsComplete);
    }

    [Fact]
    public void Update_HalfwayThroughTitleFade_IsHalfVisible()
    {
        _entrance.Update(TimeSpan.FromSeconds(0.3));

        Assert.Equal(0.5f, _entrance.TitleOpacity, 3);
    }

    [Fact]
    public void Update_HalfwayThroughChoicesFade_IsHalfVisible()
    {
        _entrance.Update(TimeSpan.FromSeconds(0.85));

        Assert.Equal(0.5f, _entrance.ChoicesOpacity, 3);
    }
}
