using System.Collections.Frozen;
using Jrpg.Core.Effects;
using Jrpg.Editor.Effects;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Effects;

// What a Tier box offers (ADR 0056): none first, then every tiered effect record by id with its line; the records are
// built here so no repository tier is pinned.
public sealed class TierChoiceTests
{
    private static readonly TieredEffect _heal2 = new("heal_2", EffectKind.RestoreHp, 4);

    // Set up against id order, in a frozen dictionary as the loader keeps them.
    [Fact]
    public void AllIn_NoneFirstThenById()
    {
        var content = SkillFixtures.TestWorld with
        {
            TieredEffects = new[] { _heal2, SkillFixtures.Fire1 }.ToFrozenDictionary(tier => tier.Id, StringComparer.Ordinal),
        };

        var choices = TierChoice.AllIn(content);

        Assert.Equal(["", "fire_1", "heal_2"], choices.Select(choice => choice.Id));
        Assert.Equal(("(none: a flat power)", (TieredEffect?)null), (choices[0].Line, choices[0].Tier));
    }

    [Fact]
    public void Line_DamageRecord_ReadsKindTypeAndPerPower()
    {
        var content = SkillFixtures.CatalogWith([], [SkillFixtures.Fire1], []);

        var choice = TierChoice.AllIn(content).Single(tier => tier.Id == "fire_1");

        Assert.Equal("fire_1: damage, fire, x3", choice.Line);
    }

    [Fact]
    public void Line_HealRecord_ReadsKindAndPerPower()
    {
        var content = SkillFixtures.CatalogWith([], [_heal2], []);

        var choice = TierChoice.AllIn(content).Single(tier => tier.Id == "heal_2");

        Assert.Equal("heal_2: restoreHp, x4", choice.Line);
    }
}
