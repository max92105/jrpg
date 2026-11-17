using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// A consumable's effect as typed (decision 881): its kind, targeting, power and a damage's type, a rest's power and the
// type off a damage hidden, never read and written empty; what no box edits (statuses, hits) carried from the record.
public sealed class ItemEffectViewModelTests
{
    private static readonly Effect _bomb = new(EffectKind.Damage, 15, Targeting.SingleEnemy, DamageType: DamageType.Fire);

    private static readonly Effect _heal = new(EffectKind.RestoreHp, 20, Targeting.SingleAlly);

    // Each typed box's edit, a value away from the bomb's.
    private static readonly Dictionary<string, Action<ItemEffectViewModel>> _typings = new()
    {
        [nameof(ItemEffectViewModel.Kind)] = effect => effect.Kind = EffectKind.Revive,
        [nameof(ItemEffectViewModel.Targeting)] = effect => effect.Targeting = Targeting.AllEnemies,
        [nameof(ItemEffectViewModel.PowerText)] = effect => effect.PowerText = "18",
        [nameof(ItemEffectViewModel.DamageType)] = effect => effect.DamageType = DamageType.Ice,
    };

    public static TheoryData<EffectKind> EveryKind => [.. Enum.GetValues<EffectKind>()];

    public static TheoryData<Targeting> EveryTargeting => [.. Enum.GetValues<Targeting>()];

    public static TheoryData<string> TypedBoxes => [.. _typings.Keys];

    [Fact]
    public void From_Bomb_ShowsDamageItsPowerAndType()
    {
        var effect = ItemEffectViewModel.From(_bomb);

        var expected = (EffectKind.Damage, Targeting.SingleEnemy, "15", (DamageType?)DamageType.Fire);
        Assert.Equal(expected, (effect.Kind, effect.Targeting, effect.PowerText, effect.DamageType));
        Assert.Equal((true, true), (effect.TakesPower, effect.TakesDamageType));
    }

    [Theory]
    [MemberData(nameof(EveryKind))]
    public void KindChoices_OfferDamageRestoreHpReviveAndRest(EffectKind kind)
    {
        var effect = ItemEffectViewModel.From(_heal);

        var isItemKind = kind is EffectKind.Damage or EffectKind.RestoreHp or EffectKind.Revive or EffectKind.Rest;
        Assert.Equal(isItemKind, effect.KindChoices.Contains(kind));
    }

    // Data takes a status grant on an item, so a hand-edited one keeps its kind in the box.
    [Fact]
    public void KindChoices_RecordGrantingAStatus_ListsItsKindLast()
    {
        var effect = ItemEffectViewModel.From(new Effect(EffectKind.GrantStatus, 0, Targeting.SingleAlly));

        Assert.Equal(EffectKind.GrantStatus, effect.KindChoices[^1]);
        Assert.Equal(5, effect.KindChoices.Count);
    }

    [Theory]
    [MemberData(nameof(EveryTargeting))]
    public void TargetingChoices_LeaveOutOtherCombatantAndAlliesInRange(Targeting targeting)
    {
        var effect = ItemEffectViewModel.From(_bomb);

        var isSkillOnly = targeting is Targeting.OtherCombatant or Targeting.AlliesInRange;
        Assert.Equal(!isSkillOnly, effect.TargetingChoices.Contains(targeting));
    }

    [Fact]
    public void DamageTypeChoices_ListEveryDamageType()
    {
        var effect = ItemEffectViewModel.From(_bomb);

        Assert.Equal(Enum.GetValues<DamageType>(), effect.DamageTypeChoices);
    }

    [Theory]
    [InlineData(EffectKind.Damage, true, true)]
    [InlineData(EffectKind.RestoreHp, true, false)]
    [InlineData(EffectKind.Revive, true, false)]
    [InlineData(EffectKind.Rest, false, false)]
    public void Kind_Picked_ShowsTheBoxesItReads(EffectKind kind, bool takesPower, bool takesDamageType)
    {
        var effect = ItemEffectViewModel.From(_heal);

        effect.Kind = kind;

        Assert.Equal((takesPower, takesDamageType), (effect.TakesPower, effect.TakesDamageType));
    }

    [Fact]
    public void Kind_Picked_RaisesTheHiddenStates()
    {
        var effect = ItemEffectViewModel.From(_bomb);
        var raised = new List<string?>();
        effect.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        effect.Kind = EffectKind.Rest;

        Assert.Contains(nameof(ItemEffectViewModel.TakesPower), raised);
        Assert.Contains(nameof(ItemEffectViewModel.TakesDamageType), raised);
    }

    [Fact]
    public void ToEffect_Untouched_EqualsTheSource()
    {
        var effect = ItemEffectViewModel.From(_bomb);

        Assert.Equal(_bomb, effect.ToEffect());
    }

    [Fact]
    public void ToEffect_PowerTyped_TakesIt()
    {
        var effect = ItemEffectViewModel.From(_bomb);

        effect.PowerText = " 18 ";

        Assert.Equal(_bomb with { Power = 18 }, effect.ToEffect());
    }

    [Fact]
    public void ToEffect_DamageTurnedHeal_DropsTheType()
    {
        var effect = ItemEffectViewModel.From(_bomb);

        effect.Kind = EffectKind.RestoreHp;
        effect.Targeting = Targeting.SingleAlly;

        Assert.Equal(new Effect(EffectKind.RestoreHp, 15, Targeting.SingleAlly), effect.ToEffect());
    }

    [Fact]
    public void ToEffect_HealTurnedRest_WritesPowerZero()
    {
        var effect = ItemEffectViewModel.From(_heal);

        effect.Kind = EffectKind.Rest;
        effect.Targeting = Targeting.AllAllies;

        Assert.Equal(new Effect(EffectKind.Rest, 0, Targeting.AllAllies), effect.ToEffect());
    }

    // A hand-written rest's power is never read; rewriting it would make an untouched OK an undo step.
    [Fact]
    public void ToEffect_RestWithAPowerTargetingChanged_KeepsTheHiddenPower()
    {
        var effect = ItemEffectViewModel.From(new Effect(EffectKind.Rest, 5, Targeting.AllAllies));

        effect.Targeting = Targeting.SingleAlly;

        Assert.Equal(new Effect(EffectKind.Rest, 5, Targeting.SingleAlly), effect.ToEffect());
    }

    [Fact]
    public void ToEffect_HealTurnedDamage_TakesThePickedType()
    {
        var effect = ItemEffectViewModel.From(_heal);

        effect.Kind = EffectKind.Damage;
        effect.DamageType = DamageType.Lightning;

        Assert.Equal(_heal with { Kind = EffectKind.Damage, DamageType = DamageType.Lightning }, effect.ToEffect());
    }

    [Fact]
    public void ToEffect_CarriesTheSourcesStatusesAndHits()
    {
        var burn = new StatusDefinition { Id = "burn_1", Name = "Burn", Description = "", Parts = [] };
        var source = _bomb with { Hits = 2, Statuses = [new StatusGrant(burn, 3)] };
        var effect = ItemEffectViewModel.From(source);

        effect.PowerText = "9";

        var made = effect.ToEffect();
        Assert.Equal((2, 3), (made.Hits, made.Statuses.Single().Turns));
        Assert.Same(burn, made.Statuses[0].Status);
    }

    [Fact]
    public void ToEffect_Problem_Throws()
    {
        var effect = ItemEffectViewModel.From(_bomb);

        effect.PowerText = "lots";

        Assert.Throws<InvalidOperationException>(effect.ToEffect);
    }

    [Fact]
    public void Problem_PowerNotANumber_NamesTheBox()
    {
        var effect = ItemEffectViewModel.From(_bomb);

        effect.PowerText = "lots";

        Assert.Equal("Power must be a whole number (was 'lots').", effect.Problem);
    }

    [Fact]
    public void Problem_RestWithGarbageInTheHiddenPower_IsNull()
    {
        var effect = ItemEffectViewModel.From(_heal);
        effect.PowerText = "lots";

        effect.Kind = EffectKind.Rest;

        Assert.Null(effect.Problem);
    }

    [Theory]
    [MemberData(nameof(TypedBoxes))]
    public void Changed_TypedBoxEdited_IsRaised(string box)
    {
        var effect = ItemEffectViewModel.From(_bomb);
        var raised = 0;
        effect.Changed += (_, _) => raised++;

        _typings[box](effect);

        Assert.Equal(1, raised);
    }

    [Fact]
    public void CopyOf_HalfTypedPower_CopiesTheBoxesAsTyped()
    {
        var effect = ItemEffectViewModel.From(_bomb);
        effect.PowerText = "1x";
        effect.Targeting = Targeting.AllEnemies;

        var copy = ItemEffectViewModel.CopyOf(effect);

        Assert.Equal(("1x", Targeting.AllEnemies), (copy.PowerText, copy.Targeting));
    }
}
