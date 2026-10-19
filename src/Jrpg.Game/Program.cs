using System.Diagnostics;
using Jrpg.Data.Art;
using Jrpg.Data.Content;
using Jrpg.Data.Messages;
using Jrpg.Game;
using Jrpg.Game.Flow;
using Jrpg.Game.Hosting;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;

string contentRoot;
UiMessages messages;
FontLibrary fonts;
try
{
    contentRoot = ContentRootLocator.Find(args, typeof(JrpgGame).Assembly);
    messages = new UiMessages(UiMessageLoader.Load(contentRoot));
    fonts = FontLibrary.Load(contentRoot);
}
catch (Exception exception) when (IsStartupProblem(exception))
{
    // Without the message table and a font nothing can be drawn, so the reason goes only where a developer sees it.
    Report($"The game can't start: {exception.Message}");
    return 1;
}

var artRoot = ArtFolders.RootIn(contentRoot);
using (fonts)
{
    using var game = new JrpgGame(FirstScene(contentRoot, artRoot, messages, fonts), artRoot, new PlayerSettings());
    game.Run();
}

return 0;

// A new game, or the reason it can't start, shown on screen as well as logged.
static IScene FirstScene(string contentRoot, string artRoot, UiMessages messages, FontLibrary fonts)
{
    string reason;
    try
    {
        var catalog = ContentCatalogLoader.Load(contentRoot);
        var artProblems = ArtReferenceCheck.FindProblems(catalog, artRoot);
        if (artProblems.Count == 0)
        {
            return new GameFlow(catalog, new ScreenText(messages, fonts));
        }

        reason = string.Join(Environment.NewLine, artProblems);
    }
    catch (Exception exception) when (IsStartupProblem(exception))
    {
        reason = exception.Message;
    }

    var message = messages.Format(MessageIds.StartupFailed, new MessageArgument("reason", reason));
    Report(message);
    return new StartupErrorScene(message, fonts);
}

static bool IsStartupProblem(Exception exception) =>
    exception is ContentValidationException or IOException or UnauthorizedAccessException or ArgumentException;

static void Report(string message)
{
    Console.Error.WriteLine(message);
    Debug.WriteLine(message);
}
