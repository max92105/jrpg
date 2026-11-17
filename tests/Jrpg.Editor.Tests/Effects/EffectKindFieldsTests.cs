using Jrpg.Core.Effects;
using Jrpg.Editor.Effects;

namespace Jrpg.Editor.Tests.Effects;

// Which boxes an effect's kind reads, shared by the Skills page's effect and the Items page's consumable: every kind
// answered, so a kind added to Core fails here before a page throws on it.
public sealed class EffectKindFieldsTests
{
    public static TheoryData<EffectKind> EveryKind => [.. Enum.GetValues<EffectKind>()];

    [Theory]
    [InlineData(EffectKind.Damage, true, true)]
    [InlineData(EffectKind.RestoreHp, true, false)]
    [InlineData(EffectKind.Revive, true, false)]
    [InlineData(EffectKind.GrantStatus, false, false)]
    [InlineData(EffectKind.ReadyExtraTurn, false, false)]
    [InlineData(EffectKind.Rest, false, false)]
    public void Reads_EachKind_PowerForHpAndTypeForDamage(EffectKind kind, bool readsPower, bool readsDamageType)
    {
        var reads = (EffectKindFields.ReadsPower(kind), EffectKindFields.ReadsDamageType(kind));

        Assert.Equal((readsPower, readsDamageType), reads);
    }

    [Theory]
    [MemberData(nameof(EveryKind))]
    public void ReadsPower_EveryKind_Answers(EffectKind kind)
    {
        var exception = Record.Exception(() => EffectKindFields.ReadsPower(kind));

        Assert.Null(exception);
    }
}
