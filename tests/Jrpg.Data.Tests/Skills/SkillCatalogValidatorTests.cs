using Jrpg.Core.Effects;
using Jrpg.Core.Resources;
using Jrpg.Core.Skills;
using Jrpg.Core.Stats;
using Jrpg.Core.Statuses;
using Jrpg.Data.Content;
using Jrpg.Data.Skills;

namespace Jrpg.Data.Tests.Skills;

// skills.json's rules, over skills built here; a skill's damage or heal names a tiered effect (ADR 0056), so the two
// fixtures name records built here too.
public sealed class SkillCatalogValidatorTests
{
    private const string FileName = "skills.json";

    private static readonly TieredEffect _heal1 = new("heal_1", EffectKind.RestoreHp, 2);

    private static readonly TieredEffect _fire1 = new("fire_1", EffectKind.Damage, 2) { DamageType = DamageType.Fire };

    private static readonly SkillDefinition _heal = new()
    {
        Id = "heal",
        Name = "Heal",
        Description = "",
        IconFile = "",
        SoundFile = "Mend.wav",
        Effect = new Effect(EffectKind.RestoreHp, Power: 0, Targeting.SingleAlly) { Tier = _heal1 },
    };

    private static readonly SkillDefinition _blast = _heal with
    {
        Effect = new Effect(EffectKind.Damage, Power: 0, Targeting.SingleEnemy, DamageType: DamageType.Fire) { Tier = _fire1 },
    };

    public static TheoryData<SkillDefinition, string> InvalidSkills => new()
    {
        { _blast with { EffectFile = " " }, "effectFile must name a strip in effects/ when present." },
        { _heal with { EffectFile = "Burst.png" }, "effectFile is only for a damaging effect (decision 707)." },
        { _heal with { Name = "" }, "name must not be empty." },
        { _heal with { SoundFile = "" }, "soundFile must name a sound." },
        { _heal with { SoundFile = "  " }, "soundFile must name a sound." },
        { _heal with { ResourceChanges = [null!] }, "resourceChanges must not contain a null entry." },
        { _heal with { ResourceChanges = [new ResourceChange("mana", -3), new ResourceChange("mana", -2)] }, "resourceChanges lists 'mana' more than once." },
        { _heal with { Effect = _heal.Effect with { Power = -5 } }, "effect.power must be at least 0 (was -5)." },
        { _heal with { Effect = new Effect(EffectKind.Revive, Power: 0, Targeting.SingleFallenAlly) }, "effect.power must be at least 1 for a revive effect (was 0)." },
        { _blast with { Effect = _blast.Effect with { Power = 10, Tier = null } }, TierRequired },
        { _heal with { Effect = _heal.Effect with { Power = 15, Tier = null } }, TierRequired },
    };

    private static string TierRequired =>
        "effect.tier is required for a skill's damage or restoreHp effect: its amount is its user's Power times the record's "
        + "perPower (decision 704).";

    [Fact]
    public void Validate_TieredDamageSkill_Accepted()
    {
        Assert.Null(Record.Exception(() => SkillCatalogValidator.Validate(FileName, [_blast])));
    }

    [Fact]
    public void Validate_GrantStatusSkillWithoutTier_Accepted()
    {
        var exposed = new StatusGrant(
            new StatusDefinition
            {
                Id = "exposed_2",
                Name = "Exposed",
                Description = "",
                Parts = [new StatusPart(StatusPartKind.DamageTakenChange, 50)],
            },
            Turns: 3);
        var expose = _heal with
        {
            Effect = new Effect(EffectKind.GrantStatus, Power: 0, Targeting.SingleEnemy) { Statuses = [exposed] },
        };

        Assert.Null(Record.Exception(() => SkillCatalogValidator.Validate(FileName, [expose])));
    }

    [Fact]
    public void Validate_ReviveSkillWithFlatPower_Accepted()
    {
        var raise = _heal with { Effect = new Effect(EffectKind.Revive, Power: 10, Targeting.SingleFallenAlly) };

        Assert.Null(Record.Exception(() => SkillCatalogValidator.Validate(FileName, [raise])));
    }

    [Fact]
    public void Validate_ValidSkill_DoesNotThrow()
    {
        var exception = Record.Exception(() => SkillCatalogValidator.Validate(FileName, [_heal]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_DamagingSkillNamingAStrip_DoesNotThrow()
    {
        var skill = _blast with { EffectFile = "Burst.png" };

        var exception = Record.Exception(() => SkillCatalogValidator.Validate(FileName, [skill]));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_RequirementBelowOne_Throws()
    {
        var idle = _heal with { Requirements = [new ResourceRequirement("faith", 0)] };

        var exception = Assert.Throws<ContentValidationException>(() => SkillCatalogValidator.Validate(FileName, [idle]));

        Assert.Equal("skills.json: skill 'heal': requirements: atLeast for 'faith' must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_QuickSkillWithoutCost_Throws()
    {
        var endless = _heal with { IsQuick = true };

        var exception = Assert.Throws<ContentValidationException>(() => SkillCatalogValidator.Validate(FileName, [endless]));

        Assert.Equal("skills.json: skill 'heal': isQuick needs a resource cost in resourceChanges, or the turn would never end.", exception.Message);
    }

    [Fact]
    public void Validate_RestSkill_Throws()
    {
        var rest = _heal with { Effect = new Effect(EffectKind.Rest, Power: 0, Targeting.AllAllies) };

        var exception = Assert.Throws<ContentValidationException>(() => SkillCatalogValidator.Validate(FileName, [rest]));

        Assert.Equal("skills.json: skill 'heal': effect.kind rest is for items only; a skill is used in battle, where nobody rests.", exception.Message);
    }

    [Fact]
    public void Validate_ZeroResourceChange_Throws()
    {
        var idle = _heal with { ResourceChanges = [new ResourceChange("faith", 0)] };

        var exception = Assert.Throws<ContentValidationException>(() => SkillCatalogValidator.Validate(FileName, [idle]));

        Assert.Equal("skills.json: skill 'heal': resourceChanges has a 0 amount for 'faith', which does nothing.", exception.Message);
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
