namespace Jrpg.Game.Battle;

/// <summary>A member's rows in their battle panel: HP, then their class resources in the class's order.</summary>
internal sealed record PartyStatusRows(BattleVitalRow Hp, ResourceGaugeView Resources);
