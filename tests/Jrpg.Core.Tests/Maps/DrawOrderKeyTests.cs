using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Maps;

public sealed class DrawOrderKeyTests
{
    [Fact]
    public void CompareTo_LowerZ_DrawsFirst()
    {
        var ground = DrawOrderKey.ForSprite(z: -3, baseY: 10, order: 5);
        var roof = DrawOrderKey.ForTile(z: 5, order: 0);

        Assert.True(ground.CompareTo(roof) < 0);
    }

    [Fact]
    public void CompareTo_SameZTileAndSprite_TileFirst()
    {
        var tile = DrawOrderKey.ForTile(z: 1, order: 9);
        var sprite = DrawOrderKey.ForSprite(z: 1, baseY: 0, order: 0);

        Assert.True(tile.CompareTo(sprite) < 0);
    }

    [Fact]
    public void CompareTo_SpritesSameZ_LowerBaseDrawsLater()
    {
        var player = DrawOrderKey.ForSprite(z: 1, baseY: 4.5, order: 0);
        var treeInFront = DrawOrderKey.ForSprite(z: 1, baseY: 6, order: 0);

        Assert.True(player.CompareTo(treeInFront) < 0);
    }

    // The canopy from the higher layer has the lower base, which alone would draw it first.
    [Fact]
    public void CompareTo_SameZSpritesFromDifferentLayers_HigherOriginDrawsLater()
    {
        var fromMinus3 = DrawOrderKey.ForCanopy(objectZ: -3, baseY: 7, order: 0);
        var fromMinus4 = DrawOrderKey.ForCanopy(objectZ: -4, baseY: 8, order: 1);

        Assert.True(fromMinus4.CompareTo(fromMinus3) < 0);
    }

    [Fact]
    public void CompareTo_TileAndCanopyOnOneZ_TileFirst()
    {
        var canopy = DrawOrderKey.ForCanopy(objectZ: -3, baseY: 0, order: 0);
        var roofTiles = DrawOrderKey.ForTile(z: MapLayers.AbovePlayerZ, order: 9);

        Assert.True(roofTiles.CompareTo(canopy) < 0);
    }

    [Fact]
    public void CompareTo_TilesSameZ_KeepInsertionOrder()
    {
        var first = DrawOrderKey.ForTile(z: 1, order: 0);
        var second = DrawOrderKey.ForTile(z: 1, order: 1);

        Assert.True(first.CompareTo(second) < 0);
    }

    [Fact]
    public void CompareTo_EqualKeys_InsertionOrderDecides()
    {
        var placedFirst = DrawOrderKey.ForSprite(z: 1, baseY: 3, order: 2);
        var placedLater = DrawOrderKey.ForSprite(z: 1, baseY: 3, order: 7);

        Assert.True(placedFirst.CompareTo(placedLater) < 0);
        Assert.True(placedLater.CompareTo(placedFirst) > 0);
    }

    [Fact]
    public void Sort_MixedKeys_FollowsZThenGroupThenBaseThenOrder()
    {
        DrawOrderKey[] keys =
        [
            DrawOrderKey.ForSprite(z: 1, baseY: 2, order: 1),
            DrawOrderKey.ForTile(z: 5, order: 0),
            DrawOrderKey.ForSprite(z: 1, baseY: 1, order: 0),
            DrawOrderKey.ForTile(z: 1, order: 3),
            DrawOrderKey.ForTile(z: -5, order: 0),
        ];

        Array.Sort(keys);

        Assert.Equal(
            [
                DrawOrderKey.ForTile(z: -5, order: 0),
                DrawOrderKey.ForTile(z: 1, order: 3),
                DrawOrderKey.ForSprite(z: 1, baseY: 1, order: 0),
                DrawOrderKey.ForSprite(z: 1, baseY: 2, order: 1),
                DrawOrderKey.ForTile(z: 5, order: 0),
            ],
            keys);
    }
}
