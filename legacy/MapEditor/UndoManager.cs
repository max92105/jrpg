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
            Push(command);
        }

        /// <summary>
        /// Records a command that has already been applied (e.g. a brush stroke or an
        /// object drag that changed the map while the mouse was moving).
        /// </summary>
        public void Push(IEditorCommand command)
        {
            undoStack.Push(command);
            redoStack.Clear(); // Clear redo when new action occurs
        }

        public bool Undo()
        {
            if (undoStack.Count == 0) return false;

            var command = undoStack.Pop();
            command.Undo();
            redoStack.Push(command);
            return true;
        }

        public bool Redo()
        {
            if (redoStack.Count == 0) return false;

            var command = redoStack.Pop();
            command.Execute();
            undoStack.Push(command);
            return true;
        }

        public void Clear()
        {
            undoStack.Clear();
            redoStack.Clear();
        }
    }
}
