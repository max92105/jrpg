namespace Jrpg.Game.Battle;

/// <summary>A member's rows in their battle panel: HP, then one per class resource in the class's order.</summary>
internal sealed record PartyStatusRows(BattleVitalRow Hp, IReadOnlyList<BattleVitalRow> Resources);
