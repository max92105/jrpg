using JRPG.Core.Maps.Models;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    public class RemoveObjectCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly PlacedObject obj;

        public RemoveObjectCommand(Map map, PlacedObject obj)
        {
            this.map = map;
            this.obj = obj;
        }

        public void Execute()
        {
            map.Objects.Remove(obj);
        }

        public void Undo()
        {
            map.Objects.Add(obj);
        }
    }
}
