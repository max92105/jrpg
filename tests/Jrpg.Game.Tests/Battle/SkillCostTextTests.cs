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

    private readonly SkillCostText _text = new(RepositoryScreenText.Text.Messages, new ResourceMeters([ResourceStart.Fixed(_mana), ResourceStart.Fixed(_faith)]));

    [Fact]
    public void CostOf_ResourceSkill_SaysAmountAndResourceName()
    {
        var cost = _text.CostOf(Skill() with { ResourceChanges = [new ResourceChange("mana", -2)] });

        Assert.Equal("2 Mana", cost);
    }

    [Fact]
    public void CostOf_RequirementShort_SaysNeeds()
    {
        var cost = _text.CostOf(Skill() with { Requirements = [new ResourceRequirement("faith", 4)] });

        Assert.Equal("Needs 4 Faith", cost);
    }

    [Fact]
    public void CostOf_RequirementMet_SaysLevel()
    {
        var cost = _text.CostOf(Skill() with { Requirements = [new ResourceRequirement("faith", 2)] });

        Assert.Equal("Faith 2", cost);
    }

    [Fact]
    public void CostOf_FreeSkill_IsEmpty()
    {
        var cost = _text.CostOf(Skill());

        Assert.Empty(cost);
    }

    [Fact]
    public void CannotAffordText_ResourceShort_NamesResource()
    {
        var text = _text.CannotAffordText(_mana);

        Assert.Equal("Not enough Mana.", text);
    }

    private static SkillDefinition Skill() => new()
    {
        Id = "spell",
        Name = "Spell",
        Description = "",
        IconFile = "",
        Effect = new Effect(EffectKind.Damage, 10, Targeting.SingleEnemy),
    };
}
