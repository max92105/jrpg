using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using JRPG.Core.Systems.Inventories;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Core.Utils;

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
            ItemDescriptionBlock.Text = "Hover an item to see its description.";

            foreach (var item in inventoryManager.Items)
            {
                var rowGrid = new Grid
                {
                    Height = 50,
                    Margin = new Thickness(10, 5, 10, 5),
                    Cursor = Cursors.Hand,
                    Background = Brushes.Transparent // ✅ Makes the full row clickable
                };

                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(8, GridUnitType.Star) });
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });

                // Left side (Icon + Name)
                var leftPanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var icon = new Image
                {
                    Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(item.IconFile))),
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                var nameText = new TextBlock
                {
                    Text = item.Name,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center
                };

                leftPanel.Children.Add(icon);
                leftPanel.Children.Add(nameText);

                Grid.SetColumn(leftPanel, 0);
                rowGrid.Children.Add(leftPanel);

                // Right side (Quantity)
                var quantityText = new TextBlock
                {
                    Text = "x" + item.Quantity,
                    FontSize = 16,
                    Foreground = Brushes.White,
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(0, 0, 10, 0)
                };

                Grid.SetColumn(quantityText, 1);
                rowGrid.Children.Add(quantityText);

                var scaleTransform = new ScaleTransform(1.0, 1.0);
                rowGrid.RenderTransformOrigin = new Point(0.5, 0.5);
                rowGrid.RenderTransform = scaleTransform;

                rowGrid.MouseEnter += (s, e) =>
                {
                    scaleTransform.ScaleX = 1.02;
                    scaleTransform.ScaleY = 1.02;
                    rowGrid.Background = new LinearGradientBrush(
                     Color.FromArgb(40, 255, 255, 255),
                     Color.FromArgb(0, 255, 255, 255),
                     new Point(0, 0),
                     new Point(1, 0));

                    ItemDescriptionBlock.Text = item.Description;
                };

                rowGrid.MouseLeave += (s, e) =>
                {
                    scaleTransform.ScaleX = 1.0;
                    scaleTransform.ScaleY = 1.0;
                    rowGrid.Background = Brushes.Transparent;
                };

                rowGrid.MouseLeftButtonDown += (s, e) =>
                {
                    ShowPopup(item);
                };

                ItemListPanel.Children.Add(rowGrid);
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