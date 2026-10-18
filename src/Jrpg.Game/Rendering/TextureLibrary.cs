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

    public void Dispose()
    {
        foreach (var texture in _textures.Values)
        {
            texture.Dispose();
        }

        _textures.Clear();
    }
}
