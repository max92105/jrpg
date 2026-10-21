namespace Jrpg.Game.Battle;

/// <summary>One timed step of a turn's playback; the screen holds for the beats' total duration.</summary>
internal sealed record PlaybackBeat(PlaybackBeatKind Kind, TimeSpan Duration);
