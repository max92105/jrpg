using JRPG.Core.Battles.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JRPG.Core.Battles;

namespace JRPG.UI.GameMenu
{
    public partial class InventoryView : UserControl
    {
        private InventoryManager inventoryManager;

        public InventoryView(InventoryManager inventoryManager)
        {
            InitializeComponent();
            this.inventoryManager = inventoryManager;
            Refresh();
        }

        private void Refresh()
        {
            ItemListPanel.Children.Clear();
            ItemDescriptionBlock.Text = "Select an item to see its description.";

            foreach (var item in inventoryManager.Items)
            {
                var itemBorder = new Border
                {
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Margin = new Thickness(5),
                    CornerRadius = new CornerRadius(5)
                };

                var itemRow = new StackPanel { Orientation = Orientation.Horizontal };
                itemBorder.Child = itemRow;

                var nameText = new TextBlock
                {
                    Text = item.Name,
                    FontSize = 20,
                    Foreground = Brushes.White,
                    Width = 250
                };

                var quantityText = new TextBlock
                {
                    Text = "x" + item.Quantity,
                    FontSize = 20,
                    Foreground = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Width = 100
                };

                itemRow.Children.Add(nameText);
                itemRow.Children.Add(quantityText);

                itemBorder.MouseEnter += (s, e) =>
                {
                    itemBorder.Background = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                    ItemDescriptionBlock.Text = item.Description;
                };

                itemBorder.MouseLeave += (s, e) =>
                {
                    itemBorder.Background = Brushes.Transparent;
                };

                itemBorder.MouseLeftButtonDown += (s, e) =>
                {
                    ShowPopup(item);
                };

                ItemListPanel.Children.Add(itemBorder);
            }
        }

        private void ShowPopup(InventoryItem item)
        {
            inventoryManager.SelectItem(item);
            PopupQuestion.Text = $"Use {item.Name}?";
            PopupOverlay.Visibility = Visibility.Visible;
        }

        private void PopupYes_Click(object sender, RoutedEventArgs e)
        {
            if (inventoryManager.SelectedItem != null)
            {
                inventoryManager.UseSelectedItem();
                inventoryManager.ClearSelection();
                Refresh();
            }
            PopupOverlay.Visibility = Visibility.Collapsed;
        }

        private void PopupNo_Click(object sender, RoutedEventArgs e)
        {
            inventoryManager.ClearSelection();
            PopupOverlay.Visibility = Visibility.Collapsed;
        }
    }
}