namespace MapEditor.Undo
{
    public interface IEditorCommand
    {
        void Execute();
        void Undo();
    }
}