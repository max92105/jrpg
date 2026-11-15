using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// What the Cinematics window's boxes offer, from lookups the test gives, and the names a cinematic gives that the
// folders lack (decision 442: kept and saved, the game's start refuses them).
public sealed class CinematicMediaChoicesTests
{
    private static readonly CinematicMediaChoices _choices = new(
        () => ["intro/1.png", "intro/2.png"],
        () => ["intro_1.wav"],
        () => ["town/loop.wav"],
        _ => TimeSpan.FromSeconds(3));

    [Fact]
    public void Tracks_FirstEntry_IsNone()
    {
        Assert.Equal(["", "town/loop.wav"], _choices.Tracks);
    }

    [Fact]
    public void MissingOf_EverythingThere_IsEmpty()
    {
        var cinematic = Cinematic(new CinematicStill { ImageFile = "intro/1.png", Seconds = 4, VoiceFile = "intro_1.wav" });
        cinematic.MusicFile = "town/loop.wav";

        Assert.Empty(_choices.MissingOf(cinematic));
    }

    [Fact]
    public void MissingOf_StillPictureNotInFolder_NamesIt()
    {
        var cinematic = Cinematic(new CinematicStill { ImageFile = "intro/7.png", Seconds = 4 });

        Assert.Equal(["intro/7.png"], _choices.MissingOf(cinematic));
    }

    [Fact]
    public void MissingOf_VoiceNotInFolder_NamesIt()
    {
        var cinematic = Cinematic(new CinematicStill { ImageFile = "intro/1.png", Seconds = 4, VoiceFile = "gone.wav" });

        Assert.Equal(["gone.wav"], _choices.MissingOf(cinematic));
    }

    [Fact]
    public void MissingOf_TrackNotInFolder_NamesIt()
    {
        var cinematic = Cinematic(new CinematicStill { ImageFile = "intro/1.png", Seconds = 4 });
        cinematic.MusicFile = "gone/loop.wav";

        Assert.Equal(["gone/loop.wav"], _choices.MissingOf(cinematic));
    }

    [Fact]
    public void VoiceLengthOf_NameInTheFolder_ReadsIt()
    {
        Assert.Equal(TimeSpan.FromSeconds(3), _choices.VoiceLengthOf("intro_1.wav"));
    }

    // Windows would open Intro_1.wav as intro_1.wav; the game's start refuses it, so the window says it isn't there.
    [Fact]
    public void VoiceLengthOf_NameInAnotherCase_IsNull()
    {
        Assert.Null(_choices.VoiceLengthOf("Intro_1.wav"));
    }

    private static CinematicViewModel Cinematic(CinematicStill still) =>
        CinematicViewModel.From(new CinematicDefinition { Id = "intro", Stills = [still] });
}
