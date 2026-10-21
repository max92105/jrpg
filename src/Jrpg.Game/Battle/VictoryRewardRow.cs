namespace Jrpg.Game.Battle;

/// <summary>One pointed reward bar on the victory screen, such as "EXP 120"; gold and drops add rows later (decision 103).</summary>
/// <param name="LabelId">The label's message id.</param>
/// <param name="Amount">How much the battle gave.</param>
internal readonly record struct VictoryRewardRow(string LabelId, int Amount);
