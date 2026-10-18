using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Rendering;

/// <summary>
/// The one place the game opens image files (ADR 0009, <c>game.md</c>): PNGs under the art root, loaded on first use,
/// kept for the session and disposed with it.
/// </summary>
internal sealed class TextureLibrary(GraphicsDevice device, string artRoot) : IDisposable
{
    // Keyed by folder and file rather than a combined path, so a lookup while drawing builds no string.
    private readonly Dictionary<(string Folder, string File), Texture2D> _textures = [];
    private readonly List<Texture2D> _created = [];

    /// <param name="folder">A folder from <c>ArtFolders</c>.</param>
    /// <param name="file">The file name content gives.</param>
    public Texture2D Get(string folder, string file)
    {
        if (!_textures.TryGetValue((folder, file), out var texture))
        {
            // Premultiplied, because SpriteBatch's default AlphaBlend expects it; straight alpha shows dark fringes.
            texture = Texture2D.FromFile(device, Path.Combine(artRoot, folder, file), DefaultColorProcessors.PremultiplyAlpha);
            _textures.Add((folder, file), texture);
        }

        return texture;
    }

    /// <summary>A texture the game composed itself, such as a baked canopy, kept and disposed with the loaded ones.</summary>
    /// <param name="width">Width in pixels.</param>
    /// <param name="height">Height in pixels.</param>
    /// <param name="premultipliedPixels">Row by row, with premultiplied alpha like the loaded textures.</param>
    public Texture2D Create(int width, int height, Color[] premultipliedPixels)
    {
        var texture = new Texture2D(device, width, height);
        texture.SetData(premultipliedPixels);
        _created.Add(texture);
        return texture;
    }

    public void Dispose()
    {
        foreach (var texture in _textures.Values.Concat(_created))
        {
            texture.Dispose();
        }

        _textures.Clear();
        _created.Clear();
    }
}
