using Jrpg.Core.Dialogue;
using Jrpg.Core.Events;
using Jrpg.Core.Geometry;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

public sealed class EventStepViewModelTests
{
    [Fact]
    public void Label_Warp_NamesMapAndCell()
    {
        Assert.Equal("Warp to: interior_1 (2, 3)", EventStepViewModel.From(new WarpStep("interior_1", new GridCell(2, 3))).Label);
    }

    [Fact]
    public void Label_LongDialogueWithSpeaker_ShowsTheStartOfTheFirstLineAndHowManyMore()
    {
        var step = EventStepViewModel.From(new DialogueStep(new Speech(["Welcome to the old mill, traveller from afar.", "Rest here."], "Miller")));

        Assert.Equal("Dialogue: Miller: \"Welcome to the old mill, trave...\" (+1)", step.Label);
    }

    [Fact]
    public void ToStep_Dialogue_LeavesOutBlankLinesAndEmptyFields()
    {
        var step = EventStepViewModel.NewDialogue();
        step.LinesText = "Hello.\r\n\r\n  How are you? \r\n";
        step.Speaker = " ";

        var speech = Assert.IsType<DialogueStep>(step.ToStep()).Speech;

        Assert.Equal(["Hello.", "How are you?"], speech.Lines);
        Assert.Null(speech.Speaker);
        Assert.Null(speech.PortraitFile);
    }

    [Fact]
    public void ToStep_GiveItem_KeepsTheStepAsItWas()
    {
        var give = new GiveItemStep("potion", 2);

        var step = EventStepViewModel.From(give);

        Assert.False(step.IsDialogue);
        Assert.Same(give, step.ToStep());
    }
}
