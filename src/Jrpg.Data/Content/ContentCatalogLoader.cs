using System.Collections.Frozen;
using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Skills;
using Jrpg.Data.Battle;
using Jrpg.Data.Items;
using Jrpg.Data.Maps;
using Jrpg.Data.Party;
using Jrpg.Data.Skills;

namespace Jrpg.Data.Content;

public static class ContentCatalogLoader
{
    private const string ClassesFile = "classes.json";
    private const string RulesFile = "rules.json";
    internal const string ItemsFile = "items.json";
    private const string SkillsFile = "skills.json";
    private const string PartyMembersFile = "party-members.json";
    private const string EnemiesFile = "enemies.json";
    private const string EncountersFile = "encounters.json";
    internal const string NewGameFile = "new-game.json";

    internal const string TilesheetsFile = "tilesheets.json";
    internal const string TilesFile = "tiles.json";
    internal const string ObjectsFile = "objects.json";

    /// <summary>Loads and validates every content file under <paramref name="contentRoot"/>, referenced files first.</summary>
    /// <exception cref="ContentValidationException">A file is malformed or breaks a content rule.</exception>
    public static ContentCatalog Load(string contentRoot)
    {
        var classes = ReadValidated<IReadOnlyList<ClassDefinition>>(contentRoot, ClassesFile, ClassCatalogValidator.Validate);
        var classIds = classes.Select(classDefinition => classDefinition.Id).ToFrozenSet();
        var rules = ReadValidated<GameRules>(contentRoot, RulesFile, GameRulesValidator.Validate);
        var items = ReadValidated<IReadOnlyList<ItemDefinition>>(
            contentRoot, ItemsFile, (file, list) => ItemCatalogValidator.Validate(file, list, classIds));
        var skills = ReadValidated<IReadOnlyList<SkillDefinition>>(contentRoot, SkillsFile, SkillCatalogValidator.Validate);
        var memberReferences = new PartyMemberReferences(classIds, skills.Select(skill => skill.Id).ToFrozenSet(), rules.Experience);
        var members = ReadValidated<IReadOnlyList<PartyMemberDefinition>>(
            contentRoot, PartyMembersFile, (file, list) => PartyMemberCatalogValidator.Validate(file, list, memberReferences));
        var enemies = ReadValidated<IReadOnlyList<EnemyDefinition>>(contentRoot, EnemiesFile, EnemyCatalogValidator.Validate);
        var enemyIds = enemies.Select(enemy => enemy.Id).ToFrozenSet();
        var encounters = ReadValidated<IReadOnlyList<EncounterDefinition>>(
            contentRoot, EncountersFile, (file, list) => EncounterCatalogValidator.Validate(file, list, enemyIds));
        var tilesheets = ReadValidated<IReadOnlyList<TilesheetDefinition>>(contentRoot, TilesheetsFile, TilesheetCatalogValidator.Validate);
        var sheetIds = tilesheets.Select(sheet => sheet.Id).ToFrozenSet();
        var tiles = ReadValidated<IReadOnlyList<TileDefinition>>(
            contentRoot, TilesFile, (file, list) => TileCatalogValidator.Validate(file, list, sheetIds));
        var tileIds = tiles.Select(tile => tile.Id).ToFrozenSet();
        var objects = ReadValidated<IReadOnlyList<ObjectDefinition>>(
            contentRoot, ObjectsFile, (file, list) => ObjectCatalogValidator.Validate(file, list, tileIds));
        var objectsById = objects.ToFrozenDictionary(definition => definition.Id);
        var mapContent = new MapContentReferences(
            tileIds,
            objectsById,
            items.Select(item => item.Id).ToFrozenSet(),
            encounters.Select(encounter => encounter.Id).ToFrozenSet(),
            members.Select(member => member.Id).ToFrozenSet());
        var maps = MapFolderReader.Read(contentRoot, mapContent);
        var mapsById = maps.ToFrozenDictionary(map => map.Id);
        var newGameReferences = new NewGameReferences(
            members.Select(member => member.Id).ToFrozenSet(), items.ToFrozenDictionary(item => item.Id), mapsById);
        var newGame = ReadValidated<NewGameDefinition>(
            contentRoot, NewGameFile, (file, definition) => NewGameValidator.Validate(file, definition, newGameReferences));

        return new ContentCatalog
        {
            Items = items.ToFrozenDictionary(item => item.Id),
            Classes = classes.ToFrozenDictionary(classDefinition => classDefinition.Id),
            Skills = skills.ToFrozenDictionary(skill => skill.Id),
            PartyMembers = members.ToFrozenDictionary(member => member.Id),
            Enemies = enemies.ToFrozenDictionary(enemy => enemy.Id),
            Encounters = encounters.ToFrozenDictionary(encounter => encounter.Id),
            Tiles = tiles.ToFrozenDictionary(tile => tile.Id),
            Tilesheets = tilesheets.ToFrozenDictionary(sheet => sheet.Id),
            Objects = objectsById,
            Maps = mapsById,
            Rules = rules,
            NewGame = newGame,
        };
    }

    private static T ReadValidated<T>(string contentRoot, string fileName, Action<string, T> validate)
    {
        var content = ContentFileReader.Read<T>(Path.Combine(contentRoot, fileName));
        validate(fileName, content);
        return content;
    }
}
