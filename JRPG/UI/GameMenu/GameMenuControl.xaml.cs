using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using JRPG.Core.Systems.Equipments;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Systems.Parties;
using JRPG.Core.Utils;

namespace JRPG.UI.GameMenu
{
    public partial class GameMenuControl : UserControl
    {
        private PartyManager partyManager;
        private InventoryManager inventoryManager;
        private EquipmentManager equipmentManager;

        private readonly GameSystemManager gameSystemManager;

        public GameMenuControl()
        {
            InitializeComponent();
        }

        public GameMenuControl(PartyManager partyManager, InventoryManager inventoryManager, EquipmentManager equipmentManager, GameSystemManager gameSystemManager)
        {
            InitializeComponent();

            this.partyManager = partyManager;
            this.inventoryManager = inventoryManager;
            this.equipmentManager = equipmentManager;
            this.gameSystemManager = gameSystemManager;
        }

        private void InventoryButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new InventoryView(inventoryManager);
        }

        private void PartyButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new PartyView(partyManager);
        }

        private void EquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new EquipmentView(partyManager, equipmentManager);
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new StatusView(partyManager);
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

            // Show start screen and hide game elements
            var mainWindow = (MainWindow)Application.Current.MainWindow;
            mainWindow.GameMenuControl.Visibility = Visibility.Collapsed;
            mainWindow.GameCanvas.Visibility = Visibility.Collapsed;
            mainWindow.StartScreen.Visibility = Visibility.Visible;

            await Task.Delay(300); // Allow layout update

            await FadeHelper.FadeFromBlackAsync(((MainWindow)Application.Current.MainWindow).FadeRect);
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            gameSystemManager.ExitGame();
        }

        private void CloseGameMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Collapsed;
        }

        public void ShowContent(UserControl content)
        {
            GameMenuContent.Content = content;
        }
    }
}
