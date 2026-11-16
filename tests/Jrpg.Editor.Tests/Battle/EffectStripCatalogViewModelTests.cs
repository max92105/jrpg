using System.ComponentModel;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Battle;

namespace Jrpg.Editor.Tests.Battle;

// The Effect strips page's rules (decision 659): the test world's catalog with records built here, over a folder the
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
        var catalog = Catalog();

        Assert.Equal(["Burst.png", "Slash.png", "Ash.png", "Gone.png"], catalog.Strips.Select(strip => strip.File));
    }

    [Fact]
    public void Strips_ImageWithoutARecord_ShowsTheUnlistedPace()
    {
        var burst = Catalog().Strips[0];

        Assert.Equal(("80", "0", false), (burst.FrameMillisecondsText, burst.HitFrameText, burst.IsListed));
    }

    [Fact]
    public void ToStrips_UntouchedImage_WritesNoRecord()
    {
        Assert.Equal([_ash, _gone, _slash], Catalog().ToStrips());
    }

    [Fact]
    public void ToStrips_EditedImage_WritesItsRecord()
    {
        var catalog = Catalog();

        catalog.Strips[0].FrameMillisecondsText = "50";

        Assert.Contains(new EffectStripDefinition { File = "Burst.png", FrameMilliseconds = 50 }, catalog.ToStrips());
    }

    [Fact]
    public void ResetToUnlisted_ListedStrip_DropsItsRecord()
    {
        var catalog = Catalog();
        catalog.SelectedStrip = catalog.Strips[1];

        catalog.ResetToUnlistedCommand.Execute(null);

        Assert.Equal([_ash, _gone], catalog.ToStrips());
    }

    [Fact]
    public void ResetToUnlisted_UnlistedStrip_IsGreyed()
    {
        var catalog = Catalog();
        catalog.SelectedStrip = catalog.Strips[0];

        Assert.False(catalog.ResetToUnlistedCommand.CanExecute(null));
    }

    [Fact]
    public void Problem_ChosenStripsBoxNotANumber_IsItsProblem()
    {
        var catalog = Catalog();

        catalog.SelectedStrip!.FrameMillisecondsText = "fast";

        Assert.Equal("Frame time must be a whole number (was 'fast').", catalog.Problem);
    }

    [Fact]
    public void Problem_ListFine_IsNone()
    {
        Assert.Null(Catalog().Problem);
    }

    [Fact]
    public void Problem_HitFramePastTheImage_IsTheChosenStrips()
    {
        var catalog = Catalog();

        catalog.SelectedStrip!.HitFrameText = "9";

        Assert.Equal("Hit frame 9 is past the image's 4 frames (from 0).", catalog.Problem);
    }

    [Fact]
    public void Problem_BoxChanged_IsRaised()
    {
        var catalog = Catalog();
        var notified = new List<string?>();
        ((INotifyPropertyChanged)catalog).PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Strips[1].HitFrameText = "0";

        Assert.Contains(nameof(EffectStripCatalogViewModel.Problem), notified);
    }

    [Fact]
    public void Problem_AnotherStripsBox_NamesItsFile()
    {
        var catalog = Catalog();

        catalog.Strips[1].FrameMillisecondsText = "fast";

        Assert.Equal("Slash.png: Frame time must be a whole number (was 'fast').", catalog.Problem);
    }

    [Fact]
    public void Problem_FrameTimeZero_IsDatasMessage()
    {
        var catalog = Catalog();

        catalog.SelectedStrip!.FrameMillisecondsText = "0";

        Assert.Equal(
            "effect-strips.json: effect strip 'Burst.png': frameMilliseconds must be at least 1 (was 0).", catalog.Problem);
    }

    [Fact]
    public void ResetToUnlisted_UnlistedStripEdited_IsLit()
    {
        var catalog = Catalog();
        var raised = 0;
        catalog.ResetToUnlistedCommand.CanExecuteChanged += (_, _) => raised++;

        catalog.SelectedStrip!.HitFrameText = "1";

        Assert.Equal((1, true), (raised, catalog.ResetToUnlistedCommand.CanExecute(null)));
    }

    [Fact]
    public void SelectedStrip_Changed_RaisesResetsCanExecute()
    {
        var catalog = Catalog();
        var raised = 0;
        catalog.ResetToUnlistedCommand.CanExecuteChanged += (_, _) => raised++;

        catalog.SelectedStrip = catalog.Strips[1];

        Assert.Equal((1, true), (raised, catalog.ResetToUnlistedCommand.CanExecute(null)));
    }

    [Fact]
    public void SwingChoices_Folder_OffersItsImagesInNameOrder()
    {
        Assert.Equal(["Burst.png", "Slash.png"], Catalog().SwingChoices);
    }

    [Fact]
    public void ToStrips_UntouchedRecordAtTheUnlistedPace_KeepsIt()
    {
        Assert.Contains(_ash, Catalog().ToStrips());
    }

    [Fact]
    public void ResetToUnlisted_RecordAtTheUnlistedPace_DropsIt()
    {
        var catalog = Catalog();
        catalog.SelectedStrip = catalog.Strips[2];

        catalog.ResetToUnlistedCommand.Execute(null);

        Assert.DoesNotContain(_ash, catalog.ToStrips());
    }

    [Fact]
    public void ResetToUnlisted_RecordAtTheUnlistedPace_GreysItself()
    {
        var catalog = Catalog();
        catalog.SelectedStrip = catalog.Strips[2];
        var raised = 0;
        catalog.ResetToUnlistedCommand.CanExecuteChanged += (_, _) => raised++;

        catalog.ResetToUnlistedCommand.Execute(null);

        Assert.Equal((1, false), (raised, catalog.ResetToUnlistedCommand.CanExecute(null)));
    }

    [Fact]
    public void Choose_UnlistedImage_ChoosesIt()
    {
        var catalog = Catalog();
        catalog.SelectedStrip = catalog.Strips[1];

        catalog.Choose("Burst.png");

        Assert.Same(catalog.Strips[0], catalog.SelectedStrip);
    }

    [Fact]
    public void Choose_UnknownFile_KeepsTheChoice()
    {
        var catalog = Catalog();
        catalog.SelectedStrip = catalog.Strips[1];

        catalog.Choose("Nowhere.png");

        Assert.Same(catalog.Strips[1], catalog.SelectedStrip);
    }

    [Fact]
    public void ChosenKey_ChosenStrip_IsItsFile()
    {
        var catalog = Catalog();

        catalog.Choose("Gone.png");

        Assert.Equal("Gone.png", catalog.ChosenKey);
    }

    // The records are set up out of order, so the list's order is the page's own.
    private static EffectStripCatalogViewModel Catalog() => new(
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
