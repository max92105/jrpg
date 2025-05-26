using MapEditor.Enums;


namespace MapEditor.Utils
{
    public static class EditModeHelper
    {
        private static EditorMode _currentMode = EditorMode.Tile;
         
        public static EditorMode CurrentMode { get; set; }

        public static bool IsInTileMode => _currentMode == EditorMode.Tile;
        public static bool IsInObjectMode => _currentMode == EditorMode.Object;
        public static bool IsInTriggerMode => _currentMode == EditorMode.Trigger;
        public static bool IsInNpcMode => _currentMode == EditorMode.NPC;
    }
}
