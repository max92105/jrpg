using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Saving;
using Jrpg.Core.World;
using Jrpg.Data.Content;
using Jrpg.Data.Saving;

namespace Jrpg.Data.Tests.Saving;

/// <summary>
/// Repairs saves against the repository content: sarxian has 208 HP at level 3 in chain mail, and potions stack to 99.
/// </summary>
public sealed class SaveGameRepairTests
{
    private const string Slot = "slot_01";

    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    private static readonly PartyMemberProgress _sarxian = new("sarxian", Level: 3, Experience: 42, Hp: 150, WeaponId: "bronze_sword", ArmorId: "chain_mail", LastingAmounts: new Dictionary<string, int>());
    private static readonly PartyMemberProgress _jazog = new("jazog", Level: 1, Experience: 0, Hp: 100, WeaponId: null, ArmorId: "wizard_robe", LastingAmounts: new Dictionary<string, int>());

    private static readonly SaveGame _save = new()
    {
        SavedAt = new DateTimeOffset(2026, 10, 17, 20, 30, 0, TimeSpan.Zero),
        PlayTime = TimeSpan.FromMinutes(30),
        Location = new MapLocation("start", new Point2D(10, 3), Facing.Down),
        Members = [_sarxian, _jazog],
        ActiveMemberIds = ["sarxian", "jazog"],
        Inventory = [new InventoryEntry("potion", 4)],
        CompletedEvents = [new MapEventKey("start", "0bff6243_0")],
        ObjectSwaps = [new ObjectSwap("start", "0bff6243", "chest_opened")],
    };

    [Fact]
    public void Repair_ValidSave_ReportsNoChanges()
    {
        var repaired = Repair(_save);

        Assert.Empty(repaired.Changes);
        Assert.Equal(_save.Members, repaired.Save.Members);
        Assert.Equal(_save.Inventory, repaired.Save.Inventory);
        Assert.Equal(_save.Location, repaired.Save.Location);
    }

    [Fact]
    public void Repair_UnknownLastingId_DroppedAndReported()
    {
        var sarxian = _sarxian with { LastingAmounts = new Dictionary<string, int> { ["ghost"] = 2 } };

        var repaired = Repair(_save with { Members = [sarxian, _jazog] });

        Assert.Empty(repaired.Save.Members[0].LastingAmounts);
        Assert.Contains("party member 'sarxian': dropped lasting resource 'ghost', which the class no longer has as lasting.", repaired.Changes);
    }

    // The repository has no lasting resource yet (LR3), so Sarxian's class gets Focus (max 3) for this test.
    [Theory]
    [InlineData(7, 3)]
    [InlineData(-2, 0)]
    public void Repair_LastingOutOfRange_Clamped(int saved, int expected)
    {
        var classId = _catalog.PartyMembers["sarxian"].ClassId;
        var focus = new ResourceDefinition("focus", "Focus", Max: 3, StartsAt: 3) { IsLasting = true };
        var classes = new Dictionary<string, ClassDefinition>(_catalog.Classes) { [classId] = _catalog.Classes[classId] with { Resources = [focus] } };
        var sarxian = _sarxian with { LastingAmounts = new Dictionary<string, int> { ["focus"] = saved } };

        var repaired = SaveGameRepair.Repair(Slot, _save with { Members = [sarxian, _jazog] }, _catalog with { Classes = classes });

        Assert.Equal(expected, repaired.Save.Members[0].LastingAmounts["focus"]);
        Assert.Contains($"party member 'sarxian': focus {saved} brought within 0..3.", repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownMember_DropsItFromMembersAndActiveOrder()
    {
        var ghost = _jazog with { Id = "ghost" };

        var repaired = Repair(_save with { Members = [_sarxian, ghost, _jazog], ActiveMemberIds = ["sarxian", "ghost"] });

        Assert.Equal(["sarxian", "jazog"], repaired.Save.Members.Select(member => member.Id));
        Assert.Equal(["sarxian"], repaired.Save.ActiveMemberIds);
        Assert.Contains("dropped party member 'ghost', who no longer exists.", repaired.Changes);
    }

    [Fact]
    public void Repair_NoActiveMemberLeft_ThrowsUnreadable()
    {
        var save = _save with { Members = [_sarxian, _jazog with { Id = "ghost" }], ActiveMemberIds = ["ghost"] };

        var exception = Assert.Throws<UnreadableSaveException>(() => Repair(save));

        Assert.Equal("none of its active party members exist anymore.", exception.Reason);
    }

    [Fact]
    public void Repair_TooManyActiveMembers_MovesExtraToReserve()
    {
        string[] ids = ["sarxian", "jazog", "teelle", "zaruk", "zanyr", "thaali"];
        var members = ids.Select(id => _jazog with { Id = id, ArmorId = null }).ToList();

        var repaired = Repair(_save with { Members = members, ActiveMemberIds = ids });

        Assert.Equal(ids.Take(5), repaired.Save.ActiveMemberIds);
        Assert.Equal(6, repaired.Save.Members.Count);
        Assert.Contains("moved 'thaali' to the reserve; the active party holds 5.", repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownItem_DropsEntry()
    {
        var repaired = Repair(_save with { Inventory = [new InventoryEntry("megalixir", 2), new InventoryEntry("potion", 4)] });

        Assert.Equal([new InventoryEntry("potion", 4)], repaired.Save.Inventory);
        Assert.Single(repaired.Changes);
    }

    [Fact]
    public void Repair_QuantityAboveMaxStack_Clamps()
    {
        var repaired = Repair(_save with { Inventory = [new InventoryEntry("potion", 150)] });

        Assert.Equal([new InventoryEntry("potion", 99)], repaired.Save.Inventory);
        Assert.Contains("kept 99 x 'potion' of 150; the stack holds 99.", repaired.Changes);
    }

    [Fact]
    public void Repair_ZeroQuantityAndDuplicateEntries_DropsAndMerges()
    {
        var repaired = Repair(_save with { Inventory = [new InventoryEntry("bomb", 0), new InventoryEntry("potion", 4), new InventoryEntry("potion", 2)] });

        Assert.Equal([new InventoryEntry("potion", 6)], repaired.Save.Inventory);
    }

    [Fact]
    public void Repair_UnknownWornItem_DropsIt()
    {
        var repaired = Repair(_save with { Members = [_sarxian with { WeaponId = "excalibur" }, _jazog] });

        Assert.Null(repaired.Save.Members[0].WeaponId);
        Assert.Equal(0, repaired.Save.Inventory.Count(entry => entry.ItemId == "excalibur"));
    }

    [Fact]
    public void Repair_WornItemNoLongerWearable_ReturnsItToInventory()
    {
        // A sword isn't armor; a barbarian keeps his wizard robe, since any class wears anything (decision 165).
        var repaired = Repair(_save with { Members = [_sarxian with { ArmorId = "bronze_sword" }, _jazog] });

        Assert.Equal([null, "wizard_robe"], repaired.Save.Members.Select(member => member.ArmorId));
        Assert.Equal([new InventoryEntry("potion", 4), new InventoryEntry("bronze_sword", 1)], repaired.Save.Inventory);
    }

    [Fact]
    public void Repair_ExperienceAtThreshold_ClampsBelowIt()
    {
        var repaired = Repair(_save with { Members = [_sarxian with { Level = 1, Experience = 100 }, _jazog] });

        Assert.Equal((1, 99), (repaired.Save.Members[0].Level, repaired.Save.Members[0].Experience));
    }

    [Fact]
    public void Repair_LevelAboveTable_ClampsToMaximumWithNoExperience()
    {
        var repaired = Repair(_save with { Members = [_sarxian with { Level = 99, Experience = 5 }, _jazog] });

        Assert.Equal((_catalog.Rules.Experience.MaxLevel, 0), (repaired.Save.Members[0].Level, repaired.Save.Members[0].Experience));
    }

    [Fact]
    public void Repair_HpAboveClassMaxAtSavedLevel_ClampsAndReports()
    {
        var repaired = Repair(_save with { Members = [_sarxian with { Hp = 999 }, _jazog] });

        // A warlock in chain mail: 188 HP at level 3, raised 11 % by 8 Constitution and the mail's 3 (ADR 0026).
        Assert.Equal(208, repaired.Save.Members[0].Hp);
        Assert.Equal(["party member 'sarxian': HP 999 brought within 0..208."], repaired.Changes);
    }

    [Fact]
    public void Repair_CompletedEventOnUnknownMap_DropsIt()
    {
        var events = new[] { new MapEventKey("castle", "gate_0"), new MapEventKey("start", "removed_0"), new MapEventKey("start", "0bff6243_0") };

        var repaired = Repair(_save with { CompletedEvents = events });

        Assert.Equal([new MapEventKey("start", "0bff6243_0")], repaired.Save.CompletedEvents);
        Assert.Equal(2, repaired.Changes.Count);
    }

    [Fact]
    public void Repair_SwapToUnknownObject_DropsIt()
    {
        var swaps = new[] { new ObjectSwap("start", "0bff6243", "golden_chest"), new ObjectSwap("start", "missing_uid", "chest_opened") };

        var repaired = Repair(_save with { ObjectSwaps = swaps });

        Assert.Empty(repaired.Save.ObjectSwaps);
        Assert.Equal(2, repaired.Changes.Count);
    }

    [Fact]
    public void Repair_UnknownLocationMap_UsesNewGameStart()
    {
        var repaired = Repair(_save with { Location = new MapLocation("castle", new Point2D(4, 4), Facing.Up) });

        Assert.Equal(_catalog.NewGame.StartLocation, repaired.Save.Location);
        Assert.Contains("map 'castle' no longer exists, so the party starts at the new game's start.", repaired.Changes);
    }

    [Fact]
    public void Repair_DuplicateMember_KeepsTheFirstCopy()
    {
        var repaired = Repair(_save with { Members = [_sarxian, _sarxian with { Level = 9 }, _jazog] });

        Assert.Equal([_sarxian, _jazog], repaired.Save.Members);
        Assert.Contains("dropped a second copy of party member 'sarxian'.", repaired.Changes);
    }

    [Fact]
    public void Repair_ActiveOrderWithRepeatAndUnrecruitedMember_KeepsEachRecruitedMemberOnce()
    {
        var repaired = Repair(_save with { ActiveMemberIds = ["sarxian", "sarxian", "teelle", "jazog"] });

        Assert.Equal(["sarxian", "jazog"], repaired.Save.ActiveMemberIds);
        Assert.Equal(2, repaired.Changes.Count);
    }

    [Fact]
    public void Repair_QuantitiesSummingPastIntMax_ClampsToMaxStack()
    {
        var repaired = Repair(_save with { Inventory = [new InventoryEntry("potion", int.MaxValue), new InventoryEntry("potion", 1)] });

        Assert.Equal([new InventoryEntry("potion", 99)], repaired.Save.Inventory);
    }

    [Fact]
    public void Repair_WornItemThatIsNotEquipment_ReturnsItToInventory()
    {
        var repaired = Repair(_save with { Members = [_sarxian with { WeaponId = "bomb" }, _jazog] });

        Assert.Null(repaired.Save.Members[0].WeaponId);
        Assert.Contains(new InventoryEntry("bomb", 1), repaired.Save.Inventory);
    }

    [Fact]
    public void Repair_SwapOnUnknownMap_DropsItNamingTheMap()
    {
        var repaired = Repair(_save with { ObjectSwaps = [new ObjectSwap("castle", "0bff6243", "chest_opened")] });

        Assert.Empty(repaired.Save.ObjectSwaps);
        Assert.Equal(["forgot that '0bff6243' on map 'castle' became 'chest_opened': the map no longer exists."], repaired.Changes);
    }

    [Fact]
    public void Repair_RemovedEventOnKnownMap_NamesTheEvent()
    {
        var repaired = Repair(_save with { CompletedEvents = [new MapEventKey("start", "removed_0")] });

        Assert.Equal(["forgot done event 'removed_0' on map 'start': the event no longer exists."], repaired.Changes);
    }

    private static RepairedSave Repair(SaveGame save) => SaveGameRepair.Repair(Slot, save, _catalog);
}
