using Jrpg.Core.Exploration;
using Jrpg.Game.Input;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Exploration;

/// <summary>
/// Plays one rest step: a free rest starts its beat on the step's first update, the fade to black, a moment of black
/// and the fade back in (decision 257); an inn's stay asks first, its price under the question (decisions 380, 382),
/// and Yes pays through Core before the beat, or shows the refusal line without it when the gold is short, while No
/// declines. The step's line may show once the screen is back, and reading it finishes the playback so the next rest
/// starts its own.
/// </summary>
/// <param name="messages">The question's and the lines' text.</param>
internal sealed class RestPlayback(UiMessages messages)
{
    // Out, a second of black, back in (placeholder timing).
    private readonly ScreenFade _fade = new(TimeSpan.FromSeconds(0.5), TimeSpan.FromSeconds(1.0), TimeSpan.FromSeconds(0.5));

    // The stay asked about and its question, while it is open.
    private (InnStay Stay, ConfirmPrompt Question)? _asking;

    /// <summary>
    /// Whether the current rest has started, so its question's and its line's updates don't start it again.
    /// </summary>
    public bool IsStarted { get; private set; }

    /// <summary>The inn's question while it is open; null for a free rest and once answered.</summary>
    public ConfirmPrompt? Question => _asking?.Question;

    /// <summary>Whether the player said No: nothing to show, the event goes on unrested.</summary>
    public bool IsDeclined { get; private set; }

    /// <summary>The line the step's page says once the beat is over: the party rests, or the inn's refusal.</summary>
    public string LineId { get; private set; } = MessageIds.PartyRests;

    /// <summary>Whether the screen is still going black or coming back; the line waits for it.</summary>
    public bool IsFading => _fade.Phase != FadePhase.Idle;

    /// <summary>How black to draw over the map, 0..1.</summary>
    public float Blackness => _fade.Blackness;

    /// <summary>
    /// Starts the rest: a free one (<paramref name="stay"/> null) fades at once; an inn's asks first.
    /// </summary>
    public void Start(InnStay? stay)
    {
        IsStarted = true;
        IsDeclined = false;
        LineId = MessageIds.PartyRests;
        if (stay is null)
        {
            _fade.Start();
            return;
        }

        _asking = (stay, new ConfirmPrompt(messages.Text(MessageIds.InnQuestion)) { Price = stay.Price });
    }

    /// <summary>The open question's input; Yes pays and fades, or refuses; No declines.</summary>
    public void Answer(InputFrame input)
    {
        var (stay, question) = _asking ?? throw new InvalidOperationException("No question is open.");
        if (question.Update(input) is not { } answer)
        {
            return;
        }

        _asking = null;
        if (answer == ConfirmAnswer.No)
        {
            IsDeclined = true;
        }
        else if (stay.TryPay())
        {
            _fade.Start();
        }
        else
        {
            LineId = MessageIds.InnShortOfGold;
        }
    }

    public void Advance(TimeSpan delta) => _fade.Update(delta);

    /// <summary>The rest's line was read, or the stay declined: the next rest starts its own.</summary>
    public void Finish() => IsStarted = false;
}
