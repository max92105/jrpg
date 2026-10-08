using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Core.Utils;
using JRPG.Enums;
using JRPG.Managers;
using JRPG.Utils;

namespace JRPG.UI.GameMenu
{
    public partial class GameMenuControl : UserControl
    {
        public Action<int> OnLoadSlotConfirmed { get; set; }
        public event Func<Task> ReturnToMainMenuRequested;

        public GameMenuControl()
        {
            InitializeComponent();

            MenuBackground.Source = ImageLoader.LoadBackground("GameMenu.png");

            ContainerGameMenuContent.Visibility = Visibility.Collapsed;
        }

        private void SetContextOptions(params UIElement[] options)
        {
            ContextOptionsPanel.Children.Clear();

            if (options != null)
            {
                foreach (var option in options)
                {
                    ContextOptionsPanel.Children.Add(option);
                }
            }
        }

        private Button CreateFilterIcon<T>(FilterOption<T> option)
        {
            string fullPath;

            if (option.PathResolver != null)
                fullPath = option.PathResolver(option.IconFile);
            else
                fullPath = AssetPathResolver.GetIconsPath(option.IconFile); // default to icons

            var icon = new Button
            {
                Style = (Style)FindResource("FilterIconStyle"),
                ToolTip = option.Label,
                Content = new Image
                {
                    Source = new BitmapImage(new Uri(fullPath, UriKind.Absolute)),
                    Stretch = Stretch.Uniform
                }
            };

            icon.Click += (s, e) =>
            {
                if (GameMenuContent.Content is IFilterable<T> filterable)
                {
                    filterable.ApplyFilter(option.FilterValue);
                    HideSecondary();
                }
            };

            return icon;
        }

        private void MenuButton_Checked(object sender, RoutedEventArgs e)
        {
            var clicked = (ToggleButton)sender;

            ContainerGameMenuContent.Visibility = Visibility.Visible;

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
            HideSecondary();

            var inventoryView = new InventoryView();
            GameMenuContent.Content = inventoryView;

            var filters = new List<FilterOption<Type>>
            {
                new FilterOption<Type> { IconFile = "All.png", FilterValue = null },
                new FilterOption<Type> { IconFile = "Consumable.png", FilterValue = typeof(Consumable) },
                new FilterOption<Type> { IconFile = "Armor.png", FilterValue = typeof(Armor) },
                new FilterOption<Type> { IconFile = "Weapon.png", FilterValue = typeof(Weapon) }
            };

            var icons = filters.Select(f => CreateFilterIcon(f)).ToArray();
            SetContextOptions(icons);
        }

        private void PartyButton_Click(object sender, RoutedEventArgs e)
        {
            HideSecondary();

            GameMenuContent.Content = new PartyView();

            SetContextOptions(null);
        }

        private void EquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            HideSecondary();

            var equipmentView = new EquipmentView();
            GameMenuContent.Content = equipmentView;

            var party = GameSystems.Party.AllCharacters;

            var characterFilters = party.Select(c => new FilterOption<Character>
            {
                FilterValue = c,
                IconFile = c.PortraitFile,
                Label = c.Name,
                PathResolver = AssetPathResolver.GetPortraitPath
            });

            var buttons = characterFilters.Select(CreateFilterIcon).ToArray();
            SetContextOptions(buttons);
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            HideSecondary();

            GameMenuContent.Content = new StatusView();

            var party = GameSystems.Party.AllCharacters;

            var characterFilters = party.Select(c => new FilterOption<Character>
            {
                FilterValue = c,
                IconFile = c.PortraitFile,
                Label = c.Name,
                PathResolver = AssetPathResolver.GetPortraitPath
            });

            var buttons = characterFilters.Select(CreateFilterIcon).ToArray();
            SetContextOptions(buttons);
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            HideSecondary();

            GameMenuContent.Content = new TextBlock { Text = "Options (Coming Soon)", FontSize = 24, Foreground = System.Windows.Media.Brushes.White };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            HideSecondary();

            GameMenuContent.Content = new SaveLoadView(
                 SaveLoadView.SaveLoadMode.Save,
                 async (slotIndex) => { /* no-op, logic already handled in view */ },
                 () => { }
             );

            SetContextOptions(null);
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            HideSecondary();

            GameMenuContent.Content = new SaveLoadView(
                SaveLoadView.SaveLoadMode.Load,
                async (slotIndex) =>
                {
                    OnLoadSlotConfirmed?.Invoke(slotIndex);

                    foreach (var child in ButtonPanel.Children)
                    {
                        if (child is ToggleButton button)
                        {
                            button.IsChecked = false;
                        }
                    }
                },
                () =>
                {
                    GameMenuContent.Content = null;
                }
            );

            SetContextOptions(null);
        }

        private async void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var child in ButtonPanel.Children)
            {
                if (child is ToggleButton button)
                {
                    button.IsChecked = false;
                }
            }

            if (ReturnToMainMenuRequested != null)
                await ReturnToMainMenuRequested.Invoke();
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

            GameStateManager.SetState(SceneType.Exploration);

            this.Visibility = Visibility.Collapsed;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        public void ShowContent(UserControl content)
        {
            GameMenuContent.Content = content;
        }

        public void ShowSecondary(UserControl view)
        {
            SecondaryPanel.Content = view;
            SecondaryPanelContainer.Visibility = Visibility.Visible;
        }

        public void HideSecondary()
        {
            SecondaryPanel.Content = null;
            SecondaryPanelContainer.Visibility = Visibility.Collapsed;
        }

    }
}
