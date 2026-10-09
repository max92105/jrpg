using MapEditor.Enums;


namespace MapEditor.Utils
{
    public static class EditModeHelper
    {
        public static EditorMode CurrentMode { get; set; } = EditorMode.Tile;

        public static bool IsInTileMode => CurrentMode == EditorMode.Tile;
        public static bool IsInObjectMode => CurrentMode == EditorMode.Object;
        public static bool IsInTriggerMode => CurrentMode == EditorMode.Trigger;
        public static bool IsInNpcMode => CurrentMode == EditorMode.NPC;
    }
}
