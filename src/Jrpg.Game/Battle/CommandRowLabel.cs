namespace Jrpg.Game.Battle;

/// <summary>What one row of the command panel shows: a command or a skill.</summary>
/// <param name="IconFile">The icon in <c>icons/</c>; empty for a skill without one.</param>
/// <param name="Label">The command's or the skill's name.</param>
/// <param name="Opacity">1, or less when greyed.</param>
internal readonly record struct CommandRowLabel(string IconFile, string Label, float Opacity);
