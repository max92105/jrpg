using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>One action as the Forge shows it, formatted once when the actions open.</summary>
/// <param name="Action">Which action.</param>
/// <param name="Label">The action's name.</param>
/// <param name="Materials">What it gives or costs, measured for right-aligning: "2 Scrap, 1 Essence".</param>
/// <param name="Reason">Why Core refuses it, as the player reads it; null when it can be done (decision 370).</param>
internal sealed record ForgeActionRow(ForgeAction Action, string Label, MeasuredText Materials, string? Reason)
{
    public bool IsAvailable => Reason is null;
}
