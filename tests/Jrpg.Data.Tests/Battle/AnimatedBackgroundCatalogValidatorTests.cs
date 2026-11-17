using Jrpg.Core.Battle;
using Jrpg.Core.Maps;
using Jrpg.Data.Battle;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Battle;

// The rules for animated-backgrounds.json (ADR 0059, decision 897): one record per file, a frame from 1 x 1 px, and an
// animation of at least 2 frames from 1 ms each.
public sealed class AnimatedBackgroundCatalogValidatorTests
{
    private const string FileName = "animated-backgrounds.json";

    private const string Pulse = "animated-backgrounds.json: animated background 'ForestPulse.png'";

    private static readonly AnimatedBackgroundDefinition _pulse = new()
    {
        File = "ForestPulse.png",
        FrameWidth = 960,
        FrameHeight = 540,
        Animation = new TileAnimation { FrameCount = 4, FrameMilliseconds = 200 },
    };

    public static TheoryData<AnimatedBackgroundDefinition[], string> InvalidBackgrounds => new()
    {
        {
            [_pulse, _pulse with { FrameWidth = 480 }],
            "animated-backgrounds.json: animated background id 'ForestPulse.png' is used more than once."
        },
        { [_pulse with { File = " " }], "animated-backgrounds.json: entry 0 has an empty id." },
        { [_pulse with { FrameWidth = 0 }], $"{Pulse}: frameWidth must be at least 1 (was 0)." },
        { [_pulse with { FrameHeight = 0 }], $"{Pulse}: frameHeight must be at least 1 (was 0)." },
        {
            [_pulse with { Animation = _pulse.Animation with { FrameCount = 1 } }],
            $"{Pulse}: animation.frameCount must be at least 2 (was 1)."
        },
        {
            [_pulse with { Animation = _pulse.Animation with { FrameMilliseconds = 0 } }],
            $"{Pulse}: animation.frameMilliseconds must be at least 1 (was 0)."
        },
        { [_pulse with { Animation = null! }], $"{Pulse}: animation is required." },
    };

    [Theory]
    [MemberData(nameof(InvalidBackgrounds))]
    public void Validate_InvalidRecord_ThrowsNamingTheFileAndBackground(
        AnimatedBackgroundDefinition[] backgrounds, string message)
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => AnimatedBackgroundCatalogValidator.Validate(FileName, backgrounds));

        Assert.Equal(message, exception.Message);
    }

    [Fact]
    public void Validate_SmallestAcceptedRecord_Accepts()
    {
        var smallest = _pulse with
        {
            FrameWidth = 1,
            FrameHeight = 1,
            Animation = new TileAnimation { FrameCount = 2, FrameMilliseconds = 1 },
        };

        Assert.Null(Record.Exception(() => AnimatedBackgroundCatalogValidator.Validate(FileName, [smallest])));
    }

    [Fact]
    public void Validate_EmptyList_Accepts()
    {
        Assert.Null(Record.Exception(() => AnimatedBackgroundCatalogValidator.Validate(FileName, [])));
    }

    [Fact]
    public void Validate_NullEntry_IsRefused()
    {
        var exception = Assert.Throws<ContentValidationException>(
            () => AnimatedBackgroundCatalogValidator.Validate(FileName, [null!]));

        Assert.Equal("animated-backgrounds.json: entry 0 is null.", exception.Message);
    }
}
