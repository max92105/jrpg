using Jrpg.Core.Cinematics;
using Jrpg.Editor.Cinematics;

namespace Jrpg.Editor.Tests.Cinematics;

// Each still's voice length beside its Seconds box (decision 620), from a lookup the test gives: intro_1.wav lasts 5.21
// s, shown as 5.3 (up to a tenth), short.wav 2 s, any other name can't be read.
public sealed class CinematicStillRowVoiceTests
{
    private static readonly CinematicStill _voiced = new() { ImageFile = "intro/1.png", Seconds = 4, VoiceFile = "intro_1.wav" };

    [Fact]
    public void VoiceLengthText_VoiceFound_ShowsSecondsToATenth()
    {
        Assert.Equal("voice 5.3 s", Row(_voiced).VoiceLengthText);
    }

    [Fact]
    public void VoiceLengthText_VoiceMissing_SaysNotFound()
    {
        Assert.Equal("voice not found", Row(_voiced with { VoiceFile = "gone.wav" }).VoiceLengthText);
    }

    [Fact]
    public void VoiceLengthText_NoVoice_IsEmpty()
    {
        Assert.Equal("", Row(_voiced with { VoiceFile = null }).VoiceLengthText);
    }

    [Fact]
    public void VoiceLengthText_VoiceChanged_IsRecomputedAndRaised()
    {
        var row = Row(_voiced);
        var raised = new List<string?>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        row.VoiceFile = "short.wav";

        Assert.Equal("voice 2.0 s", row.VoiceLengthText);
        Assert.Contains(nameof(CinematicStillRow.VoiceLengthText), raised);
    }

    [Fact]
    public void IsShorterThanVoice_SecondsBelowLength_IsTrue()
    {
        Assert.True(Row(_voiced with { Seconds = 5.2 }).IsShorterThanVoice);
    }

    [Fact]
    public void IsShorterThanVoice_SecondsAtTheShownLength_IsFalse()
    {
        Assert.False(Row(_voiced with { Seconds = 5.3 }).IsShorterThanVoice);
    }

    [Fact]
    public void IsShorterThanVoice_SecondsTyped_IsRaised()
    {
        var row = Row(_voiced);
        var raised = new List<string?>();
        row.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        row.SecondsText = "6";

        Assert.Contains(nameof(CinematicStillRow.IsShorterThanVoice), raised);
    }

    // The page's choices reach a row the list already holds and one added later.
    [Fact]
    public void VoiceLengthOf_FromTheWindowsChoices_ReachesEveryRow()
    {
        var catalog = new CinematicCatalogViewModel(
            [new CinematicDefinition { Id = "intro", Stills = [_voiced] }],
            new CinematicMediaChoices(() => ["intro/1.png"], () => ["intro_1.wav", "short.wav"], () => [], LengthOf));
        var intro = catalog.Cinematics[0];
        intro.ImageToAdd = "intro/1.png";
        intro.AddStillCommand.Execute(null);
        intro.Stills[1].VoiceFile = "short.wav";

        Assert.Equal(["voice 5.3 s", "voice 2.0 s"], intro.Stills.Select(row => row.VoiceLengthText));
    }

    private static CinematicStillRow Row(CinematicStill still)
    {
        var row = CinematicStillRow.From(still);
        row.VoiceLengthOf = LengthOf;
        return row;
    }

    private static TimeSpan? LengthOf(string voice) => voice switch
    {
        "intro_1.wav" => TimeSpan.FromSeconds(5.21),
        "short.wav" => TimeSpan.FromSeconds(2),
        _ => null,
    };
}
