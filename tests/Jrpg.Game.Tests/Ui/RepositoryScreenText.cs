using Jrpg.Data.Messages;
using Jrpg.Game.Rendering;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Ui;

/// <summary>The repository's message table and fonts, loaded once for every test that builds a scene.</summary>
internal static class RepositoryScreenText
{
    private static readonly string _contentRoot = Path.Combine(AppContext.BaseDirectory, "content");

    // Never disposed: the fonts live as long as the test run, like the game's live as long as the game.
    public static ScreenText Text { get; } = new(new UiMessages(UiMessageLoader.Load(_contentRoot)), FontLibrary.Load(_contentRoot));
}
