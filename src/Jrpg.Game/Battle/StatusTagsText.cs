using System.Globalization;
using Jrpg.Core.Statuses;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>
/// One combatant's statuses as a plain line of short tags, each its name in capitals and the turns it has left
/// ("STUN 1"), in the order they were applied. It is formatted again only when a kind or a count changed, so refreshing
/// every update allocates nothing while nothing changes. LK2 replaces the line with marks.
/// </summary>
internal sealed class StatusTagsText(StatusSet statuses, UiMessages messages)
{
    private readonly List<(StatusKind Kind, int TurnsLeft)> _shown = [];

    /// <summary>Empty when the combatant has no status.</summary>
    public string Line { get; private set; } = string.Empty;

    /// <summary>The message id of a status's name (ADR 0010).</summary>
    public static string NameIdOf(StatusKind kind) => kind switch
    {
        StatusKind.Stun => MessageIds.StatusStun,
        StatusKind.Taunt => MessageIds.StatusTaunt,
        StatusKind.Guarded => MessageIds.StatusGuarded,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A status with no name in ui.json."),
    };

    public void Refresh()
    {
        if (IsShowing(statuses.Active))
        {
            return;
        }

        _shown.Clear();
        _shown.AddRange(statuses.Active.Select(status => (status.Kind, status.TurnsLeft)));
        Line = string.Join(' ', _shown.Select(TagOf));
    }

    private bool IsShowing(IReadOnlyList<ActiveStatus> active)
    {
        if (active.Count != _shown.Count)
        {
            return false;
        }

        for (var index = 0; index < active.Count; index++)
        {
            if (active[index].Kind != _shown[index].Kind || active[index].TurnsLeft != _shown[index].TurnsLeft)
            {
                return false;
            }
        }

        return true;
    }

    private string TagOf((StatusKind Kind, int TurnsLeft) status) => messages.Format(
        MessageIds.BattleStatusTag,
        new MessageArgument("status", messages.Text(NameIdOf(status.Kind)).ToUpper(CultureInfo.InvariantCulture)),
        MessageArgument.Number("turns", status.TurnsLeft));
}
