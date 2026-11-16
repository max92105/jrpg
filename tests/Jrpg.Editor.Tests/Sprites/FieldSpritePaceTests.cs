using Jrpg.Core.Content;
using Jrpg.Data.Content;
using Jrpg.Editor.Catalogs;
using Jrpg.Editor.Sprites;

namespace Jrpg.Editor.Tests.Sprites;

// The pace line beside the Frame time box (decision 774), on a new setup over an image size given here.
public sealed class FieldSpritePaceTests
{
    private static readonly ContentCatalog _catalog =
        ContentCatalogLoader.Load(Path.Combine(AppContext.BaseDirectory, "test-world"));

    private readonly FieldSpriteSetupViewModel _setup =
        FieldSpriteSetupViewModel.ForNew("paced.png", (128, 128), new CatalogDraft(_catalog));

    [Fact]
    public void PaceText_150Milliseconds_ReadsSixPointSevenFramesASecond()
    {
        _setup.Walk.FrameMilliseconds = 150;

        Assert.Equal("150 ms a frame, 6.7 frames a second", _setup.PaceText);
    }

    [Fact]
    public void PaceText_FrameTimeZero_IsEmpty()
    {
        _setup.Walk.FrameMilliseconds = 0;

        Assert.Equal("", _setup.PaceText);
    }

    // The line follows the box as typed.
    [Fact]
    public void PaceText_FrameTimeTyped_IsRaised()
    {
        var raised = new List<string?>();
        _setup.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _setup.Walk.FrameMilliseconds = 250;

        Assert.Contains(nameof(FieldSpriteSetupViewModel.PaceText), raised);
    }
}
