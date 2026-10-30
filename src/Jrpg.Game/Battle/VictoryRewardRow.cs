namespace Jrpg.Game.Battle;

/// <summary>
/// One pointed reward bar on the victory screen: "EXP 120", or the gold with its coin (decision 103).
/// </summary>
/// <param name="LabelId">The label's message id.</param>
/// <param name="Amount">How much the battle gave.</param>
/// <param name="IsGold">Whether the amount is gold, drawn after the coin (decision 380).</param>
internal readonly record struct VictoryRewardRow(string LabelId, int Amount, bool IsGold);
