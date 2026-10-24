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
        var step = EventStepViewModel.New(EventStepKind.Dialogue);
        step.LinesText = "Hello.\r\n\r\n  How are you? \r\n";
        step.Speaker = " ";

        var speech = Assert.IsType<DialogueStep>(step.ToStep()).Speech;

        Assert.Equal(["Hello.", "How are you?"], speech.Lines);
        Assert.Null(speech.Speaker);
        Assert.Null(speech.PortraitFile);
    }

    [Theory]
    [MemberData(nameof(StepsOfEveryKind))]
    public void ToStep_Unedited_GivesBackTheSameStep(EventStep step)
    {
        Assert.Equal(step, EventStepViewModel.From(step).ToStep());
    }

    [Fact]
    public void Label_Still_NamesImageAndSeconds()
    {
        Assert.Equal("Still: Placeholder1.png, 2.5 s", EventStepViewModel.From(new StillStep("Placeholder1.png", 2.5, Caption: null)).Label);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("four")]
    [InlineData("")]
    public void Problem_StillWithSecondsNotAboveZero_SaysSo(string seconds)
    {
        var step = EventStepViewModel.New(EventStepKind.Still);
        step.ImageFile = "Placeholder1.png";
        step.SecondsText = seconds;

        Assert.Equal("The seconds must be a number above 0.", step.Problem);
    }

    [Fact]
    public void ToStep_StillWithBlankCaption_HasNoCaption()
    {
        var step = EventStepViewModel.New(EventStepKind.Still);
        step.ImageFile = " Placeholder1.png ";
        step.Caption = " ";

        Assert.Equal(new StillStep("Placeholder1.png", 4, Caption: null), step.ToStep());
    }

    [Fact]
    public void ToStep_GiveItemEdited_UsesTheNewFields()
    {
        var step = EventStepViewModel.From(new GiveItemStep("potion", 2));
        step.ItemId = "ether";
        step.QuantityText = " 3 ";

        Assert.Equal(new GiveItemStep("ether", 3), step.ToStep());
        Assert.Equal("Give item: ether x 3 ", step.Label);
    }

    [Fact]
    public void ToStep_WarpEdited_UsesTheNewMapAndCell()
    {
        var step = EventStepViewModel.From(new WarpStep("start", new GridCell(1, 1)));
        step.MapId = "interior_1";
        step.CellXText = "0";
        step.CellYText = "7";

        Assert.Equal(new WarpStep("interior_1", new GridCell(0, 7)), step.ToStep());
    }

    [Fact]
    public void Problem_NegativeWarpCellX_SaysSoAndToStepRefuses()
    {
        var step = NewWarp();

        step.CellXText = "-1";

        Assert.Equal("The warp cell must be two whole numbers of at least 0.", step.Problem);
        Assert.Throws<InvalidOperationException>(step.ToStep);
    }

    [Fact]
    public void Problem_NegativeWarpCellY_SaysSo()
    {
        var step = NewWarp();

        step.CellYText = "-1";

        Assert.Equal("The warp cell must be two whole numbers of at least 0.", step.Problem);
    }

    [Fact]
    public void Problem_ZeroQuantity_SaysSo()
    {
        var step = NewGiveItem();

        step.QuantityText = "0";

        Assert.Equal("The quantity must be a whole number of at least 1.", step.Problem);
    }

    [Fact]
    public void Problem_NewGiveItemWithItemChosen_IsNullAndGivesOne()
    {
        var step = NewGiveItem();

        Assert.Null(step.Problem);
        Assert.Equal(new GiveItemStep("potion", 1), step.ToStep());
    }

    [Fact]
    public void Problem_WarpWithoutMap_AsksForOne()
    {
        Assert.Equal("Choose the map to warp to.", EventStepViewModel.New(EventStepKind.Warp).Problem);
    }

    [Fact]
    public void Label_SwapObject_NamesTheObject()
    {
        Assert.Equal("Swap object to: chest_opened", EventStepViewModel.From(new SwapObjectStep("chest_opened")).Label);
    }

    private static EventStepViewModel NewWarp()
    {
        var step = EventStepViewModel.New(EventStepKind.Warp);
        step.MapId = "start";
        return step;
    }

    private static EventStepViewModel NewGiveItem()
    {
        var step = EventStepViewModel.New(EventStepKind.GiveItem);
        step.ItemId = "potion";
        return step;
    }

    [Fact]
    public void From_RecruitStep_KeepsTheMember()
    {
        var step = EventStepViewModel.From(new RecruitStep("omer"));

        Assert.Equal("omer", step.MemberId);
        Assert.Equal("Recruit: omer", step.Label);
    }

    [Fact]
    public void From_VoiceStep_KeepsTheFile()
    {
        var step = EventStepViewModel.From(new VoiceStep("Placeholder.wav"));

        Assert.Equal("Placeholder.wav", step.VoiceFile);
        Assert.Equal("Voice: Placeholder.wav", step.Label);
    }

    [Fact]
    public void ToStep_VoiceFileWithSpaces_TrimsTheFile()
    {
        var step = EventStepViewModel.New(EventStepKind.Voice);
        step.VoiceFile = " Placeholder.wav ";

        Assert.Equal(new VoiceStep("Placeholder.wav"), step.ToStep());
    }

    [Fact]
    public void Problem_VoiceWithoutFile_SaysSo()
    {
        var step = EventStepViewModel.New(EventStepKind.Voice);

        Assert.Equal("Write the voice's file.", step.Problem);
    }

    [Fact]
    public void ToStep_RecruitWithMember_BuildsRecruitStep()
    {
        var step = EventStepViewModel.New(EventStepKind.Recruit);
        step.MemberId = "lazarus";

        Assert.Null(step.Problem);
        Assert.Equal(new RecruitStep("lazarus"), step.ToStep());
    }

    [Fact]
    public void Problem_RecruitWithoutMember_SaysSo()
    {
        var step = EventStepViewModel.New(EventStepKind.Recruit);

        Assert.Equal("Choose the member who joins.", step.Problem);
    }

    public static TheoryData<EventStep> StepsOfEveryKind() =>
    [
        new GiveItemStep("potion", 2),
        new SwapObjectStep("door_open"),
        new WarpStep("interior_1", new GridCell(2, 3)),
        new StillStep("Placeholder1.png", 2.5, "Long ago."),
        new StillStep("Placeholder2.png", 4, Caption: null),
        new RecruitStep("omer"),
        new VoiceStep("Placeholder.wav"),
    ];
}
