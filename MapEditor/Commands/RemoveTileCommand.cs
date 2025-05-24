// RemoveTileCommand.cs
using JRPG.Core.Maps.Models;
using MapEditor.Undo;
using System.Collections.Generic;
using System.Linq;

namespace MapEditor.Commands
{
    public class RemoveTileCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly List<Tile> removedTiles = new List<Tile>();

        private readonly int layerZ;
        private readonly int tileX;
        private readonly int tileY;

        public RemoveTileCommand(Map map, int tileX, int tileY, int layerZ)
        {
            this.map = map;
            this.tileX = tileX;
            this.tileY = tileY;
            this.layerZ = layerZ;

            var tiles = map.GetTilesAt(tileX, tileY).Where(t => t.Z == layerZ).ToList();
            removedTiles.AddRange(tiles);
        }

        public void Execute()
        {
            map.RemoveTileAt(tileX, tileY, layerZ);
        }

        public void Undo()
        {
            map.Tiles.AddRange(removedTiles);
        }
    }
}
