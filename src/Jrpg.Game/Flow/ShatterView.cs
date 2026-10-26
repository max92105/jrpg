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
    // Placeholder look: each shard 3 px out from the impact; hairlines white at 55 %; a flown shard 25 % smaller.
    private const float CrackGap = 3;
    private const float ShrinkMax = 0.25f;
    private static readonly Color _hairline = Color.White * 0.55f;
    private static readonly Vector2 _screenSize = new(VirtualScreen.Width, VirtualScreen.Height);

    private IReadOnlyList<Shard> _shards = [];
    private Vector2[] _restingCenters = [];
    private VertexPositionTexture[] _vertices = [];
    private float _opacity = 1;

    // Made on the first draw, which has the device; it lives as long as the flow, which lives as long as the game.
    private BasicEffect? _effect;

    /// <summary>Builds the triangles once per layout, at rest, so drawing allocates nothing.</summary>
    public void Show(ShatterLayout layout)
    {
        _shards = layout.Shards;
        _restingCenters = [.. layout.Shards.Select(shard => shard.Center + (Vector2.Normalize(shard.Center - layout.Impact) * CrackGap))];
        _vertices = new VertexPositionTexture[layout.Shards.Count * 3];
        for (var index = 0; index < _shards.Count; index++)
        {
            var shard = _shards[index];
            _vertices[index * 3].TextureCoordinate = shard.A / _screenSize;
            _vertices[(index * 3) + 1].TextureCoordinate = shard.B / _screenSize;
            _vertices[(index * 3) + 2].TextureCoordinate = shard.C / _screenSize;
        }

        PlaceShards(seconds: 0, share: 0);
    }

    /// <summary>
    /// Moves every shard to where <paramref name="shatter"/> has flown it: out along its flight, turned about its
    /// center, shrunk and faded. Writes into the vertices it holds.
    /// </summary>
    public void Fly(EncounterShatter shatter) => PlaceShards(shatter.FlightSeconds, shatter.Share);

    /// <param name="batch">Begun with <see cref="VirtualScreen.BeginBatch"/>; ended, then begun again with it.</param>
    /// <param name="frozenFrame">The field's last frame, which the shards show.</param>
    /// <param name="pixel">The plain pixel the hairlines are drawn with.</param>
    public void Draw(SpriteBatch batch, Texture2D frozenFrame, Texture2D pixel)
    {
        batch.End();
        DrawTriangles(batch.GraphicsDevice, frozenFrame);
        VirtualScreen.BeginBatch(batch);
        for (var index = 0; index < _vertices.Length; index++)
        {
            var from = _vertices[index].Position;
            var to = _vertices[index % 3 == 2 ? index - 2 : index + 1].Position;
            DrawHairline(batch, pixel, new Vector2(from.X, from.Y), new Vector2(to.X, to.Y));
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
        _effect.Alpha = _opacity;
        device.BlendState = BlendState.AlphaBlend;
        device.DepthStencilState = DepthStencilState.None;
        device.RasterizerState = RasterizerState.CullNone;
        device.SamplerStates[0] = SamplerState.PointClamp;
        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            device.DrawUserPrimitives(PrimitiveType.TriangleList, _vertices, 0, _vertices.Length / 3);
        }
    }

    // Each shard after flying for seconds: its center out along its flight, its corners turned and shrunk about it.
    private void PlaceShards(float seconds, float share)
    {
        var scale = 1 - (ShrinkMax * share);
        _opacity = 1 - share;
        for (var index = 0; index < _shards.Count; index++)
        {
            var shard = _shards[index];
            var center = _restingCenters[index] + shard.OffsetAt(seconds);
            var (sin, cos) = MathF.SinCos(shard.RotationAt(seconds));
            Place(index * 3, center + (Turned(shard.A - shard.Center, sin, cos) * scale));
            Place((index * 3) + 1, center + (Turned(shard.B - shard.Center, sin, cos) * scale));
            Place((index * 3) + 2, center + (Turned(shard.C - shard.Center, sin, cos) * scale));
        }
    }

    private static Vector2 Turned(Vector2 offset, float sin, float cos) =>
        new((offset.X * cos) - (offset.Y * sin), (offset.X * sin) + (offset.Y * cos));

    private void Place(int vertex, Vector2 position) => _vertices[vertex].Position = new Vector3(position, 0);

    // A 1 px line from one corner to the next, stretched and turned from the plain pixel.
    private void DrawHairline(SpriteBatch batch, Texture2D pixel, Vector2 from, Vector2 to)
    {
        var angle = MathF.Atan2(to.Y - from.Y, to.X - from.X);
        var length = Vector2.Distance(from, to);
        batch.Draw(pixel, from, null, _hairline * _opacity, angle, Vector2.Zero, new Vector2(length, 1), SpriteEffects.None, 0);
    }
}
