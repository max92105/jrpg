using JRPG.Core.Events.Models;
using JRPG.Core.Maps.Models;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    /// <summary>Sets (or removes, with null) the event of a placed object.</summary>
    public class SetObjectEventCommand : IEditorCommand
    {
        private readonly PlacedObject obj;
        private readonly MapEvent newEvent;
        private readonly MapEvent oldEvent;

        public SetObjectEventCommand(PlacedObject obj, MapEvent newEvent)
        {
            this.obj = obj;
            this.newEvent = newEvent;
            oldEvent = obj.Event;
        }

        public void Execute() => obj.Event = newEvent;

        public void Undo() => obj.Event = oldEvent;
    }
}
