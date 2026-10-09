using JRPG.Core.Maps.Models;
using MapEditor.Undo;
using System.Collections.Generic;
using System.Linq;

namespace MapEditor.Commands
{
    /// <summary>
    /// Erases the visible tile(s) of one cell on one layer. A tile that carries a trigger
    /// is turned into an invisible Id 0 holder instead of being removed, so erasing
    /// graphics never deletes a trigger.
    /// </summary>
    public class RemoveTileCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly List<Tile> removedTiles = new List<Tile>();
        private readonly List<(Tile tile, int oldId)> clearedTriggerTiles = new List<(Tile tile, int oldId)>();

        public bool IsEmpty => removedTiles.Count == 0 && clearedTriggerTiles.Count == 0;

        public RemoveTileCommand(Map map, int tileX, int tileY, int layerZ)
        {
            this.map = map;

            foreach (var tile in map.GetTilesAt(tileX, tileY).Where(t => t.Z == layerZ && t.Id != 0))
            {
                if (tile.Trigger != null)
                    clearedTriggerTiles.Add((tile, tile.Id));
                else
                    removedTiles.Add(tile);
            }
        }

        public void Execute()
        {
            foreach (var tile in removedTiles)
                map.RemoveTile(tile);

            foreach (var (tile, _) in clearedTriggerTiles)
                tile.Id = 0;
        }

        public void Undo()
        {
            map.AddTiles(removedTiles);

            foreach (var (tile, oldId) in clearedTriggerTiles)
                tile.Id = oldId;
        }
    }
}
