namespace Jrpg.Game.Battle;

/// <summary>What the battle screen calls for from the music.</summary>
internal enum BattleMusicCue
{
    /// <summary>The encounter's theme, while fighting.</summary>
    Theme,

    /// <summary>Nothing: under the fanfare, or after a defeat.</summary>
    Silence,

    /// <summary>The map's track, back again.</summary>
    Field,
}
