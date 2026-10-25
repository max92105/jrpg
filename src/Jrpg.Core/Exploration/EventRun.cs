using Jrpg.Core.Events;
using Jrpg.Core.Party;

namespace Jrpg.Core.Exploration;

/// <summary>
/// One running map event, as a cursor over its steps (ADR 0006). Steps the player doesn't need to see apply at once;
/// dialogue, a received item, a recruit and a warp stay <see cref="Current"/> until the Game calls
/// <see cref="Continue"/>.
/// </summary>
internal sealed class EventRun
{
    private readonly TriggeredEvent _triggered;
    private readonly ActiveMap _map;
    private readonly ExplorationContext _context;
    private readonly PartyState _party;
    private int _index = -1;

    /// <summary>Starts the event: marks a "once" event done first, then runs up to the first step that waits.</summary>
    public EventRun(TriggeredEvent triggered, ActiveMap map, ExplorationContext context, PartyState party)
    {
        _triggered = triggered;
        _map = map;
        _context = context;
        _party = party;

        // Marked before any step, so a warp in the middle can't make it run twice.
        if (triggered.Event.IsOnce)
        {
            map.MarkDone(triggered.Event);
        }

        AdvanceToNextWaitingStep();
    }

    private IReadOnlyList<EventStep> Steps => _triggered.Event.Steps;

    /// <summary>The step waiting for the player or the Game, or null once the event is over.</summary>
    public EventStep? Current => IsFinished ? null : Steps[_index];

    /// <summary>What the current give-item step added and lost; null on any other step.</summary>
    public ItemReceipt? CurrentReceipt { get; private set; }

    /// <summary>What the current recruit step did; null on any other step.</summary>
    public Recruitment? CurrentRecruitment { get; private set; }

    /// <summary>
    /// The voice-over this event started, which speaks on over the steps after it; null before one starts and once the
    /// player skips, since skipping silences the cutscene.
    /// </summary>
    public VoiceStep? StartedVoice { get; private set; }

    public bool IsFinished => _index >= Steps.Count;

    /// <summary>The player read the current step, or the Game finished its warp fade-out.</summary>
    /// <exception cref="InvalidOperationException">The event is already over.</exception>
    public void Continue()
    {
        if (IsFinished)
        {
            throw new InvalidOperationException($"Event '{_triggered.Event.Id}' is already over.");
        }

        AdvanceToNextWaitingStep();
    }

    /// <summary>
    /// Runs on past the presentation steps from the current one (ADR 0016), applying any step on the way that changes
    /// the game, and stops on the next step that waits for the player or the Game, or at the end.
    /// </summary>
    public void SkipPresentation()
    {
        while (!IsFinished && Steps[_index].IsPresentation)
        {
            AdvanceToNextWaitingStep();
        }

        StartedVoice = null;
    }

    private void AdvanceToNextWaitingStep()
    {
        CurrentReceipt = null;
        CurrentRecruitment = null;
        for (_index++; _index < Steps.Count; _index++)
        {
            if (ApplyAndCheckWait(Steps[_index]))
            {
                return;
            }
        }
    }

    private bool ApplyAndCheckWait(EventStep step)
    {
        switch (step)
        {
            case SwapObjectStep swap:
                // Content validation only allows swaps in object events (slice 3b).
                var source = _triggered.Source
                    ?? throw new InvalidOperationException($"Event '{_triggered.Event.Id}' swaps an object but has none.");
                _map.SwapObject(source, swap.ObjectId);
                return false;
            case VoiceStep voice:
                StartedVoice = voice;
                return false;
            case GiveItemStep give:
                var item = _context.Catalog.Items[give.ItemId];
                var lost = _context.Inventory.Add(item, give.Quantity);
                CurrentReceipt = new ItemReceipt(item, give.Quantity - lost, lost);
                return true;
            case RecruitStep recruit:
                CurrentRecruitment = Recruit(recruit.MemberId);
                return true;
            case RestStep:
                foreach (var member in _party.Members)
                {
                    member.Rest();
                }

                return true;
            case DialogueStep or WarpStep or StillStep:
                return true;
            default:
                throw new InvalidOperationException($"Step {step.GetType().Name} has no rule in EventRun.");
        }
    }

    private Recruitment Recruit(string memberId)
    {
        if (_party.IsRecruited(memberId))
        {
            return new Recruitment(_party.MemberById(memberId), HasJoined: false);
        }

        var catalog = _context.Catalog;
        var definition = catalog.PartyMembers[memberId];
        var member = PartyMember.Recruit(definition, catalog.ClassOf(definition), catalog.Skills);
        _party.Recruit(member);
        return new Recruitment(member, HasJoined: true);
    }
}
