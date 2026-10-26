using Jrpg.Core.Content;
using Jrpg.Core.Session;
using Jrpg.Game.Flow;
using Jrpg.Game.Hosting;
using Jrpg.Game.Input;
using Jrpg.Game.Saving;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Input;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Flow;

/// <summary>Ways into a game for the flow's tests: through the title and the leader choice, to the map.</summary>
internal static class FlowStarts
{
    /// <summary>The flow's screen-change fade out, before 0.15 s on black and 0.5 s back in.</summary>
    public static readonly TimeSpan FadeOut = TimeSpan.FromSeconds(0.5);

    /// <summary>The whole screen-change fade: 0.5 s out, 0.15 s on black, 0.5 s in.</summary>
    public static readonly TimeSpan WholeFade = TimeSpan.FromSeconds(1.15);

    /// <summary>One update at the host's fixed step.</summary>
    public static readonly TimeSpan OneUpdate = TimeSpan.FromSeconds(1.0 / 60);

    // Sarxian is the only leader offered, so the forest tests fight with the party they count on, whatever order the
    // content lists the choices in; the game starts where the intro ends, unless a test brings its own catalog. The
    // fade to the map starts on the update that confirms him.
    public static GameFlow FlowWithLeaderConfirmed(SaveSlots? slots = null, ContentCatalog? catalog = null)
    {
        var content = catalog ?? StartMapScenarios.AfterTheIntro;
        var ledBySarxian = content with { NewGame = content.NewGame with { LeaderChoices = [Choice("sarxian")] } };
        var flow = new GameFlow(ledBySarxian, RepositoryScreenText.Text, slots ?? UnwrittenSaveSlots.Create(), new PlayerSettings());
        ConfirmNewGameAndLeader(flow);
        return flow;
    }

    // New Game is the title's first entry with no save, so Confirm alone picks it; the choice takes input once the fade
    // is over, and Confirm takes the leader the cursor starts on, answering Yes to "Begin as X?".
    public static void ConfirmNewGameAndLeader(GameFlow flow)
    {
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        flow.Update(WholeFade, default);
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
        AnswerYes(flow);
    }

    // "Begin as X?" starts on No, so a stray Confirm changes nothing: Left moves to Yes.
    public static void AnswerYes(GameFlow flow)
    {
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Left));
        flow.Update(OneUpdate, InputFrames.Pressing(GameButton.Confirm));
    }

    public static LeaderChoice Choice(string memberId) => new() { MemberId = memberId };

    // A new game whose fade is over, so the map gets input.
    public static GameFlow StartedFlow(SaveSlots? slots = null, ContentCatalog? catalog = null)
    {
        var flow = FlowWithLeaderConfirmed(slots, catalog);
        flow.Update(FadeOut, default);
        flow.Update(WholeFade - FadeOut, default);
        return flow;
    }

    public static GameSession SessionOf(GameFlow flow) =>
        flow.Session ?? throw new InvalidOperationException("The flow has no session yet.");
}
