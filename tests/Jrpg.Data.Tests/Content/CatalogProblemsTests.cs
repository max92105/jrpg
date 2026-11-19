using Jrpg.Core.Battle;
using Jrpg.Core.Cinematics;
using Jrpg.Core.Content;
using Jrpg.Core.Crafting;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Core.Trade;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

/// <summary>
/// The loader's own message for an edited catalog, for the editor's problem lines; null when it would load.
/// </summary>
public sealed class CatalogProblemsTests
{
    private static readonly string _testWorld = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_testWorld);

    [Fact]
    public void OfFieldSprites_TestWorldSheets_IsNull() =>
        Assert.Null(CatalogProblems.OfFieldSprites([.. _catalog.FieldSprites.Values], _catalog));

    [Fact]
    public void OfShops_TestWorldShops_IsNull() => Assert.Null(CatalogProblems.OfShops([.. _catalog.Shops.Values], _catalog));

    [Fact]
    public void OfShops_EmptyName_NamesShopAndField()
    {
        var shops = WithGeneralStore(store => store with { Name = " " });

        Assert.Equal("shops.json: shop 'general_store': name must not be empty.", CatalogProblems.OfShops(shops, _catalog));
    }

    [Fact]
    public void OfShops_UnknownItem_NamesIt()
    {
        var shops = WithGeneralStore(store => store with { ItemIds = ["elixir_of_nothing"] });

        var problem = CatalogProblems.OfShops(shops, _catalog);

        Assert.Equal("shops.json: shop 'general_store': itemIds[0] has unknown item id 'elixir_of_nothing'.", problem);
    }

    [Fact]
    public void OfEnemies_TestWorldEnemies_IsNull() =>
        Assert.Null(CatalogProblems.OfEnemies([.. _catalog.Enemies.Values], _catalog));

    [Fact]
    public void OfEnemies_AccuracyTooHigh_NamesEnemyAndField()
    {
        List<EnemyDefinition> enemies =
            [.. _catalog.Enemies.Values.Select(enemy => enemy.Id == "gnoll" ? enemy with { Accuracy = 250 } : enemy)];

        var problem = CatalogProblems.OfEnemies(enemies, _catalog);

        Assert.Equal("enemies.json: enemy 'gnoll': accuracy must be within 0..200 (was 250).", problem);
    }

    [Fact]
    public void OfDropTables_TestWorldTables_IsNull() =>
        Assert.Null(CatalogProblems.OfDropTables([.. _catalog.DropTables.Values], _catalog));

    [Fact]
    public void OfDropTables_ChanceZero_NamesTableAndEntry()
    {
        var problem = CatalogProblems.OfDropTables([GoblinTableWithFirstChance(0)], _catalog);

        Assert.Equal("drop-tables.json: drop table 'goblin': entries[0]: chance must be within 1..100 (was 0).", problem);
    }

    // The accepted bound: a certain drop loads.
    [Fact]
    public void OfDropTables_ChanceHundred_IsNull() =>
        Assert.Null(CatalogProblems.OfDropTables([GoblinTableWithFirstChance(100)], _catalog));

    [Fact]
    public void OfFieldSprites_ScaleZero_NamesTheFileAndTheField()
    {
        var sheets = _catalog.FieldSprites.Values
            .Select(sheet => sheet.File == "player.png" ? sheet with { Scale = 0 } : sheet)
            .ToList();

        var problem = CatalogProblems.OfFieldSprites(sheets, _catalog);

        Assert.Equal("field-sprites.json: field sprite 'player.png': scale must be within 1..8 (was 0).", problem);
    }

    [Fact]
    public void OfFieldSprites_ObjectNamingADroppedSheet_NamesTheObject()
    {
        var withoutPlaceholder = _catalog.FieldSprites.Values.Where(sheet => sheet.File != "npc_placeholder.png").ToList();

        var problem = CatalogProblems.OfFieldSprites(withoutPlaceholder, _catalog);

        Assert.Equal(
            "field-sprites.json: object 'villager' names field sprite 'npc_placeholder.png', which has no record.", problem);
    }

    [Fact]
    public void OfFieldSprites_ObjectWithoutSpriteDroppedSheet_IsNull()
    {
        var tileObjects = _catalog.Objects.Values
            .Where(definition => definition.FieldSpriteFile is null)
            .ToDictionary(definition => definition.Id);
        var content = _catalog with { Objects = tileObjects, NewGame = _catalog.NewGame with { FieldSpriteFile = "player.png" } };

        Assert.Null(CatalogProblems.OfFieldSprites([_catalog.FieldSprites["player.png"]], content));
    }

    [Fact]
    public void OfEffectStrips_FileListedTwice_NamesIt()
    {
        var burst = new EffectStripDefinition { File = "Burst.png", FrameMilliseconds = 60 };

        var problem = CatalogProblems.OfEffectStrips([burst, burst with { HitFrame = 1 }]);

        Assert.Equal("effect-strips.json: effect strip id 'Burst.png' is used more than once.", problem);
    }

    [Fact]
    public void OfEffectStrips_ValidList_IsNone() =>
        Assert.Null(CatalogProblems.OfEffectStrips([new EffectStripDefinition { File = "Burst.png", FrameMilliseconds = 60 }]));

    [Fact]
    public void OfCinematics_Valid_IsNull() => Assert.Null(CatalogProblems.OfCinematics([IntroZoomingTo(120)]));

    [Fact]
    public void OfCinematics_ZoomAt150_IsNull() => Assert.Null(CatalogProblems.OfCinematics([IntroZoomingTo(150)]));

    [Fact]
    public void OfCinematics_ZoomPast150_NamesCinematicAndStill()
    {
        var problem = CatalogProblems.OfCinematics([IntroZoomingTo(151)]);

        Assert.Equal(
            "cinematics.json: cinematic 'intro': stills[0]: zoom endPercent must be within 100..150 (was 151).", problem);
    }

    [Fact]
    public void OfItems_ValidItems_IsNull() => Assert.Null(CatalogProblems.OfItems([.. _catalog.Items.Values], _catalog));

    [Fact]
    public void OfItems_MaterialStackBelowTheForgesCost_NamesRulesJson()
    {
        var problem = OfItemsWithAForgeOfOre(rerollQuantity: 3);

        Assert.Equal("rules.json: crafting.rerollCost[0]: quantity must be within 1..2 (was 3).", problem);
    }

    // The accepted bound: a cost of a whole stack loads.
    [Fact]
    public void OfItems_MaterialStackAtTheForgesCost_IsNull() => Assert.Null(OfItemsWithAForgeOfOre(rerollQuantity: 2));

    [Fact]
    public void OfAffixes_ValidPool_IsNull() =>
        Assert.Null(CatalogProblems.OfAffixes([.. _catalog.Affixes.Values], _catalog));

    [Fact]
    public void OfAffixes_RangeAcrossZero_IsDatasMessage()
    {
        var affixes = WithAgility(agility => agility with { Min = -2, Max = 5 });

        var problem = CatalogProblems.OfAffixes(affixes, _catalog);

        Assert.Equal(
            "affixes.json: affix 'agility': min and max must be on the same side of 0, or a roll could add nothing "
                + "(was -2 and 5).",
            problem);
    }

    // A unique's value is free of its affix's range but not of its side of zero (decision 866).
    [Fact]
    public void OfAffixes_RangeMovedBelowZeroUnderAUnique_IsTheItemsMessage()
    {
        var affixes = WithAgility(agility => agility with { Min = -5, Max = -2 });

        var problem = CatalogProblems.OfAffixes(affixes, WithAgileCharm());

        Assert.Equal(
            "items.json: item 'test_charm': fixedAffixes[0] (agility): value must be below 0, as affix 'agility' rolls (was 4).",
            problem);
    }

    [Fact]
    public void OfAffixes_RangeNarrowedUnderAUniquesValue_IsNull()
    {
        var affixes = WithAgility(agility => agility with { Min = 1, Max = 2 });

        Assert.Null(CatalogProblems.OfAffixes(affixes, WithAgileCharm()));
    }

    private static List<AffixDefinition> WithAgility(Func<AffixDefinition, AffixDefinition> change) =>
        [.. _catalog.Affixes.Values.Select(affix => affix.Id == "agility" ? change(affix) : affix)];

    // The test world with a unique of the test's own, agility fixed at 4: no test world unique names agility.
    private static ContentCatalog WithAgileCharm()
    {
        var charm = new ItemDefinition
        {
            Id = "test_charm",
            Name = "Test Charm",
            Description = "",
            IconFile = "Armor.png",
            MaxStack = 99,
            IsUsableFromMenu = false,
            Equipment = new EquipmentStats(EquipmentKind.Amulet) { Bonuses = StatBonuses.None },
            IsUnique = true,
            FixedAffixes = [new RolledAffix("agility", StatKind.Agility, 4)],
        };
        var items = _catalog.Items.Values.Append(charm).ToDictionary(item => item.Id, StringComparer.Ordinal);
        return _catalog with { Items = items };
    }

    // The test world's items and a material of the test's own, stacking to 2, that every Forge list names.
    private static string? OfItemsWithAForgeOfOre(int rerollQuantity)
    {
        var ore = new ItemDefinition
        {
            Id = "ore",
            Name = "Ore",
            Description = "",
            IconFile = "Ore.png",
            MaxStack = 2,
            IsUsableFromMenu = false,
            IsMaterial = true,
        };
        IReadOnlyList<InventoryEntry> one = [new InventoryEntry("ore", 1)];
        var crafting = new CraftingRules
        {
            Salvage = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>>
            {
                [Rarity.Common] = one,
                [Rarity.Magic] = one,
                [Rarity.Rare] = one,
            },
            UpgradeCosts = new Dictionary<Rarity, IReadOnlyList<InventoryEntry>> { [Rarity.Magic] = one, [Rarity.Rare] = one },
            RerollCost = [new InventoryEntry("ore", rerollQuantity)],
        };
        var content = _catalog with { Rules = _catalog.Rules with { Crafting = crafting } };

        return CatalogProblems.OfItems([.. _catalog.Items.Values, ore], content);
    }

    private static CinematicDefinition IntroZoomingTo(int endPercent) => new()
    {
        Id = "intro",
        Stills =
        [
            new CinematicStill
            {
                ImageFile = "intro/1.png",
                Seconds = 4,
                Zoom = new StillZoom { StartPercent = 100, EndPercent = endPercent },
            },
        ],
    };

    private static List<ShopDefinition> WithGeneralStore(Func<ShopDefinition, ShopDefinition> change) =>
        [.. _catalog.Shops.Values.Select(shop => shop.Id == "general_store" ? change(shop) : shop)];

    private static DropTable GoblinTableWithFirstChance(int chance)
    {
        var goblin = _catalog.DropTables["goblin"];
        return goblin with { Entries = [goblin.Entries[0] with { Chance = chance }, .. goblin.Entries.Skip(1)] };
    }
}
