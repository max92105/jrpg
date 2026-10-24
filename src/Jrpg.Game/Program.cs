using System.Diagnostics;
using Jrpg.Data.Art;
using Jrpg.Data.Audio;
using Jrpg.Data.Content;
using Jrpg.Data.Messages;
using Jrpg.Data.Saving;
using Jrpg.Game;
using Jrpg.Game.Flow;
using Jrpg.Game.Hosting;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
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
    // One settings object: the host applies the window mode, the menu edits it, dialogue reads the text speed.
    var settings = new PlayerSettings();
    using var game = new JrpgGame(FirstScene(contentRoot, artRoot, new ScreenText(messages, fonts), settings), artRoot, settings);
    game.Run();
}

return 0;

// A new game, or the reason it can't start, shown on screen as well as logged.
static IScene FirstScene(string contentRoot, string artRoot, ScreenText text, PlayerSettings settings)
{
    string reason;
    try
    {
        var catalog = ContentCatalogLoader.Load(contentRoot);
        var problems = ArtReferenceCheck.FindProblems(catalog, artRoot)
            .Concat(AudioReferenceCheck.FindProblems(catalog, AudioFolders.RootIn(contentRoot)))
            .ToList();
        if (problems.Count == 0)
        {
            // Saves are stamped with the real clock here, the one place the game reads it (ADR 0008).
            var saveSlots = new SaveSlots(new FileSaveStore(GamePaths.SaveDirectory, catalog), catalog, () => DateTimeOffset.Now);
            return new GameFlow(catalog, text, saveSlots, settings);
        }

        reason = string.Join(Environment.NewLine, problems);
    }
    catch (Exception exception) when (IsStartupProblem(exception))
    {
        reason = exception.Message;
    }

    var message = text.Messages.Format(MessageIds.StartupFailed, new MessageArgument("reason", reason));
    Report(message);
    return new StartupErrorScene(message, text.Fonts);
}

static bool IsStartupProblem(Exception exception) =>
    exception is ContentValidationException or IOException or UnauthorizedAccessException or ArgumentException;

static void Report(string message)
{
    Console.Error.WriteLine(message);
    Debug.WriteLine(message);
}
