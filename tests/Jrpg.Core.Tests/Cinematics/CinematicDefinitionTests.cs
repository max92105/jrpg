using Jrpg.Core.Cinematics;

namespace Jrpg.Core.Tests.Cinematics;

// Two cinematics are equal by content, stills in order included (ADR 0046): the editor's draft records nothing for a
// window that changed nothing; and where a time into one falls among its stills (decision 629), on 2 s then 3 s.
public sealed class CinematicDefinitionTests
{
    private static readonly CinematicStill _home = new()
    {
        ImageFile = "intro/1.png",
        Seconds = 4,
        Zoom = new StillZoom { StartPercent = 100, EndPercent = 110 },
    };

    private static readonly CinematicStill _road = new() { ImageFile = "intro/2.png", Seconds = 3, Caption = "He left." };
    private static readonly CinematicDefinition _intro =
        new() { Id = "intro", MusicFile = "town/loop.wav", Stills = [_home, _road] };

    [Fact]
    public void Equals_SameStillsInNewLists_IsTrue()
    {
        var copy = _intro with { Stills = [_home with { }, _road with { }] };

        Assert.Equal(_intro, copy);
        Assert.Equal(_intro.GetHashCode(), copy.GetHashCode());
    }

    [Fact]
    public void Equals_StillZoomDiffers_IsFalse()
    {
        var zoomedMore = _home with { Zoom = new StillZoom { StartPercent = 100, EndPercent = 120 } };

        Assert.NotEqual(_intro, _intro with { Stills = [zoomedMore, _road] });
    }

    [Fact]
    public void Equals_StillFocusDiffers_IsFalse()
    {
        var focused = _home with { Focus = new StillFocus { XPercent = 30, YPercent = 62 } };

        Assert.NotEqual(_intro, _intro with { Stills = [focused, _road] });
    }

    [Fact]
    public void Equals_StillsInOtherOrder_IsFalse()
    {
        Assert.NotEqual(_intro, _intro with { Stills = [_road, _home] });
    }

    [Fact]
    public void Equals_TrackDiffers_IsFalse()
    {
        Assert.NotEqual(_intro, _intro with { MusicFile = null });
    }

    [Fact]
    public void Duration_TwoStills_SumsTheirSeconds()
    {
        Assert.Equal(TimeSpan.FromSeconds(5), TwoSecondsThenThree().Duration);
    }

    [Fact]
    public void MomentAt_Zero_IsFirstStillAtItsStart()
    {
        Assert.Equal(new StillMoment(0, TimeSpan.Zero), TwoSecondsThenThree().MomentAt(TimeSpan.Zero));
    }

    [Fact]
    public void MomentAt_JustBeforeTwoSeconds_IsStillTheFirst()
    {
        var justBefore = TimeSpan.FromSeconds(2) - TimeSpan.FromTicks(1);

        Assert.Equal(new StillMoment(0, justBefore), TwoSecondsThenThree().MomentAt(justBefore));
    }

    [Fact]
    public void MomentAt_TwoSeconds_IsSecondAtItsStart()
    {
        Assert.Equal(new StillMoment(1, TimeSpan.Zero), TwoSecondsThenThree().MomentAt(TimeSpan.FromSeconds(2)));
    }

    [Fact]
    public void MomentAt_InTheSecondStill_CountsFromItsStart()
    {
        Assert.Equal(new StillMoment(1, TimeSpan.FromSeconds(1.5)), TwoSecondsThenThree().MomentAt(TimeSpan.FromSeconds(3.5)));
    }

    [Fact]
    public void MomentAt_Duration_IsNull()
    {
        Assert.Null(TwoSecondsThenThree().MomentAt(TimeSpan.FromSeconds(5)));
    }

    [Fact]
    public void MomentAt_Negative_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => TwoSecondsThenThree().MomentAt(TimeSpan.FromTicks(-1)));
    }

    [Fact]
    public void VoicedStillAt_StillWithVoice_IsItself()
    {
        Assert.Equal(1, VoicedVoicedSilent().VoicedStillAt(1));
    }

    // The latest earlier voice speaks on, not the first.
    [Fact]
    public void VoicedStillAt_StillWithout_IsTheLatestEarlierVoiced()
    {
        Assert.Equal(1, VoicedVoicedSilent().VoicedStillAt(2));
    }

    [Fact]
    public void VoicedStillAt_NoEarlierVoice_IsNull()
    {
        Assert.Null(_intro.VoicedStillAt(1));
    }

    private static CinematicDefinition VoicedVoicedSilent() => _intro with
    {
        Stills = [_home with { VoiceFile = "one.wav" }, _road with { VoiceFile = "two.wav" }, _home],
    };

    private static CinematicDefinition TwoSecondsThenThree() =>
        _intro with { Stills = [_home with { Seconds = 2 }, _road with { Seconds = 3 }] };
}
