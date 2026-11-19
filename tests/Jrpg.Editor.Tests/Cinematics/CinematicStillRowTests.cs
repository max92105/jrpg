using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// One still as the Cinematics page types it (ADR 0046): the boxes from a record and back, and the box that can't be
// read, named by the still's number.
public sealed class CinematicStillRowTests
{
    private static readonly CinematicStill _road = new()
    {
        ImageFile = "intro/2_road.png",
        Seconds = 5.5,
        Captions = [new StillCaption { Text = "He left at dawn.", Seconds = 2 }],
        VoiceFile = "intro_2.wav",
        Zoom = new StillZoom { StartPercent = 110, EndPercent = 100 },
    };

    [Fact]
    public void From_StillWithEveryField_FillsTheBoxes()
    {
        var row = CinematicStillRow.From(_road);

        Assert.Equal(("intro/2_road.png", "5.5"), (row.ImageFile, row.SecondsText));
        Assert.Equal(("He left at dawn.", "2"), (row.Captions.Single().Text, row.Captions.Single().SecondsText));
        Assert.Equal("intro_2.wav", row.VoiceFile);
        Assert.Equal(("110", "100"), (row.ZoomStartText, row.ZoomEndText));
    }

    [Fact]
    public void From_StillWithoutZoom_ShowsHundredTwice()
    {
        var row = CinematicStillRow.From(_road with { Zoom = null });

        Assert.Equal(("100", "100"), (row.ZoomStartText, row.ZoomEndText));
    }

    [Fact]
    public void New_Picture_HasFourSecondsAndNoZoom()
    {
        var still = CinematicStillRow.New("intro/1_home.png").ToStill();

        Assert.Equal(new CinematicStill { ImageFile = "intro/1_home.png", Seconds = 4 }, still);
    }

    [Fact]
    public void ToStill_BoxesAsTyped_MakesTheStill()
    {
        var row = CinematicStillRow.New("intro/1_home.png");
        row.ImageFile = " intro/2_road.png ";
        row.SecondsText = "5.5";
        row.AddCaptionCommand.Execute(null);
        row.Captions[0].Text = " He left at dawn. ";
        row.Captions[0].SecondsText = "2";
        row.VoiceFile = "intro_2.wav";
        row.ZoomStartText = "110";

        Assert.Equal(_road, row.ToStill());
    }

    [Fact]
    public void ToStill_ZoomHundredToHundred_HasNoZoom()
    {
        var row = CinematicStillRow.From(_road);
        row.ZoomStartText = "100";

        Assert.Null(row.ToStill().Zoom);
    }

    [Fact]
    public void ToStill_EmptyVoice_IsNull()
    {
        var row = CinematicStillRow.From(_road);
        row.VoiceFile = "";

        var still = row.ToStill();

        Assert.Null(still.VoiceFile);
    }

    [Fact]
    public void ToStill_WithProblem_Throws()
    {
        var row = CinematicStillRow.From(_road);
        row.SecondsText = "";

        Assert.Throws<InvalidOperationException>(row.ToStill);
    }

    [Fact]
    public void Problem_NoPicture_NamesTheStillByNumber()
    {
        var row = CinematicStillRow.New(" ");
        row.Number = 3;

        Assert.Equal("Still 3: choose its picture.", row.Problem);
    }

    [Theory]
    [InlineData("Infinity")]
    [InlineData("0")]
    [InlineData("four")]
    public void Problem_SecondsNotAboveZero_AsksForANumber(string secondsText)
    {
        var row = Numbered(2);
        row.SecondsText = secondsText;

        Assert.Equal($"Still 2: the seconds must be a number above 0 (was '{secondsText}').", row.Problem);
    }

    [Fact]
    public void Problem_SecondsPointFive_IsNull()
    {
        var row = Numbered(2);
        row.SecondsText = "0.5";

        Assert.Null(row.Problem);
    }

    // A valid file's tiny time comes back from the box as it was shown.
    [Fact]
    public void ToStill_TinySecondsFromAFile_ReadsBack()
    {
        var row = CinematicStillRow.From(_road with { Seconds = 0.00001 });

        Assert.Equal(0.00001, row.ToStill().Seconds);
    }

    [Fact]
    public void Problem_ZoomNotANumber_NamesTheBox()
    {
        var row = Numbered(2);
        row.ZoomEndText = "big";

        Assert.Equal("Still 2: zoom end must be a whole number (was 'big').", row.Problem);
    }

    [Fact]
    public void Problem_NumberSet_IsRaised()
    {
        var row = CinematicStillRow.New(" ");
        var raised = new List<string?>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        row.Number = 4;

        Assert.Contains(nameof(CinematicStillRow.Problem), raised);
    }

    [Fact]
    public void Changed_TypingSeconds_IsRaised()
    {
        var row = Numbered(1);
        var raised = 0;
        row.Changed += (_, _) => raised++;

        row.SecondsText = "6";

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_NumberSet_IsNotRaised()
    {
        var row = Numbered(1);
        var raised = 0;
        row.Changed += (_, _) => raised++;

        row.Number = 2;

        Assert.Equal(0, raised);
    }

    private static CinematicStillRow Numbered(int number)
    {
        var row = CinematicStillRow.From(_road);
        row.Number = number;
        return row;
    }
}
