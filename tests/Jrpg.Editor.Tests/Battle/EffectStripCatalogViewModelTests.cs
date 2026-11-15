using System.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The Effect strips window's rules (decision 659): the test world's catalog with records built here, over a folder the
// test lists: Burst.png (four 8 x 8 frames, no record), Slash.png (two frames, listed), and records for Gone.png and
// Ash.png (at the unlisted pace), images the folder lacks.
public sealed class EffectStripCatalogViewModelTests
{
    private static readonly ContentCatalog _testWorld =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly EffectStripDefinition _slash = new() { File = "Slash.png", FrameMilliseconds = 60, HitFrame = 1 };
    private static readonly EffectStripDefinition _gone = new() { File = "Gone.png", FrameMilliseconds = 40 };
    private static readonly EffectStripDefinition _ash = new() { File = "Ash.png", FrameMilliseconds = 80 };

    [Fact]
    public void Strips_ImagesAndRecords_ListsEveryImageThenTheMissing()
    {
        var window = Window();

        Assert.Equal(["Burst.png", "Slash.png", "Ash.png", "Gone.png"], window.Strips.Select(strip => strip.File));
    }

    [Fact]
    public void Strips_ImageWithoutARecord_ShowsTheUnlistedPace()
    {
        var burst = Window().Strips[0];

        Assert.Equal(("80", "0", false), (burst.FrameMillisecondsText, burst.HitFrameText, burst.IsListed));
    }

    [Fact]
    public void ToStrips_UntouchedImage_WritesNoRecord()
    {
        Assert.Equal([_ash, _gone, _slash], Window().ToStrips());
    }

    [Fact]
    public void ToStrips_EditedImage_WritesItsRecord()
    {
        var window = Window();

        window.Strips[0].FrameMillisecondsText = "50";

        Assert.Contains(new EffectStripDefinition { File = "Burst.png", FrameMilliseconds = 50 }, window.ToStrips());
    }

    [Fact]
    public void ResetToUnlisted_ListedStrip_DropsItsRecord()
    {
        var window = Window();
        window.SelectedStrip = window.Strips[1];

        window.ResetToUnlistedCommand.Execute(null);

        Assert.Equal([_ash, _gone], window.ToStrips());
    }

    [Fact]
    public void ResetToUnlisted_UnlistedStrip_IsGreyed()
    {
        var window = Window();
        window.SelectedStrip = window.Strips[0];

        Assert.False(window.ResetToUnlistedCommand.CanExecute(null));
    }

    [Fact]
    public void Problem_ChosenStripsBoxNotANumber_IsItsProblem()
    {
        var window = Window();

        window.SelectedStrip!.FrameMillisecondsText = "fast";

        Assert.Equal("Frame time must be a whole number (was 'fast').", window.Problem);
    }

    [Fact]
    public void Problem_ListFine_IsNone()
    {
        Assert.Null(Window().Problem);
    }

    [Fact]
    public void CanApply_WithAProblem_IsFalse()
    {
        var window = Window();

        window.SelectedStrip!.HitFrameText = "9";

        Assert.False(window.CanApply);
    }

    [Fact]
    public void Problem_BoxChanged_IsRaised()
    {
        var window = Window();
        var notified = new List<string?>();
        ((INotifyPropertyChanged)window).PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        window.Strips[1].HitFrameText = "0";

        Assert.Contains(nameof(EffectStripCatalogViewModel.Problem), notified);
    }

    [Fact]
    public void Problem_AnotherStripsBox_NamesItsFile()
    {
        var window = Window();

        window.Strips[1].FrameMillisecondsText = "fast";

        Assert.Equal("Slash.png: Frame time must be a whole number (was 'fast').", window.Problem);
    }

    [Fact]
    public void Problem_FrameTimeZero_IsDatasMessage()
    {
        var window = Window();

        window.SelectedStrip!.FrameMillisecondsText = "0";

        Assert.Equal(
            "effect-strips.json: effect strip 'Burst.png': frameMilliseconds must be at least 1 (was 0).", window.Problem);
    }

    [Fact]
    public void ResetToUnlisted_UnlistedStripEdited_IsLit()
    {
        var window = Window();
        var raised = 0;
        window.ResetToUnlistedCommand.CanExecuteChanged += (_, _) => raised++;

        window.SelectedStrip!.HitFrameText = "1";

        Assert.Equal((1, true), (raised, window.ResetToUnlistedCommand.CanExecute(null)));
    }

    [Fact]
    public void SelectedStrip_Changed_RaisesResetsCanExecute()
    {
        var window = Window();
        var raised = 0;
        window.ResetToUnlistedCommand.CanExecuteChanged += (_, _) => raised++;

        window.SelectedStrip = window.Strips[1];

        Assert.Equal((1, true), (raised, window.ResetToUnlistedCommand.CanExecute(null)));
    }

    [Fact]
    public void SwingChoices_Folder_OffersItsImagesInNameOrder()
    {
        Assert.Equal(["Burst.png", "Slash.png"], Window().SwingChoices);
    }

    [Fact]
    public void ToStrips_UntouchedRecordAtTheUnlistedPace_KeepsIt()
    {
        Assert.Contains(_ash, Window().ToStrips());
    }

    [Fact]
    public void ResetToUnlisted_RecordAtTheUnlistedPace_DropsIt()
    {
        var window = Window();
        window.SelectedStrip = window.Strips[2];

        window.ResetToUnlistedCommand.Execute(null);

        Assert.DoesNotContain(_ash, window.ToStrips());
    }

    [Fact]
    public void ResetToUnlisted_RecordAtTheUnlistedPace_GreysItself()
    {
        var window = Window();
        window.SelectedStrip = window.Strips[2];
        var raised = 0;
        window.ResetToUnlistedCommand.CanExecuteChanged += (_, _) => raised++;

        window.ResetToUnlistedCommand.Execute(null);

        Assert.Equal((1, false), (raised, window.ResetToUnlistedCommand.CanExecute(null)));
    }

    // The records are set up out of order, so the list's order is the window's own.
    private static EffectStripCatalogViewModel Window() => new(
        _testWorld with
        {
            EffectStrips = new Dictionary<string, EffectStripDefinition>
            {
                ["Slash.png"] = _slash,
                ["Gone.png"] = _gone,
                ["Ash.png"] = _ash,
            },
        },
        ["Slash.png", "Burst.png"],
        file => file switch
        {
            "Burst.png" => (32, 8),
            "Slash.png" => (16, 8),
            _ => null,
        });
}
