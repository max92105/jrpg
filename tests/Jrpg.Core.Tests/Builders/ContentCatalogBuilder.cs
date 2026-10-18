using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Skills;

namespace Jrpg.Core.Tests.Builders;

/// <summary>A catalog with empty defaults, so tests only state the content they rely on.</summary>
internal sealed class ContentCatalogBuilder
{
    private ItemDefinition[] _items = [];
    private SkillDefinition[] _skills = [];
    private PartyMemberDefinition[] _members = [];
    private NewGameDefinition _newGame = new() { RecruitedMemberIds = [], ActiveMemberIds = [], StartingItems = [] };
    private TileDefinition[] _tiles = [];
    private ObjectDefinition[] _objects = [];
    private MapLayout[] _maps = [];

    public ContentCatalogBuilder WithItems(params ItemDefinition[] items)
    {
        _items = items;
        return this;
    }

    public ContentCatalogBuilder WithSkills(params SkillDefinition[] skills)
    {
        _skills = skills;
        return this;
    }

    public ContentCatalogBuilder WithMembers(params PartyMemberDefinition[] members)
    {
        _members = members;
        return this;
    }

    public ContentCatalogBuilder WithTiles(params TileDefinition[] tiles)
    {
        _tiles = tiles;
        return this;
    }

    public ContentCatalogBuilder WithObjects(params ObjectDefinition[] objects)
    {
        _objects = objects;
        return this;
    }

    /// <summary>Each layout gets settings with its id as display name.</summary>
    public ContentCatalogBuilder WithMaps(params MapLayout[] maps)
    {
        _maps = maps;
        return this;
    }

    public ContentCatalogBuilder WithNewGame(NewGameDefinition newGame)
    {
        _newGame = newGame;
        return this;
    }

    public ContentCatalog Build() => new()
    {
        Items = _items.ToDictionary(item => item.Id),
        Classes = new Dictionary<string, ClassDefinition>(),
        Skills = _skills.ToDictionary(skill => skill.Id),
        PartyMembers = _members.ToDictionary(member => member.Id),
        Enemies = new Dictionary<string, EnemyDefinition>(),
        Encounters = new Dictionary<string, EncounterDefinition>(),
        Tiles = _tiles.ToDictionary(tile => tile.Id),
        Objects = _objects.ToDictionary(definition => definition.Id),
        Maps = _maps.ToDictionary(layout => layout.Id, layout => new MapDefinition(layout, new MapSettings { Id = layout.Id, DisplayName = layout.Id })),
        Rules = new GameRules(new PartyRules(MaxActiveMembers: 4), new ExperienceTable([100]), new ExplorationRules { WalkSpeed = 3, PlayerRadius = 0.1, InteractReach = 0.6 }),
        NewGame = _newGame,
    };
}
