using JRPG.Enums;
using System;

namespace JRPG.Managers
{
    public static class GameStateManager
    {
        public static SceneType Current { get; private set; } = SceneType.Exploration;

        public static bool IsGamePaused => IsInBattle || IsDialogue || IsCutscene;

        public static event Action<SceneType> OnStateChanged;

        public static void SetState(SceneType newState)
        {
            if (Current == newState)
                return;

            Current = newState;
            OnStateChanged?.Invoke(newState);
        }

        public static bool IsExploration => Current == SceneType.Exploration;
        public static bool IsInBattle => Current == SceneType.Battle;
        public static bool IsDialogue => Current == SceneType.Dialogue;
        public static bool IsCutscene => Current == SceneType.Cutscene;
        public static bool IsMenu => Current == SceneType.Menu;
    }
}
