namespace Jrpg.Core.Maps;

/// <summary>What a <see cref="MapDrawItem"/> stands for.</summary>
public enum MapDrawItemKind
{
    Layer,
    ObjectBody,

    /// <summary>The part of an object drawn over the player (its above-player zones).</summary>
    Canopy,
    Player,
}
