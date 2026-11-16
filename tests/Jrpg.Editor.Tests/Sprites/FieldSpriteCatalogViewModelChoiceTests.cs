using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

// The Character sprites page's choice by file, as its hosts make it (the mode bar plan's IEditedCatalog), over the test
// world's two records with the folder's listing and the image sizes faked: the placeholder's 384 x 288 sheet in the
// folder, the player's record without its image, and a bought 128 x 224 image not set up yet.
public sealed class FieldSpriteCatalogViewModelChoiceTests
{
    private const string Placeholder = "npc_placeholder.png";
    private const string Player = "player.png";
    private const string Bought = "villager.png";

    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly Dictionary<string, (int Width, int Height)> _sizes = new(StringComparer.Ordinal)
    {
        [Placeholder] = (384, 288),
        [Bought] = (128, 224),
    };

    [Fact]
    public void Choose_ImageWithoutRecord_ChoosesIt()
    {
        var sprites = Open();

        sprites.Choose(Bought);

        Assert.Equal(Bought, sprites.Chosen?.File);
    }

    [Fact]
    public void Choose_RecordWithoutImage_ChoosesIt()
    {
        var sprites = Open();

        sprites.Choose(Player);

        Assert.Equal((Player, true), (sprites.Chosen?.File, sprites.Chosen?.IsImageMissing));
    }

    [Fact]
    public void Choose_UnknownFile_KeepsTheChoice()
    {
        var sprites = Open();
        sprites.Choose(Bought);

        sprites.Choose("nobody.png");

        Assert.Equal(Bought, sprites.Chosen?.File);
    }

    [Fact]
    public void ChosenKey_ChosenImage_IsItsFile()
    {
        var sprites = Open();

        sprites.Chosen = sprites.Choices.Single(choice => choice.File == Placeholder);

        Assert.Equal(Placeholder, sprites.ChosenKey);
    }

    private FieldSpriteCatalogViewModel Open() =>
        new(
            new CatalogDraft(_catalog),
            () => [Placeholder, Bought],
            file => _sizes.TryGetValue(file, out var size) ? size : null);
}
