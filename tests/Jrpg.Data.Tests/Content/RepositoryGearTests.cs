using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// The gear for the new slots (G2, decision 332): a piece of every kind in content, each bonus big enough to move a
// shown number, and one of each in the new game's bag.
public sealed class RepositoryGearTests
{
    // A shown percent is a whole number (ADR 0026), so a bonus under this could change nothing the player sees.
    private const int LeastBonus = 5;

    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "content"));

    [Fact]
    public void Load_RepositoryItems_HaveAPieceOfEveryKind()
    {
        var kinds = _catalog.Items.Values.Select(item => item.Equipment?.Kind).OfType<EquipmentKind>().ToHashSet();

        Assert.All(Enum.GetValues<EquipmentKind>(), kind => Assert.Contains(kind, kinds));
    }

    // The weapons and chest pieces keep their earlier, smaller bonuses (P4); the new kinds follow the rule.
    [Fact]
    public void Load_RepositoryGearOfTheNewKinds_EveryBonusAtLeastFive()
    {
        var newKinds = _catalog.Items.Values
            .Where(item => item.Equipment is { Kind: not (EquipmentKind.Weapon or EquipmentKind.Chest) })
            .ToList();

        Assert.NotEmpty(newKinds);
        Assert.All(newKinds, item => Assert.All(item.Equipment!.Bonuses.Values, bonus => Assert.True(Math.Abs(bonus.Value) >= LeastBonus, $"{item.Id}: {bonus.Key} {bonus.Value}")));
    }

    [Fact]
    public void Load_RepositoryNewGame_StartsWithAPowerRing()
    {
        Assert.Contains(_catalog.NewGame.StartingItems, entry => entry.ItemId == "power_ring" && entry.Quantity >= 1);
    }
}
