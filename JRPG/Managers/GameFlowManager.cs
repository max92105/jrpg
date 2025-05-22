

using JRPG.Managers.Input;

namespace JRPG.Managers
{
    public static class GameFlowManager
    {
        public static bool IsInBattle { get; private set; }
        public static bool IsInDialogue { get; private set; }
        public static bool IsInCutscene { get; private set; }

        public static bool IsGamePaused => IsInBattle || IsInDialogue || IsInCutscene;

        public static void StartBattle()
        {
            IsInBattle = true;
            InputManager.Disable();
        }

        public static void EndBattle()
        {
            IsInBattle = false;
            InputManager.Enable();
        }

        public static void StartDialogue()
        {
            IsInDialogue = true;
            InputManager.Disable();
        }

        public static void EndDialogue()
        {
            IsInDialogue = false;
            InputManager.Enable();
        }

        public static void StartCutscene()
        {
            IsInCutscene = true;
            InputManager.Disable();
        }

        public static void EndCutscene()
        {
            IsInCutscene = false;
            InputManager.Enable();
        }

        public static void ResetAll()
        {
            IsInBattle = false;
            IsInDialogue = false;
            IsInCutscene = false;
            InputManager.Enable();
        }
    }
}