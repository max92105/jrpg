using Jrpg.Core.Loot;

namespace Jrpg.Core.Tests.Loot;

// A drop table compares its entries by content and in order (decision 604): the editor's "nothing changed" check rests
// on it, and the order is the one the roll draws in.
public sealed class DropTableTests
{
    private static readonly DropEntry _potion = new("potion", 30);
    private static readonly DropEntry _scrap = new("scrap", 20) { MaxQuantity = 2 };

    [Fact]
    public void Equals_SameEntriesInNewLists_IsTrue()
    {
        var table = new DropTable("goblin", [_potion, _scrap]);
        var copy = new DropTable("goblin", new List<DropEntry> { _potion with { }, _scrap with { } });

        Assert.Equal(table, copy);
        Assert.Equal(table.GetHashCode(), copy.GetHashCode());
    }

    [Fact]
    public void Equals_EntryChanceDiffers_IsFalse()
    {
        var table = new DropTable("goblin", [_potion, _scrap]);

        Assert.NotEqual(table, new DropTable("goblin", [_potion with { Chance = 31 }, _scrap]));
    }

    [Fact]
    public void Equals_EntriesInOtherOrder_IsFalse()
    {
        var table = new DropTable("goblin", [_potion, _scrap]);

        Assert.NotEqual(table, new DropTable("goblin", [_scrap, _potion]));
    }

    [Fact]
    public void Equals_OtherId_IsFalse()
    {
        Assert.NotEqual(new DropTable("goblin", [_potion]), new DropTable("slime", [_potion]));
    }
}
