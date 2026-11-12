using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// field-sprites.json is checked against the objects and the new game (ADR 0043, decision 567): a copy of the content
// with a sprite named without a record proves the loader makes that check and names who names the sprite.
public sealed class ContentCatalogLoaderFieldSpriteTests : IDisposable
{
    private const string PlaceholderSprite = "npc_placeholder.png";

    private static readonly string _repository = Path.Combine(AppContext.BaseDirectory, "content");

    private readonly CopiedContentRoot _content = new("field-sprites", "test-world");

    public void Dispose() => _content.Dispose();

    // The repository is pinned by id only (ADR 0037): its numbers are the developer's to change.
    [Fact]
    public void Load_Repository_HasThePlayerAndPlaceholderSheets()
    {
        var catalog = ContentCatalogLoader.Load(_repository);

        Assert.Contains(catalog.NewGame.FieldSpriteFile, catalog.FieldSprites.Keys);
        Assert.Contains(PlaceholderSprite, catalog.FieldSprites.Keys);
    }

    // The frozen copy keeps today's player numbers (decision 561), which FS3 pins the drawing against.
    [Fact]
    public void Load_TestWorld_PlayerSheetHoldsTodaysNumbers()
    {
        var catalog = ContentCatalogLoader.Load(_content.Path);

        var player = catalog.FieldSprites[catalog.NewGame.FieldSpriteFile];
        Assert.Equal((48, 48, 1, 0), (player.FrameWidth, player.FrameHeight, player.Scale, player.FeetPixels));
        Assert.Equal((8, 100), (player.Walk.Frames.FrameCount, player.Walk.Frames.FrameMilliseconds));
    }

    [Fact]
    public void Load_ObjectSpriteWithoutRecord_ThrowsNamingTheObject()
    {
        _content.Replace("objects.json", $"\"{PlaceholderSprite}\"", "\"ghost.png\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.StartsWith("field-sprites.json: object '", exception.Message, StringComparison.Ordinal);
        Assert.EndsWith("names field sprite 'ghost.png', which has no record.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_NewGameSpriteWithoutRecord_ThrowsNamingTheNewGame()
    {
        _content.Replace("new-game.json", "\"player.png\"", "\"hero.png\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("field-sprites.json: the new game names field sprite 'hero.png', which has no record.", exception.Message);
    }

    // The game looks the record up by the name as written (decision 567).
    [Fact]
    public void Load_SpriteNamedInAnotherCase_Throws()
    {
        _content.Replace("new-game.json", "\"player.png\"", "\"Player.png\"");

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Contains("'Player.png', which has no record", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Load_MissingFieldSpritesFile_ThrowsNamingTheFile()
    {
        File.Delete(_content.PathOf("field-sprites.json"));

        var exception = Assert.Throws<ContentValidationException>(() => ContentCatalogLoader.Load(_content.Path));

        Assert.Equal("field-sprites.json: the file is missing.", exception.Message);
    }
}
