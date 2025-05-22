using JRPG.Enums;

namespace JRPG.Managers
{
    public static class GameFlowManager
    {
        public static bool IsInBattle => GameStateManager.Current == SceneType.Battle;
        public static bool IsInDialogue => GameStateManager.Current == SceneType.Dialogue;
        public static bool IsInCutscene => GameStateManager.Current == SceneType.Cutscene;   

        public static void StartBattle() => GameStateManager.SetState(SceneType.Battle);
        public static void EndBattle() => GameStateManager.SetState(SceneType.Exploration);

        public static void StartDialogue() => GameStateManager.SetState(SceneType.Dialogue);
        public static void EndDialogue() => GameStateManager.SetState(SceneType.Exploration);

        public static void StartCutscene() => GameStateManager.SetState(SceneType.Cutscene);
        public static void EndCutscene() => GameStateManager.SetState(SceneType.Exploration);

        public static void ResetAll() => GameStateManager.SetState(SceneType.Exploration);
    }
}