using JRPG.Core.Maps.Models;
using MapEditor.Undo;
using System.Collections.Generic;

namespace MapEditor.Commands
{
    public class FillTileCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly int targetId;
        private readonly int newId;
        private readonly List<(int x, int y)> affectedTiles = new List<(int x, int y)>();

        public FillTileCommand(Map map, int startX, int startY, int newId)
        {
            this.map = map;
            this.newId = newId;

            targetId = map.GetTileIdAt(startX, startY);
            FloodFill(startX, startY);
        }

        private void FloodFill(int startX, int startY)
        {
            var visited = new HashSet<(int x, int y)>();
            var queue = new Queue<(int x, int y)>();
            queue.Enqueue((startX, startY));

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();
                if (visited.Contains((x, y))) continue;
                visited.Add((x, y));

                if (x < 0 || x >= map.Width || y < 0 || y >= map.Height) continue;
                if (map.GetTileIdAt(x, y) != targetId) continue;

                affectedTiles.Add((x, y));

                queue.Enqueue((x + 1, y));
                queue.Enqueue((x - 1, y));
                queue.Enqueue((x, y + 1));
                queue.Enqueue((x, y - 1));
            }
        }

        public void Execute()
        {
            foreach (var (x, y) in affectedTiles)
                map.SetTile(x, y, newId);
        }

        public void Undo()
        {
            foreach (var (x, y) in affectedTiles)
                map.SetTile(x, y, targetId);
        }
    }
}
