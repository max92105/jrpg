using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JRPG.Core.Systems.Equipments;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Systems.Inventories.Enums;
using JRPG.Core.Systems.Parties;
using JRPG.Core.Utils;
using JRPG.Utils;

namespace JRPG.UI.GameMenu
{
    public partial class GameMenuControl : UserControl
    {
        private readonly GameSystemManager gameSystemManager;

        public GameMenuControl()
        {
            InitializeComponent();
        }

        public GameMenuControl(GameSystemManager gameSystemManager)
        {
            InitializeComponent();

            this.gameSystemManager = gameSystemManager;

            MenuBackground.Source = new BitmapImage(new Uri(AssetPathResolver.GetBackgroundsPath("GameMenu.png")));
        }

        private void SetContextOptions(params UIElement[] options)
        {
            ContextOptionsPanel.Children.Clear();

            foreach (var option in options)
            {
                ContextOptionsPanel.Children.Add(option);
            }
        }

        private Button CreateFilterIcon<T>(FilterOption<T> option)
        {
            var icon = new Button
            {
                Style = (Style)FindResource("FilterIconStyle"),
                ToolTip = option.Label,
                Content = new Image
                {
                    Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(option.IconFile))),
                    Stretch = Stretch.Uniform
                }
            };

            icon.Click += (s, e) =>
            {
                if (GameMenuContent.Content is IFilterable<T> filterable)
                {
                    filterable.ApplyFilter(option.FilterValue);
                }
            };

            return icon;
        }

        private void MenuButton_Checked(object sender, RoutedEventArgs e)
        {
            var clicked = (ToggleButton)sender;

            // If already checked, prevent unchecking
            if (clicked.IsChecked == false)
            {
                clicked.IsChecked = true;
                return;
            }

            foreach (var child in ButtonPanel.Children)
            {
                if (child is ToggleButton button && button != sender)
                {
                    button.IsChecked = false;
                }
            }

            string selectedMenu = clicked.Content.ToString();

            switch (selectedMenu)
            {
                case "Inventory":
                    InventoryButton_Click(sender, e);
                    break;
                case "Manage Party":
                    PartyButton_Click(sender, e);
                    break;
                case "Equipment":
                    EquipmentButton_Click(sender, e);
                    break;
                case "Status":
                    StatusButton_Click(sender, e);
                    break;
                case "Save Game":
                    SaveButton_Click(sender, e);
                    break;
                case "Load Game":
                    LoadButton_Click(sender, e);
                    break;
                case "Options":
                    OptionsButton_Click(sender, e);
                    break;
                case "Return to Main Menu":
                    ReturnToMenuButton_Click(sender, e);
                    break;
                case "Exit Game":
                    ExitButton_Click(sender, e);
                    break;
            }
        }

        private void InventoryButton_Click(object sender, RoutedEventArgs e)
        {
            var inventoryView = new InventoryView();
            GameMenuContent.Content = inventoryView;

            var filters = new List<FilterOption<ItemType?>>
            {
                new FilterOption<ItemType?> { IconFile = "All.png", FilterValue = null },
                new FilterOption<ItemType?> { IconFile = "Item.png", FilterValue = ItemType.Potion },
                new FilterOption<ItemType?> { IconFile = "Attack.png", FilterValue = ItemType.Bomb }
            };

            var icons = filters.Select(f => CreateFilterIcon(f)).ToArray();
            SetContextOptions(icons);
        }

        private void PartyButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new PartyView();
        }

        private void EquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new EquipmentView();
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new StatusView();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            gameSystemManager.SaveGame();
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new TextBlock { Text = "Options (Coming Soon)", FontSize = 24, Foreground = System.Windows.Media.Brushes.White };
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            await gameSystemManager.LoadGameAsync(
                ((MainWindow)Application.Current.MainWindow).StartScreen,
                ((MainWindow)Application.Current.MainWindow).GameCanvas
            );
        }

        private async void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeHelper.FadeToBlackAsync(((MainWindow)Application.Current.MainWindow).FadeRect);

            foreach (var child in ButtonPanel.Children)
            {
                if (child is ToggleButton button)
                {
                    button.IsChecked = false;
                }
            }

            // Show start screen and hide game elements
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.GameMenuControl.Visibility = Visibility.Collapsed;
            mainWindow.GameCanvas.Visibility = Visibility.Collapsed;
            mainWindow.StartScreen.Visibility = Visibility.Visible;

            await Task.Delay(300); // Allow layout update

            await FadeHelper.FadeFromBlackAsync(((MainWindow)Application.Current.MainWindow).FadeRect);
        }

        private void CloseGameMenu_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in ButtonPanel.Children)
            {
                if (child is ToggleButton button)
                {
                    button.IsChecked = false;
                }
            }

            this.Visibility = Visibility.Collapsed;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            gameSystemManager.ExitGame();
        }

        public void ShowContent(UserControl content)
        {
            GameMenuContent.Content = content;
        }
    }
}
