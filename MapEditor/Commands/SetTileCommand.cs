using JRPG.Core.Maps.Models;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    public class SetTileCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly int x, y, z;
        private readonly int oldTileId;
        private readonly int newTileId;

        public SetTileCommand(Map map, int x, int y, int z, int newTileId)
        {
            this.map = map;
            this.x = x;
            this.y = y;
            this.z = z;
            this.newTileId = newTileId;
            this.oldTileId = map.GetTileIdAt(x, y);
        }

        public void Execute()
        {
            map.SetTile(x, y, z, newTileId);
        }

        public void Undo()
        {
            map.SetTile(x, y, z, oldTileId);
        }
    }
}
