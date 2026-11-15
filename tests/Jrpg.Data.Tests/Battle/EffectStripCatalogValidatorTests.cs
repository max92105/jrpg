using Jrpg.Core.Battle;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Battle;

// The rules for effect-strips.json (ADR 0048): one record per file, a pace from 1 ms, a hit frame from 0, and a swing
// named when there is one.
public sealed class EffectStripCatalogValidatorTests
{
    private const string FileName = "effect-strips.json";

    private static readonly EffectStripDefinition _slash = new()
    {
        File = "GoldenSlashImpact.png",
        FrameMilliseconds = 17,
        HitFrame = 3,
        SwingFile = "GoldenSlashAttack.png",
    };

    private const string Slash = "effect-strips.json: effect strip 'GoldenSlashImpact.png'";

    public static TheoryData<EffectStripDefinition[], string> InvalidStrips => new()
    {
        { [_slash with { File = " " }], "effect-strips.json: entry 0 has an empty id." },
        {
            [_slash, _slash with { HitFrame = 1 }],
            "effect-strips.json: effect strip id 'GoldenSlashImpact.png' is used more than once."
        },
        { [_slash with { FrameMilliseconds = 0 }], $"{Slash}: frameMilliseconds must be at least 1 (was 0)." },
        { [_slash with { HitFrame = -1 }], $"{Slash}: hitFrame must be at least 0 (was -1)." },
        { [_slash with { SwingFile = "" }], $"{Slash}: swingFile must not be blank." },
    };

    [Theory]
    [MemberData(nameof(InvalidStrips))]
    public void Validate_InvalidStrip_ThrowsNamingTheFileAndStrip(EffectStripDefinition[] strips, string message)
    {
        var exception = Assert.Throws<ContentValidationException>(() => EffectStripCatalogValidator.Validate(FileName, strips));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Validate_LowestPaceAndHitFrameZero_Passes()
    {
        var strip = _slash with { FrameMilliseconds = 1, HitFrame = 0 };

        Assert.Null(Record.Exception(() => EffectStripCatalogValidator.Validate(FileName, [strip])));
    }

    [Fact]
    public void Validate_NoSwing_Passes()
    {
        Assert.Null(Record.Exception(() => EffectStripCatalogValidator.Validate(FileName, [_slash with { SwingFile = null }])));
    }
}
