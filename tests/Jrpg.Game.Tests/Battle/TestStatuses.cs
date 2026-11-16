using Jrpg.Core.Statuses;

namespace Jrpg.Game.Tests.Battle;

/// <summary>
/// The statuses the battle screen's tests put in events, in one place, so the status model can change under them
/// (decision 811): today <see cref="StatusKind"/> values; from the swap, records built here.
/// </summary>
internal static class TestStatuses
{
    public static StatusKind Stun => StatusKind.Stun;

    public static StatusKind Poison => StatusKind.Poison;

    public static StatusKind Regen => StatusKind.Regen;
}
