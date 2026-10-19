using JRPG.Core.Maps.Models;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    public class PlaceObjectCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly PlacedObject placedObject;

        public PlaceObjectCommand(Map map, PlacedObject obj)
        {
            this.map = map;
            this.placedObject = obj;
        }

        public void Execute()
        {
            map.Objects.Add(placedObject);
        }

        public void Undo()
        {
            map.Objects.Remove(placedObject);
        }
    }
}