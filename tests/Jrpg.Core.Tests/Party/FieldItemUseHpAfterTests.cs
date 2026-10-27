using Jrpg.Core.Effects;
using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Core.Tests.Builders;

namespace Jrpg.Core.Tests.Party;

public sealed class FieldItemUseHpAfterTests
{
    private const int PotionPower = 30;
    private const int FeatherPower = 25;

    private readonly ItemDefinition _potion = Item("potion", EffectKind.RestoreHp, PotionPower, Targeting.SingleAlly);
    private readonly ItemDefinition _feather = Item("revive_feather", EffectKind.Revive, FeatherPower, Targeting.SingleFallenAlly);
    private readonly PartyMember _hurt = Member("hurt", hp: 40);
    private readonly PartyMember _nearlyFull = Member("nearly_full", hp: 90);
    private readonly PartyMember _fallen = Member("fallen", hp: 0);

    [Fact]
    public void HpAfterUse_PotionOnHurtMember_AddsThePotionsPower()
    {
        Assert.Equal(40 + PotionPower, FieldItemUse.HpAfterUse(_potion, _hurt));
    }

    [Fact]
    public void HpAfterUse_PotionNearFullHp_StopsAtMaxHp()
    {
        Assert.Equal(_nearlyFull.Vitals.MaxHp, FieldItemUse.HpAfterUse(_potion, _nearlyFull));
    }

    [Fact]
    public void HpAfterUse_PotionOnFallenMember_StaysAtZero()
    {
        Assert.Equal(0, FieldItemUse.HpAfterUse(_potion, _fallen));
    }

    [Fact]
    public void HpAfterUse_ReviveOnFallenMember_IsTheRevivedHp()
    {
        Assert.Equal(FeatherPower, FieldItemUse.HpAfterUse(_feather, _fallen));
    }

    [Fact]
    public void HpAfterUse_AnyItem_LeavesTheMembersHpAsItWas()
    {
        FieldItemUse.HpAfterUse(_potion, _hurt);
        FieldItemUse.HpAfterUse(_feather, _fallen);

        Assert.Equal(40, _hurt.Vitals.Hp);
        Assert.Equal(0, _fallen.Vitals.Hp);
    }

    [Fact]
    public void HpAfterUse_RestItem_Throws()
    {
        var kit = Item("camp_kit", EffectKind.Rest, 0, Targeting.AllAllies);

        Assert.Throws<InvalidOperationException>(() => FieldItemUse.HpAfterUse(kit, _hurt));
    }

    private static PartyMember Member(string id, int hp) =>
        new PartyMemberBuilder().WithId(id).WithVitals(hp, maxHp: 100).Build();

    private static ItemDefinition Item(string id, EffectKind kind, int power, Targeting targeting) =>
        new ItemDefinitionBuilder().WithId(id).WithEffect(new Effect(kind, power, targeting)).Build() with { IsUsableFromMenu = true };
}
