using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Flow;

/// <summary>
/// Draws a <see cref="ShatterLayout"/>: each shard is its piece of the frozen frame, pushed a few pixels away from the
/// impact so the cracks show, with a bright hairline along its edges (decision 258). Shards are textured triangles,
/// which the sprite batch can't draw, so the view ends the batch, draws them with its own effect, and begins it again.
/// </summary>
internal sealed class ShatterView : IDisposable
{
    // Each shard sits 4 px out from the impact; its edges are white at 60 % (placeholders).
    private const float CrackWidth = 4;
    private const float EdgeOpacity = 0.6f;

    private readonly BasicEffect _textured;
    private readonly BasicEffect _edges;
    private VertexPositionTexture[] _shards = [];
    private VertexPositionColor[] _edgeLines = [];
    private ShatterLayout? _shown;

    /// <param name="device">The device the virtual screen is drawn on.</param>
    public ShatterView(GraphicsDevice device)
    {
        var projection = Matrix.CreateOrthographicOffCenter(0, VirtualScreen.Width, VirtualScreen.Height, 0, 0, 1);
        _textured = new BasicEffect(device) { TextureEnabled = true, Projection = projection };
        _edges = new BasicEffect(device) { VertexColorEnabled = true, Projection = projection };
    }

    /// <summary>Whether <paramref name="layout"/>'s vertices are the ones built.</summary>
    public bool IsBuiltFor(ShatterLayout layout) => ReferenceEquals(_shown, layout);

    /// <summary>Builds the shards' vertices once, when the frame breaks.</summary>
    /// <param name="layout">The shards, in virtual-screen pixels.</param>
    /// <param name="impact">Where the glass broke from; each shard moves away from it.</param>
    public void BuildVertices(ShatterLayout layout, Vector2 impact)
    {
        _shown = layout;
        var size = new Vector2(VirtualScreen.Width, VirtualScreen.Height);
        var edgeColor = Color.White * EdgeOpacity;
        _shards = new VertexPositionTexture[layout.Shards.Count * 3];
        _edgeLines = new VertexPositionColor[layout.Shards.Count * 6];
        for (var index = 0; index < layout.Shards.Count; index++)
        {
            var shard = layout.Shards[index];
            var push = Vector2.Normalize(shard.Center - impact) * CrackWidth;
            Vector2[] corners = [shard.A, shard.B, shard.C];
            for (var corner = 0; corner < 3; corner++)
            {
                var placed = new Vector3(corners[corner] + push, 0);
                _shards[(index * 3) + corner] = new VertexPositionTexture(placed, corners[corner] / size);
                var following = new Vector3(corners[(corner + 1) % 3] + push, 0);
                _edgeLines[(index * 6) + (corner * 2)] = new VertexPositionColor(placed, edgeColor);
                _edgeLines[(index * 6) + (corner * 2) + 1] = new VertexPositionColor(following, edgeColor);
            }
        }
    }

    /// <param name="batch">Begun by the host with point sampling; it is ended and begun again the same way.</param>
    /// <param name="frame">The frozen frame the shards are cut from.</param>
    public void Draw(SpriteBatch batch, Texture2D frame)
    {
        var device = batch.GraphicsDevice;
        batch.End();
        device.SamplerStates[0] = SamplerState.PointClamp;
        device.RasterizerState = RasterizerState.CullNone;
        _textured.Texture = frame;
        _textured.CurrentTechnique.Passes[0].Apply();
        device.DrawUserPrimitives(PrimitiveType.TriangleList, _shards, 0, _shards.Length / 3);
        _edges.CurrentTechnique.Passes[0].Apply();
        device.DrawUserPrimitives(PrimitiveType.LineList, _edgeLines, 0, _edgeLines.Length / 2);
        batch.Begin(samplerState: SamplerState.PointClamp);
    }

    public void Dispose()
    {
        _textured.Dispose();
        _edges.Dispose();
    }
}
