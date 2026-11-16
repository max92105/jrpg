using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;
using Jrpg.Data.Effects;

namespace Jrpg.Data.Tests.Effects;

// effects.json's rules (ADR 0056), over records built here.
public sealed class EffectCatalogValidatorTests
{
    private const string FileName = "effects.json";

    private static readonly TieredEffect _fire1 = new("fire_1", EffectKind.Damage, 2) { DamageType = DamageType.Fire };

    private static readonly TieredEffect _heal1 = new("heal_1", EffectKind.RestoreHp, 2);

    public static TheoryData<TieredEffect, string> InvalidEffects => new()
    {
        { _fire1 with { Kind = EffectKind.Revive }, "kind must be damage or restoreHp (was revive)." },
        { _fire1 with { DamageType = null }, "a damage effect needs a damageType." },
        { _heal1 with { DamageType = DamageType.Light }, "a restoreHp effect has no damageType." },
        { _fire1 with { PerPower = 0 }, "perPower must be within 1..1000 (was 0)." },
        { _fire1 with { PerPower = 1001 }, "perPower must be within 1..1000 (was 1001)." },
    };

    [Theory]
    [MemberData(nameof(InvalidEffects))]
    public void Validate_InvalidEffect_ThrowsNamingTheFileTheEffectAndTheProblem(TieredEffect effect, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => EffectCatalogValidator.Validate(FileName, [effect]));

        Assert.Equal($"effects.json: effect '{effect.Id}': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_IdTwice_Throws()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => EffectCatalogValidator.Validate(FileName, [_fire1, _fire1 with { PerPower = 4 }]));

        Assert.Equal("effects.json: effect id 'fire_1' is used more than once.", exception.Message);
    }

    [Fact]
    public void Validate_PerPowerOne_Accepted()
    {
        Assert.Null(Record.Exception(() => EffectCatalogValidator.Validate(FileName, [_fire1 with { PerPower = 1 }, _heal1])));
    }

    [Fact]
    public void Validate_PerPowerOneThousand_Accepted()
    {
        Assert.Null(Record.Exception(() => EffectCatalogValidator.Validate(FileName, [_fire1 with { PerPower = 1000 }])));
    }
}
