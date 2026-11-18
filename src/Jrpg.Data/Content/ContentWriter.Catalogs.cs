using System.Text.Json.Serialization;
using Jrpg.Core.Battle;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Quests;
using Jrpg.Core.Skills;
using Jrpg.Core.Sprites;
using Jrpg.Core.Statuses;
using Jrpg.Core.Trade;
using Jrpg.Data.Items;
using Jrpg.Data.Maps;
using Jrpg.Data.Quests;

namespace Jrpg.Data.Content;

/// <summary>
/// The catalog files the writer writes, the new game's among them (decision 961): each checked as the loader checks it,
/// then serialized sorted by id; one place to add a catalog.
/// </summary>
public sealed partial class ContentWriter
{
    private static void CheckCatalogs(ContentCatalog catalog)
    {
        TilesheetCatalogValidator.Validate(ContentCatalogLoader.TilesheetsFile, SortedSheets(catalog));
        TileCatalogValidator.Validate(ContentCatalogLoader.TilesFile, SortedTiles(catalog), catalog.Tilesheets.Keys.ToHashSet());
        ObjectCatalogValidator.Validate(ContentCatalogLoader.ObjectsFile, SortedObjects(catalog), catalog.Tiles.Keys.ToHashSet());
        QuestCatalogValidator.Validate(ContentCatalogLoader.QuestsFile, SortedQuests(catalog));
        // The new game has no check of its own here, so it is checked once (decision 1018): its sprite's record by the
        // field sprites' check, its rules against the members, items and maps by CheckItems.
        CatalogProblems.CheckFieldSprites(SortedFieldSprites(catalog), catalog);
        // As the loader reads them: a unique names the pool, so the pool's own problem comes first.
        CatalogProblems.CheckAffixes(SortedAffixes(catalog));
        CatalogProblems.CheckItems(SortedItems(catalog), catalog);
        CatalogProblems.CheckShops(SortedShops(catalog), catalog);
        CatalogProblems.CheckDropTables(SortedDropTables(catalog), catalog);

        // Before the enemies, so a skill's own problem is named before an enemy's tactic naming it.
        CatalogProblems.CheckTieredEffects(SortedTieredEffects(catalog));
        CatalogProblems.CheckStatuses(SortedStatuses(catalog));
        // The classes' own rules too, against these skills, so they are checked once; their skills' levels right after.
        CatalogProblems.CheckSkills(SortedSkills(catalog), catalog);
        CatalogProblems.CheckClassSkillLevels(SortedClasses(catalog), catalog.Rules.Experience);
        // After the classes, so a class's own problem is named before a member's naming it.
        CatalogProblems.CheckPartyMembers(SortedPartyMembers(catalog), catalog);
        CatalogProblems.CheckEnemies(SortedEnemies(catalog), catalog);
        CatalogProblems.CheckEncounters(SortedEncounters(catalog), catalog);
        CatalogProblems.CheckCinematics(SortedCinematics(catalog));
        CatalogProblems.CheckEffectStrips(SortedEffectStrips(catalog));
        CatalogProblems.CheckAnimatedBackgrounds(SortedAnimatedBackgrounds(catalog));
    }

    // Each catalog file's bytes as it would be written, sorted by id (the effect strips and animated backgrounds by
    // file; the new game is one record): what the stamps compare and the writes carry.
    private static List<CatalogWrite> SerializedCatalogs(ContentCatalog catalog) =>
    [
        new(ContentCatalogLoader.TilesheetsFile, ContentFileWriter.Serialize(SortedSheets(catalog))),
        new(ContentCatalogLoader.TilesFile, ContentFileWriter.Serialize(SortedTiles(catalog))),
        new(ContentCatalogLoader.ObjectsFile, ContentFileWriter.Serialize(SortedObjects(catalog))),
        new(ContentCatalogLoader.QuestsFile, ContentFileWriter.Serialize(SortedQuests(catalog))),
        new(ContentCatalogLoader.FieldSpritesFile, ContentFileWriter.Serialize(SortedFieldSprites(catalog))),
        new(ContentCatalogLoader.ShopsFile, ContentFileWriter.Serialize(SortedShops(catalog))),
        new(ContentCatalogLoader.EnemiesFile, ContentFileWriter.Serialize(SortedEnemies(catalog))),
        new(ContentCatalogLoader.EncountersFile, ContentFileWriter.Serialize(SortedEncounters(catalog))),
        new(ContentCatalogLoader.DropTablesFile, ContentFileWriter.Serialize(SortedDropTables(catalog))),
        new(ContentCatalogLoader.CinematicsFile, ContentFileWriter.Serialize(SortedCinematics(catalog))),
        new(ContentCatalogLoader.EffectStripsFile, ContentFileWriter.Serialize(SortedEffectStrips(catalog))),
        new(ContentCatalogLoader.AnimatedBackgroundsFile, ContentFileWriter.Serialize(SortedAnimatedBackgrounds(catalog))),
        new(ContentCatalogLoader.EffectsFile, ContentFileWriter.Serialize(SortedTieredEffects(catalog))),
        new(ContentCatalogLoader.StatusesFile, ContentFileWriter.Serialize(SortedStatuses(catalog))),
        new(ContentCatalogLoader.SkillsFile, ContentFileWriter.Serialize(SortedSkills(catalog), ReferenceConverters(catalog))),
        new(ContentCatalogLoader.ClassesFile, ContentFileWriter.Serialize(SortedClasses(catalog))),
        new(ContentCatalogLoader.PartyMembersFile, ContentFileWriter.Serialize(SortedPartyMembers(catalog))),
        new(ContentCatalogLoader.ItemsFile, ContentFileWriter.Serialize(SortedItems(catalog), ItemConverters(catalog))),
        new(ContentCatalogLoader.AffixesFile, ContentFileWriter.Serialize(SortedAffixes(catalog))),
        NewGameWrite(catalog),
    ];

    // Shared with the map rename, which rewrites the new game's start and renews its stamp with these bytes.
    private static CatalogWrite NewGameWrite(ContentCatalog catalog) =>
        new(ContentCatalogLoader.NewGameFile, ContentFileWriter.Serialize(catalog.NewGame));

    // A skill's or an item's effect names its tier and the statuses it grants by id (decision 972); effects.json and
    // statuses.json hold those records whole, so only skills.json and items.json are written with these.
    private static JsonConverter[] ReferenceConverters(ContentCatalog catalog) =>
    [
        new CatalogReferenceConverter<TieredEffect>(catalog.TieredEffects, effect => effect.Id, "effect"),
        new CatalogReferenceConverter<StatusDefinition>(catalog.Statuses, status => status.Id, "status"),
    ];

    // An item's effect names records as a skill's does; a unique's fixed affix is written as its affix's id and value,
    // the stat being the pool's (decision 865).
    private static JsonConverter[] ItemConverters(ContentCatalog catalog) =>
        [.. ReferenceConverters(catalog), new FixedAffixConverter(catalog.Affixes)];

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

    private static List<EncounterDefinition> SortedEncounters(ContentCatalog catalog) =>
        [.. catalog.Encounters.Values.OrderBy(encounter => encounter.Id, StringComparer.Ordinal)];

    private static List<DropTable> SortedDropTables(ContentCatalog catalog) =>
        [.. catalog.DropTables.Values.OrderBy(table => table.Id, StringComparer.Ordinal)];

    private static List<CinematicDefinition> SortedCinematics(ContentCatalog catalog) =>
        [.. catalog.Cinematics.Values.OrderBy(cinematic => cinematic.Id, StringComparer.Ordinal)];

    private static List<EffectStripDefinition> SortedEffectStrips(ContentCatalog catalog) =>
        [.. catalog.EffectStrips.Values.OrderBy(strip => strip.File, StringComparer.Ordinal)];

    private static List<AnimatedBackgroundDefinition> SortedAnimatedBackgrounds(ContentCatalog catalog) =>
        [.. catalog.AnimatedBackgrounds.Values.OrderBy(background => background.File, StringComparer.Ordinal)];

    private static List<TieredEffect> SortedTieredEffects(ContentCatalog catalog) =>
        [.. catalog.TieredEffects.Values.OrderBy(effect => effect.Id, StringComparer.Ordinal)];

    private static List<StatusDefinition> SortedStatuses(ContentCatalog catalog) =>
        [.. catalog.Statuses.Values.OrderBy(status => status.Id, StringComparer.Ordinal)];

    private static List<SkillDefinition> SortedSkills(ContentCatalog catalog) =>
        [.. catalog.Skills.Values.OrderBy(skill => skill.Id, StringComparer.Ordinal)];

    private static List<ClassDefinition> SortedClasses(ContentCatalog catalog) =>
        [.. catalog.Classes.Values.OrderBy(classDefinition => classDefinition.Id, StringComparer.Ordinal)];

    private static List<PartyMemberDefinition> SortedPartyMembers(ContentCatalog catalog) =>
        [.. catalog.PartyMembers.Values.OrderBy(member => member.Id, StringComparer.Ordinal)];

    private static List<ItemDefinition> SortedItems(ContentCatalog catalog) =>
        [.. catalog.Items.Values.OrderBy(item => item.Id, StringComparer.Ordinal)];

    private static List<AffixDefinition> SortedAffixes(ContentCatalog catalog) =>
        [.. catalog.Affixes.Values.OrderBy(affix => affix.Id, StringComparer.Ordinal)];

    // A catalog file's name and its bytes as they will be written.
    private sealed record CatalogWrite(string File, byte[] Bytes);
}
