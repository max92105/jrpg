using Jrpg.Core.Items;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Items;

public sealed class InventoryTests
{
    private readonly Inventory _inventory = new();

    [Fact]
    public void Add_NewItems_KeepsPickupOrder()
    {
        _inventory.Add(new ItemDefinitionBuilder().WithId("potion").Build(), 2);

        _inventory.Add(new ItemDefinitionBuilder().WithId("bomb").Build(), 1);

        Assert.Equal([new InventoryEntry("potion", 2), new InventoryEntry("bomb", 1)], _inventory.Entries);
    }

    [Fact]
    public void Add_ExistingItem_IncreasesQuantity()
    {
        var potion = new ItemDefinitionBuilder().Build();
        _inventory.Add(potion, 2);

        var leftover = _inventory.Add(potion, 3);

        Assert.Equal(5, _inventory.QuantityOf(potion.Id));
        Assert.Equal(0, leftover);
    }

    [Fact]
    public void Add_BeyondMaxStack_FillsToMaxAndReturnsLeftover()
    {
        var potion = new ItemDefinitionBuilder().WithMaxStack(99).Build();
        _inventory.Add(potion, 95);

        var leftover = _inventory.Add(potion, 10);

        Assert.Equal(99, _inventory.QuantityOf(potion.Id));
        Assert.Equal(6, leftover);
    }

    [Fact]
    public void Add_StackAlreadyFull_ReturnsEverythingAsLeftover()
    {
        var potion = new ItemDefinitionBuilder().WithMaxStack(3).Build();
        _inventory.Add(potion, 3);

        var leftover = _inventory.Add(potion, 2);

        Assert.Equal(3, _inventory.QuantityOf(potion.Id));
        Assert.Equal(2, leftover);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Add_NonPositiveQuantity_Throws(int quantity)
    {
        var potion = new ItemDefinitionBuilder().Build();

        Assert.Throws<ArgumentOutOfRangeException>(() => _inventory.Add(potion, quantity));
    }

    [Fact]
    public void TryRemove_SomeCopies_ReturnsTrueAndDecreasesQuantity()
    {
        var potion = new ItemDefinitionBuilder().Build();
        _inventory.Add(potion, 3);

        var removed = _inventory.TryRemove(potion.Id, 2);

        Assert.True(removed);
        Assert.Equal(1, _inventory.QuantityOf(potion.Id));
    }

    [Fact]
    public void TryRemove_LastCopy_RemovesEntry()
    {
        var potion = new ItemDefinitionBuilder().Build();
        _inventory.Add(potion, 1);

        var removed = _inventory.TryRemove(potion.Id, 1);

        Assert.True(removed);
        Assert.Empty(_inventory.Entries);
    }

    [Fact]
    public void TryRemove_MoreThanOwned_ReturnsFalseAndKeepsQuantity()
    {
        var potion = new ItemDefinitionBuilder().Build();
        _inventory.Add(potion, 2);

        var removed = _inventory.TryRemove(potion.Id, 3);

        Assert.False(removed);
        Assert.Equal(2, _inventory.QuantityOf(potion.Id));
    }

    [Fact]
    public void Add_AfterLastCopyRemoved_GoesToTheEnd()
    {
        var potion = new ItemDefinitionBuilder().WithId("potion").Build();
        _inventory.Add(potion, 1);
        _inventory.Add(new ItemDefinitionBuilder().WithId("bomb").Build(), 1);
        _inventory.TryRemove("potion", 1);

        _inventory.Add(potion, 1);

        Assert.Equal([new InventoryEntry("bomb", 1), new InventoryEntry("potion", 1)], _inventory.Entries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void TryRemove_NonPositiveQuantity_Throws(int quantity)
    {
        _inventory.Add(new ItemDefinitionBuilder().Build(), 1);

        Assert.Throws<ArgumentOutOfRangeException>(() => _inventory.TryRemove("potion", quantity));
    }

    [Fact]
    public void TryRemove_ItemNotOwned_ReturnsFalse()
    {
        var removed = _inventory.TryRemove("potion", 1);

        Assert.False(removed);
    }

    [Fact]
    public void HasRoomFor_StackBelowMax_ReturnsTrue()
    {
        var potion = new ItemDefinitionBuilder().WithMaxStack(3).Build();
        _inventory.Add(potion, 2);

        var hasRoom = _inventory.HasRoomFor(potion);

        Assert.True(hasRoom);
    }

    [Fact]
    public void HasRoomFor_StackAtMax_ReturnsFalse()
    {
        var potion = new ItemDefinitionBuilder().WithMaxStack(3).Build();
        _inventory.Add(potion, 3);

        var hasRoom = _inventory.HasRoomFor(potion);

        Assert.False(hasRoom);
    }

    [Fact]
    public void QuantityOf_ItemNotOwned_ReturnsZero()
    {
        var quantity = _inventory.QuantityOf("potion");

        Assert.Equal(0, quantity);
    }
}
