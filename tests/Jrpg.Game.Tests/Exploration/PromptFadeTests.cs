using Jrpg.Core.Events;
using Jrpg.Core.Exploration;
using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Game.Exploration;

namespace Jrpg.Game.Tests.Exploration;

public sealed class PromptFadeTests
{
    private static readonly InteractionTarget _chest = TargetNamed("chest");
    private static readonly InteractionTarget _door = TargetNamed("door");

    private readonly PromptFade _fade = new();

    [Fact]
    public void Update_AfterShowAndFadeInTime_IsOpaque()
    {
        _fade.Show(_chest);

        _fade.Update(PromptFade.FadeInTime);

        Assert.Equal(1f, _fade.Opacity);
        Assert.Same(_chest, _fade.Shown);
    }

    [Fact]
    public void Update_HalfwayThroughFadeIn_IsHalfOpaque()
    {
        _fade.Show(_chest);

        _fade.Update(PromptFade.FadeInTime / 2);

        Assert.Equal(0.5f, _fade.Opacity, 3);
    }

    [Fact]
    public void Hide_ThenUpdatePastFadeOut_IsTransparentAndShowsNothing()
    {
        ShowFully(_chest);
        _fade.Hide();

        _fade.Update(PromptFade.FadeOutTime);
        _fade.Update(PromptFade.FadeOutTime);

        Assert.Equal(0f, _fade.Opacity);
        Assert.Null(_fade.Shown);
    }

    [Fact]
    public void Show_OtherTargetWhileShown_KeepsTheOldTargetUntilFadedOut()
    {
        ShowFully(_chest);
        _fade.Show(_door);

        _fade.Update(PromptFade.FadeOutTime / 2);
        var halfway = _fade.Shown;
        _fade.Update(PromptFade.FadeOutTime / 2);
        _fade.Update(PromptFade.FadeInTime);

        Assert.Same(_chest, halfway);
        Assert.Same(_door, _fade.Shown);
        Assert.Equal(1f, _fade.Opacity);
    }

    [Fact]
    public void Show_SameTargetAgainDuringFadeOut_FadesBackInFromCurrentOpacity()
    {
        ShowFully(_chest);
        _fade.Hide();
        _fade.Update(PromptFade.FadeOutTime / 2);

        _fade.Show(_chest);
        _fade.Update(PromptFade.FadeInTime / 4);

        Assert.Equal(0.75f, _fade.Opacity, 3);
        Assert.Same(_chest, _fade.Shown);
    }

    [Fact]
    public void Update_NothingShown_StaysTransparent()
    {
        _fade.Update(PromptFade.FadeInTime);

        Assert.Equal(0f, _fade.Opacity);
        Assert.Null(_fade.Shown);
    }

    private void ShowFully(InteractionTarget target)
    {
        _fade.Show(target);
        _fade.Update(PromptFade.FadeInTime);
    }

    private static InteractionTarget TargetNamed(string id) => new(
        new PlacedObject { Uid = id, ObjectId = id, Position = new Point2D(0, 0), Z = MapLayers.PlayerZ, Events = [] },
        new MapEvent { Id = id, Activation = EventActivation.Interact, Steps = [], Prompt = "Check" });
}
