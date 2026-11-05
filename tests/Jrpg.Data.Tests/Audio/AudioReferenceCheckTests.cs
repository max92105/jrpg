using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
using Jrpg.Core.Skills;
using Jrpg.Data.Audio;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Audio;

public sealed class AudioReferenceCheckTests
{
    private static readonly string _audioRoot = AudioFolders.RootIn(Path.Combine(AppContext.BaseDirectory, "content"));
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void FindProblems_RepositoryContent_IsEmpty()
    {
        Assert.Empty(AudioReferenceCheck.FindProblems(_catalog, _audioRoot));
    }

    [Fact]
    public void FindProblems_PlaceholderVoice_IsFound()
    {
        Assert.Empty(AudioReferenceCheck.FindProblems(WithIntroVoice("Placeholder.wav"), _audioRoot));
    }

    [Fact]
    public void FindProblems_MissingVoice_NamesTheEventAndMap()
    {
        var problem = Assert.Single(AudioReferenceCheck.FindProblems(WithIntroVoice("Ghost.wav"), _audioRoot));

        Assert.Equal($"voices/Ghost.wav: not found in {_audioRoot}. Named by event 'cell_0_0' on map 'intro'.", problem);
    }

    [Fact]
    public void FindProblems_CaseMismatch_NamesTheActualFile()
    {
        var problem = Assert.Single(AudioReferenceCheck.FindProblems(WithIntroVoice("placeholder.wav"), _audioRoot));

        Assert.Equal(
            "voices/placeholder.wav: the file is named Placeholder.wav; names must match exactly. Named by event 'cell_0_0' on map 'intro'.",
            problem);
    }

    [Fact]
    public void FindProblems_MapTrackMissing_NamesTheFileAndTheMap()
    {
        var problem = Assert.Single(AudioReferenceCheck.FindProblems(WithIntroTrack("Ghost.wav"), _audioRoot));

        Assert.Equal($"music/Ghost.wav: not found in {_audioRoot}. Named by map 'intro'.", problem);
    }

    // The track's folder is part of the name checked, so a case slip in it is found too (decision 496).
    [Fact]
    public void FindProblems_MapTrackInOtherCase_NamesTheMismatch()
    {
        var problem = Assert.Single(AudioReferenceCheck.FindProblems(WithIntroTrack("Town/loop.wav"), _audioRoot));

        Assert.Equal(
            "music/Town/loop.wav: the file is named town/loop.wav; names must match exactly. Named by map 'intro'.",
            problem);
    }

    [Fact]
    public void FindProblems_MapWithoutATrack_IsEmpty()
    {
        Assert.Empty(AudioReferenceCheck.FindProblems(WithIntroTrack(null), _audioRoot));
    }

    [Fact]
    public void FindProblems_EncounterThemeMissing_NamesTheEncounter()
    {
        var encounter = _catalog.Encounters["goblin_trio"] with { MusicFile = "Ghost.wav" };
        var catalog = _catalog with { Encounters = new Dictionary<string, EncounterDefinition>(_catalog.Encounters) { ["goblin_trio"] = encounter } };

        var problem = Assert.Single(AudioReferenceCheck.FindProblems(catalog, _audioRoot));

        Assert.Equal($"music/Ghost.wav: not found in {_audioRoot}. Named by encounter 'goblin_trio'.", problem);
    }

    [Fact]
    public void FindProblems_SkillSoundMissing_NamesTheSkill()
    {
        var problem = Assert.Single(AudioReferenceCheck.FindProblems(WithHealSound("Ghost.wav"), _audioRoot));

        Assert.Equal($"effects/Ghost.wav: not found in {_audioRoot}. Named by skill 'heal'.", problem);
    }

    [Fact]
    public void FindProblems_SkillSoundInOtherCase_NamesTheMismatch()
    {
        var problem = Assert.Single(AudioReferenceCheck.FindProblems(WithHealSound("placeholderskillmend.wav"), _audioRoot));

        Assert.Equal(
            "effects/placeholderskillmend.wav: the file is named PlaceholderSkillMend.wav; names must match exactly. Named by skill 'heal'.",
            problem);
    }

    private static ContentCatalog WithHealSound(string file) =>
        _catalog with { Skills = new Dictionary<string, SkillDefinition>(_catalog.Skills) { ["heal"] = _catalog.Skills["heal"] with { SoundFile = file } } };

    private static ContentCatalog WithIntroTrack(string? file)
    {
        var intro = _catalog.Maps["intro"];
        var settings = intro.Settings with { MusicFile = file };
        return _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["intro"] = new MapDefinition(intro.Layout, settings) } };
    }

    // The intro's first event, which plays for Sarxian, opening with a voice step naming the file.
    private static ContentCatalog WithIntroVoice(string file)
    {
        var intro = _catalog.Maps["intro"];
        var first = intro.Layout.CellEvents[0];
        var voiced = first with { Event = first.Event with { Steps = [new VoiceStep(file), .. first.Event.Steps] } };
        var layout = intro.Layout with { CellEvents = [voiced, .. intro.Layout.CellEvents.Skip(1)] };
        return _catalog with { Maps = new Dictionary<string, MapDefinition>(_catalog.Maps) { ["intro"] = new MapDefinition(layout, intro.Settings) } };
    }
}
