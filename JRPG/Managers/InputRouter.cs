using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JRPG.Enums;

namespace JRPG.Managers
{
    public static class InputRouter
    {
        public static void HandleKeyDown(MainWindow window, Key key)
        {
            // Cancel load overlay
            if (key == Key.Escape && window.OverlayViewHost.Visibility == Visibility.Visible)
            {
                _ = SceneManager.CancelLoadScreenAsync();
                return;
            }

            // Toggle in-game menu
            if (key == Key.Escape && !GameStateManager.IsMenu && !GameStateManager.IsCutscene && !GameStateManager.IsDialogue && window.StartScreen?.Visibility != Visibility.Visible)
            {
                if (window.GameMenuControl != null)
                {
                    bool showing = window.GameMenuControl.Visibility == Visibility.Visible;
                    window.GameMenuControl.Visibility = showing ? Visibility.Collapsed : Visibility.Visible;
                    Panel.SetZIndex(window.GameMenuControl, showing ? 0 : 1000);

                    GameStateManager.SetState(showing ? SceneType.Exploration : SceneType.Menu);
                }
                return;
            }

            // Skip dialogue
            if ((GameStateManager.IsDialogue || GameStateManager.IsCutscene) && key == Key.Space)
            {
                window.dialogueManager?.Skip();
                return;
            }

            // Launch test cutscene
            if (key == Key.F1 && window.CutscenePlayer != null)
            {
                GameFlowManager.StartCutscene();
                var cutsceneManager = new CutsceneManager(window.CutscenePlayer);
                cutsceneManager.StartIntroCutscene(() => GameFlowManager.EndCutscene());
                return;
            }

            // Block input unless game is active
            if (GameStateManager.IsGamePaused) return;

            Input.InputManager.OnKeyDown(key);
        }

        public static void HandleKeyUp(Key key)
        {
            if (GameStateManager.IsGamePaused) return;

            Input.InputManager.OnKeyUp(key);
        }
    }
}