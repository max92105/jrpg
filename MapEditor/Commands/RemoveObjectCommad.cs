using JRPG.Core.Maps.Models;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    public class RemoveObjectCommand : IEditorCommand
    {
        private readonly Map map;
        private readonly PlacedObject obj;
        private int index = -1;

        public RemoveObjectCommand(Map map, PlacedObject obj)
        {
            this.map = map;
            this.obj = obj;
        }

        public void Execute()
        {
            index = map.Objects.IndexOf(obj);
            map.Objects.Remove(obj);
        }

        public void Undo()
        {
            // Put it back at the same position in the list so draw order is unchanged
            if (index >= 0 && index <= map.Objects.Count)
                map.Objects.Insert(index, obj);
            else
                map.Objects.Add(obj);
        }
    }
}
