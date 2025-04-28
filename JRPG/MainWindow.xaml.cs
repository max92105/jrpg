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

        private List<InventoryItem> playerInventory = new List<InventoryItem>();
        private InventoryItem selectedItem;

        public MainWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;

            dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);
            game = new Game(GameCanvas);

            InitializeInventory();

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

        private void InitializeInventory()
        {
            playerInventory.Add(new InventoryItem { Name = "Potion", Quantity = 5, Description = "Restores a small amount of HP." });
            playerInventory.Add(new InventoryItem { Name = "Hi-Potion", Quantity = 2, Description = "Restores a medium amount of HP." });
            playerInventory.Add(new InventoryItem { Name = "Elixir", Quantity = 1, Description = "Fully restores HP and MP." });
            playerInventory.Add(new InventoryItem { Name = "Antidote", Quantity = 7, Description = "Cures poison." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
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
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(7, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });

            var itemListStack = new StackPanel();

            // Description Block
            TextBlock descriptionBlock = new TextBlock
            {
                Text = "Select an item to see its description.",
                FontSize = 18,
                Foreground = Brushes.White,
                Margin = new Thickness(10),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Top
            };

            // Wrap description inside a Border
            Border descriptionBorder = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                Margin = new Thickness(10),
                Child = descriptionBlock
            };

            foreach (var item in playerInventory)
            {
                var itemRow = new Border
                {
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Margin = new Thickness(5),
                    CornerRadius = new CornerRadius(5)
                };

                var itemStack = new StackPanel { Orientation = Orientation.Horizontal };
                itemRow.Child = itemStack;

                itemStack.Children.Add(new TextBlock
                {
                    Text = item.Name,
                    FontSize = 24,
                    Foreground = Brushes.White,
                    Width = 200,
                    VerticalAlignment = VerticalAlignment.Top
                });

                itemStack.Children.Add(new TextBlock
                {
                    Text = "x" + item.Quantity,
                    FontSize = 24,
                    Foreground = Brushes.White,
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right
                });

                // Highlight + description on hover
                itemRow.MouseEnter += (s, args) =>
                {
                    itemRow.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                    descriptionBlock.Text = item.Description;
                };

                itemRow.MouseLeave += (s, args) =>
                {
                    itemRow.Background = Brushes.Transparent;
                };

                itemRow.MouseLeftButtonDown += async (s, args) =>
                {
                    selectedItem = item;
                    PopupQuestion.Text = $"Use {item.Name}?";
                    await FadeInPopup();
                };

                itemListStack.Children.Add(itemRow);
            }



            ScrollViewer scroll = new ScrollViewer
            {
                Content = itemListStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(10),
                Background = Brushes.Transparent
            };

            Grid.SetRow(scroll, 0);
            Grid.SetRow(descriptionBorder, 1);

            mainGrid.Children.Add(scroll);
            mainGrid.Children.Add(descriptionBorder);

            GameMenuContent.Content = mainGrid;
        }

        private async void PopupYes_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                selectedItem.Quantity--;

                if (selectedItem.Quantity <= 0)
                {
                    playerInventory.Remove(selectedItem);
                }

                selectedItem = null;
                await FadeOutPopup();

                // Refresh Inventory page
                InventoryButton_Click(null, null);
            }
        }

        private async void PopupNo_Click(object sender, RoutedEventArgs e)
        {
            selectedItem = null;
            await FadeOutPopup();
        }

        private async Task FadeInPopup()
        {
            PopupConfirmation.Visibility = Visibility.Visible;
            PopupConfirmation.Opacity = 0;

            for (double opacity = 0; opacity <= 1; opacity += 0.05)
            {
                PopupConfirmation.Opacity = opacity;
                await Task.Delay(10);
            }

            PopupConfirmation.Opacity = 1;
        }

        private async Task FadeOutPopup()
        {
            for (double opacity = 1; opacity >= 0; opacity -= 0.05)
            {
                PopupConfirmation.Opacity = opacity;
                await Task.Delay(10);
            }

            PopupConfirmation.Visibility = Visibility.Collapsed;
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