using JRPG.Core.Maps.Models;
using JRPG.Core.Triggers.Models;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    public class SetTriggerCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly int x;
        private readonly int y;
        private readonly Trigger newTrigger;
        private readonly Trigger previousTrigger;

        public SetTriggerCommand(Map map, int x, int y, Trigger newTrigger)
        {
            this.map = map;
            this.x = x;
            this.y = y;
            this.newTrigger = newTrigger;

            var tile = map.GetTileAt(x, y);
            this.previousTrigger = tile?.Trigger;
        }

        public void Execute()
        {
            var tile = map.GetTileAt(x, y);
            if (tile == null)
            {
                tile = new Tile { X = x, Y = y, Id = 0 };
                map.Tiles.Add(tile);
            }
            tile.Trigger = newTrigger;
        }

        public void Undo()
        {
            var tile = map.GetTileAt(x, y);
            if (tile != null)
            {
                tile.Trigger = previousTrigger;
            }
        }
    }
}
