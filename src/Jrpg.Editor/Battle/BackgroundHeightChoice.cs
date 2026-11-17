namespace Jrpg.Editor.Battle;

/// <summary>
/// One frame height the GIF dialog's background kind offers (decision 898), with its words: "443 px (the GIF's)" for
/// the GIF's own, "360 px" for a divisor of 1080.
/// </summary>
internal sealed record BackgroundHeightChoice(int Height, string Label);
