using Jrpg.Core.Effects;
using Jrpg.Core.Skills;
using Jrpg.Data.Content;
using Jrpg.Data.Skills;

namespace Jrpg.Data.Tests.Skills;

public sealed class SkillCatalogValidatorTests
{
    private const string FileName = "skills.json";

    private static readonly SkillDefinition _heal = new()
    {
        Id = "heal",
        Name = "Heal",
        Description = "",
        IconFile = "",
        MpCost = 8,
        Effect = new Effect(EffectKind.RestoreHp, Power: 15, Targeting.SingleAlly),
    };

    public static TheoryData<SkillDefinition, string> InvalidSkills => new()
    {
        { _heal with { Name = "" }, "name must not be empty." },
        { _heal with { Effect = _heal.Effect with { Power = -5 } }, "effect.power must be at least 0 (was -5)." },
        { _heal with { Effect = new Effect(EffectKind.Revive, Power: 0, Targeting.SingleFallenAlly) }, "effect.power must be at least 1 for a revive effect (was 0)." },
    };

    [Fact]
    public void Validate_ValidSkill_DoesNotThrow()
    {
        var exception = Record.Exception(() => SkillCatalogValidator.Validate(FileName, [_heal]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_NegativeMpCost_ThrowsNamingTheSkill()
    {
        var exception = Assert.Throws<ContentValidationException>(() => SkillCatalogValidator.Validate(FileName, [_heal with { MpCost = -1 }]));

        Assert.Equal("skills.json: skill 'heal': mpCost must be at least 0 (was -1).", exception.Message);
    }

    [Theory]
    [MemberData(nameof(InvalidSkills))]
    public void Validate_InvalidField_ThrowsNamingTheSkillAndField(SkillDefinition skill, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => SkillCatalogValidator.Validate(FileName, [skill]));

        Assert.Equal($"skills.json: skill 'heal': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_DuplicateId_ThrowsNamingTheId()
    {
        var exception = Assert.Throws<ContentValidationException>(() => SkillCatalogValidator.Validate(FileName, [_heal, _heal]));

        Assert.Equal("skills.json: skill id 'heal' is used more than once.", exception.Message);
    }
}
