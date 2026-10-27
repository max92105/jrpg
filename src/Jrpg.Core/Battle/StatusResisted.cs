using Jrpg.Core.Statuses;

namespace Jrpg.Core.Battle;

/// <summary>
/// A status that didn't land on <paramref name="Target"/>: its roll lost to their crowd control resistance.
/// </summary>
public sealed record StatusResisted(Combatant Target, StatusKind Status) : BattleEvent;
