using Jrpg.Core.Statuses;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>One status's mark as shown: its letter, its turns left, and its pop and fade clocks.</summary>
/// <param name="kind">The status it shows.</param>
/// <param name="letter">The status name's first letter, measured once.</param>
internal sealed class StatusMark(StatusKind kind, MeasuredText letter)
{
    public StatusKind Kind => kind;

    public MeasuredText Letter => letter;

    /// <summary>The turns left as text, formatted again only when they change.</summary>
    public MeasuredText Turns { get; set; } = new(string.Empty, 0);

    public int TurnsLeft { get; set; }

    public TimeSpan SinceShown { get; set; }

    /// <summary>Set once the status is gone; the mark fades from then.</summary>
    public TimeSpan? SinceEnded { get; set; }
}
