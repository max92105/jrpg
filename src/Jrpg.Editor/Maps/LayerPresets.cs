using Jrpg.Core.Maps;

namespace Jrpg.Editor.Maps;

/// <summary>
/// The layers offered for painting, from the legacy editor; any other Z can still be typed in (question 71). Below
/// <see cref="MapLayers.PlayerZ"/> draws under the player, at it depth-sorts with the player, and above it draws over.
/// </summary>
internal static class LayerPresets
{
    public const int GroundZ = -5;

    public static IReadOnlyList<LayerPreset> All { get; } =
    [
        new("Ground", GroundZ),
        new("Ground detail", -4),
        new("Decoration (below player)", -3),
        new("Same as player (Y-sorted)", MapLayers.PlayerZ),
        new("Above player (roofs, treetops)", MapLayers.AbovePlayerZ),
    ];

    /// <returns>The preset's name for <paramref name="z"/>, or "Layer" for a Z no preset uses.</returns>
    public static string NameOf(int z) => All.FirstOrDefault(preset => preset.Z == z)?.Name ?? "Layer";
}
