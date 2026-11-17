using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Backgrounds;
using Jrpg.Editor.Tiles;
using static Jrpg.Editor.Tests.Backgrounds.BackgroundFixtures;

namespace Jrpg.Editor.Tests.Backgrounds;

// The Battle backgrounds page's rules (decisions 901, 902): the test world's catalog with records built here, over a
// folder the test lists: A.png (a still), Pulse.png (animated), and a record for Gone.png, missing from the folder; a
// file an import copies in reads the pulse sheet's size.
public sealed class BattleBackgroundCatalogViewModelTests
{
    private static readonly ContentCatalog _testWorld =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private static readonly AnimatedBackgroundDefinition _gone =
        Pulse with { File = "Gone.png", Animation = Pulse.Animation with { FrameMilliseconds = 120 } };

    // The images given out of name order, so the page's own sort shows.
    private static readonly string[] _images = ["Pulse.png", "A.png"];

    [Fact]
    public void Constructor_ImagesThenMissingRecords_InThatOrder()
    {
        var catalog = Catalog(_images);

        Assert.Equal(
            [("A.png", false, false), ("Pulse.png", true, false), ("Gone.png", true, true)],
            catalog.Backgrounds.Select(row => (row.File, row.IsAnimated, row.IsImageMissing)));
    }

    [Fact]
    public void Constructor_RecordWhoseImageIsMissing_IsNeverLookedUp()
    {
        var catalog = new BattleBackgroundCatalogViewModel(WithRecords(), _images, _ => PulseSheet);

        Assert.True(catalog.Backgrounds.Single(row => row.File == _gone.File).IsImageMissing);
    }

    [Fact]
    public void Constructor_RowsAndRecords_ChoosesTheFirstRow() => Assert.Equal("A.png", Catalog(_images).ChosenKey);

    [Fact]
    public void Constructor_NoImagesNorRecords_ChoosesNothing()
    {
        var noRecords = _testWorld with { AnimatedBackgrounds = new Dictionary<string, AnimatedBackgroundDefinition>() };

        var catalog = new BattleBackgroundCatalogViewModel(noRecords, [], _ => null);

        Assert.Equal((null, null), (catalog.SelectedBackground, catalog.Problem));
    }

    [Fact]
    public void Problem_Untouched_IsNull() => Assert.Null(Catalog(_images).Problem);

    [Fact]
    public void Problem_ChosenRowsTimeInvalid_IsItsOwn()
    {
        var catalog = Catalog(_images);
        catalog.Choose(Pulse.File);

        catalog.SelectedBackground!.FrameMillisecondsText = "0";

        Assert.Equal("Frame time must be a whole number of milliseconds from 1.", catalog.Problem);
    }

    [Fact]
    public void Problem_AnotherRowsTimeInvalid_NamesItsFile()
    {
        var catalog = Catalog(_images);

        Row(catalog, Pulse.File).FrameMillisecondsText = "0";

        Assert.Equal("Pulse.png: Frame time must be a whole number of milliseconds from 1.", catalog.Problem);
    }

    [Fact]
    public void Problem_AnotherRowsTimeTyped_RaisesItsChange()
    {
        var catalog = Catalog(_images);
        List<string?> raised = [];
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        Row(catalog, Pulse.File).FrameMillisecondsText = "0";

        Assert.Contains(nameof(BattleBackgroundCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void SelectedBackground_Changed_RaisesProblemsChange()
    {
        var catalog = Catalog(_images);
        List<string?> raised = [];
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        catalog.Choose(Pulse.File);

        Assert.Contains(nameof(BattleBackgroundCatalogViewModel.Problem), raised);
    }

    // A folder never lists one image twice; the duplicate is how a list Data refuses reaches the page here.
    [Fact]
    public void Problem_RecordListedTwice_IsDatasLine()
    {
        var catalog = Catalog(["Pulse.png", "Pulse.png"]);

        Assert.NotNull(catalog.Problem);
        Assert.Equal(CatalogProblems.OfAnimatedBackgrounds([_gone, Pulse, Pulse]), catalog.Problem);
    }

    // Pulse.png is listed before Gone.png, so the records' order is the sort's own.
    [Fact]
    public void ToRecords_TwoAnimated_SortedByFile() => Assert.Equal([_gone, Pulse], Catalog(_images).ToRecords());

    [Fact]
    public void ToRecords_TypedTime_IsInItsRecord()
    {
        var catalog = Catalog(_images);

        Row(catalog, Pulse.File).FrameMillisecondsText = "350";

        Assert.Equal(350, catalog.ToRecords().Single(record => record.File == Pulse.File).Animation.FrameMilliseconds);
    }

    [Fact]
    public void ToRecords_WithAProblem_Throws()
    {
        var catalog = Catalog(_images);
        Row(catalog, Pulse.File).FrameMillisecondsText = "0";

        Assert.Throws<InvalidOperationException>(catalog.ToRecords);
    }

    [Fact]
    public void Choose_KnownFile_ChoosesIt()
    {
        var catalog = Catalog(_images);

        catalog.Choose(_gone.File);

        Assert.Same(Row(catalog, _gone.File), catalog.SelectedBackground);
    }

    [Fact]
    public void Choose_UnknownFile_KeepsTheChoice()
    {
        var catalog = Catalog(_images);
        catalog.Choose(Pulse.File);

        catalog.Choose("Nowhere.png");

        Assert.Equal(Pulse.File, catalog.ChosenKey);
    }

    [Fact]
    public void ChosenKey_RowChosen_IsItsFile()
    {
        var catalog = Catalog(_images);

        catalog.SelectedBackground = Row(catalog, _gone.File);

        Assert.Equal(_gone.File, catalog.ChosenKey);
    }

    [Fact]
    public void TakeImport_OverAnimated_MakesItStillAtTheNewSize()
    {
        var size = PulseSheet;
        var catalog = new BattleBackgroundCatalogViewModel(WithRecords(), _images, _ => size);
        size = (1280, 720);

        catalog.TakeImport(new SheetImportResult([Pulse.File], []));

        var row = Row(catalog, Pulse.File);
        Assert.Equal((false, "Still, 1280 x 720 px"), (row.IsAnimated, row.KindLine));
    }

    [Fact]
    public void TakeImport_NewFile_InsertedInNameOrderAndChosen()
    {
        var catalog = Catalog(_images);

        catalog.TakeImport(new SheetImportResult(["Zed.png", "M.png"], []));

        Assert.Equal(
            ("M.png", "A.png, M.png, Pulse.png, Zed.png, Gone.png"),
            (catalog.ChosenKey, string.Join(", ", catalog.Backgrounds.Select(row => row.File))));
    }

    [Fact]
    public void TakeImport_NoFiles_KeepsTheRowsAndChoice()
    {
        var catalog = Catalog(_images);
        catalog.Choose(Pulse.File);

        catalog.TakeImport(new SheetImportResult([], []));

        Assert.Equal((Pulse.File, 3), (catalog.ChosenKey, catalog.Backgrounds.Count));
    }

    [Fact]
    public void TakeImport_ImportedRowTyped_RaisesProblemsChange()
    {
        var catalog = Catalog(_images);
        catalog.TakeImport(new SheetImportResult(["M.png"], []));
        List<string?> raised = [];
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        Row(catalog, "M.png").FrameMillisecondsText = "0";

        Assert.Contains(nameof(BattleBackgroundCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void TakeImport_ReplacedRowTyped_RaisesNothing()
    {
        var catalog = Catalog(_images);
        var replaced = Row(catalog, Pulse.File);
        catalog.TakeImport(new SheetImportResult([Pulse.File], []));
        List<string?> raised = [];
        catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        replaced.FrameMillisecondsText = "0";

        Assert.Empty(raised);
    }

    private static BattleBackgroundCatalogViewModel Catalog(string[] images) =>
        new(WithRecords(), images, file => file switch
        {
            "Gone.png" => null,
            _ => PulseSheet,
        });

    // The records are set up out of order, so the list's order is the page's own.
    private static ContentCatalog WithRecords() => _testWorld with
    {
        AnimatedBackgrounds = new Dictionary<string, AnimatedBackgroundDefinition>
        {
            [_gone.File] = _gone,
            [Pulse.File] = Pulse,
        },
    };

    private static BattleBackgroundViewModel Row(BattleBackgroundCatalogViewModel catalog, string file) =>
        catalog.Backgrounds.Single(row => row.File == file);
}
