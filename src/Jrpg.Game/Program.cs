using Jrpg.Data.Art;
using Jrpg.Data.Audio;
using Jrpg.Data.Content;
using Jrpg.Data.Messages;
using Jrpg.Data.Saving;
using Jrpg.Game;
using Jrpg.Game.Audio;
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
    DeveloperLog.Report($"The game can't start: {exception.Message}");
    return 1;
}

using (fonts)
{
    // One settings object: the host applies the window mode, the menu edits it, dialogue reads the text speed.
    var settings = new PlayerSettings();
    var (rootScene, voice) = FirstScene(contentRoot, new ScreenText(messages, fonts), settings);
    using var game = new JrpgGame(rootScene, contentRoot, settings, voice);
    game.Run();
}

return 0;

// A new game, or the reason it can't start, shown on screen as well as logged; each with the channel the host plays.
static (IScene Scene, VoiceChannel Voice) FirstScene(string contentRoot, ScreenText text, PlayerSettings settings)
{
    string reason;
    try
    {
        var catalog = ContentCatalogLoader.Load(contentRoot);
        var problems = ArtReferenceCheck.FindProblems(catalog, ArtFolders.RootIn(contentRoot))
            .Concat(AudioReferenceCheck.FindProblems(catalog, AudioFolders.RootIn(contentRoot)))
            .ToList();
        if (problems.Count == 0)
        {
            // Saves are stamped with the real clock here, the one place the game reads it (ADR 0008).
            var saveSlots = new SaveSlots(new FileSaveStore(GamePaths.SaveDirectory, catalog), catalog, () => DateTimeOffset.Now);
            var flow = new GameFlow(catalog, text, saveSlots, settings);
            return (flow, flow.Voice);
        }

        reason = string.Join(Environment.NewLine, problems);
    }
    catch (Exception exception) when (IsStartupProblem(exception))
    {
        reason = exception.Message;
    }

    var message = text.Messages.Format(MessageIds.StartupFailed, new MessageArgument("reason", reason));
    DeveloperLog.Report(message);
    return (new StartupErrorScene(message, text.Fonts), new VoiceChannel());
}

static bool IsStartupProblem(Exception exception) =>
    exception is ContentValidationException or IOException or UnauthorizedAccessException or ArgumentException;
