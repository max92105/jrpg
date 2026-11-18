using Jrpg.Editor.Balance;

namespace Jrpg.Editor.Tests.Balance;

// The key the page's hosts read back and choose by (IEditedCatalog): a table has no records, so never one.
public sealed class BalanceTableViewModelChoiceTests
{
    [Fact]
    public void ChosenKey_NewTable_IsNull()
    {
        Assert.Null(new BalanceTableViewModel(BalanceFixtures.World).ChosenKey);
    }

    [Fact]
    public void Choose_AnyKey_KeepsNothingChosen()
    {
        var table = new BalanceTableViewModel(BalanceFixtures.World);

        table.Choose(BalanceFixtures.Fireball.Id);

        Assert.Null(table.ChosenKey);
    }
}
