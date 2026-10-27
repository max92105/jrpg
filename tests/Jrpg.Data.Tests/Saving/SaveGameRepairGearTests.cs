using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Items;
using Jrpg.Core.Maps;
using Jrpg.Core.Party;
using Jrpg.Core.Saving;
using Jrpg.Data.Content;
using Jrpg.Data.Saving;

namespace Jrpg.Data.Tests.Saving;

/// <summary>
/// Repairs HP against the max a member has with what they wear, on the repository content: sarxian, a level 3 warlock,
/// has 188 HP in nothing (203 with 8 Constitution) and 208 in chain mail (its +3 Constitution).
/// </summary>
public sealed class SaveGameRepairGearTests
{
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Repair_HpWithinMaxWithGear_KeepsHp()
    {
        var repaired = SaveGameRepair.Repair("slot_01", SaveWithSarxian(hp: 206, armorId: "chain_mail"), _catalog);

        Assert.Equal(206, repaired.Save.Members[0].Hp);
        Assert.Empty(repaired.Changes);
    }

    [Fact]
    public void Repair_SameHpWithoutTheGear_ClampsToTheMaxWithoutIt()
    {
        var repaired = SaveGameRepair.Repair("slot_01", SaveWithSarxian(hp: 206, armorId: null), _catalog);

        Assert.Equal(203, repaired.Save.Members[0].Hp);
    }

    private static SaveGame SaveWithSarxian(int hp, string? armorId) => new()
    {
        SavedAt = new DateTimeOffset(2026, 10, 27, 12, 0, 0, TimeSpan.Zero),
        PlayTime = TimeSpan.FromMinutes(30),
        Location = new MapLocation("start", new Point2D(10, 3), Facing.Down),
        Members = [new PartyMemberProgress("sarxian", Level: 3, Experience: 0, hp, armorId is null ? new Dictionary<EquipmentSlot, string>() : new Dictionary<EquipmentSlot, string> { [EquipmentSlot.Chest] = armorId }, LastingAmounts: new Dictionary<string, int>())],
        ActiveMemberIds = ["sarxian"],
        Inventory = [],
        CompletedEvents = [],
        ObjectSwaps = [],
    };
}
