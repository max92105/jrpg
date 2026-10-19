using JRPG.Core.Maps.Models;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    public class MoveObjectCommand : IEditorCommand
    {
        private readonly PlacedObject obj;
        private readonly double oldX, oldY;
        private readonly double newX, newY;

        public MoveObjectCommand(PlacedObject obj, double oldX, double oldY, double newX, double newY)
        {
            this.obj = obj;
            this.oldX = oldX;
            this.oldY = oldY;
            this.newX = newX;
            this.newY = newY;
        }

        public void Execute()
        {
            obj.X = newX;
            obj.Y = newY;
        }

        public void Undo()
        {
            obj.X = oldX;
            obj.Y = oldY;
        }
    }
}
