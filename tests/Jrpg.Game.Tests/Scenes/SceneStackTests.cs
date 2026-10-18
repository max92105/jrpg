using Jrpg.Game.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Tests.Scenes;

public sealed class SceneStackTests
{
    // Draw needs a SpriteBatch only to pass it on; the recording scenes never use it, so no graphics device is needed.
    private static readonly SpriteBatch _noBatch = null!;

    private readonly List<string> _calls = [];

    [Fact]
    public void Update_OnlyTopSceneUpdates()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("menu", _calls));

        stack.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal(["update menu"], _calls);
    }

    [Fact]
    public void Draw_DrawsBottomToTop()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("menu", _calls));

        stack.Draw(_noBatch);

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
        stack.Update(TimeSpan.FromSeconds(0.1));

        Assert.Equal(["update map"], _calls);
    }

    [Fact]
    public void Replace_TopScene_DrawsTheNewOneInItsPlace()
    {
        var stack = new SceneStack();
        stack.Push(new RecordingScene("map", _calls));
        stack.Push(new RecordingScene("battle", _calls));

        stack.Replace(new RecordingScene("victory", _calls));
        stack.Draw(_noBatch);

        Assert.Equal(["draw map", "draw victory"], _calls);
    }

    [Fact]
    public void Replace_EmptyStack_Throws()
    {
        var stack = new SceneStack();

        Assert.Throws<InvalidOperationException>(() => stack.Replace(new RecordingScene("map", _calls)));
    }

    private sealed class RecordingScene(string name, List<string> calls) : IScene
    {
        public void Update(TimeSpan delta) => calls.Add($"update {name}");

        public void Draw(SpriteBatch batch) => calls.Add($"draw {name}");
    }
}
