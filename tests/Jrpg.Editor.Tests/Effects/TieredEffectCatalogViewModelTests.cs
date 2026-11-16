using Jrpg.Core.Effects;
using Jrpg.Editor.Effects;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Effects;

// The Effects page's list (ADR 0045, decision 983): the draft's records in id order, Add, Duplicate under the chosen
// one's next tier (decision 985), only an added effect removable (decision 581), and the problem line ending with
// Data's message, the skills naming the records included (decision 975).
public sealed class TieredEffectCatalogViewModelTests
{
    private static readonly TieredEffect _fire2 = SkillFixtures.Fire1 with { Id = "fire_2", PerPower = 5 };

    // Set up against id order.
    [Fact]
    public void Effects_Order_IdOrderThenAdded()
    {
        var catalog = CatalogOf(_fire2, SkillFixtures.Fire1);

        catalog.AddEffectCommand.Execute(null);

        Assert.Equal(["fire_1", "fire_2", ""], catalog.Effects.Select(effect => effect.Id));
    }

    // fire_2 is taken, so the copy of fire_1 is the stem's next free tier, placed after fire_1.
    [Fact]
    public void DuplicateEffect_Fire1_AddsFire3AfterIt()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1, _fire2);
        catalog.Choose("fire_1");

        catalog.DuplicateEffectCommand.Execute(null);

        Assert.Equal(["fire_1", "fire_3", "fire_2"], catalog.Effects.Select(effect => effect.Id));
        Assert.Same(catalog.Effects[1], catalog.SelectedEffect);
        Assert.Equal("3", catalog.SelectedEffect!.PerPowerText);
    }

    [Fact]
    public void DuplicateEffect_Fire1_AddsFire2AfterIt()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);

        catalog.DuplicateEffectCommand.Execute(null);

        Assert.Equal(["fire_1", "fire_2"], catalog.Effects.Select(effect => effect.Id));
        Assert.Null(catalog.Problem);
    }

    // The copy's tier counts the typed ids too, an added fire_2 among them.
    [Fact]
    public void DuplicateEffect_Twice_TakesTheNextTierEachTime()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);

        catalog.DuplicateEffectCommand.Execute(null);
        catalog.Choose("fire_1");
        catalog.DuplicateEffectCommand.Execute(null);

        Assert.Equal(["fire_1", "fire_3", "fire_2"], catalog.Effects.Select(effect => effect.Id));
    }

    [Fact]
    public void DuplicateEffect_AddedWithoutId_CopyHasAnEmptyId()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);
        catalog.AddEffectCommand.Execute(null);

        catalog.DuplicateEffectCommand.Execute(null);

        Assert.Equal(["fire_1", "", ""], catalog.Effects.Select(effect => effect.IdText));
        Assert.Same(catalog.Effects[2], catalog.SelectedEffect);
    }

    [Fact]
    public void DuplicateEffect_NoEffects_Greyed()
    {
        var catalog = CatalogOf();

        Assert.False(catalog.DuplicateEffectCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveEffect_SavedRecord_Greyed()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);

        Assert.False(catalog.RemoveEffectCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveEffect_AddedEffect_RemovesIt()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);
        catalog.AddEffectCommand.Execute(null);

        catalog.RemoveEffectCommand.Execute(null);

        Assert.Equal(["fire_1"], catalog.Effects.Select(effect => effect.Id));
        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void SelectedEffect_Chosen_RaisesDuplicateAndRemoveStates()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1, _fire2);
        var raised = new List<string>();
        catalog.DuplicateEffectCommand.CanExecuteChanged += (_, _) => raised.Add("duplicate");
        catalog.RemoveEffectCommand.CanExecuteChanged += (_, _) => raised.Add("remove");

        catalog.Choose("fire_2");

        Assert.Equal(["duplicate", "remove"], raised);
    }

    [Fact]
    public void Problem_IdTwice_NamesIt()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);
        catalog.AddEffectCommand.Execute(null);
        catalog.SelectedEffect!.IdText = "spark_1";

        catalog.AddEffectCommand.Execute(null);
        catalog.SelectedEffect.IdText = "spark_1";

        Assert.Equal("Effect spark_1 is listed twice.", catalog.Problem);
    }

    [Fact]
    public void Problem_KindChangedUnderASkill_IsDataMessageNamingTheSkill()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);

        catalog.Effects[0].Kind = EffectKind.RestoreHp;

        Assert.Equal("skills.json: skill 'bolt': effect.kind is damage but fire_1 is restoreHp.", catalog.Problem);
    }

    [Fact]
    public void Problem_PerPowerTyped_IsRaised()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Effects[0].PerPowerText = "0";

        Assert.Contains(nameof(TieredEffectCatalogViewModel.Problem), notified);
        Assert.Equal(
            "effects.json: effect 'fire_1': perPower must be within 1..1000 (was 0).",
            catalog.Problem);
    }

    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1, _fire2);

        catalog.Choose("fire_2");

        Assert.Equal("fire_2", catalog.SelectedEffect!.Id);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1, _fire2);
        catalog.Choose("fire_2");

        catalog.Choose("missing");

        Assert.Equal("fire_2", catalog.SelectedEffect!.Id);
    }

    [Fact]
    public void ChosenKey_NewEffectTyped_IsItsTypedId()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);
        catalog.AddEffectCommand.Execute(null);

        catalog.SelectedEffect!.IdText = " spark_1 ";

        Assert.Equal("spark_1", catalog.ChosenKey);
    }

    [Fact]
    public void ToEffects_Problem_Throws()
    {
        var catalog = CatalogOf(SkillFixtures.Fire1);

        catalog.AddEffectCommand.Execute(null);

        Assert.Throws<InvalidOperationException>(catalog.ToEffects);
    }

    // Bolt names fire_1, so each record's "Used by" line reads the draft's skills.
    private static TieredEffectCatalogViewModel CatalogOf(params TieredEffect[] effects) =>
        new(SkillFixtures.CatalogWithoutUsers(effects.Any(effect => effect.Id == "fire_1") ? [SkillFixtures.Bolt] : [], effects));
}
