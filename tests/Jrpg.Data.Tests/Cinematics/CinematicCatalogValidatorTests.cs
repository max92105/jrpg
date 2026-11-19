using Jrpg.Core.Cinematics;
using Jrpg.Data.Cinematics;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Cinematics;

// cinematics.json's rules (ADR 0046): every problem names the cinematic, and a still's problem names the still too.
public sealed class CinematicCatalogValidatorTests
{
    private const string FileName = "cinematics.json";

    private static readonly CinematicStill _first = new() { ImageFile = "intro/1_home.png", Seconds = 4 };

    private static readonly CinematicStill _second = new()
    {
        ImageFile = "intro/2_road.png",
        Seconds = 5,
        Captions = [new StillCaption { Text = "He left at dawn.", Seconds = 2 }],
        VoiceFile = "intro_2.wav",
        Zoom = new StillZoom { StartPercent = 100, EndPercent = 120 },
    };

    private static readonly CinematicDefinition _twoStills = new()
    {
        Id = "intro",
        MusicFile = "town/loop.wav",
        Stills = [_first, _second],
    };

    // Each still lasts two thirds of the longest TimeSpan: each is countable, the two together are not.
    private static readonly double _twoThirdsOfTheLongest = TimeSpan.MaxValue.TotalSeconds / 1.5;

    public static TheoryData<CinematicDefinition, string> InvalidCinematics => new()
    {
        { _twoStills with { Stills = [] }, "stills must list at least one still." },
        { _twoStills with { Stills = null! }, "stills must list at least one still." },
        { _twoStills with { Stills = [_first, null!] }, "stills[1]: is null." },
        { WithSecond(_second with { ImageFile = " " }), "stills[1]: imageFile must name a picture." },
        { WithSecond(_second with { Seconds = 0 }), "stills[1]: seconds must be above 0 (was 0)." },
        { WithSecond(_second with { Seconds = -1 }), "stills[1]: seconds must be above 0 (was -1)." },
        { WithSecond(_second with { Seconds = double.NaN }), "stills[1]: seconds must be above 0 (was NaN)." },
        { WithSecond(_second with { Seconds = double.PositiveInfinity }), "stills[1]: seconds must be above 0 (was Infinity)." },
        {
            _twoStills with
            {
                Stills = [_first with { Seconds = _twoThirdsOfTheLongest }, _second with { Seconds = _twoThirdsOfTheLongest }],
            },
            "the stills' seconds together must be a time the game can count."
        },
        { WithCaption(Line("Late.", 0)), "stills[1]: captions[0]: seconds must be above 0 (was 0)." },
        { WithCaption(Line("Late.", double.NaN)), "stills[1]: captions[0]: seconds must be above 0 (was NaN)." },
        { WithCaption(null!), "stills[1]: captions[0] must hold a text, empty for a pause." },
        { WithCaption(Line(null!, 1)), "stills[1]: captions[0] must hold a text, empty for a pause." },
        { WithSecond(_second with { Captions = null! }), "stills[1]: captions must be a list or be left out." },
        { WithSecond(_second with { VoiceFile = "" }), "stills[1]: voiceFile must name a voice line or be left out." },
        { _twoStills with { MusicFile = " " }, "musicFile must name a track or be left out." },
        { WithZoom(99, 120), "stills[1]: zoom startPercent must be within 100..150 (was 99)." },
        { WithZoom(151, 120), "stills[1]: zoom startPercent must be within 100..150 (was 151)." },
        { WithZoom(120, 99), "stills[1]: zoom endPercent must be within 100..150 (was 99)." },
        { WithZoom(120, 151), "stills[1]: zoom endPercent must be within 100..150 (was 151)." },
        { WithFocus(-1, 50), "stills[1]: focus xPercent must be within 0..100 (was -1)." },
        { WithFocus(101, 50), "stills[1]: focus xPercent must be within 0..100 (was 101)." },
        { WithFocus(50, -1), "stills[1]: focus yPercent must be within 0..100 (was -1)." },
        { WithFocus(50, 101), "stills[1]: focus yPercent must be within 0..100 (was 101)." },
        {
            WithSecond(_second with { Zoom = null, Focus = Focus(101, 50) }),
            "stills[1]: focus xPercent must be within 0..100 (was 101)."
        },
        {
            WithSecond(_second with { Zoom = new StillZoom { StartPercent = 100, EndPercent = 151 }, Focus = Focus(101, 50) }),
            "stills[1]: zoom endPercent must be within 100..150 (was 151)."
        },
    };

    [Theory]
    [MemberData(nameof(InvalidCinematics))]
    public void Validate_InvalidCinematic_ThrowsNamingTheCinematicAndTheField(CinematicDefinition cinematic, string problem)
    {
        var exception = Assert.IsType<ContentValidationException>(ValidationError(cinematic));

        Assert.Equal($"cinematics.json: cinematic 'intro': {problem}", exception.Message);
    }

    [Fact]
    public void Validate_RepeatedId_ThrowsNamingIt()
    {
        var exception = Assert.IsType<ContentValidationException>(ValidationError(_twoStills, _twoStills));

        Assert.Equal("cinematics.json: cinematic id 'intro' is used more than once.", exception.Message);
    }

    [Theory]
    [InlineData(100, 150)]
    [InlineData(150, 100)]
    public void Validate_ZoomAtBothBounds_Passes(int startPercent, int endPercent)
    {
        Assert.Null(ValidationError(WithZoom(startPercent, endPercent)));
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(100, 0)]
    public void Validate_FocusAtBothBounds_Passes(int xPercent, int yPercent)
    {
        Assert.Null(ValidationError(WithFocus(xPercent, yPercent)));
    }

    // A focus is kept on a still without a zoom, where it changes nothing (decision 750).
    [Fact]
    public void Validate_FocusWithoutZoom_Passes()
    {
        Assert.Null(ValidationError(WithSecond(_second with { Zoom = null, Focus = Focus(30, 62) })));
    }

    [Fact]
    public void Validate_NoOptionalFields_Passes()
    {
        var bare = new CinematicDefinition { Id = "intro", Stills = [_first] };

        Assert.Null(ValidationError(bare));
    }

    [Fact]
    public void Validate_SmallestSeconds_Passes()
    {
        var shortest = _second with { Seconds = 0.01, Captions = [Line("He left at dawn.", 0.01)] };

        Assert.Null(ValidationError(WithSecond(shortest)));
    }

    [Fact]
    public void Validate_OneStillOfTwoThirdsTheLongestTime_Passes()
    {
        var oneLongStill = _twoStills with { Stills = [_first with { Seconds = _twoThirdsOfTheLongest }] };

        Assert.Null(ValidationError(oneLongStill));
    }

    // A pause is an empty line; the lines may fill the still to its last second, the typed tenths' rounding included.
    [Fact]
    public void Validate_CaptionsFillingTheStillWithAPause_Passes()
    {
        var filled = _second with { Seconds = 0.3, Captions = [Line("He left.", 0.1), Line("", 0.1), Line("At dawn.", 0.1)] };

        Assert.Null(ValidationError(WithSecond(filled)));
    }

    [Fact]
    public void Validate_CaptionsLongerThanTheirStill_ThrowsNamingBothTimes()
    {
        var tooLong = _second with { Seconds = 5, Captions = [Line("He left.", 3), Line("At dawn.", 2.5)] };

        var error = Assert.IsType<ContentValidationException>(ValidationError(WithSecond(tooLong)));

        var problem = "stills[1]: captions show for 5.5 seconds together, longer than the still's 5.";
        Assert.Equal($"cinematics.json: cinematic 'intro': {problem}", error.Message);
    }

    private static StillCaption Line(string text, double seconds) => new() { Text = text, Seconds = seconds };

    private static CinematicDefinition WithCaption(StillCaption caption) => WithSecond(_second with { Captions = [caption] });

    private static Exception? ValidationError(params CinematicDefinition[] cinematics) =>
        Record.Exception(() => CinematicCatalogValidator.Validate(FileName, cinematics));

    private static CinematicDefinition WithSecond(CinematicStill second) => _twoStills with { Stills = [_first, second] };

    private static CinematicDefinition WithZoom(int startPercent, int endPercent) =>
        WithSecond(_second with { Zoom = new StillZoom { StartPercent = startPercent, EndPercent = endPercent } });

    private static CinematicDefinition WithFocus(int xPercent, int yPercent) =>
        WithSecond(_second with { Focus = Focus(xPercent, yPercent) });

    private static StillFocus Focus(int xPercent, int yPercent) => new() { XPercent = xPercent, YPercent = yPercent };
}
