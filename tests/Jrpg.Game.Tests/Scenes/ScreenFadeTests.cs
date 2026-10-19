using Jrpg.Game.Scenes;

namespace Jrpg.Game.Tests.Scenes;

public sealed class ScreenFadeTests
{
    private readonly ScreenFade _fade = new(TimeSpan.FromSeconds(0.2), TimeSpan.FromSeconds(0.1), TimeSpan.FromSeconds(0.2));

    [Fact]
    public void Update_HalfwayThroughFadeOut_IsHalfBlack()
    {
        _fade.Start();

        _fade.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal(FadePhase.FadingOut, _fade.Phase);
        Assert.Equal(0.5f, _fade.Blackness, 3);
    }

    [Fact]
    public void Update_PastFadeOut_IsFullyBlackDuringHold()
    {
        _fade.Start();

        _fade.Update(TimeSpan.FromSeconds(0.25));

        Assert.Equal(FadePhase.Holding, _fade.Phase);
        Assert.Equal(1f, _fade.Blackness);
    }

    [Fact]
    public void Update_LeftoverPastHold_CarriesIntoFadeIn()
    {
        _fade.Start();
        _fade.Update(TimeSpan.FromSeconds(0.25));

        _fade.Update(TimeSpan.FromSeconds(0.15));

        Assert.Equal(FadePhase.FadingIn, _fade.Phase);
        Assert.Equal(0.5f, _fade.Blackness, 3);
    }

    [Fact]
    public void Update_AfterFadeIn_IsIdle()
    {
        _fade.Start();
        _fade.Update(TimeSpan.FromSeconds(0.25));

        _fade.Update(TimeSpan.FromSeconds(0.3));

        Assert.Equal(FadePhase.Idle, _fade.Phase);
        Assert.Equal(0f, _fade.Blackness);
    }

    [Fact]
    public void Update_Idle_StaysClear()
    {
        _fade.Update(TimeSpan.FromSeconds(1));

        Assert.Equal(FadePhase.Idle, _fade.Phase);
        Assert.Equal(0f, _fade.Blackness);
    }

    [Fact]
    public void Start_WhileFading_Throws()
    {
        _fade.Start();

        Assert.Throws<InvalidOperationException>(_fade.Start);
    }
}
