using Jrpg.Core.Maps;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Rendering;

/// <summary>
/// Every object's baked canopy, made once for the whole game. Baking needs the graphics device, which exists only once
/// drawing starts, so the first canopy drawn bakes them all.
/// </summary>
/// <param name="objects">Every object definition.</param>
/// <param name="atlas">Where each tile's picture is.</param>
/// <param name="cellPixels">Screen pixels per map cell, the size canopies are baked at.</param>
internal sealed class CanopyTextures(IReadOnlyDictionary<string, ObjectDefinition> objects, TileAtlas atlas, int cellPixels)
{
    private IReadOnlyDictionary<string, Texture2D>? _canopies;

    /// <param name="objectId">An object with above-player zones.</param>
    /// <param name="textures">
    /// Supplies the tilesheets and keeps the baked textures; only the first call's library is used, since the game has one.
    /// </param>
    public Texture2D CanopyOf(string objectId, TextureLibrary textures)
    {
        _canopies ??= ObjectSpriteBaker.BakeCanopies(objects, atlas, textures, cellPixels);
        return _canopies[objectId];
    }
}
