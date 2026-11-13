using Jrpg.Core.Geometry;
using Jrpg.Core.Maps;
using Jrpg.Core.Sprites;
using Jrpg.Game.Exploration;
using Microsoft.Xna.Framework;

namespace Jrpg.Game.Tests.Exploration;

public sealed class ObjectScreenAreaTests
{
    private const string PlayerShaped = "placeholder.png";
    private const string Tall = "villager.png";

    // The player's shape (48 px at scale 1) and a bought sheet's (32 px at scale 3, decision 562).
    private static readonly Dictionary<string, FieldSpriteSheet> _sprites = new(StringComparer.Ordinal)
    {
        [PlayerShaped] = Sheet(PlayerShaped, 48, 1),
        [Tall] = Sheet(Tall, 32, 3),
    };

    [Fact]
    public void Of_TileObject_CoversItsCells()
    {
        var area = ObjectScreenArea.Of(new Point2D(1, 3), Definition(2, 1, sprite: null), _sprites, Vector2.Zero);

        Assert.Equal(new Rectangle(48, 144, 96, 48), area);
    }

    // Rounded once at the top-left (120 - 10.6 = 109.4 rounds to 109), so the tiles follow one whole pixel.
    [Fact]
    public void Of_ObjectBetweenCells_RoundsTheTopLeftOnce()
    {
        var area = ObjectScreenArea.Of(new Point2D(2.5, 1.25), Definition(1, 1, sprite: null), _sprites, new Vector2(10.6f, 0));

        Assert.Equal(new Rectangle(109, 60, 48, 48), area);
    }

    [Fact]
    public void Of_PlayerShapedNpc_IsItsCell()
    {
        var area = ObjectScreenArea.Of(new Point2D(1, 3), Definition(1, 1, PlayerShaped), _sprites, Vector2.Zero);

        Assert.Equal(new Rectangle(48, 144, 48, 48), area);
    }

    [Fact]
    public void Of_TallNpc_RisesAboveItsCell()
    {
        var area = ObjectScreenArea.Of(new Point2D(1, 3), Definition(1, 1, Tall), _sprites, Vector2.Zero);

        Assert.Equal(new Rectangle(48 - 24, 144 - 48, 96, 96), area);
    }

    private static ObjectDefinition Definition(int width, int height, string? sprite) => new()
    {
        Id = "thing",
        Width = width,
        Height = height,
        CollisionPolygon = [],
        Category = "test",
        Tiles = sprite is null ? [new ObjectTile(0, 0, 1)] : [],
        FieldSpriteFile = sprite,
        AbovePlayerZones = [],
    };

    private static FieldSpriteSheet Sheet(string file, int frame, int scale) => new()
    {
        File = file,
        FrameWidth = frame,
        FrameHeight = frame,
        Scale = scale,
        Walk = new FacingCycle
        {
            Rows = new FacingRows { Down = 0, Left = 1, Right = 2, Up = 3 },
            Frames = new TileAnimation { FrameCount = 4, FrameMilliseconds = 150 },
        },
    };
}
