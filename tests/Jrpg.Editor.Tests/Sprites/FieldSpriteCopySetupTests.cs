using Jrpg.Core.Content;
using Jrpg.Core.Geometry;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

/// <summary>
/// Copy setup from... (decision 570): a second character from the same pack takes the first one's layout. The test
/// world's placeholder record is changed to a bought shape (32 px frames at scale 3, 4 frames at 150 ms, feet 2, Right
/// mirroring Left) so the copy has numbers and a walk to carry.
/// </summary>
public sealed class FieldSpriteCopySetupTests
{
    private const string Placeholder = "npc_placeholder.png";
    private const string Player = "player.png";
    private const string Bought = "villager.png";
    private const string Wide = "wide.png";
    private const string Short = "short.png";

    private static readonly string _testWorld = Path.Combine(AppContext.BaseDirectory, "test-world");
    private static readonly ContentCatalog _catalog = ContentCatalogLoader.Load(_testWorld);

    private static readonly Dictionary<string, (int Width, int Height)> _sizes = new(StringComparer.Ordinal)
    {
        [Placeholder] = (128, 224),
        [Player] = (384, 288),
        [Bought] = (128, 224),
        [Wide] = (144, 288),
        [Short] = (384, 96),
    };

    private readonly FieldSpriteCatalogViewModel _window;

    public FieldSpriteCopySetupTests()
    {
        var placeholder = _catalog.FieldSprites[Placeholder];
        var bought = placeholder with
        {
            FrameWidth = 32,
            FrameHeight = 32,
            Scale = 3,
            FeetPixels = 2,
            Walk = placeholder.Walk with
            {
                Frames = placeholder.Walk.Frames with { FrameCount = 4, FrameMilliseconds = 150 },
                Sequences = new FacingSequences { Right = new FacingSequence { MirrorOf = Facing.Left } },
            },
        };
        var sheets = new Dictionary<string, FieldSpriteSheet>(_catalog.FieldSprites, StringComparer.Ordinal);
        sheets[Placeholder] = bought;
        var draft = new CatalogDraft(_catalog with { FieldSprites = sheets });
        _window = new FieldSpriteCatalogViewModel(draft, () => [Placeholder, Player, Bought, Wide, Short], file => _sizes[file]);
        _window.Chosen = _window.Choices.Single(choice => choice.File == Bought);
    }

    [Fact]
    public void CopySetupFrom_SameSizedSheet_CopiesEveryField()
    {
        _window.CopySetupFrom(Placeholder);

        var setup = _window.Setup!;
        Assert.Equal((4, 7, 32, 32), (setup.Columns, setup.Rows, setup.FrameWidth, setup.FrameHeight));
        Assert.Equal((3, 2), (setup.Scale, setup.FeetPixels));
        Assert.Equal((4, 150), (setup.Walk.Frames, setup.Walk.FrameMilliseconds));
        Assert.Null(_window.Problem);
    }

    // The player's 48 px layout splits a 384 x 96 image into 8 x 2 frames, but its walk reads rows up to the fourth.
    [Fact]
    public void CopySetupFrom_WalkRowsPastThisImage_ShowsTheSetupProblem()
    {
        _window.Chosen = _window.Choices.Single(choice => choice.File == Short);

        _window.CopySetupFrom(Player);

        Assert.Equal("The up row must be within 1..2 (was 4).", _window.Problem);
    }

    // 144 px isn't a whole number of 32 px frames: the copy takes nothing and says why until the next edit.
    [Fact]
    public void CopySetupFrom_FrameSizeNotSplittingThisImage_RefusesAndSaysSo()
    {
        _window.Chosen = _window.Choices.Single(choice => choice.File == Wide);

        _window.CopySetupFrom(Placeholder);

        Assert.Equal("144 x 288 px doesn't split into 32 x 32 px frames.", _window.Problem);
        Assert.Equal((4, 1), (_window.Setup!.Columns, _window.Setup.Scale));
    }

    [Fact]
    public void CopySetupFrom_RefusedCopy_ProblemClearsOnTheNextEdit()
    {
        _window.Chosen = _window.Choices.Single(choice => choice.File == Wide);
        _window.CopySetupFrom(Placeholder);

        _window.Setup!.Scale = 2;

        Assert.Null(_window.Problem);
    }

    // Decision 570 copies the walk whole: a facing's mirror goes with it (ADR 0054).
    [Fact]
    public void CopySetupFrom_SourceWithMirror_CopiesIt()
    {
        _window.CopySetupFrom(Placeholder);

        var right = _window.Setup!.Walk.Right;
        Assert.Equal((FacingSource.Mirror, Facing.Left), (right.Source, right.MirrorOf));
    }

    [Fact]
    public void CopySources_ExcludeTheChosenFile()
    {
        _window.Chosen = _window.Choices.Single(choice => choice.File == Player);

        Assert.Equal([Placeholder], _window.CopySources);
    }
}
