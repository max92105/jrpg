using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Editor.Effects;
using Jrpg.Editor.Tests.Skills;

namespace Jrpg.Editor.Tests.Effects;

// One tiered effect as the Effects page edits it (decision 983): its boxes, the skills naming it and its amount at the
// shown Power, the id fixed once saved.
public sealed class TieredEffectViewModelTests
{
    private static readonly TieredEffect _cure1 = new("cure_1", EffectKind.RestoreHp, 2);

    [Fact]
    public void From_DamageRecord_FillsEveryBox()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);

        Assert.Equal(
            ("fire_1", EffectKind.Damage, (DamageType?)DamageType.Fire, "3"),
            (effect.IdText, effect.Kind, effect.DamageType, effect.PerPowerText));
        Assert.False(effect.IsNew);
        Assert.Null(effect.Problem);
    }

    // Set up against id order, with a skill naming another tier between them.
    [Fact]
    public void UsedByText_TwoSkillsNameIt_NamesBoth()
    {
        var zap = SkillFixtures.Bolt with { Id = "zap" };
        var mend = SkillFixtures.Bolt with { Id = "mend", Effect = SkillFixtures.Bolt.Effect with { Tier = _cure1 } };

        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, [zap, mend, SkillFixtures.Bolt]);

        Assert.Equal("Used by: bolt, zap", effect.UsedByText);
    }

    [Fact]
    public void UsedByText_NoSkill_SaysSo()
    {
        var effect = TieredEffectViewModel.From(_cure1, [SkillFixtures.Bolt]);

        Assert.Equal("Used by no skill", effect.UsedByText);
    }

    [Fact]
    public void AmountText_PerPowerTwo_ReadsTwentyAtPowerTen()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);

        effect.PerPowerText = "2";

        Assert.Equal("At Power 10: 20 per hit", effect.AmountText);
    }

    [Fact]
    public void AmountText_PerPowerNotANumber_IsEmpty()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);

        effect.PerPowerText = "x";

        Assert.Equal("", effect.AmountText);
    }

    [Fact]
    public void PerPowerText_Typed_RaisesAmountText()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);
        var notified = new List<string?>();
        effect.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        effect.PerPowerText = "2";

        Assert.Contains(nameof(TieredEffectViewModel.AmountText), notified);
    }

    [Fact]
    public void ToEffect_RestoreHp_WritesNoDamageType()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);

        effect.Kind = EffectKind.RestoreHp;

        Assert.Equal(new TieredEffect("fire_1", EffectKind.RestoreHp, 3), effect.ToEffect());
    }

    [Fact]
    public void ToEffect_TypeAndPerPowerRetyped_WritesThem()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);

        effect.DamageType = DamageType.Ice;
        effect.PerPowerText = " 5 ";

        Assert.Equal(SkillFixtures.Fire1 with { PerPower = 5, DamageType = DamageType.Ice }, effect.ToEffect());
    }

    [Fact]
    public void Kind_ChangedToHeal_GreysTheDamageType()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);
        var notified = new List<string?>();
        effect.PropertyChanged += (_, e) => notified.Add(e.PropertyName);

        effect.Kind = EffectKind.RestoreHp;

        Assert.False(effect.CanTypeDamageType);
        Assert.Contains(nameof(TieredEffectViewModel.CanTypeDamageType), notified);
    }

    [Fact]
    public void Problem_NewWithATakenId_NamesIt()
    {
        var effect = TieredEffectViewModel.New(["fire_1"]);

        effect.IdText = "fire_1";

        Assert.Equal("The id fire_1 is taken.", effect.Problem);
    }

    [Fact]
    public void Problem_PerPowerNotANumber_NamesTheBox()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);

        effect.PerPowerText = "x";

        Assert.Equal("Per Power must be a whole number (was 'x').", effect.Problem);
    }

    // Data's least per-Power and Core's first type: only the id is left to type.
    [Fact]
    public void New_IdTyped_IsADamageAtTheLeastPerPower()
    {
        var effect = TieredEffectViewModel.New([]);

        effect.IdText = "spark_1";

        var expected = new TieredEffect("spark_1", EffectKind.Damage, 1) { DamageType = Enum.GetValues<DamageType>()[0] };
        Assert.Equal(expected, effect.ToEffect());
    }

    [Fact]
    public void DuplicateOf_HalfTypedPerPower_CopiesTheBoxesUnderTheNextId()
    {
        var chosen = TieredEffectViewModel.From(SkillFixtures.Fire1, [SkillFixtures.Bolt]);
        chosen.PerPowerText = "x";

        var copy = TieredEffectViewModel.DuplicateOf(chosen, "fire_2", ["fire_1"]);

        Assert.Equal(
            ("fire_2", EffectKind.Damage, (DamageType?)DamageType.Fire, "x"),
            (copy.IdText, copy.Kind, copy.DamageType, copy.PerPowerText));
        Assert.True(copy.IsNew);
        Assert.Equal("Used by no skill", copy.UsedByText);
    }

    [Fact]
    public void Changed_KindPicked_IsRaised()
    {
        var effect = TieredEffectViewModel.From(SkillFixtures.Fire1, []);
        var raised = 0;
        effect.Changed += (_, _) => raised++;

        effect.Kind = EffectKind.RestoreHp;

        Assert.Equal(1, raised);
    }
}
