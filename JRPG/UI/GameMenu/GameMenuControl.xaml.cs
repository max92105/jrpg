using System.Windows;
using System.Windows.Controls;
using JRPG.Core.Battles;
using JRPG.Core.System;

namespace JRPG.UI.GameMenu
{
    public partial class GameMenuControl : UserControl
    {
        private PartyManager partyManager;
        private InventoryManager inventoryManager;
        private EquipmentManager equipmentManager;

        public GameMenuControl()
        {
            InitializeComponent();
        }

        public GameMenuControl(PartyManager partyManager, InventoryManager inventoryManager, EquipmentManager equipmentManager)
        {
            InitializeComponent();

            this.partyManager = partyManager;
            this.inventoryManager = inventoryManager;
            this.equipmentManager = equipmentManager;
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

        //private void SaveButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ((MainWindow)Application.Current.MainWindow).SaveButton_Click(sender, e);
        //}

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = new TextBlock { Text = "Options (Coming Soon)", FontSize = 24, Foreground = System.Windows.Media.Brushes.White };
        }

        //private void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ((MainWindow)Application.Current.MainWindow).ReturnToMenuButton_Click(sender, e);
        //}

        //private void ExitButton_Click(object sender, RoutedEventArgs e)
        //{
        //    ((MainWindow)Application.Current.MainWindow).ExitButton_Click(sender, e);
        //}

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
