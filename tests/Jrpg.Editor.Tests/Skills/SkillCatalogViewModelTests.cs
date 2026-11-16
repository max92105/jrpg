using Jrpg.Core.Effects;
using Jrpg.Core.Party;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// The Skills page's list (ADR 0045): the draft's skills in id order, Add from the first damage tier and Duplicate after
// the chosen one (decision 977), only an added skill removable (decision 581), and the problem line ending with Data's
// message, the classes naming the skills included (decision 974).
public sealed class SkillCatalogViewModelTests
{
    private static readonly SkillDefinition _zap = SkillFixtures.Bolt with { Id = "zap", Name = "Zap" };

    [Fact]
    public void Skills_RepositoryOrder_IdOrderThenAdded()
    {
        var catalog = CatalogOf(_zap, SkillFixtures.Bolt);

        catalog.AddSkillCommand.Execute(null);

        Assert.Equal(["bolt", "zap", ""], catalog.Skills.Select(skill => skill.Id));
    }

    [Fact]
    public void AddSkill_EmptyCatalogOfTiers_StartsAFlatDamage()
    {
        var catalog = CatalogWithTiers();

        catalog.AddSkillCommand.Execute(null);
        catalog.SelectedSkill!.IdText = "spark";

        var effect = catalog.SelectedSkill.ToSkill().Effect;
        Assert.Null(effect.Tier);
        var expected = (EffectKind.Damage, 1, Targeting.SingleEnemy, (DamageType?)DamageType.Blunt);
        Assert.Equal(expected, (effect.Kind, effect.Power, effect.Targeting, effect.DamageType));
    }

    // Set up against id order, a heal first by id: the tier is the damage one first by id, not the first listed.
    [Fact]
    public void AddSkill_WithTiers_NamesTheFirstDamageTierById()
    {
        var fire2 = new TieredEffect("fire_2", EffectKind.Damage, 5) { DamageType = DamageType.Fire };
        var cure1 = new TieredEffect("cure_1", EffectKind.RestoreHp, 2);
        var catalog = CatalogWithTiers(fire2, cure1, SkillFixtures.Fire1);

        catalog.AddSkillCommand.Execute(null);
        catalog.SelectedSkill!.IdText = "spark";

        var effect = catalog.SelectedSkill.ToSkill().Effect;
        Assert.Same(SkillFixtures.Fire1, effect.Tier);
        Assert.Equal((EffectKind.Damage, DamageType.Fire, 0), (effect.Kind, effect.DamageType, effect.Power));
    }

    [Fact]
    public void AddSkill_TypedIdNameAndSound_HasNoProblem()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt);
        catalog.AddSkillCommand.Execute(null);

        catalog.SelectedSkill!.IdText = "spark";
        catalog.SelectedSkill.Name = "Spark";
        catalog.SelectedSkill.SoundFile = "Spark.wav";

        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void DuplicateSkill_Chosen_CopiesItAfterWithAnEmptyId()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt, _zap);
        catalog.Choose("bolt");

        catalog.DuplicateSkillCommand.Execute(null);

        var copy = catalog.Skills[1];
        Assert.Same(copy, catalog.SelectedSkill);
        Assert.True(copy.IsNew);
        Assert.Equal(("", "Bolt", "BoltStrip.png"), (copy.IdText, copy.Name, copy.EffectFile));
    }

    [Fact]
    public void DuplicateSkill_NoSkills_Greyed()
    {
        var catalog = CatalogOf();

        Assert.False(catalog.DuplicateSkillCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveSkill_SavedSkill_Greyed()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt);

        Assert.False(catalog.RemoveSkillCommand.CanExecute(null));
    }

    [Fact]
    public void RemoveSkill_AddedSkill_RemovesIt()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt);
        catalog.AddSkillCommand.Execute(null);

        catalog.RemoveSkillCommand.Execute(null);

        Assert.Equal(["bolt"], catalog.Skills.Select(skill => skill.Id));
        Assert.Null(catalog.Problem);
    }

    [Fact]
    public void SelectedSkill_Chosen_RaisesDuplicateAndRemoveStates()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt, _zap);
        var raised = new List<string>();
        catalog.DuplicateSkillCommand.CanExecuteChanged += (_, _) => raised.Add("duplicate");
        catalog.RemoveSkillCommand.CanExecuteChanged += (_, _) => raised.Add("remove");

        catalog.Choose("zap");

        Assert.Equal(["duplicate", "remove"], raised);
    }

    [Fact]
    public void Problem_IdTwice_NamesIt()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt);
        catalog.AddSkillCommand.Execute(null);
        catalog.SelectedSkill!.IdText = "spark";

        catalog.AddSkillCommand.Execute(null);
        catalog.SelectedSkill.IdText = "spark";

        Assert.Equal("Skill spark is listed twice.", catalog.Problem);
    }

    // The record's cost is one the mage can't pay.
    [Fact]
    public void Problem_ClassCantPayAnEditedSkill_IsDataMessage()
    {
        var bolt = SkillFixtures.Bolt with { ResourceChanges = [new ResourceChange("stamina", -1)] };
        var mage = SkillFixtures.TestWorld.Classes["mage"] with { SkillIds = ["bolt"] };
        var content = SkillFixtures.CatalogWithoutUsers([bolt], [SkillFixtures.Fire1]) with
        {
            Classes = new Dictionary<string, ClassDefinition> { ["mage"] = mage },
        };

        var catalog = new SkillCatalogViewModel(content, SkillFixtures.NoChoices);

        Assert.Equal(
            "classes.json: class 'mage': skillIds lists 'bolt', which uses 'stamina', a resource mage doesn't have.",
            catalog.Problem);
    }

    [Fact]
    public void Problem_ZeroAmountTyped_IsDataMessage()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt);

        catalog.Skills[0].Costs.Changes[0].AmountText = "0";

        Assert.Equal(
            "skills.json: skill 'bolt': resourceChanges has a 0 amount for 'mana', which does nothing.",
            catalog.Problem);
    }

    // Clarity, offered first, isn't the resource the saved bolt names: the added row reads the offered list.
    [Fact]
    public void AddSkill_ThenAddChange_StartsOnTheFirstOfferedResource()
    {
        var resources = new ResourceChoice[] { new("clarity", "Clarity (mage)"), new("mana", "Mana (mage)") };
        var content = SkillFixtures.CatalogWithoutUsers([SkillFixtures.Bolt], [SkillFixtures.Fire1]);
        var catalog = new SkillCatalogViewModel(content, SkillFixtures.NoChoices with { Resources = resources });
        catalog.AddSkillCommand.Execute(null);

        catalog.SelectedSkill!.Costs.AddChangeCommand.Execute(null);

        Assert.Equal("clarity", catalog.SelectedSkill.Costs.Changes.Single().ResourceId);
    }

    [Fact]
    public void Problem_SkillTyped_IsRaised()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt);
        var notified = new List<string?>();
        catalog.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        catalog.Skills[0].Name = " ";

        Assert.Contains(nameof(SkillCatalogViewModel.Problem), notified);
        Assert.Equal("skills.json: skill 'bolt': name must not be empty.", catalog.Problem);
    }

    [Fact]
    public void Choose_KnownId_ChoosesIt()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt, _zap);

        catalog.Choose("zap");

        Assert.Equal("zap", catalog.SelectedSkill!.Id);
    }

    [Fact]
    public void Choose_UnknownId_KeepsTheChoice()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt, _zap);
        catalog.Choose("zap");

        catalog.Choose("missing");

        Assert.Equal("zap", catalog.SelectedSkill!.Id);
    }

    [Fact]
    public void ChosenKey_NewSkillTyped_IsItsTypedId()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt);
        catalog.AddSkillCommand.Execute(null);

        catalog.SelectedSkill!.IdText = " spark ";

        Assert.Equal("spark", catalog.ChosenKey);
    }

    [Fact]
    public void ToSkills_Problem_Throws()
    {
        var catalog = CatalogOf(SkillFixtures.Bolt);

        catalog.AddSkillCommand.Execute(null);

        Assert.Throws<InvalidOperationException>(catalog.ToSkills);
    }

    private static SkillCatalogViewModel CatalogOf(params SkillDefinition[] skills) =>
        new(SkillFixtures.CatalogWithoutUsers(skills, [SkillFixtures.Fire1]), SkillFixtures.NoChoices);

    private static SkillCatalogViewModel CatalogWithTiers(params TieredEffect[] tiers) =>
        new(SkillFixtures.CatalogWithoutUsers([], tiers), SkillFixtures.NoChoices);
}
