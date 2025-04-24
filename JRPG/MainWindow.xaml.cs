using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace JRPG
{
    public partial class MainWindow : Window
    {
        private Game game;

        public MainWindow()
        {
            InitializeComponent();
            game = new Game(GameCanvas);

            CompositionTarget.Rendering += GameLoop;
            this.Loaded += (s, e) => Keyboard.Focus(GameCanvas);
        }

        private void GameLoop(object sender, EventArgs e)
        {
            game.Update();
            game.Render();
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

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (RootGrid.Children.Contains(GameCanvas))
            {
                game.OnKeyDown(e.Key);
            }
        }
    }
}