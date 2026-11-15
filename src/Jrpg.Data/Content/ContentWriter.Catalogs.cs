using Jrpg.Core.Battle;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
using Jrpg.Core.Loot;
using Jrpg.Core.Maps;
using Jrpg.Core.Quests;
using Jrpg.Core.Sprites;
using Jrpg.Core.Trade;
using Jrpg.Data.Maps;
using Jrpg.Data.Quests;

namespace Jrpg.Data.Content;

/// <summary>
/// The catalog files the writer writes: each checked as the loader checks it, then serialized sorted by id; one place
/// to add a catalog.
/// </summary>
public sealed partial class ContentWriter
{
    private static void CheckCatalogs(ContentCatalog catalog)
    {
        TilesheetCatalogValidator.Validate(ContentCatalogLoader.TilesheetsFile, SortedSheets(catalog));
        TileCatalogValidator.Validate(ContentCatalogLoader.TilesFile, SortedTiles(catalog), catalog.Tilesheets.Keys.ToHashSet());
        ObjectCatalogValidator.Validate(ContentCatalogLoader.ObjectsFile, SortedObjects(catalog), catalog.Tiles.Keys.ToHashSet());
        QuestCatalogValidator.Validate(ContentCatalogLoader.QuestsFile, SortedQuests(catalog));
        CatalogProblems.CheckFieldSprites(SortedFieldSprites(catalog), catalog);
        CatalogProblems.CheckShops(SortedShops(catalog), catalog);
        CatalogProblems.CheckDropTables(SortedDropTables(catalog), catalog);
        CatalogProblems.CheckEnemies(SortedEnemies(catalog), catalog);
        CatalogProblems.CheckCinematics(SortedCinematics(catalog));
    }

    // Each catalog file's bytes as it would be written, sorted by id: what the stamps compare and the writes carry.
    private static List<CatalogWrite> SerializedCatalogs(ContentCatalog catalog) =>
    [
        new(ContentCatalogLoader.TilesheetsFile, ContentFileWriter.Serialize(SortedSheets(catalog))),
        new(ContentCatalogLoader.TilesFile, ContentFileWriter.Serialize(SortedTiles(catalog))),
        new(ContentCatalogLoader.ObjectsFile, ContentFileWriter.Serialize(SortedObjects(catalog))),
        new(ContentCatalogLoader.QuestsFile, ContentFileWriter.Serialize(SortedQuests(catalog))),
        new(ContentCatalogLoader.FieldSpritesFile, ContentFileWriter.Serialize(SortedFieldSprites(catalog))),
        new(ContentCatalogLoader.ShopsFile, ContentFileWriter.Serialize(SortedShops(catalog))),
        new(ContentCatalogLoader.EnemiesFile, ContentFileWriter.Serialize(SortedEnemies(catalog))),
        new(ContentCatalogLoader.DropTablesFile, ContentFileWriter.Serialize(SortedDropTables(catalog))),
        new(ContentCatalogLoader.CinematicsFile, ContentFileWriter.Serialize(SortedCinematics(catalog))),
    ];

    private static List<TilesheetDefinition> SortedSheets(ContentCatalog catalog) =>
        [.. catalog.Tilesheets.Values.OrderBy(sheet => sheet.Id, StringComparer.Ordinal)];

    private static List<TileDefinition> SortedTiles(ContentCatalog catalog) => [.. catalog.Tiles.Values.OrderBy(tile => tile.Id)];

    private static List<ObjectDefinition> SortedObjects(ContentCatalog catalog) =>
        [.. catalog.Objects.Values.OrderBy(definition => definition.Id, StringComparer.Ordinal)];

    private static List<QuestDefinition> SortedQuests(ContentCatalog catalog) =>
        [.. catalog.Quests.Values.OrderBy(quest => quest.Id, StringComparer.Ordinal)];

    private static List<FieldSpriteSheet> SortedFieldSprites(ContentCatalog catalog) =>
        [.. catalog.FieldSprites.Values.OrderBy(sheet => sheet.File, StringComparer.Ordinal)];

    private static List<ShopDefinition> SortedShops(ContentCatalog catalog) =>
        [.. catalog.Shops.Values.OrderBy(shop => shop.Id, StringComparer.Ordinal)];

    private static List<EnemyDefinition> SortedEnemies(ContentCatalog catalog) =>
        [.. catalog.Enemies.Values.OrderBy(enemy => enemy.Id, StringComparer.Ordinal)];

    private static List<DropTable> SortedDropTables(ContentCatalog catalog) =>
        [.. catalog.DropTables.Values.OrderBy(table => table.Id, StringComparer.Ordinal)];

    private static List<CinematicDefinition> SortedCinematics(ContentCatalog catalog) =>
        [.. catalog.Cinematics.Values.OrderBy(cinematic => cinematic.Id, StringComparer.Ordinal)];

    // A catalog file's name and its bytes as they will be written.
    private sealed record CatalogWrite(string File, byte[] Bytes);
}
