using Jrpg.Core.Battle;

namespace Jrpg.Game.Battle;

/// <summary>
/// Who the turn order bar shows and who is lit: the acting combatant, then the next turns of the cycle (developer
/// decision 104). Refreshed at each turn start, never per frame.
/// </summary>
internal sealed class TurnOrderStrip
{
    /// <summary>How many turns follow the acting combatant on the bar.</summary>
    public const int UpcomingCount = 8;

    private IReadOnlyCollection<Combatant> _highlighted = [];
    private bool[] _isExtra = [];

    /// <summary>The acting combatant first, then the next <see cref="UpcomingCount"/> turns; empty until the first refresh.</summary>
    public IReadOnlyList<Combatant> Shown { get; private set; } = [];

    /// <summary>Reads the order as it stands now.</summary>
    public void Refresh(BattleState battle)
    {
        var upcoming = battle.UpcomingTurns(UpcomingCount);
        Shown = [battle.CurrentActor, .. upcoming.Select(turn => turn.Actor)];
        _isExtra = [false, .. upcoming.Select(turn => turn.IsExtra)];
    }

    /// <summary>Whether the turn shown at <paramref name="index"/> is a readied extra one; never the first.</summary>
    public bool IsExtraAt(int index) => _isExtra[index];

    /// <summary>Lights only <paramref name="targets"/>, while one is being chosen; empty lights everyone.</summary>
    public void Highlight(IReadOnlyCollection<Combatant> targets) => _highlighted = targets;

    public bool IsLit(Combatant combatant) => _highlighted.Count == 0 || _highlighted.Contains(combatant);
}
