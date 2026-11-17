using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Stats;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// A unique's fixed affix rows as typed (decision 882): in the record's order, added after the chosen row with the first
// pool affix no row names at its range's min, removed and moved, their stats from the pool, their own problems named by
// affix, the piece's price through Core's formula, relayed as typed changes.
public sealed class UniqueAffixesViewModelTests
{
    private const int SellPercent = 50;

    // Three affixes, so the warlord axe's two leave fire_ward to add; its min and gold differ from the other two's.
    private static readonly Dictionary<string, AffixDefinition> _pool = new[]
    {
        Affix("power", StatKind.Power, 2, 5),
        Affix("crit_chance", StatKind.CritChance, 1, 3),
        Affix("fire_ward", StatKind.FireResistance, 4, 10) with { GoldValue = 35 },
    }.ToDictionary(affix => affix.Id);

    // Each change the form relays: the pool's last affix is free, so picking it and adding a row both change the rows.
    private static readonly Dictionary<string, Action<UniqueAffixesViewModel>> _rowEdits = new()
    {
        ["affix picked"] = unique => unique.Rows[0].Affix = unique.AffixChoices.Single(choice => choice.Id == "fire_ward"),
        ["value typed"] = unique => unique.Rows[0].ValueText = "4",
        ["row added"] = unique => unique.AddRowCommand.Execute(null),
        ["row removed"] = unique => unique.RemoveRowCommand.Execute(null),
    };

    public static TheoryData<string> RowEdits => [.. _rowEdits.Keys];

    // The record lists power before crit_chance, against the id order the affix box offers them in.
    [Fact]
    public void From_Unique_ShowsItsFixedAffixesInOrder()
    {
        var unique = WarlordAxeRows();

        var rows = unique.Rows.Select(row => (row.Affix.Id, row.ValueText, row.RollLine));
        Assert.Equal([("power", "6", "rolls 2..5"), ("crit_chance", "3", "rolls 1..3")], rows);
    }

    [Fact]
    public void AffixChoices_Pool_ListsEveryAffixById()
    {
        var unique = WarlordAxeRows();

        string[] expected = ["crit_chance: CritChance", "fire_ward: FireResistance", "power: Power"];
        Assert.Equal(expected, unique.AffixChoices.Select(choice => choice.Label));
    }

    [Fact]
    public void AddRow_AfterTheChosenRow_StartsAtTheAffixsMin()
    {
        var unique = WarlordAxeRows();
        unique.SelectedRow = unique.Rows[0];

        unique.AddRowCommand.Execute(null);

        Assert.Same(unique.Rows[1], unique.SelectedRow);
        var added = unique.SelectedRow;
        Assert.Equal(("fire_ward", "4", "rolls 4..10"), (added.Affix.Id, added.ValueText, added.RollLine));
    }

    [Fact]
    public void AddRow_WholePoolListed_CannotExecute()
    {
        var unique = WarlordAxeRows();

        unique.AddRowCommand.Execute(null);

        Assert.False(unique.AddRowCommand.CanExecute(null));
    }

    [Fact]
    public void AddRow_RowRemovedFromAFullList_RaisesCanExecuteChanged()
    {
        var unique = WarlordAxeRows();
        unique.AddRowCommand.Execute(null);
        var raised = 0;
        unique.AddRowCommand.CanExecuteChanged += (_, _) => raised++;

        unique.RemoveRowCommand.Execute(null);

        Assert.Equal(1, raised);
        Assert.True(unique.AddRowCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveRow_ChosenRow_RemovesIt()
    {
        var unique = WarlordAxeRows();
        unique.SelectedRow = unique.Rows[0];

        unique.RemoveRowCommand.Execute(null);

        Assert.Equal(["crit_chance"], unique.Rows.Select(row => row.Affix.Id));
    }

    [Fact]
    public void RemoveRow_NoRowChosen_CannotExecute()
    {
        var unique = WarlordAxeRows();

        unique.SelectedRow = null;

        Assert.False(unique.RemoveRowCommand.CanExecute(null));
        Assert.False(unique.MoveRowUpCommand.CanExecute(null));
        Assert.False(unique.MoveRowDownCommand.CanExecute(null));
    }

    [Fact]
    public void MoveRowUp_SecondRow_ComesFirst()
    {
        var unique = WarlordAxeRows();
        unique.SelectedRow = unique.Rows[1];

        unique.MoveRowUpCommand.Execute(null);

        Assert.Equal(["crit_chance", "power"], unique.Rows.Select(row => row.Affix.Id));
    }

    [Fact]
    public void MoveRowDown_FirstRow_GoesSecond()
    {
        var unique = WarlordAxeRows();
        unique.SelectedRow = unique.Rows[0];

        unique.MoveRowDownCommand.Execute(null);

        Assert.Equal(["crit_chance", "power"], unique.Rows.Select(row => row.Affix.Id));
    }

    [Fact]
    public void ToFixedAffixes_RowsTyped_TakeTheirStatsFromThePool()
    {
        var unique = WarlordAxeRows();

        unique.Rows[0].Affix = unique.AffixChoices.Single(choice => choice.Id == "fire_ward");
        unique.Rows[0].ValueText = "40";

        RolledAffix[] expected = [new("fire_ward", StatKind.FireResistance, 40), new("crit_chance", StatKind.CritChance, 3)];
        Assert.Equal(expected, unique.ToFixedAffixes());
    }

    [Fact]
    public void Problem_ValueNotANumber_NamesTheRow()
    {
        var unique = WarlordAxeRows();

        unique.Rows[1].ValueText = "x";

        Assert.Equal("fixedAffixes[1] (crit_chance): Value must be a whole number (was 'x').", unique.Problem);
        Assert.Throws<InvalidOperationException>(unique.ToFixedAffixes);
    }

    // Base 400 and two affixes worth 20 each: 440, half of it on a sale.
    [Fact]
    public void PieceValueLine_PriceAndTwoAffixes_SaysTheirSumAndItsShare()
    {
        var unique = WarlordAxeRows();

        Assert.Equal("Price as a piece: 440, sells for 220", unique.PieceValueLine("400", SellPercent));
    }

    // fire_ward adds 35 whatever its value, so the line follows the affix and not the number typed.
    [Fact]
    public void PieceValueLine_AffixPicked_AddsItsGold()
    {
        var unique = WarlordAxeRows();

        unique.Rows[0].Affix = unique.AffixChoices.Single(choice => choice.Id == "fire_ward");

        Assert.Equal("Price as a piece: 455, sells for 227", unique.PieceValueLine("400", SellPercent));
    }

    [Theory]
    [InlineData("", "Never bought nor sold")]
    [InlineData("  ", "Never bought nor sold")]
    [InlineData("4x", "")]
    public void PieceValueLine_PriceNotANumber_SaysWhy(string price, string expected)
    {
        var unique = WarlordAxeRows();

        Assert.Equal(expected, unique.PieceValueLine(price, SellPercent));
    }

    [Theory]
    [MemberData(nameof(RowEdits))]
    public void Changed_RowEdit_IsRaisedOnce(string edit)
    {
        var unique = WarlordAxeRows();
        var raised = 0;
        unique.Changed += (_, _) => raised++;

        _rowEdits[edit](unique);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_RowMoved_IsRaised()
    {
        var unique = WarlordAxeRows();
        unique.SelectedRow = unique.Rows[1];
        var raised = 0;
        unique.Changed += (_, _) => raised++;

        unique.MoveRowUpCommand.Execute(null);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void CopyOf_HalfTypedValue_CopiesTheRowsAsTyped()
    {
        var unique = WarlordAxeRows();
        unique.Rows[0].ValueText = "6x";

        var copy = UniqueAffixesViewModel.CopyOf(unique);

        Assert.Equal([("power", "6x"), ("crit_chance", "3")], copy.Rows.Select(row => (row.Affix.Id, row.ValueText)));
        Assert.NotSame(unique.Rows[0], copy.Rows[0]);
    }

    private static UniqueAffixesViewModel WarlordAxeRows() =>
        UniqueAffixesViewModel.From(ItemFixtures.WarlordAxe.FixedAffixes, _pool);

    private static AffixDefinition Affix(string id, StatKind stat, int min, int max) => new()
    {
        Id = id,
        Stat = stat,
        Min = min,
        Max = max,
        Kinds = [EquipmentKind.Weapon],
        Weight = 10,
        GoldValue = 20,
    };
}
