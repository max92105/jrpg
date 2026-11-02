namespace Jrpg.Game.Battle;

/// <summary>One sound of a turn.</summary>
/// <param name="File">Its file in the effects folder.</param>
/// <param name="At">When it plays, from the playback's start.</param>
internal readonly record struct TimedSound(string File, TimeSpan At);
