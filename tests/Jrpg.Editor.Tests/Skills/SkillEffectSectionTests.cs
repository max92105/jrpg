using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Editor.Effects;
using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// A skill's effect as typed (decision 980): a tier sets the kind, type and amount and greys them; without one they are
// typed; the boxes a kind or a targeting doesn't read are greyed and written empty.
public sealed class SkillEffectSectionTests
{
    private static readonly TieredEffect _heal2 = new("heal_2", EffectKind.RestoreHp, 4);

    private static readonly IReadOnlyList<TierChoice> _tiers =
        TierChoice.AllIn(SkillFixtures.CatalogWith([], [SkillFixtures.Fire1, _heal2], []));

    private static readonly Effect _flatIce = new(EffectKind.Damage, 12, Targeting.SingleEnemy, DamageType: DamageType.Ice);

    public static TheoryData<EffectKind> EveryKind => [.. Enum.GetValues<EffectKind>()];

    [Fact]
    public void From_TieredSkill_ShowsTheTierAndGreysKindAndPower()
    {
        var section = SkillEffectSection.From(SkillFixtures.Bolt.Effect, _tiers);

        var expected = ("fire_1", EffectKind.Damage, (DamageType?)DamageType.Fire);
        Assert.Equal(expected, (section.TierId, section.Kind, section.DamageType));
        Assert.Equal((false, false, false), (section.CanTypeKind, section.CanTypePower, section.CanTypeDamageType));
    }

    [Fact]
    public void ChooseTier_HealRecord_SetsKindRestoreHpAndNoType()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.TierId = "heal_2";

        Assert.Equal((EffectKind.RestoreHp, (DamageType?)null, "0"), (section.Kind, section.DamageType, section.PowerText));
    }

    [Fact]
    public void ChooseTier_None_KeepsKindAndTypeTypeable()
    {
        var section = SkillEffectSection.From(SkillFixtures.Bolt.Effect, _tiers);

        section.TierId = "";

        Assert.Equal((EffectKind.Damage, (DamageType?)DamageType.Fire), (section.Kind, section.DamageType));
        Assert.Equal((true, true, true), (section.CanTypeKind, section.CanTypePower, section.CanTypeDamageType));
    }

    [Fact]
    public void CanTypeRange_AlliesInRange_True()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.Targeting = Targeting.AlliesInRange;

        Assert.True(section.CanTypeRange);
    }

    [Fact]
    public void CanTypeRange_SingleEnemy_False()
    {
        var section = SkillEffectSection.From(_flatIce with { Targeting = Targeting.AlliesInRange }, _tiers);

        section.Targeting = Targeting.SingleEnemy;

        Assert.False(section.CanTypeRange);
    }

    [Fact]
    public void CanTypePower_GrantStatus_False()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.Kind = EffectKind.GrantStatus;

        Assert.Equal((false, false), (section.CanTypePower, section.CanTypeDamageType));
    }

    [Fact]
    public void ToEffect_Tiered_WritesPowerZeroAndTheRecord()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.TierId = "fire_1";

        var effect = section.ToEffect();
        Assert.Same(SkillFixtures.Fire1, effect.Tier);
        Assert.Equal((0, (DamageType?)DamageType.Fire), (effect.Power, effect.DamageType));
    }

    [Fact]
    public void ToEffect_FlatRevive_KeepsTheTypedPowerAndNoType()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.Kind = EffectKind.Revive;
        section.PowerText = "30";

        var effect = section.ToEffect();
        Assert.Equal((EffectKind.Revive, 30, (DamageType?)null), (effect.Kind, effect.Power, effect.DamageType));
        Assert.Null(effect.Tier);
    }

    [Fact]
    public void ToEffect_RangeTypedOffAlliesInRange_WritesZero()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.RangeText = "2";

        Assert.Equal(0, section.ToEffect().Range);
    }

    [Fact]
    public void ToEffect_RangeTypedOnAlliesInRange_WritesIt()
    {
        var section = SkillEffectSection.From(_flatIce with { Targeting = Targeting.AlliesInRange }, _tiers);

        section.RangeText = "2";

        Assert.Equal(2, section.ToEffect().Range);
    }

    [Fact]
    public void ToEffect_HitsTyped_WritesThem()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.HitsText = "3";

        Assert.Equal(3, section.ToEffect().Hits);
    }

    [Theory]
    [MemberData(nameof(EveryKind))]
    public void Kinds_EveryKindButRest_Offered(EffectKind kind)
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        Assert.Equal(kind != EffectKind.Rest, section.Kinds.Contains(kind));
    }

    [Fact]
    public void Problem_HitsNotANumber_NamesHits()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.HitsText = "x";

        Assert.Equal("Hits must be a whole number (was 'x').", section.Problem);
    }

    [Fact]
    public void Problem_GreyedPowerNotANumber_IsNull()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);
        section.PowerText = "x";

        section.Kind = EffectKind.GrantStatus;

        Assert.Null(section.Problem);
    }

    [Fact]
    public void Problem_TypeablePowerNotANumber_NamesPower()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.PowerText = "x";

        Assert.Equal("Power must be a whole number (was 'x').", section.Problem);
    }

    [Fact]
    public void ToEffect_Problem_Throws()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);

        section.HitsText = "";

        Assert.Throws<InvalidOperationException>(section.ToEffect);
    }

    // Copied from the boxes: reading the half-typed power would throw.
    [Fact]
    public void CopyOf_PowerHalfTyped_CopiesTheBoxes()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);
        section.PowerText = "-";

        var copy = SkillEffectSection.CopyOf(section);

        Assert.Equal(("", "-", (DamageType?)DamageType.Ice), (copy.TierId, copy.PowerText, copy.DamageType));
    }

    [Fact]
    public void PropertyChanged_TierChosen_RaisesTheGreyedStates()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);
        var notified = Notices(section);

        section.TierId = "fire_1";

        Assert.Superset(
            new HashSet<string?>
            {
                nameof(SkillEffectSection.CanTypeKind),
                nameof(SkillEffectSection.CanTypePower),
                nameof(SkillEffectSection.CanTypeDamageType),
            },
            notified);
    }

    [Fact]
    public void PropertyChanged_KindChosen_RaisesPowerAndTypeStates()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);
        var notified = Notices(section);

        section.Kind = EffectKind.RestoreHp;

        Assert.Superset(
            new HashSet<string?> { nameof(SkillEffectSection.CanTypePower), nameof(SkillEffectSection.CanTypeDamageType) },
            notified);
    }

    [Fact]
    public void PropertyChanged_TargetingChosen_RaisesCanTypeRange()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);
        var notified = Notices(section);

        section.Targeting = Targeting.AlliesInRange;

        Assert.Contains(nameof(SkillEffectSection.CanTypeRange), notified);
    }

    // The power's Problem notice is raised too; only the typed field tells the skill.
    [Fact]
    public void Changed_PowerTyped_RaisedOnce()
    {
        var section = SkillEffectSection.From(_flatIce, _tiers);
        var raised = 0;
        section.Changed += (_, _) => raised++;

        section.PowerText = "13";

        Assert.Equal(1, raised);
    }

    private static HashSet<string?> Notices(SkillEffectSection section)
    {
        var notified = new HashSet<string?>();
        section.PropertyChanged += (_, e) => notified.Add(e.PropertyName);
        return notified;
    }
}
