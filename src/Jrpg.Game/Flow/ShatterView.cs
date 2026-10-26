using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Flow;

/// <summary>
/// Draws a <see cref="ShatterLayout"/> as textured triangles cut from the frozen frame (decision 258), each pushed a
/// few pixels away from the impact so dark cracks open between them, with a bright hairline along every edge. A
/// sprite batch can only draw rectangles, so the triangles go through a <see cref="BasicEffect"/>: the batch ends
/// before them and begins again after.
/// </summary>
internal sealed class ShatterView
{
    // Placeholder look: each shard 3 px out from the impact; hairlines white at 55 %.
    private const float CrackGap = 3;
    private static readonly Color _hairline = Color.White * 0.55f;
    private static readonly Vector2 _screenSize = new(VirtualScreen.Width, VirtualScreen.Height);

    private VertexPositionTexture[] _vertices = [];
    private Edge[] _edges = [];

    // Made on the first draw, which has the device; it lives as long as the flow, which lives as long as the game.
    private BasicEffect? _effect;

    /// <summary>Builds the triangles and hairlines once per layout, so drawing allocates nothing.</summary>
    public void Show(ShatterLayout layout)
    {
        _vertices = new VertexPositionTexture[layout.Shards.Count * 3];
        _edges = new Edge[layout.Shards.Count * 3];
        for (var index = 0; index < layout.Shards.Count; index++)
        {
            var shard = layout.Shards[index];
            var offset = Vector2.Normalize(shard.Center - layout.Impact) * CrackGap;
            Vector2[] corners = [shard.A, shard.B, shard.C];
            for (var corner = 0; corner < 3; corner++)
            {
                var from = corners[corner] + offset;
                var to = corners[(corner + 1) % 3] + offset;
                _vertices[(index * 3) + corner] = new VertexPositionTexture(new Vector3(from, 0), corners[corner] / _screenSize);
                _edges[(index * 3) + corner] = new Edge(from, MathF.Atan2(to.Y - from.Y, to.X - from.X), Vector2.Distance(from, to));
            }
        }
    }

    /// <param name="batch">Begun with <see cref="VirtualScreen.BeginBatch"/>; ended, then begun again with it.</param>
    /// <param name="frozenFrame">The field's last frame, which the shards show.</param>
    /// <param name="pixel">The plain pixel the hairlines are drawn with.</param>
    public void Draw(SpriteBatch batch, Texture2D frozenFrame, Texture2D pixel)
    {
        batch.End();
        DrawTriangles(batch.GraphicsDevice, frozenFrame);
        VirtualScreen.BeginBatch(batch);
        foreach (var edge in _edges)
        {
            batch.Draw(pixel, edge.Start, null, _hairline, edge.Angle, Vector2.Zero, new Vector2(edge.Length, 1), SpriteEffects.None, 0);
        }
    }

    // Screen pixels map one to one onto the virtual screen; the triangles wind either way, so none is culled.
    private void DrawTriangles(GraphicsDevice device, Texture2D frozenFrame)
    {
        _effect ??= new BasicEffect(device)
        {
            TextureEnabled = true,
            Projection = Matrix.CreateOrthographicOffCenter(0, VirtualScreen.Width, VirtualScreen.Height, 0, 0, 1),
        };
        _effect.Texture = frozenFrame;
        device.BlendState = BlendState.Opaque;
        device.DepthStencilState = DepthStencilState.None;
        device.RasterizerState = RasterizerState.CullNone;
        device.SamplerStates[0] = SamplerState.PointClamp;
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length / 3);
        }
    }

    // One hairline: where it starts, its direction in radians, and its length in pixels.
    private readonly record struct Edge(Vector2 Start, float Angle, float Length);
}
