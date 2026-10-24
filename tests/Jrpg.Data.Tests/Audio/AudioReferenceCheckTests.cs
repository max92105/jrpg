using Jrpg.Core.Content;
using Jrpg.Core.Events;
using Jrpg.Core.Maps;
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
