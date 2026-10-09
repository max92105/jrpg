using JRPG.Core.Maps.Models;
using MapEditor.Undo;
using System.Collections.Generic;
using System.Linq;

namespace MapEditor.Commands
{
    public class FillTileCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly int newId;
        private readonly int layerZ;

        // hadTile: whether a tile existed on this layer before, so undo can remove instead of writing Id 0
        private readonly List<(int x, int y, bool hadTile, int oldId)> affected = new List<(int x, int y, bool hadTile, int oldId)>();

        public FillTileCommand(Map map, int startX, int startY, int layerZ, int newId)
        {
            this.map = map;
            this.newId = newId;
            this.layerZ = layerZ;

            // Get target ID at layerZ (even if no tile exists)
            var tile = map.GetTileAt(startX, startY, layerZ);
            int targetId = tile?.Id ?? 0;

            // Always fill unless you're already drawing the same tile at same layer
            if (targetId != newId)
            {
                FloodFill(startX, startY, targetId);
            }
        }

        private void FloodFill(int startX, int startY, int targetId)
        {
            var visited = new HashSet<(int x, int y)>();
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                if (!visited.Add((x, y))) continue;

                if (x < 0 || x >= map.Width || y < 0 || y >= map.Height)
                    continue;

                var tile = map.GetTileAt(x, y, layerZ);
                int currentId = tile?.Id ?? 0;

                if (currentId != targetId)
                    continue;

                affected.Add((x, y, tile != null, currentId));
                map.SetTile(x, y, layerZ, newId);

                queue.Enqueue((x + 1, y));
                queue.Enqueue((x - 1, y));
                queue.Enqueue((x, y + 1));
                queue.Enqueue((x, y - 1));
            }
        }

        public void Execute()
        {
            foreach (var (x, y, _, _) in affected)
                map.SetTile(x, y, layerZ, newId);
        }

        public void Undo()
        {
            foreach (var (x, y, hadTile, oldId) in affected)
            {
                if (hadTile)
                    map.SetTile(x, y, layerZ, oldId);
                else
                    map.RemoveTileAt(x, y, layerZ);
            }
        }
    }
}