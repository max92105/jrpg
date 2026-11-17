using Jrpg.Core.Items;
using Jrpg.Core.Stats;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// An equipment's bonus rows as typed (decision 880): in StatKind order, each stat once from those a base item may give,
// added after the chosen row and removed, their own problems named by stat, relayed as typed changes.
public sealed class ItemEquipmentViewModelBonusTests
{
    // Given out of StatKind order, so the order test can't pass on the order the bonuses were given in.
    private static readonly EquipmentStats _armor = new(EquipmentKind.Chest)
    {
        Bonuses = new StatBonuses(new Dictionary<StatKind, int>
        {
            [StatKind.FireResistance] = 5,
            [StatKind.MaxHp] = 12,
            [StatKind.Constitution] = 2,
        }),
    };

    // Each change the form relays for its rows: the armor lists neither Will nor Power, so picking Will and adding
    // Power both change the bonuses.
    private static readonly Dictionary<string, Action<ItemEquipmentViewModel>> _rowEdits = new()
    {
        ["stat picked"] = equipment => equipment.Bonuses[0].Stat = StatKind.Will,
        ["value typed"] = equipment => equipment.Bonuses[0].ValueText = "3",
        ["row added"] = equipment => equipment.AddBonusCommand.Execute(null),
        ["row removed"] = equipment => equipment.RemoveBonusCommand.Execute(null),
    };

    [Fact]
    public void From_Armor_ListsItsBonusesInKindOrder()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        var rows = equipment.Bonuses.Select(row => (row.Stat, row.ValueText));
        Assert.Equal([(StatKind.Constitution, "2"), (StatKind.MaxHp, "12"), (StatKind.FireResistance, "5")], rows);
    }

    [Fact]
    public void StatChoices_LeaveOutTheAffixOnlyStats()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        Assert.DoesNotContain(StatKind.LifeSteal, equipment.StatChoices);
        Assert.Contains(StatKind.Power, equipment.StatChoices);
        Assert.Equal(Enum.GetValues<StatKind>().Except(StatKinds.AffixOnly), equipment.StatChoices);
    }

    // Constitution is listed, so the fourth add skips it.
    [Fact]
    public void AddBonus_FirstStatsListed_AddsTheNextUnlistedStat()
    {
        var equipment = ItemEquipmentViewModel.From(ItemFixtures.CopperVest.Equipment!);

        for (var add = 0; add < 4; add++)
        {
            equipment.AddBonusCommand.Execute(null);
        }

        var stats = equipment.Bonuses.Select(row => row.Stat);
        Assert.Equal([StatKind.Constitution, StatKind.Power, StatKind.Agility, StatKind.Intelligence, StatKind.Will], stats);
    }

    [Fact]
    public void AddBonus_AddedRow_IsChosen()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        equipment.SelectedBonus = equipment.Bonuses[0];

        equipment.AddBonusCommand.Execute(null);

        Assert.Same(equipment.Bonuses[1], equipment.SelectedBonus);
        Assert.Equal((StatKind.Power, "1"), (equipment.SelectedBonus.Stat, equipment.SelectedBonus.ValueText));
    }

    [Fact]
    public void AddBonus_EveryStatListed_CannotExecute()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        while (equipment.Bonuses.Count < equipment.StatChoices.Count)
        {
            equipment.AddBonusCommand.Execute(null);
        }

        Assert.False(equipment.AddBonusCommand.CanExecute(null));
    }

    [Fact]
    public void AddBonus_OneStatLeftUnlisted_CanExecute()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        while (equipment.Bonuses.Count < equipment.StatChoices.Count - 1)
        {
            equipment.AddBonusCommand.Execute(null);
        }

        Assert.True(equipment.AddBonusCommand.CanExecute(null));
    }

    [Fact]
    public void AddBonusCommand_RowStatPicked_RaisesCanExecuteChanged()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        var raised = 0;
        equipment.AddBonusCommand.CanExecuteChanged += (_, _) => raised++;

        equipment.Bonuses[0].Stat = StatKind.Will;

        Assert.Equal(1, raised);
    }

    [Fact]
    public void RemoveBonus_ChosenRow_RemovesIt()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        equipment.SelectedBonus = equipment.Bonuses[1];

        equipment.RemoveBonusCommand.Execute(null);

        Assert.Equal([StatKind.Constitution, StatKind.FireResistance], equipment.Bonuses.Select(row => row.Stat));
    }

    [Fact]
    public void RemoveBonus_NoRowChosen_CannotExecute()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        equipment.SelectedBonus = null;

        Assert.False(equipment.RemoveBonusCommand.CanExecute(null));
    }

    [Fact]
    public void ToEquipment_UntouchedArmor_EqualsTheSource()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        Assert.Equal(_armor, equipment.ToEquipment());
    }

    [Fact]
    public void ToEquipment_RowsTyped_GivesTheirBonuses()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        equipment.Bonuses[0].Stat = StatKind.Power;
        equipment.Bonuses[1].ValueText = "-7";

        var expected = new Dictionary<StatKind, int>
        {
            [StatKind.Power] = 2,
            [StatKind.MaxHp] = -7,
            [StatKind.FireResistance] = 5,
        };
        Assert.Equal(new StatBonuses(expected), equipment.ToEquipment().Bonuses);
    }

    [Fact]
    public void ToEquipment_EveryRowRemoved_HasNoBonus()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        while (equipment.SelectedBonus is not null)
        {
            equipment.RemoveBonusCommand.Execute(null);
        }

        Assert.Equal(StatBonuses.None, equipment.ToEquipment().Bonuses);
    }

    [Fact]
    public void Problem_TwoRowsWithOneStat_NamesIt()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        equipment.Bonuses[2].Stat = StatKind.MaxHp;

        Assert.Equal("bonuses names maxHp twice.", equipment.Problem);
    }

    [Fact]
    public void Problem_ValueNotANumber_NamesTheRow()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);

        equipment.Bonuses[1].ValueText = "x";

        Assert.Equal("bonuses[1] (maxHp): Value must be a whole number (was 'x').", equipment.Problem);
    }

    [Fact]
    public void CopyOf_RowsTyped_CopiesThemAsTyped()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        equipment.Bonuses[0].ValueText = "2x";

        var copy = ItemEquipmentViewModel.CopyOf(equipment);

        var rows = copy.Bonuses.Select(row => (row.Stat, row.ValueText));
        Assert.Equal([(StatKind.Constitution, "2x"), (StatKind.MaxHp, "12"), (StatKind.FireResistance, "5")], rows);
        Assert.NotSame(equipment.Bonuses[0], copy.Bonuses[0]);
    }

    [Fact]
    public void RemoveBonusCommand_RowChosen_RaisesCanExecuteChanged()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        var raised = 0;
        equipment.RemoveBonusCommand.CanExecuteChanged += (_, _) => raised++;

        equipment.SelectedBonus = equipment.Bonuses[2];

        Assert.Equal(1, raised);
    }

    [Theory]
    [InlineData("stat picked")]
    [InlineData("value typed")]
    [InlineData("row added")]
    [InlineData("row removed")]
    public void Changed_RowTypedAddedOrRemoved_IsRaised(string edit)
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        var raised = 0;
        equipment.Changed += (_, _) => raised++;

        _rowEdits[edit](equipment);

        Assert.Equal(1, raised);
    }

    // The value's typing also raises the row's problem notice, which the form must not relay a second time.
    [Fact]
    public void Changed_RowProblemNotice_IsNotRaised()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        var rowNotices = new List<string?>();
        equipment.Bonuses[0].PropertyChanged += (_, e) => rowNotices.Add(e.PropertyName);
        var raised = 0;
        equipment.Changed += (_, _) => raised++;

        equipment.Bonuses[0].ValueText = "x";

        Assert.Contains(nameof(StatBonusRow.Problem), rowNotices);
        Assert.Equal(1, raised);
    }

    [Fact]
    public void Changed_RemovedRowTyped_IsNotRaised()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        var removed = equipment.Bonuses[0];
        equipment.SelectedBonus = removed;
        equipment.RemoveBonusCommand.Execute(null);
        var raised = 0;
        equipment.Changed += (_, _) => raised++;

        removed.ValueText = "9";

        Assert.Equal(0, raised);
    }

    [Fact]
    public void PropertyChanged_RowValueTyped_RaisesProblem()
    {
        var equipment = ItemEquipmentViewModel.From(_armor);
        var raised = new List<string?>();
        equipment.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        equipment.Bonuses[0].ValueText = "x";

        Assert.Contains(nameof(ItemEquipmentViewModel.Problem), raised);
    }
}
