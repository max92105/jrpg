namespace Jrpg.Core.Maps;

/// <summary>The draw layers (Z) with a meaning of their own; lower Z draws first.</summary>
public static class MapLayers
{
    /// <summary>The player's layer: tiles and objects here are depth-sorted with the player by their base.</summary>
    public const int PlayerZ = 1;

    /// <summary>Always drawn over the player, such as roofs and canopies.</summary>
    public const int AbovePlayerZ = 5;

    /// <summary>
    /// The layer an object's above-player zones draw on: over the player, or the object's own layer if that's higher
    /// still, so a canopy never ends up under its own trunk (legacy <c>PlacedObject.AbovePlayerZoneZ</c>).
    /// </summary>
    public static int CanopyZOf(int objectZ) => Math.Max(AbovePlayerZ, objectZ);
}
