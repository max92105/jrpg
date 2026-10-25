using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>How a battle panel's HP row looks.</summary>
/// <param name="Label">The small "HP" font.</param>
/// <param name="Value">The large current value's font.</param>
/// <param name="Max">The small "/max" font.</param>
/// <param name="Width">The row's width, which its bar spans.</param>
/// <param name="SmallTextLift">How far the label and the maximum sit above the value's bottom, in pixels.</param>
internal sealed record VitalRowStyle(OutlinedFont Label, OutlinedFont Value, OutlinedFont Max, int Width, int SmallTextLift);
