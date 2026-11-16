using Jrpg.Core.Statuses;
using Jrpg.Editor.Statuses;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Statuses;

// The Statuses page's list (ADR 0045, decision 984): the draft's records in id order, Add, Duplicate with its id to
// type, New tier as the chosen one's family's next tier (decision 985), only an added status removable (decision 581),
// and the problem line ending with Data's message, a family whose records differ included.
public sealed class StatusCatalogViewModelTests
{
    private static readonly StatusDefinition _poison1 = SkillFixtures.Poison1 with { Family = "poison" };

    private static readonly StatusDefinition _stun = new()
    {
        Id = "stun",
        Name = "Stun",
        Description = "",
        Parts = [new StatusPart(StatusPartKind.SkipTurn, 0)],
    };

    // Set up against id order.
    [Fact]
    public void Statuses_Order_IdOrderThenAdded()
    {
        var catalog = CatalogOf(_stun, _poison1);

        catalog.AddStatusCommand.Execute(null);

        Assert.Equal(["poison_1", "stun", ""], catalog.Statuses.Select(status => status.Id));
    }

    [Fact]
    public void NewTier_Poison1_AddsPoison2InItsFamilyAfterIt()
    {
        var catalog = CatalogOf(_poison1, _stun);

        catalog.NewTierCommand.Execute(null);

        Assert.Equal(["poison_1", "poison_2", "stun"], catalog.Statuses.Select(status => status.Id));
        Assert.Same(catalog.Statuses[1], catalog.SelectedStatus);
        Assert.Equal("poison", catalog.SelectedStatus!.FamilyText);
        Assert.Null(catalog.Problem);
    }

    // The copy names the chosen one's id as its family, so the two are one family once written.
    [Fact]
    public void NewTier_StatusWithoutFamily_GivesBothTheSameFamilyKey()
    {
        var catalog = CatalogOf(_stun);

        catalog.NewTierCommand.Execute(null);

        Assert.Equal(
            [("stun", "stun"), ("stun_1", "stun")],
            catalog.ToStatuses().Select(status => (status.Id, status.FamilyKey)));
    }

    // The copy's tier counts the typed ids too, an added poison_2 among them.
    [Fact]
    public void NewTier_Twice_TakesTheNextTierEachTime()
    {
        var catalog = CatalogOf(_poison1);

        catalog.NewTierCommand.Execute(null);
        catalog.Choose("poison_1");
        catalog.NewTierCommand.Execute(null);

        Assert.Equal(["poison_1", "poison_3", "poison_2"], catalog.Statuses.Select(status => status.Id));
    }

    [Fact]
    public void NewTier_AddedWithoutId_CopyHasAnEmptyId()
    {
        var catalog = CatalogOf(_poison1);
        catalog.AddStatusCommand.Execute(null);

        catalog.NewTierCommand.Execute(null);

        Assert.Equal(["poison_1", "", ""], catalog.Statuses.Select(status => status.IdText));
        Assert.Same(catalog.Statuses[2], catalog.SelectedStatus);
    }

    [Fact]
    public void NewTier_NoStatuses_Greyed()
    {
        var catalog = CatalogOf();

        Assert.False(catalog.NewTierCommand.CanExecute(null));
    }

    [Fact]
    public void DuplicateStatus_Poison1_AddsACopyWithAnEmptyIdAfterIt()
    {
        var catalog = CatalogOf(_poison1, _stun);

        catalog.DuplicateStatusCommand.Execute(null);

        Assert.Equal(["poison_1", "", "stun"], catalog.Statuses.Select(status => status.IdText));
        Assert.Equal("poison", catalog.Statuses[1].FamilyText);
    }

    [Fact]
    public void DuplicateStatus_NoStatuses_Greyed()
    {
        var catalog = CatalogOf();

        Assert.False(catalog.DuplicateStatusCommand.CanExecute(null));
    }

    // Set up against sorted order, the typed family trimmed.
    [Fact]
    public void Families_TypedFamily_Offered()
    {
        var catalog = CatalogOf(_stun, _poison1);
        catalog.AddStatusCommand.Execute(null);

        catalog.SelectedStatus!.FamilyText = " bleed ";

        Assert.Equal(["bleed", "poison", "stun"], catalog.Families);
    }

    [Fact]
    public void Families_SharedFamily_OfferedOnce()
    {
        var catalog = CatalogOf(_poison1);

        catalog.NewTierCommand.Execute(null);

        Assert.Equal(["poison"], catalog.Families);
    }

    [Fact]
    public void Families_AddedWithNothingTyped_OffersNoEmptyKey()
    {
        var catalog = CatalogOf(_poison1);

        catalog.AddStatusCommand.Execute(null);

        Assert.Equal(["poison"], catalog.Families);
    }

    [Fact]
    public void RemoveStatus_SavedStatus_Greyed()
    {
        var catalog = CatalogOf(_poison1);

        Assert.False(catalog.RemoveStatusCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveStatus_AddedStatus_RemovesIt()
    {
        var catalog = CatalogOf(_poison1);
        catalog.AddStatusCommand.Execute(null);

        catalog.RemoveStatusCommand.Execute(null);

        Assert.Equal(["poison_1"], catalog.Statuses.Select(status => status.Id));
        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void SelectedStatus_Chosen_RaisesCommandStates()
    {
        var catalog = CatalogOf(_poison1, _stun);
        var raised = new List<string>();
        catalog.DuplicateStatusCommand.CanExecuteChanged += (_, _) => raised.Add("duplicate");
        catalog.NewTierCommand.CanExecuteChanged += (_, _) => raised.Add("new tier");
        catalog.RemoveStatusCommand.CanExecuteChanged += (_, _) => raised.Add("remove");

        catalog.Choose("stun");

        Assert.Equal(["duplicate", "new tier", "remove"], raised);
    }

    // The Family box reads the families again on a choice, not on each typed letter.
    [Fact]
    public void SelectedStatus_Chosen_RaisesFamilies()
    {
        var catalog = CatalogOf(_poison1, _stun);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Choose("stun");

        Assert.Contains(nameof(StatusCatalogViewModel.Families), notified);
    }

    // Raised per letter, the page's editable Family box would reset the text being typed.
    [Fact]
    public void Families_FamilyTyped_NotRaised()
    {
        var catalog = CatalogOf(_poison1, _stun);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.SelectedStatus!.FamilyText = "venom";

        Assert.DoesNotContain(nameof(StatusCatalogViewModel.Families), notified);
    }

    [Fact]
    public void Problem_FamilyPartsDiffer_IsDataMessage()
    {
        var catalog = CatalogOf(_poison1);
        catalog.NewTierCommand.Execute(null);

        catalog.SelectedStatus!.Parts[0].Kind = StatusPartKind.HealEachTurn;

        Assert.Equal("statuses.json: family 'poison': poison_2's parts differ from poison_1's.", catalog.Problem);
    }

    // poison_1's amount is Data's problem and comes first in the list, yet a status's own problem is reported first.
    [Fact]
    public void Problem_OwnAndDataProblems_OwnFirst()
    {
        var catalog = CatalogOf(_poison1);
        catalog.Statuses[0].Parts[0].AmountText = "0";

        catalog.AddStatusCommand.Execute(null);

        Assert.Equal("Status #2: The id must not be empty.", catalog.Problem);
    }

    [Fact]
    public void Problem_AmountTyped_IsRaised()
    {
        var catalog = CatalogOf(_poison1);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Statuses[0].Parts[0].AmountText = "0";

        Assert.Contains(nameof(StatusCatalogViewModel.Problem), notified);
        Assert.Equal(
            "statuses.json: status 'poison_1': parts[0]: damageEachTurn's amount must be at least 1 (was 0).",
            catalog.Problem);
    }

    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = CatalogOf(_poison1, _stun);

        catalog.Choose("stun");

        Assert.Equal("stun", catalog.SelectedStatus!.Id);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        var catalog = CatalogOf(_poison1, _stun);
        catalog.Choose("stun");

        catalog.Choose("missing");

        Assert.Equal("stun", catalog.SelectedStatus!.Id);
    }

    [Fact]
    public void ChosenKey_NewStatusTyped_IsItsTypedId()
    {
        var catalog = CatalogOf(_poison1);
        catalog.AddStatusCommand.Execute(null);

        catalog.SelectedStatus!.IdText = " bleed_1 ";

        Assert.Equal("bleed_1", catalog.ChosenKey);
    }

    [Fact]
    public void ToStatuses_Problem_Throws()
    {
        var catalog = CatalogOf(_poison1);

        catalog.AddStatusCommand.Execute(null);

        Assert.Throws<InvalidOperationException>(catalog.ToStatuses);
    }

    // Bolt grants poison_1, so a list holding it has a skill the rebinding points at the edited record.
    private static StatusCatalogViewModel CatalogOf(params StatusDefinition[] statuses) =>
        new(SkillFixtures.CatalogWithoutUsers(
            statuses.Any(status => status.Id == "poison_1") ? [SkillFixtures.Bolt] : [],
            [SkillFixtures.Fire1]) with
        {
            Statuses = statuses.ToDictionary(status => status.Id, StringComparer.Ordinal),
        });
}
