using System;
using JRPG.Core.Maps.Models;

namespace JRPG.Core.Maps
{
    /// <summary>
    /// Draw order shared by the game and the map editor, so the editor shows exactly what the
    /// game draws. Lower keys are drawn first:
    ///   1. Z (layer)
    ///   2. on the same layer, map tiles before sprites (objects and the player)
    ///   3. sprites are depth-sorted as whole units by their bottom edge (object base,
    ///      player's feet): lower on screen = in front. Sorting per tile instead would
    ///      interleave the tiles of overlapping objects.
    ///   4. ties keep insertion order (map tile layers, placement order of objects).
    /// </summary>
    public struct DrawSortKey : IComparable<DrawSortKey>
    {
        public const int MapTileGroup = 0;
        public const int SpriteGroup = 1;

        public int Z;
        public int Group;
        public double BaseY; // in tiles; only used for sprites
        public int Order;

        public static DrawSortKey ForMapTile(int z, int order)
        {
            return new DrawSortKey { Z = z, Group = MapTileGroup, BaseY = 0, Order = order };
        }

        public static DrawSortKey ForSprite(int z, double baseY, int order)
        {
            return new DrawSortKey { Z = z, Group = SpriteGroup, BaseY = baseY, Order = order };
        }

        /// <summary>Key for a tile of a placed object: the whole object sorts by its base.</summary>
        public static DrawSortKey ForObject(int z, PlacedObject obj, ObjectDefinition def, int order)
        {
            var (_, _, _, height) = def.GetTileBounds();
            return ForSprite(z, obj.Y + height, order);
        }

        public int CompareTo(DrawSortKey other)
        {
            int cmp = Z.CompareTo(other.Z);
            if (cmp != 0) return cmp;

            cmp = Group.CompareTo(other.Group);
            if (cmp != 0) return cmp;

            if (Group == SpriteGroup)
            {
                cmp = BaseY.CompareTo(other.BaseY);
                if (cmp != 0) return cmp;
            }

            return Order.CompareTo(other.Order);
        }
    }
}
