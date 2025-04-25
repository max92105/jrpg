using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace JRPG
{
    public partial class MainWindow : Window
    {
        private Game game;
        private DialogueManager dialogueManager;

        public MainWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;

            dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);
            game = new Game(GameCanvas);

            GameCanvas.Visibility = Visibility.Collapsed; // Hide gameplay at start

            CompositionTarget.Rendering += (s, e) =>
            {
                if (StartScreen.Visibility == Visibility.Visible || GameMenu.Visibility == Visibility.Visible)
                    return; // Don't update world if Start Menu or Game Menu open

                if (!dialogueManager.IsActive)
                    game.Update();

                game.Render();
            };

            Loaded += (s, e) => Keyboard.Focus(GameCanvas);
        }

        public void StartBattle()
        {
            Fade(1, 0.5, () =>
            {
                var player = new Fighter("Hero", 100, 20, 5);
                var enemy = new Enemy("Goblin", 40, 10, 2, "Assets/goblin.png");

                var battleUI = new BattleUI(player, enemy);
                battleUI.OnBattleEnd += EndBattle;

                RootGrid.Children.Clear();
                RootGrid.Children.Add(GameCanvas);
                RootGrid.Children.Add(battleUI);

                Fade(0, 0.5);
            });
        }

        private void EndBattle()
        {
            Fade(1, 0.5, () =>
            {
                RootGrid.Children.Clear();
                RootGrid.Children.Add(GameCanvas);
                Focus();
                Keyboard.Focus(GameCanvas);

                Fade(0, 0.5);
            });
        }

        private void Fade(double toOpacity, double durationSec, Action onComplete = null)
        {
            FadeRect.Visibility = Visibility.Visible;
            FadeRect.Width = ActualWidth;
            FadeRect.Height = ActualHeight;

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = toOpacity,
                Duration = TimeSpan.FromSeconds(durationSec),
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(animation, FadeRect);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(animation);

            storyboard.Completed += (s, e) =>
            {
                FadeRect.Opacity = toOpacity;
                if (toOpacity == 0)
                    FadeRect.Visibility = Visibility.Collapsed;

                onComplete?.Invoke();
            };

            storyboard.Begin();
        }

        private async Task FadeInOut(Action onFadeOutComplete)
        {
            FadeRect.Visibility = Visibility.Visible;

            // Fade In to Black
            for (double opacity = 0; opacity <= 1; opacity += 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8); // ~60fps
            }

            onFadeOutComplete?.Invoke();

            // Fade Out to Clear
            for (double opacity = 1; opacity >= 0; opacity -= 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8);
            }

            FadeRect.Visibility = Visibility.Collapsed;
        }

        private async Task FadeToBlack()
        {
            FadeRect.Visibility = Visibility.Visible;

            for (double opacity = 0; opacity <= 1; opacity += 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8); // ~30 FPS
            }
        }

        private async Task FadeFromBlack()
        {
            for (double opacity = 1; opacity >= 0; opacity -= 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8);
            }

            FadeRect.Visibility = Visibility.Collapsed;
        }

        public void ShowDialogue(List<string> pages, string portraitPath = null, string speaker = null, Action callback = null)
        {
            dialogueManager.Show(pages, portraitPath, speaker, callback);
        }

        private UIElement CreatePlaceholderPage(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape && StartScreen.Visibility != Visibility.Visible)
            {
                if (GameMenu.Visibility == Visibility.Visible)
                    GameMenu.Visibility = Visibility.Collapsed;
                else
                    GameMenu.Visibility = Visibility.Visible;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (dialogueManager.IsActive && e.Key == Key.Space)
            {
                dialogueManager.Skip();
                return;
            }

            if (!dialogueManager.IsActive && RootGrid.Children.Contains(GameCanvas))
                game.OnKeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (!dialogueManager.IsActive && RootGrid.Children.Contains(GameCanvas))
                game.OnKeyUp(e.Key);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Fade to black first
            await FadeToBlack();

            // Switch screens AFTER full black
            StartScreen.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Visible;

            // Small delay to make sure WPF refreshes layout
            await Task.Delay(500);

            // Then fade back in
            await FadeFromBlack();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenu.Visibility = Visibility.Collapsed;
        }

        private async void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeInOut(() =>
            {
                GameMenu.Visibility = Visibility.Collapsed;
                GameCanvas.Visibility = Visibility.Collapsed;
                StartScreen.Visibility = Visibility.Visible;
            });
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void InventoryButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = CreatePlaceholderPage("Inventory Page");
        }

        private void PartyButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = CreatePlaceholderPage("Party Management Page");
        }

        private void EquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = CreatePlaceholderPage("Equipment Page");
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = CreatePlaceholderPage("Status Page");
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = CreatePlaceholderPage("Save Game Page");
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = CreatePlaceholderPage("Options Page");
        }

        private void CloseGameMenu_Click(object sender, RoutedEventArgs e)
        {
            GameMenu.Visibility = Visibility.Collapsed;
        }
    }
}