using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// A still's subtitles on the Cinematics page: lines in the order they show, each with its seconds as typed.
public sealed class CinematicStillRowCaptionTests
{
    private static readonly CinematicStill _road = new()
    {
        ImageFile = "intro/2_road.png",
        Seconds = 6,
        Captions = [new StillCaption { Text = "He left.", Seconds = 2.5 }, new StillCaption { Text = "At dawn.", Seconds = 1 }],
    };

    [Fact]
    public void From_StillWithTwoLines_ListsThemInOrder()
    {
        var row = CinematicStillRow.From(_road);

        var lines = row.Captions.Select(caption => (caption.Text, caption.SecondsText));
        Assert.Equal([("He left.", "2.5"), ("At dawn.", "1")], lines);
    }

    [Fact]
    public void ToStill_Unchanged_EqualsTheStill()
    {
        Assert.Equal(_road, CinematicStillRow.From(_road).ToStill());
    }

    // An emptied line stays, as a pause of its seconds.
    [Fact]
    public void ToStill_LineEmptied_KeepsItAsAPause()
    {
        var row = CinematicStillRow.From(_road);

        row.Captions[0].Text = "  ";

        Assert.Equal([("", 2.5), ("At dawn.", 1)], row.ToStill().Captions.Select(caption => (caption.Text, caption.Seconds)));
    }

    [Fact]
    public void AddCaption_StillWithTimeLeft_AddsAnEmptyLineOnTheTimeLeft()
    {
        var row = CinematicStillRow.From(_road);

        row.AddCaptionCommand.Execute(null);

        Assert.Equal(("", "2.5"), (row.Captions[^1].Text, row.Captions[^1].SecondsText));
        Assert.Equal(3, row.Captions.Count);
    }

    [Fact]
    public void AddCaption_StillWithNoTimeLeft_AddsALineOfTwoSeconds()
    {
        var row = CinematicStillRow.From(_road with { Seconds = 3.5 });

        row.AddCaptionCommand.Execute(null);

        Assert.Equal("2", row.Captions[^1].SecondsText);
    }

    [Fact]
    public void RemoveCaption_FirstLine_LeavesTheOther()
    {
        var row = CinematicStillRow.From(_road);

        row.RemoveCaptionCommand.Execute(row.Captions[0]);

        Assert.Equal("At dawn.", Assert.Single(row.Captions).Text);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("")]
    [InlineData("two")]
    [InlineData("Infinity")]
    public void Problem_LineSecondsNotANumberAboveZero_NamesTheLine(string typed)
    {
        var row = CinematicStillRow.From(_road);
        row.Number = 3;

        row.Captions[1].SecondsText = typed;

        Assert.Equal($"Still 3: subtitle 2's seconds must be a number above 0 (was '{typed}').", row.Problem);
        Assert.Throws<InvalidOperationException>(() => row.ToStill());
    }

    [Fact]
    public void Problem_SmallestSeconds_IsNull()
    {
        var row = CinematicStillRow.From(_road);

        row.Captions[1].SecondsText = "0.01";

        Assert.Null(row.Problem);
    }

    [Fact]
    public void CaptionsTimeText_TwoLines_SumsThemAgainstTheStill()
    {
        Assert.Equal("Subtitles: 3.5 s of 6 s", CinematicStillRow.From(_road).CaptionsTimeText);
    }

    [Fact]
    public void CaptionsTimeText_NoLine_IsEmpty()
    {
        Assert.Equal("", CinematicStillRow.From(_road with { Captions = [] }).CaptionsTimeText);
    }

    [Fact]
    public void CaptionsTimeText_ASecondsBoxUnreadable_IsEmpty()
    {
        var row = CinematicStillRow.From(_road);

        row.Captions[0].SecondsText = "x";

        Assert.Equal("", row.CaptionsTimeText);
    }

    [Fact]
    public void Changed_LineTyped_IsRaisedOnceWithTheTimeLineAndTheProblem()
    {
        var row = CinematicStillRow.From(_road);
        var changes = 0;
        List<string?> raised = [];
        row.Changed += (_, _) => changes++;
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        row.Captions[0].SecondsText = "3";

        Assert.Equal(1, changes);
        Assert.Contains(nameof(CinematicStillRow.CaptionsTimeText), raised);
        Assert.Contains(nameof(CinematicStillRow.Problem), raised);
    }

    [Fact]
    public void Changed_LineAdded_IsRaised()
    {
        var row = CinematicStillRow.From(_road);
        var changes = 0;
        row.Changed += (_, _) => changes++;

        row.AddCaptionCommand.Execute(null);

        Assert.Equal(1, changes);
    }

    [Fact]
    public void Changed_LineRemoved_IsRaisedAndTheLineNoLongerWatched()
    {
        var row = CinematicStillRow.From(_road);
        var removed = row.Captions[0];
        var changes = 0;
        row.Changed += (_, _) => changes++;

        row.RemoveCaptionCommand.Execute(removed);
        removed.Text = "Typed after its removal.";

        Assert.Equal(1, changes);
    }

    [Fact]
    public void SecondsText_StillsSecondsTyped_RaisesTheTimeLine()
    {
        var row = CinematicStillRow.From(_road);
        List<string?> raised = [];
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        row.SecondsText = "8";

        Assert.Contains(nameof(CinematicStillRow.CaptionsTimeText), raised);
        Assert.Equal("Subtitles: 3.5 s of 8 s", row.CaptionsTimeText);
    }
}
