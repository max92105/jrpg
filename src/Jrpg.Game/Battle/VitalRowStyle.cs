using Jrpg.Game.Ui;

namespace Jrpg.Game.Battle;

/// <summary>How one vital's row looks in a battle panel (legacy <c>CharacterPanel</c>'s HP or MP row).</summary>
/// <param name="Label">The small "HP" or "MP" font.</param>
/// <param name="Value">The large current value's font.</param>
/// <param name="Max">The small "/max" font.</param>
/// <param name="Width">The row's width, which its bar spans.</param>
/// <param name="SmallTextLift">How far the label and the maximum sit above the value's bottom, in pixels.</param>
internal sealed record VitalRowStyle(OutlinedFont Label, OutlinedFont Value, OutlinedFont Max, int Width, int SmallTextLift);
