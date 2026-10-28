using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Loot;

// A scripted random with no values throws on any draw, so a test without values proves nothing was drawn. A chance
// draws one value below 1000: a draw below the chance in tenths of a percent drops.
public sealed class DropRollTests
{
    private static readonly ItemDefinition _potion = new ItemDefinitionBuilder().WithId("potion").Build();
    private static readonly ItemDefinition _sword = new ItemDefinitionBuilder().WithId("bronze_sword").AsWeapon(24).Build();
    private static readonly Dictionary<string, ItemDefinition> _items = new() { ["potion"] = _potion, ["bronze_sword"] = _sword };

    [Fact]
    public void Roll_CertainEntry_DropsWithoutDrawing()
    {
        var drops = DropRoll.Roll([Table(new DropEntry("potion", 100))], _items, new FixedSequenceRandom());

        Assert.Equal([new ItemDrop(_potion, 1)], drops.Stacks);
    }

    [Fact]
    public void Roll_DrawBelowChance_Drops()
    {
        var drops = DropRoll.Roll([Table(new DropEntry("potion", 30))], _items, new FixedSequenceRandom(299));

        Assert.Equal([new ItemDrop(_potion, 1)], drops.Stacks);
    }

    [Fact]
    public void Roll_DrawAtChance_DropsNothing()
    {
        var drops = DropRoll.Roll([Table(new DropEntry("potion", 30))], _items, new FixedSequenceRandom(300));

        Assert.True(drops.IsEmpty);
    }

    // 2..4 copies, both ends included: the draw picks one of three quantities, and the highest draw gives 2 + 2.
    [Fact]
    public void Roll_QuantityRange_DrawsWithinIt()
    {
        var entry = new DropEntry("potion", 100) { MinQuantity = 2, MaxQuantity = 4 };

        var drops = DropRoll.Roll([Table(entry)], _items, new FixedSequenceRandom(2));

        Assert.Equal([new ItemDrop(_potion, 4)], drops.Stacks);
    }

    [Fact]
    public void Roll_SameItemFromTwoEnemies_MergesStack()
    {
        var table = Table(new DropEntry("potion", 100));

        var drops = DropRoll.Roll([table, table], _items, new FixedSequenceRandom());

        Assert.Equal([new ItemDrop(_potion, 2)], drops.Stacks);
    }

    [Fact]
    public void Roll_Equipment_DropsPlainPiece()
    {
        var drops = DropRoll.Roll([Table(new DropEntry("bronze_sword", 100) { MinQuantity = 2, MaxQuantity = 2 })], _items, new FixedSequenceRandom());

        Assert.Equal([GearPiece.Plain(_sword), GearPiece.Plain(_sword)], drops.Gear);
        Assert.Empty(drops.Stacks);
    }

    private static DropTable Table(params DropEntry[] entries) => new("table", entries);
}
