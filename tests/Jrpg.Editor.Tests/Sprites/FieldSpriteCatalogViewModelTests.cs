using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

/// <summary>
/// The Character sprites page's list and setups (decisions 569, 570) over the test world's two records, with the
/// folder's listing and the image sizes faked: the player's and placeholder's 384 x 288 sheets, and a bought 128 x 224
/// image not set up yet.
/// </summary>
public sealed class FieldSpriteCatalogViewModelTests
{
    private const string Placeholder = "npc_placeholder.png";
    private const string Player = "player.png";
    private const string Bought = "villager.png";

    private static readonly string _testWorld = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_testWorld);

    private readonly Dictionary<string, (int Width, int Height)> _sizes = new(StringComparer.Ordinal)
    {
        [Placeholder] = (384, 288),
        [Player] = (384, 288),
        [Bought] = (128, 224),
        ["odd.png"] = (102, 224),
    };

    private readonly CatalogDraft _draft = new(_catalog);
    private List<string> _files = [Placeholder, Player, Bought];

    [Fact]
    public void Choices_FolderAndRecords_ListsEveryImageThenTheMissingOnes()
    {
        _files = [Bought, Placeholder];

        var sprites = Open();

        string[] labels = [$"{Bought} (not set up)", Placeholder, $"{Player} (image missing)"];
        Assert.Equal(labels, sprites.Choices.Select(choice => choice.Label));
    }

    [Fact]
    public void Choose_SetUpImage_StartsFromItsRecord()
    {
        var sprites = Open();

        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Player);

        Assert.Equal((8, 6, 1), (sprites.Setup!.Columns, sprites.Setup.Rows, sprites.Setup.Scale));
    }

    [Fact]
    public void Choose_NewImage_StartsFromTheGuess()
    {
        var sprites = Open();

        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Bought);

        Assert.Equal((4, 7), (sprites.Setup!.Columns, sprites.Setup.Rows));
        Assert.Null(sprites.Problem);
    }

    [Fact]
    public void Choose_ImageMissing_HasNoSetup()
    {
        _files = [Placeholder];
        var sprites = Open();

        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Player);

        Assert.Null(sprites.Setup);
    }

    [Fact]
    public void Choose_BackToEditedImage_KeepsItsEdits()
    {
        var sprites = Open();
        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Player);
        sprites.Setup!.Scale = 2;

        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Placeholder);
        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Player);

        Assert.Equal(2, sprites.Setup!.Scale);
    }

    [Fact]
    public void ToSheets_OneImageEdited_ReturnsOnlyIt()
    {
        var sprites = Open();
        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Placeholder);
        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Player);
        sprites.Setup!.Scale = 2;

        var sheet = Assert.Single(sprites.ToSheets());

        Assert.Equal((Player, 2), (sheet.File, sheet.Scale));
    }

    [Fact]
    public void Problem_OtherEditedImageBroken_NamesItsFile()
    {
        var sprites = Open();
        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Player);
        sprites.Setup!.Scale = 0;

        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Placeholder);

        Assert.Equal(
            $"{Player}: field-sprites.json: field sprite '{Player}': scale must be within 1..8 (was 0).", sprites.Problem);
    }

    // The problem reaches the page's host whichever setup changed, so OK follows every edit (the editor rule on notes).
    [Fact]
    public void Problem_SetupEdited_RaisesTheChange()
    {
        var sprites = Open();
        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Player);
        var raised = new List<string?>();
        sprites.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        sprites.Setup!.Scale = 0;

        Assert.Contains(nameof(FieldSpriteCatalogViewModel.Problem), raised);
    }

    [Fact]
    public void RefreshAfterImport_NewImage_ListsAndChoosesIt()
    {
        _files = [Placeholder, Player];
        var sprites = Open();
        _files = [Placeholder, Player, Bought];

        sprites.RefreshAfterImport([Bought]);

        Assert.Equal($"{Bought} (not set up)", sprites.Chosen?.Label);
        Assert.Equal(4, sprites.Setup!.Columns);
    }

    // A replaced image may have another size: its setup opens anew from the new size, not the old guess.
    [Fact]
    public void RefreshAfterImport_ReplacedImage_ReopensItsSetup()
    {
        var sprites = Open();
        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Bought);
        Assert.Equal(32, sprites.Setup!.FrameWidth);
        _sizes[Bought] = (96, 128);

        sprites.RefreshAfterImport([Bought]);

        Assert.Equal(24, sprites.Setup!.FrameWidth);
    }

    // Choosing a new image and leaving it makes no record (a saved record can't be removed, decision 569).
    [Fact]
    public void ToSheets_NewImageChosenButUntouched_IsEmpty()
    {
        var sprites = Open();

        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Bought);

        Assert.Empty(sprites.ToSheets());
    }

    // The guess for a 102 px wide image doesn't split it: the setup says so, but a setup nobody edited never greys OK.
    [Fact]
    public void Problem_NewImageUntouchedWithABadGuess_DoesNotGreyOk()
    {
        _files = [Placeholder, Player, "odd.png"];
        var sprites = Open();

        sprites.Chosen = sprites.Choices.Single(choice => choice.File == "odd.png");

        Assert.Equal("102 px doesn't split into 4 columns.", sprites.Setup!.Problem);
        Assert.Null(sprites.Problem);
    }

    // An import means "set this up": OK keeps the guess even when nothing was touched (the merchant bug).
    [Fact]
    public void ToSheets_ImportedImageUntouched_ReturnsItsGuess()
    {
        _files = [Placeholder, Player];
        var sprites = Open();
        _files = [Placeholder, Player, Bought];

        sprites.RefreshAfterImport([Bought]);

        var sheet = Assert.Single(sprites.ToSheets());
        Assert.Equal((Bought, 32, 32), (sheet.File, sheet.FrameWidth, sheet.FrameHeight));
    }

    // An imported image whose guess doesn't split it shows the reason in its setup, but greys nothing until edited, so
    // it can't trap the session's other edits behind OK (decision 594).
    [Fact]
    public void Problem_ImportedImageWithABadGuess_DoesNotGreyOk()
    {
        _files = [Placeholder, Player];
        var sprites = Open();
        _files = [Placeholder, Player, "odd.png"];

        sprites.RefreshAfterImport(["odd.png"]);

        Assert.Equal("102 px doesn't split into 4 columns.", sprites.Setup!.Problem);
        Assert.Null(sprites.Problem);
        Assert.Empty(sprites.ToSheets());
    }

    // Once the bad guess is edited, the import counts like any edited setup.
    [Fact]
    public void ToSheets_ImportedBadGuessThenFixed_ReturnsTheFix()
    {
        _files = [Placeholder, Player];
        var sprites = Open();
        _files = [Placeholder, Player, "odd.png"];
        sprites.RefreshAfterImport(["odd.png"]);

        sprites.Setup!.Columns = 3;
        sprites.Setup.Rows = 7;
        sprites.Setup.Walk.Frames = 3;

        var sheet = Assert.Single(sprites.ToSheets());
        Assert.Equal(("odd.png", 34, 32), (sheet.File, sheet.FrameWidth, sheet.FrameHeight));
    }

    // A failed import copies nothing: the choice and the edits stay, and nothing new counts.
    [Fact]
    public void RefreshAfterImport_NothingCopied_KeepsTheChoiceAndCountsNothing()
    {
        var sprites = Open();
        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Bought);

        sprites.RefreshAfterImport([]);

        Assert.Equal(Bought, sprites.Chosen?.File);
        Assert.Empty(sprites.ToSheets());
    }

    private FieldSpriteCatalogViewModel Open() =>
        new(_draft, () => _files, file => _sizes.TryGetValue(file, out var size) ? size : null);
}
