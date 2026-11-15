using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
using Jrpg.Data.Audio;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Audio;

// A cinematic's voices and track (ADR 0046), on the repository's audio, whose Placeholder.wav voice and town track the
// other audio tests also read; the cinematic is built here, since the repository's are the developer's.
public sealed class AudioReferenceCheckCinematicTests
{
    private static readonly string _audioRoot = AudioFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    private static readonly CinematicStill _voiced =
        new() { ImageFile = "intro/1.png", Seconds = 4, VoiceFile = "Placeholder.wav" };
    private static readonly CinematicStill _silent = new() { ImageFile = "intro/2.png", Seconds = 4 };

    [Fact]
    public void FindProblems_CinematicVoiceMissing_NamesCinematicAndStill()
    {
        var catalog = WithIntro(stills: [_voiced, _silent with { VoiceFile = "Ghost.wav" }]);

        var problem = Assert.Single(AudioReferenceCheck.FindProblems(catalog, _audioRoot));

        Assert.Equal($"voices/Ghost.wav: not found in {_audioRoot}. Named by cinematic 'intro' still 2.", problem);
    }

    // A still without a voice beside one whose voice is there: neither is reported.
    [Fact]
    public void FindProblems_CinematicStillWithoutVoice_IsFine()
    {
        Assert.Empty(AudioReferenceCheck.FindProblems(WithIntro(stills: [_voiced, _silent]), _audioRoot));
    }

    [Fact]
    public void FindProblems_CinematicTrackMissing_NamesCinematic()
    {
        var problem = Assert.Single(AudioReferenceCheck.FindProblems(WithIntro([_silent], "ghost/loop.wav"), _audioRoot));

        Assert.Equal($"music/ghost/loop.wav: not found in {_audioRoot}. Named by cinematic 'intro'.", problem);
    }

    [Fact]
    public void FindProblems_CinematicTrackPresent_IsFine()
    {
        Assert.Empty(AudioReferenceCheck.FindProblems(WithIntro([_silent], "town/loop.wav"), _audioRoot));
    }

    [Fact]
    public void FindProblems_CinematicWithoutTrack_IsFine()
    {
        Assert.Empty(AudioReferenceCheck.FindProblems(WithIntro([_silent], musicFile: null), _audioRoot));
    }

    private static ContentCatalog WithIntro(IReadOnlyList<CinematicStill> stills, string? musicFile = null) =>
        _catalog with
        {
            Cinematics = new Dictionary<string, CinematicDefinition>
            {
                ["intro"] = new() { Id = "intro", MusicFile = musicFile, Stills = stills },
            },
        };
}
