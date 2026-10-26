using Jrpg.Game.Rendering;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Flow;

/// <summary>
/// The frozen frame broken like glass around an impact point (decision 258). Rays run from the impact to the screen's
/// four corners and to one jittered point on each side; a jittered ring crosses them. Each sector between two rays is a
/// triangle at the impact and a quad outside the ring cut in two, so neighbors share edges and the shards tile the
/// screen exactly: 24 shards. The randomness is the Game's own, from a seed, never the battle's (design, key
/// decisions).
/// </summary>
internal sealed class ShatterLayout
{
    // Each side's ray lands 30-70 % along it; the ring crosses each ray 33-57 % of the way out (placeholders).
    private const float SidePointMin = 0.3f;
    private const float SidePointSpread = 0.4f;
    private const float RingMin = 0.33f;
    private const float RingSpread = 0.24f;

    private ShatterLayout(IReadOnlyList<Shard> shards) => Shards = shards;

    public IReadOnlyList<Shard> Shards { get; }

    /// <param name="impact">Where the glass breaks from, inside the screen.</param>
    /// <param name="seed">Picks the jitter; the same seed gives the same shards.</param>
    public static ShatterLayout Break(Point impact, int seed)
    {
        var random = new Random(seed);
        var center = impact.ToVector2();
        var edges = EdgePoints(random).OrderBy(point => MathF.Atan2(point.Y - center.Y, point.X - center.X)).ToArray();
        var ring = edges.Select(edge => Vector2.Lerp(center, edge, RingMin + (RingSpread * random.NextSingle()))).ToArray();
        var shards = new List<Shard>(3 * edges.Length);
        for (var ray = 0; ray < edges.Length; ray++)
        {
            var next = (ray + 1) % edges.Length;
            shards.Add(new Shard(center, ring[ray], ring[next]));
            shards.Add(new Shard(ring[ray], edges[next], ring[next]));
            shards.Add(new Shard(ring[ray], edges[ray], edges[next]));
        }

        return new ShatterLayout(shards);
    }

    // The screen's corners, so consecutive rays always meet the border on one side, and a point along each side.
    private static IEnumerable<Vector2> EdgePoints(Random random)
    {
        const float width = VirtualScreen.Width;
        const float height = VirtualScreen.Height;
        float Along() => SidePointMin + (SidePointSpread * random.NextSingle());
        return
        [
            new(0, 0),
            new(width, 0),
            new(width, height),
            new(0, height),
            new(width * Along(), 0),
            new(width, height * Along()),
            new(width * Along(), height),
            new(0, height * Along()),
        ];
    }
}
