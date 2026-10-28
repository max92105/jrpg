using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Saving;
using Jrpg.Data.Saving;
using static Jrpg.Data.Tests.Saving.SaveGameFixtures;

namespace Jrpg.Data.Tests.Saving;

/// <summary>
/// Repairs a save's members against the repository content: sarxian has 208 HP at level 3 in chain mail.
/// </summary>
public sealed class SaveGameRepairTests
{
    private const string Slot = "slot_01";

    [Fact]
    public void Repair_ValidSave_ReportsNoChanges()
    {
        var repaired = Repair(RepairableSave);

        Assert.Empty(repaired.Changes);
        Assert.Equal(RepairableSave.Members, repaired.Save.Members);
        Assert.Equal(RepairableSave.Inventory, repaired.Save.Inventory);
        Assert.Equal(RepairableSave.Location, repaired.Save.Location);
    }

    [Fact]
    public void Repair_UnknownLastingId_DroppedAndReported()
    {
        var sarxian = Sarxian with { LastingAmounts = new Dictionary<string, int> { ["ghost"] = 2 } };

        var repaired = Repair(RepairableSave with { Members = [sarxian, Jazog] });

        Assert.Empty(repaired.Save.Members[0].LastingAmounts);
        Assert.Contains("party member 'sarxian': dropped lasting resource 'ghost', which the class no longer has as lasting.", repaired.Changes);
    }

    // The repository has no lasting resource yet (LR3), so Sarxian's class gets Focus (max 3) for this test.
    [Theory]
    [InlineData(7, 3)]
    [InlineData(-2, 0)]
    public void Repair_LastingOutOfRange_Clamped(int saved, int expected)
    {
        var classId = Catalog.PartyMembers["sarxian"].ClassId;
        var focus = new ResourceDefinition("focus", "Focus", Max: 3, StartsAt: 3) { IsLasting = true };
        var classes = new Dictionary<string, ClassDefinition>(Catalog.Classes) { [classId] = Catalog.Classes[classId] with { Resources = [focus] } };
        var sarxian = Sarxian with { LastingAmounts = new Dictionary<string, int> { ["focus"] = saved } };

        var repaired = SaveGameRepair.Repair(Slot, RepairableSave with { Members = [sarxian, Jazog] }, Catalog with { Classes = classes });

        Assert.Equal(expected, repaired.Save.Members[0].LastingAmounts["focus"]);
        Assert.Contains($"party member 'sarxian': focus {saved} brought within 0..3.", repaired.Changes);
    }

    // Sarxian's fighter has 10 Will at level 3 (8 + 2), one point at 10 per point: Focus runs to 4, not its 3.
    [Fact]
    public void Repair_LastingAboveRaisedMax_ClampedToTheRaisedMax()
    {
        var classId = Catalog.PartyMembers["sarxian"].ClassId;
        var focus = new ResourceDefinition("focus", "Focus", Max: 3, StartsAt: 3) { IsLasting = true, WillScaling = new WillScaling(WillPerPoint: 10, WillEffect.RaiseMax) };
        var classes = new Dictionary<string, ClassDefinition>(Catalog.Classes) { [classId] = Catalog.Classes[classId] with { Resources = [focus] } };
        var sarxian = Sarxian with { LastingAmounts = new Dictionary<string, int> { ["focus"] = 7 } };

        var repaired = SaveGameRepair.Repair(Slot, RepairableSave with { Members = [sarxian, Jazog] }, Catalog with { Classes = classes });

        Assert.Equal(4, repaired.Save.Members[0].LastingAmounts["focus"]);
        Assert.Contains("party member 'sarxian': focus 7 brought within 0..4.", repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownMember_DropsItFromMembersAndActiveOrder()
    {
        var ghost = Jazog with { Id = "ghost" };

        var repaired = Repair(RepairableSave with { Members = [Sarxian, ghost, Jazog], ActiveMemberIds = ["sarxian", "ghost"] });

        Assert.Equal(["sarxian", "jazog"], repaired.Save.Members.Select(member => member.Id));
        Assert.Equal(["sarxian"], repaired.Save.ActiveMemberIds);
        Assert.Contains("dropped party member 'ghost', who no longer exists.", repaired.Changes);
    }

    [Fact]
    public void Repair_NoActiveMemberLeft_ThrowsUnreadable()
    {
        var save = RepairableSave with { Members = [Sarxian, Jazog with { Id = "ghost" }], ActiveMemberIds = ["ghost"] };

        var exception = Assert.Throws<UnreadableSaveException>(() => Repair(save));

        Assert.Equal("none of its active party members exist anymore.", exception.Reason);
    }

    [Fact]
    public void Repair_TooManyActiveMembers_MovesExtraToReserve()
    {
        string[] ids = ["sarxian", "jazog", "teelle", "zaruk", "zanyr", "thaali"];
        var members = ids.Select(id => Jazog with { Id = id, Worn = Wearing() }).ToList();

        var repaired = Repair(RepairableSave with { Members = members, ActiveMemberIds = ids });

        Assert.Equal(ids.Take(5), repaired.Save.ActiveMemberIds);
        Assert.Equal(6, repaired.Save.Members.Count);
        Assert.Contains("moved 'thaali' to the reserve; the active party holds 5.", repaired.Changes);
    }

    [Fact]
    public void Repair_UnknownWornItem_DropsIt()
    {
        var repaired = Repair(RepairableSave with { Members = [Sarxian with { Worn = Wearing((EquipmentSlot.Weapon, "excalibur"), (EquipmentSlot.Chest, "chain_mail")) }, Jazog] });

        Assert.False(repaired.Save.Members[0].Worn.ContainsKey(EquipmentSlot.Weapon));
        Assert.Equal(0, repaired.Save.Inventory.Count(entry => entry.ItemId == "excalibur"));
    }

    [Fact]
    public void Repair_WornItemNoLongerWearable_ReturnsItToInventory()
    {
        // A sword isn't armor; a barbarian keeps his wizard robe, since any class wears anything (decision 165).
        var repaired = Repair(RepairableSave with { Members = [Sarxian with { Worn = Wearing((EquipmentSlot.Weapon, "bronze_sword"), (EquipmentSlot.Chest, "bronze_sword")) }, Jazog] });

        Assert.Equal([null, "wizard_robe"], repaired.Save.Members.Select(member => member.Worn.GetValueOrDefault(EquipmentSlot.Chest)?.ItemId));
        Assert.Equal([Plain("bronze_sword")], repaired.Save.Gear);
    }

    [Fact]
    public void Repair_ExperienceAtThreshold_ClampsBelowIt()
    {
        var repaired = Repair(RepairableSave with { Members = [Sarxian with { Level = 1, Experience = 100 }, Jazog] });

        Assert.Equal((1, 99), (repaired.Save.Members[0].Level, repaired.Save.Members[0].Experience));
    }

    [Fact]
    public void Repair_LevelAboveTable_ClampsToMaximumWithNoExperience()
    {
        var repaired = Repair(RepairableSave with { Members = [Sarxian with { Level = 99, Experience = 5 }, Jazog] });

        Assert.Equal((Catalog.Rules.Experience.MaxLevel, 0), (repaired.Save.Members[0].Level, repaired.Save.Members[0].Experience));
    }

    [Fact]
    public void Repair_HpAboveClassMaxAtSavedLevel_ClampsAndReports()
    {
        var repaired = Repair(RepairableSave with { Members = [Sarxian with { Hp = 999 }, Jazog] });

        // A warlock in chain mail: 188 HP at level 3, raised 11 % by 8 Constitution and the mail's 3 (ADR 0026).
        Assert.Equal(208, repaired.Save.Members[0].Hp);
        Assert.Equal(["party member 'sarxian': HP 999 brought within 0..208."], repaired.Changes);
    }

    [Fact]
    public void Repair_DuplicateMember_KeepsTheFirstCopy()
    {
        var repaired = Repair(RepairableSave with { Members = [Sarxian, Sarxian with { Level = 9 }, Jazog] });

        Assert.Equal([Sarxian, Jazog], repaired.Save.Members);
        Assert.Contains("dropped a second copy of party member 'sarxian'.", repaired.Changes);
    }

    [Fact]
    public void Repair_ActiveOrderWithRepeatAndUnrecruitedMember_KeepsEachRecruitedMemberOnce()
    {
        var repaired = Repair(RepairableSave with { ActiveMemberIds = ["sarxian", "sarxian", "teelle", "jazog"] });

        Assert.Equal(["sarxian", "jazog"], repaired.Save.ActiveMemberIds);
        Assert.Equal(2, repaired.Changes.Count);
    }

    [Fact]
    public void Repair_WornItemThatIsNotEquipment_ReturnsItToInventory()
    {
        var repaired = Repair(RepairableSave with { Members = [Sarxian with { Worn = Wearing((EquipmentSlot.Weapon, "bomb"), (EquipmentSlot.Chest, "chain_mail")) }, Jazog] });

        Assert.False(repaired.Save.Members[0].Worn.ContainsKey(EquipmentSlot.Weapon));
        Assert.Contains(new InventoryEntry("bomb", 1), repaired.Save.Inventory);
    }

    private static RepairedSave Repair(SaveGame save) => SaveGameRepair.Repair(Slot, save, Catalog);
}
