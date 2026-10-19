using System.Collections.Generic;
using MapEditor.Undo;

namespace MapEditor.Commands
{
    public class BatchCommand : IEditorCommand
    {
        private readonly List<IEditorCommand> commands;

        public BatchCommand(IEnumerable<IEditorCommand> commands)
        {
            this.commands = new List<IEditorCommand>(commands);
        }

        public void Execute()
        {
            foreach (var command in commands)
                command.Execute();
        }

        public void Undo()
        {
            for (int i = commands.Count - 1; i >= 0; i--)
                commands[i].Undo();
        }
    }
}
