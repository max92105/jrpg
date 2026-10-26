using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Flow;

/// <summary>
/// Draws a <see cref="ShatterLayout"/>: each shard is its piece of the frozen frame, pushed a few pixels away from the
/// impact so the cracks show, with a bright hairline along its edges (decision 258); as the glass flies apart each
/// shard moves, turns and shrinks about its center and fades. Shards are textured triangles, which the sprite batch
/// can't draw, so the view ends the batch, draws them with its own effect, and begins it again.
/// </summary>
internal sealed class ShatterView : IDisposable
{
    // Each shard sits 4 px out from the impact and shrinks to 70 % as it flies; its edges are white at 60 %
    // (placeholders).
    private const float CrackWidth = 4;
    private const float FlownScale = 0.7f;
    private const float EdgeOpacity = 0.6f;

    private readonly BasicEffect _textured;
    private readonly BasicEffect _edges;
    private ShatterLayout? _layout;
    private VertexPositionTexture[] _shards = [];
    private VertexPositionColor[] _edgeLines = [];

    /// <param name="device">The device the virtual screen is drawn on.</param>
    public ShatterView(GraphicsDevice device)
    {
        var projection = Matrix.CreateOrthographicOffCenter(0, VirtualScreen.Width, VirtualScreen.Height, 0, 0, 1);
        _textured = new BasicEffect(device) { TextureEnabled = true, Projection = projection };
        _edges = new BasicEffect(device) { VertexColorEnabled = true, Projection = projection };
    }

    /// <summary>Whether <paramref name="layout"/>'s vertices are the ones built.</summary>
    public bool IsBuiltFor(ShatterLayout layout) => ReferenceEquals(_layout, layout);

    /// <summary>Builds the shards' vertices once, when the frame breaks: where each corner is in the frame.</summary>
    /// <param name="layout">The shards, in virtual-screen pixels.</param>
    public void BuildVertices(ShatterLayout layout)
    {
        _layout = layout;
        var size = new Vector2(VirtualScreen.Width, VirtualScreen.Height);
        var edgeColor = Color.White * EdgeOpacity;
        _shards = new VertexPositionTexture[layout.Shards.Count * 3];
        _edgeLines = new VertexPositionColor[layout.Shards.Count * 6];
        for (var index = 0; index < layout.Shards.Count; index++)
        {
            var shard = layout.Shards[index];
            _shards[index * 3] = new VertexPositionTexture(Vector3.Zero, shard.A / size);
            _shards[(index * 3) + 1] = new VertexPositionTexture(Vector3.Zero, shard.B / size);
            _shards[(index * 3) + 2] = new VertexPositionTexture(Vector3.Zero, shard.C / size);
            for (var end = 0; end < 6; end++)
            {
                _edgeLines[(index * 6) + end].Color = edgeColor;
            }
        }
    }

    /// <param name="batch">Begun by the host with point sampling; it is ended and begun again the same way.</param>
    /// <param name="frame">The frozen frame the shards are cut from.</param>
    /// <param name="shatter">How far the shards have flown.</param>
    /// <exception cref="InvalidOperationException">No layout was built.</exception>
    public void Draw(SpriteBatch batch, Texture2D frame, EncounterShatter shatter)
    {
        PlaceShards(_layout ?? throw new InvalidOperationException("Build a layout's vertices before drawing it."), shatter);
        var device = batch.GraphicsDevice;
        batch.End();
        device.BlendState = BlendState.AlphaBlend;
        device.SamplerStates[0] = SamplerState.PointClamp;
        device.RasterizerState = RasterizerState.CullNone;
        _textured.Texture = frame;
        _textured.Alpha = 1 - shatter.Share;
        _textured.CurrentTechnique.Passes[0].Apply();
        device.DrawUserPrimitives(PrimitiveType.TriangleList, _shards, 0, _shards.Length / 3);
        _edges.Alpha = 1 - shatter.Share;
        _edges.CurrentTechnique.Passes[0].Apply();
        device.DrawUserPrimitives(PrimitiveType.LineList, _edgeLines, 0, _edgeLines.Length / 2);
        batch.Begin(samplerState: SamplerState.PointClamp);
    }

    public void Dispose()
    {
        _textured.Dispose();
        _edges.Dispose();
    }

    // Into the arrays built once, so a frame allocates nothing: each corner turned and shrunk about the shard's center,
    // which has moved out along its flight.
    private void PlaceShards(ShatterLayout layout, EncounterShatter shatter)
    {
        var seconds = shatter.FlightSeconds;
        var scale = 1 - ((1 - FlownScale) * shatter.Share);
        for (var index = 0; index < layout.Shards.Count; index++)
        {
            var shard = layout.Shards[index];
            var center = shard.Center;
            var moved = center + (shard.Direction * CrackWidth) + shard.OffsetAt(seconds);
            var pose = new ShardPose(center, moved, Matrix.CreateRotationZ(shard.RotationAt(seconds)), scale);
            var a = pose.Place(shard.A);
            var b = pose.Place(shard.B);
            var c = pose.Place(shard.C);
            _shards[index * 3].Position = a;
            _shards[(index * 3) + 1].Position = b;
            _shards[(index * 3) + 2].Position = c;
            SetEdges(index, a, b, c);
        }
    }

    private void SetEdges(int shard, Vector3 a, Vector3 b, Vector3 c)
    {
        var first = shard * 6;
        _edgeLines[first].Position = a;
        _edgeLines[first + 1].Position = b;
        _edgeLines[first + 2].Position = b;
        _edgeLines[first + 3].Position = c;
        _edgeLines[first + 4].Position = c;
        _edgeLines[first + 5].Position = a;
    }

    // Where a shard is this frame: its center in the frozen frame, where that center has flown, its turn and its size.
    private readonly record struct ShardPose(Vector2 Center, Vector2 Moved, Matrix Turn, float Scale)
    {
        public Vector3 Place(Vector2 corner) => new(Moved + Vector2.Transform((corner - Center) * Scale, Turn), 0);
    }
}
