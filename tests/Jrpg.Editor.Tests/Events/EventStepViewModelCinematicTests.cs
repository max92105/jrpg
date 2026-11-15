using Jrpg.Core.Events;
using Jrpg.Editor.Events;

namespace Jrpg.Editor.Tests.Events;

// The cinematic step in the event dialog (ADR 0046), and the voice step it shares PresentationStep with.
public sealed class EventStepViewModelCinematicTests
{
    [Fact]
    public void From_CinematicStep_KeepsItsId()
    {
        Assert.Equal("ragnir_intro", EventStepViewModel.From(new CinematicStep("ragnir_intro")).CinematicId);
    }

    [Fact]
    public void ToStep_ChosenCinematic_MakesTheStep()
    {
        var step = EventStepViewModel.New(EventStepKind.Cinematic);
        step.CinematicId = "ragnir_intro";

        Assert.Equal(new CinematicStep("ragnir_intro"), step.ToStep());
    }

    [Fact]
    public void Problem_NoCinematicChosen_AsksForOne()
    {
        Assert.Equal("Choose the cinematic.", EventStepViewModel.New(EventStepKind.Cinematic).Problem);
    }

    [Fact]
    public void Problem_CinematicChosen_IsNull()
    {
        Assert.Null(EventStepViewModel.From(new CinematicStep("ragnir_intro")).Problem);
    }

    [Fact]
    public void ToStep_NoCinematicChosen_Throws()
    {
        Assert.Throws<InvalidOperationException>(() => EventStepViewModel.New(EventStepKind.Cinematic).ToStep());
    }

    [Fact]
    public void Label_Cinematic_NamesIt()
    {
        Assert.Equal("Cinematic: ragnir_intro", EventStepViewModel.From(new CinematicStep("ragnir_intro")).Label);
    }

    [Fact]
    public void ToStep_Voice_KeepsItsFileAfterTheMove()
    {
        Assert.Equal(new VoiceStep("Placeholder.wav"), EventStepViewModel.From(new VoiceStep("Placeholder.wav")).ToStep());
    }
}
