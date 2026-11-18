using Jrpg.Core.Items;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.NewGame;

namespace Jrpg.Editor.Tests.NewGame;

// One starting item as typed (decision 968): its quantity read when it is a whole number, named when it isn't.
public sealed class StartingItemRowTests
{
    private readonly StartingItemRow _row = new(new RecordChoice("tonic", "Tonic"), "5");

    [Fact]
    public void ToEntry_TypedQuantity_ReadsIt()
    {
        _row.QuantityText = " 7 ";

        Assert.Equal(new InventoryEntry("tonic", 7), _row.ToEntry());
    }

    [Fact]
    public void Problem_NotANumber_NamesTheItem()
    {
        _row.QuantityText = "lots";

        Assert.Equal("Quantity of Tonic must be a whole number (was 'lots').", _row.Problem);
    }

    [Fact]
    public void ToEntry_WithAProblem_Throws()
    {
        _row.QuantityText = "lots";

        Assert.Throws<InvalidOperationException>(_row.ToEntry);
    }
}
