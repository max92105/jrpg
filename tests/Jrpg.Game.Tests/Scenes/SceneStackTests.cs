using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Tests.Scenes;

public sealed class SceneStackTests
{
    // Draw needs a SpriteBatch and textures only to pass them on; the recording scenes never use them, so no graphics device is needed.
    private static readonly SpriteBatch _noBatch = null!;
    private static readonly TextureLibrary _noTextures = null!;

    private readonly List<string> _calls = [];

    [Fact]
    public void Update_OnlyTopSceneUpdates()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("menu", _calls));

        stack.Update(TimeSpan.FromSeconds(0.1), default);

        Assert.Equal(["update menu"], _calls);
    }

    [Fact]
    public void Update_TopSceneAsks_ReturnsItsRequest()
    {
        var stack = new SceneStack();
        var newGame = new NewGameRequested();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("title", _calls, newGame));

        var request = stack.Update(TimeSpan.FromSeconds(0.1), default);

        Assert.Same(newGame, request);
    }

    [Fact]
    public void Update_EmptyStack_ReturnsNull()
    {
        var request = new SceneStack().Update(TimeSpan.FromSeconds(0.1), default);

        Assert.Null(request);
    }

    [Fact]
    public void Draw_DrawsBottomToTop()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("menu", _calls));

        stack.Draw(_noBatch, _noTextures);

        Assert.Equal(["draw map", "draw menu"], _calls);
    }

    [Fact]
    public void Pop_LastScene_Throws()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));

        Assert.Throws<InvalidOperationException>(stack.Pop);
    }

    [Fact]
    public void Pop_SceneOverAnother_UpdatesTheOneBelowAgain()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("menu", _calls));

        stack.Pop();
        stack.Update(TimeSpan.FromSeconds(0.1), default);

        Assert.Equal(["update map"], _calls);
    }

    [Fact]
    public void Replace_TopScene_DrawsTheNewOneInItsPlace()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("battle", _calls));

        stack.Replace(new RecordingScene("victory", _calls));
        stack.Draw(_noBatch, _noTextures);

        Assert.Equal(["draw map", "draw victory"], _calls);
    }

    [Fact]
    public void ReplaceAll_SeveralScenes_LeavesOnlyNewScene()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("menu", _calls));

        stack.ReplaceAll(new RecordingScene("title", _calls));
        stack.Draw(_noBatch, _noTextures);

        Assert.Equal(["draw title"], _calls);
    }

    [Fact]
    public void Replace_EmptyStack_Throws()
    {
        var stack = new SceneStack();

        Assert.Throws<InvalidOperationException>(() => stack.Replace(new RecordingScene("map", _calls)));
    }

    /// <param name="name">Names the scene in every recorded call.</param>
    /// <param name="calls">Where its updates and draws are recorded, in order.</param>
    /// <param name="request">What the scene asks for on every update.</param>
    private sealed class RecordingScene(string name, List<string> calls, SceneRequest? request = null) : IScene
    {
        public SceneRequest? Update(TimeSpan delta, InputFrame input)
        {
            calls.Add($"update {name}");
            return request;
        }

        public void Draw(SpriteBatch batch, TextureLibrary textures) => calls.Add($"draw {name}");
    }
}
