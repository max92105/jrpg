namespace Jrpg.Game.Exploration;

/// <summary>What a <see cref="Drawable"/> on the map stands for.</summary>
internal enum DrawableKind
{
    Layer,
    Object,

    /// <summary>The part of an object drawn over the player (its above-player zones).</summary>
    Canopy,
    Player,
}
