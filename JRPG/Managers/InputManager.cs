using JRPG.Enums;
using System.Collections.Generic;
using System.Windows.Input;

namespace JRPG.Managers.Input
{
    public static class InputManager
    {
        private static readonly HashSet<Key> keysDown = new HashSet<Key>();

        public static bool IsEnabled { get; private set; } = true;

        public static void Initialize()
        {
            GameStateManager.OnStateChanged += (newState) =>
            {
                if (newState == SceneType.Battle || newState == SceneType.Dialogue || newState == SceneType.Cutscene)
                    Disable();
                else
                    Enable();
            };
        }

        public static void Enable() => IsEnabled = true;
        public static void Disable()
        {
            IsEnabled = false;
            keysDown.Clear();
        }

        public static void OnKeyDown(Key key)
        {
            if (IsEnabled)
                keysDown.Add(key);
        }

        public static void OnKeyUp(Key key)
        {
            keysDown.Remove(key);
        }

        public static bool IsKeyDown(Key key) => keysDown.Contains(key);
        public static void Reset() => keysDown.Clear();
    }
}