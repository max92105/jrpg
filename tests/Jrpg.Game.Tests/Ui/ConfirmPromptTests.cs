using Jrpg.Game.Input;
using Jrpg.Game.Ui;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Ui;

public sealed class ConfirmPromptTests
{
    [Fact]
    public void Constructor_SelectsNo()
    {
        var prompt = new ConfirmPrompt("Quit?");

        Assert.False(prompt.IsYesSelected);
    }

    [Fact]
    public void Update_ConfirmRightAway_AnswersNo()
    {
        var prompt = new ConfirmPrompt("Quit?");

        var answer = prompt.Update(Pressing(GameButton.Confirm));

        Assert.Equal(ConfirmAnswer.No, answer);
    }

    [Fact]
    public void Update_CancelOnYes_AnswersNo()
    {
        var prompt = new ConfirmPrompt("Quit?");
        prompt.Update(Pressing(GameButton.Left));

        var answer = prompt.Update(Pressing(GameButton.Cancel));

        Assert.Equal(ConfirmAnswer.No, answer);
    }

    [Fact]
    public void Update_ConfirmOnYes_AnswersYes()
    {
        var prompt = new ConfirmPrompt("Quit?");
        prompt.Update(Pressing(GameButton.Left));

        var answer = prompt.Update(Pressing(GameButton.Confirm));

        Assert.Equal(ConfirmAnswer.Yes, answer);
    }

    [Fact]
    public void Update_RightAfterLeft_SelectsNoAgain()
    {
        var prompt = new ConfirmPrompt("Quit?");
        prompt.Update(Pressing(GameButton.Left));

        var answer = prompt.Update(Pressing(GameButton.Right));

        Assert.Null(answer);
        Assert.False(prompt.IsYesSelected);
    }

    [Fact]
    public void Update_Up_SelectsYes()
    {
        var prompt = new ConfirmPrompt("Quit?");

        prompt.Update(Pressing(GameButton.Up));

        Assert.True(prompt.IsYesSelected);
    }

    [Fact]
    public void Update_DownAfterUp_SelectsNo()
    {
        var prompt = new ConfirmPrompt("Quit?");
        prompt.Update(Pressing(GameButton.Up));

        prompt.Update(Pressing(GameButton.Down));

        Assert.False(prompt.IsYesSelected);
    }
}
