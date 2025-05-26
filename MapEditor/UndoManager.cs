using System.Collections.Generic;

namespace MapEditor.Undo
{
    public class UndoManager
    {
        private readonly Stack<IEditorCommand> undoStack = new Stack<IEditorCommand>();
        private readonly Stack<IEditorCommand> redoStack = new Stack<IEditorCommand>();

        public void ExecuteCommand(IEditorCommand command)
        {
            command.Execute();
            undoStack.Push(command);
            redoStack.Clear(); // Clear redo when new action occurs
        }

        public void Undo()
        {
            if (undoStack.Count == 0) return;

            var command = undoStack.Pop();
            command.Undo();
            redoStack.Push(command);
        }

        public void Redo()
        {
            if (redoStack.Count == 0) return;

            var command = redoStack.Pop();
            command.Execute();
            undoStack.Push(command);
        }
    }
}
