using JRPG.Core.Systems;
using JRPG.Managers;
using JRPG.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace JRPG
{
    public partial class MainWindow : Window
    {
        public DialogueManager dialogueManager;
        private GameLoopManager gameLoop;

        public MainWindow()
        {
            InitializeComponent();
            GameSystems.Initialize();
            Managers.Input.InputManager.Initialize();
            UIBootstrapper.Initialize(this);
            SceneManager.Initialize(
                RootGrid, 
                FadeRect, 
                GameCanvas, 
                StartScreen, 
                GameMenuControl, 
                OverlayViewHost, 
                ContentOverlayViewHost, 
                StartMenuContent);

            if (dialogueManager == null)
                dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);

            gameLoop = new GameLoopManager(
             () =>
             {
                SceneManager.Update();
             },
             (interpolation) =>
             {
                SceneManager.Render(interpolation);
             });

            gameLoop.Start();
        }


        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            await SceneManager.StartNewGameAsync();
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            await SceneManager.ShowLoadScreenAsync();
        }

        public async Task ReturnToMainMenuAsync()
        {
            await SceneManager.ReturnToMainMenuAsync();
        }

        public void ShowDialogue(List<string> pages, string portraitPath = null, string speaker = null, Action callback = null)
        {
            dialogueManager.ShowWithFlow(pages, portraitPath, speaker, callback);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            InputRouter.HandleKeyDown(this, e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            InputRouter.HandleKeyUp(e.Key);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
}