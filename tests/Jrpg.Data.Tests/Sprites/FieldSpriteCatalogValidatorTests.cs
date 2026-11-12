using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Data.Sprites;

namespace Jrpg.Data.Tests.Sprites;

public sealed class FieldSpriteCatalogValidatorTests
{
    private const string FileName = "field-sprites.json";

    // The repository's shape (ADR 0043): a bought sheet's 32 px frames at scale 3, four walk frames.
    private static readonly FieldSpriteSheet _villager = new()
    {
        File = "villager.png",
        FrameWidth = 32,
        FrameHeight = 32,
        Scale = 3,
        FeetPixels = 2,
        Walk = new FacingCycle
        {
            Rows = new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 },
            Frames = new TileAnimation { FrameCount = 4, FrameMilliseconds = 150 },
        },
    };

    [Fact]
    public void Validate_RepositoryShapedSheet_Passes() =>
        Assert.Null(Record.Exception(() => FieldSpriteCatalogValidator.Validate(FileName, [_villager])));

    [Fact]
    public void Validate_HugeFrameCount_Passes() =>
        Assert.Null(Record.Exception(() => FieldSpriteCatalogValidator.Validate(FileName, [WithFrames(int.MaxValue, 1)])));

    public static TheoryData<FieldSpriteSheet, string> InvalidSheets => new()
    {
        { _villager with { File = "villager.jpg" }, "file must name a .png image." },
        { _villager with { FrameWidth = 0 }, "frameWidth must be at least 1 (was 0)." },
        { _villager with { FrameHeight = 0 }, "frameHeight must be at least 1 (was 0)." },
        { _villager with { Scale = 0 }, "scale must be within 1..8 (was 0)." },
        { _villager with { Scale = 9 }, "scale must be within 1..8 (was 9)." },
        { _villager with { FeetPixels = -1 }, "feetPixels must be within 0..31 (was -1)." },
        { _villager with { FeetPixels = 32 }, "feetPixels must be within 0..31 (was 32)." },
        { WithRows(new FacingRows { Down = 0, Left = -1, Right = 2, Up = 3 }), "walk.rows.left must be at least 0 (was -1)." },
        { WithFrames(1, 150), "walk.frames.frameCount must be at least 2 (was 1)." },
        { WithFrames(4, 0), "walk.frames.frameMilliseconds must be at least 1 (was 0)." },
    };

    [Theory]
    [MemberData(nameof(InvalidSheets))]
    public void Validate_InvalidSheet_ThrowsNamingTheFileAndTheProblem(FieldSpriteSheet sheet, string problem)
    {
        var exception = Assert.Throws<ContentValidationException>(() => FieldSpriteCatalogValidator.Validate(FileName, [sheet]));

        Assert.StartsWith(FileName, exception.Message, StringComparison.Ordinal);
        Assert.Contains(sheet.File, exception.Message, StringComparison.Ordinal);
        Assert.EndsWith(problem, exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_BlankFile_Throws()
    {
        var blank = _villager with { File = " " };

        var exception = Assert.Throws<ContentValidationException>(() => FieldSpriteCatalogValidator.Validate(FileName, [blank]));

        Assert.EndsWith("entry 0 has an empty id.", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_FileListedTwice_Throws()
    {
        FieldSpriteSheet[] twice = [_villager, _villager with { Scale = 2 }];

        var exception = Assert.Throws<ContentValidationException>(() => FieldSpriteCatalogValidator.Validate(FileName, twice));

        Assert.EndsWith("field sprite id 'villager.png' is used more than once.", exception.Message, StringComparison.Ordinal);
    }

    private static FieldSpriteSheet WithRows(FacingRows rows) => _villager with { Walk = _villager.Walk with { Rows = rows } };

    private static FieldSpriteSheet WithFrames(int count, int milliseconds)
    {
        var frames = new TileAnimation { FrameCount = count, FrameMilliseconds = milliseconds };
        return _villager with { Walk = _villager.Walk with { Frames = frames } };
    }
}
