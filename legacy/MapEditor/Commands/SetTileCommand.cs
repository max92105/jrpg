using JRPG.Core.Maps.Models;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    public class SetTileCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly int x, y, z;
        private readonly bool hadTile;
        private readonly int oldTileId;
        private readonly int newTileId;

        public SetTileCommand(Map map, int x, int y, int z, int newTileId)
        {
            this.map = map;
            this.x = x;
            this.y = y;
            this.z = z;
            this.newTileId = newTileId;

            // Remember what was on this layer (not Z=0) so undo restores it exactly
            var oldTile = map.GetTileAt(x, y, z);
            hadTile = oldTile != null;
            oldTileId = oldTile?.Id ?? 0;
        }

        public void Execute()
        {
            map.SetTile(x, y, z, newTileId);
        }

        public void Undo()
        {
            if (hadTile)
                map.SetTile(x, y, z, oldTileId);
            else
                map.RemoveTileAt(x, y, z);
        }
    }
}
