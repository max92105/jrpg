using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Game.Battle;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Battle;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class SkillCostTextTests
{
    private static readonly ResourceDefinition _mana = new("mana", "Mana", Max: 10, StartsAt: 3);
    private static readonly ResourceDefinition _faith = new("faith", "Faith", Max: 5, StartsAt: 3);

    private readonly SkillCostText _text = new(RepositoryScreenText.Text.Messages, new ResourceMeters([_mana, _faith]));

    [Fact]
    public void CostOf_MpSkill_SaysMp()
    {
        var cost = _text.CostOf(Skill(mpCost: 8));

        Assert.Equal("8 MP", cost);
    }

    [Fact]
    public void CostOf_ResourceSkill_SaysAmountAndResourceName()
    {
        var cost = _text.CostOf(Skill(mpCost: 0) with { ResourceChanges = [new ResourceChange("mana", -2)] });

        Assert.Equal("2 Mana", cost);
    }

    [Fact]
    public void CostOf_RequirementShort_SaysNeeds()
    {
        var cost = _text.CostOf(Skill(mpCost: 8) with { Requirements = [new ResourceRequirement("faith", 4)] });

        Assert.Equal("Needs 4 Faith", cost);
    }

    [Fact]
    public void CostOf_RequirementMet_SaysLevel()
    {
        var cost = _text.CostOf(Skill(mpCost: 8) with { Requirements = [new ResourceRequirement("faith", 2)] });

        Assert.Equal("Faith 2", cost);
    }

    [Fact]
    public void CannotAffordText_ResourceShort_NamesResource()
    {
        var text = _text.CannotAffordText(_mana);

        Assert.Equal("Not enough Mana.", text);
    }

    [Fact]
    public void CannotAffordText_OnlyMpShort_SaysMp()
    {
        var text = _text.CannotAffordText(lacking: null);

        Assert.Equal("Not enough MP.", text);
    }

    private static SkillDefinition Skill(int mpCost) => new()
    {
        Id = "spell",
        Name = "Spell",
        Description = "",
        IconFile = "",
        MpCost = mpCost,
        Effect = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy),
    };
}
