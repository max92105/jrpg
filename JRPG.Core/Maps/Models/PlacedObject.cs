using JRPG.Core.Events.Models;
using System;
using System.Collections.Generic;

namespace JRPG.Core.Maps.Models
{
    public class PlacedObject
    {
        public int Id { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public int Z { get; set; } = 10;

        public Dictionary<string, string> Meta { get; set; } = new Dictionary<string, string>();

        public Dictionary<string, int> ZOverrides { get; set; } = new Dictionary<string, int>();

        // Stable id of this placed object on its map (used to remember opened chests etc.)
        public string Uid { get; set; }

        // Optional event (chest, door, sign...); null for plain decoration
        public MapEvent Event { get; set; }

        public static string NewUid() => Guid.NewGuid().ToString("N").Substring(0, 8);

        /// <summary>
        /// Draw layer of one tile of this object. Used by both the game and the editor so
        /// they always agree: per-instance override, else the object's layer (+ tile offset).
        /// </summary>
        public int GetTileZ(TileGroupEntry entry)
        {
            if (ZOverrides != null && ZOverrides.TryGetValue($"{entry.X},{entry.Y}", out int overrideZ))
                return overrideZ;

            return Z + entry.Z;
        }

        /// <summary>
        /// Layer of the definition's "above player" zones: always over the player,
        /// even when the object itself is on a lower layer.
        /// </summary>
        public int AbovePlayerZoneZ => Math.Max(MapConstant.AbovePlayerZ, Z);
    }
}
