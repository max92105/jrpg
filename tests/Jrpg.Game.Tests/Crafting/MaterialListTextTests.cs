using Jrpg.Core.Items;
using Jrpg.Game.Crafting;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Crafting;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class MaterialListTextTests
{
    [Fact]
    public void Of_TwoMaterials_NamesEachWithItsCountInOrder()
    {
        var items = StartMapScenarios.BelowClosedDoor.Items;

        var text = MaterialListText.Of([new InventoryEntry("scrap", 2), new InventoryEntry("essence", 1)], items, RepositoryScreenText.Text.Messages);

        Assert.Equal($"2 {items["scrap"].Name}, 1 {items["essence"].Name}", text);
    }

    [Fact]
    public void Of_NoMaterials_IsEmpty()
    {
        var text = MaterialListText.Of([], StartMapScenarios.BelowClosedDoor.Items, RepositoryScreenText.Text.Messages);

        Assert.Equal(string.Empty, text);
    }
}
