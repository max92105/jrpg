using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Flow;

/// <summary>
/// The frozen frame broken into glass shards around an impact point (decision 258). Eight rays run from the impact to
/// the screen's edge: to the four corners, and to a jittered point on each edge, so the outermost shards end exactly on
/// the screen's border. One jittered ring crosses them. The ring and the impact make a fan of 8 shards, and the
/// quads between the ring and the border split into 16 more: 24 in all, tiling the screen exactly.
/// </summary>
internal sealed class ShatterLayout
{
    /// <summary>The farthest the ring reaches, as a share of a ray from the impact to the border.</summary>
    public const float RingShareMax = 0.55f;

    // Placeholder look: the ring 35 to 55 % of the way out, the edge points 25 to 75 % along their edge.
    private const float RingShareMin = 0.35f;
    private const float EdgeShareMin = 0.25f;
    private const float EdgeShareMax = 0.75f;

    private ShatterLayout(Vector2 impact, IReadOnlyList<Shard> shards)
    {
        Impact = impact;
        Shards = shards;
    }

    /// <summary>Where the glass broke from, in screen pixels.</summary>
    public Vector2 Impact { get; }

    public IReadOnlyList<Shard> Shards { get; }

    /// <param name="impact">Where the glass breaks from, inside the screen.</param>
    /// <param name="seed">
    /// The jitter's seed. The Game's own <see cref="Random"/>, never the battle's random source, so the look can't
    /// change a battle.
    /// </param>
    public static ShatterLayout Break(Point impact, int seed)
    {
        var random = new Random(seed);
        var center = impact.ToVector2();
        var border = BorderPoints(random);
        var ring = border.Select(point => Vector2.Lerp(center, point, Between(random, RingShareMin, RingShareMax))).ToArray();
        var shards = new List<Shard>();
        for (var ray = 0; ray < border.Length; ray++)
        {
            var next = (ray + 1) % border.Length;
            shards.Add(new Shard(center, ring[ray], ring[next]));
            shards.Add(new Shard(ring[ray], ring[next], border[next]));
            shards.Add(new Shard(ring[ray], border[next], border[ray]));
        }

        return new ShatterLayout(center, shards);
    }

    // Clockwise around the screen from its top-left corner: each corner, then a jittered point on the edge after it, so
    // any impact inside the screen sees them in order.
    private static Vector2[] BorderPoints(Random random)
    {
        float Along(int length) => length * Between(random, EdgeShareMin, EdgeShareMax);
        const int width = VirtualScreen.Width;
        const int height = VirtualScreen.Height;
        return
        [
            new(0, 0), new(Along(width), 0),
            new(width, 0), new(width, Along(height)),
            new(width, height), new(width - Along(width), height),
            new(0, height), new(0, height - Along(height)),
        ];
    }

    private static float Between(Random random, float min, float max) => min + ((max - min) * random.NextSingle());
}
