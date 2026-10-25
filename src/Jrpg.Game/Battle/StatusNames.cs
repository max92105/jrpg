using Jrpg.Core.Statuses;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>Each status's name in ui.json (ADR 0010): its cue when granted, and its mark's letter.</summary>
internal static class StatusNames
{
    public static string IdOf(StatusKind kind) => kind switch
    {
        StatusKind.Stun => MessageIds.StatusStun,
        StatusKind.Taunt => MessageIds.StatusTaunt,
        StatusKind.Guarded => MessageIds.StatusGuarded,
        StatusKind.Exposed => MessageIds.StatusExposed,
        StatusKind.Poison => MessageIds.StatusPoison,
        StatusKind.Regen => MessageIds.StatusRegen,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A status with no name in ui.json."),
    };
}
