using Jrpg.Game.Menus;

namespace Jrpg.Game.Tests.Menus;

public sealed class CategoryRailTests
{
    [Fact]
    public void MoveDown_FromArmor_ReachesMaterials()
    {
        var rail = new CategoryRail();
        rail.MoveUp();
        rail.MoveUp();
        Assert.Equal(InventoryCategory.Armor, rail.Category);

        rail.MoveDown();

        Assert.Equal(InventoryCategory.Materials, rail.Category);
    }

    [Fact]
    public void MoveDown_FromMaterials_WrapsToAll()
    {
        var rail = new CategoryRail();
        rail.MoveUp();
        Assert.Equal(InventoryCategory.Materials, rail.Category);

        rail.MoveDown();

        Assert.Equal(InventoryCategory.All, rail.Category);
    }

    [Fact]
    public void Reset_AfterMoving_IsAllWithTheCursorOnTheList()
    {
        var rail = new CategoryRail();
        rail.MoveDown();
        rail.TakeCursor();

        rail.Reset();

        Assert.Equal(InventoryCategory.All, rail.Category);
        Assert.False(rail.HasCursor);
    }
}
