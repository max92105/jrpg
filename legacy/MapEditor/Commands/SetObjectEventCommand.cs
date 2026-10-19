using JRPG.Core.Events.Models;
using JRPG.Core.Maps.Models;
using MapEditor.Undo;
using System.Collections.Generic;

namespace MapEditor.Commands
{
    /// <summary>Sets (or removes, with an empty list) the events of a placed object.</summary>
    public class SetObjectEventCommand : IEditorCommand
    {
        private readonly PlacedObject obj;
        private readonly List<MapEvent> newEvents;
        private readonly List<MapEvent> oldEvents;

        public SetObjectEventCommand(PlacedObject obj, List<MapEvent> newEvents)
        {
            this.obj = obj;
            this.newEvents = newEvents ?? new List<MapEvent>();
            oldEvents = obj.Events;
        }

        public void Execute() => obj.Events = newEvents;

        public void Undo() => obj.Events = oldEvents;
    }
}
