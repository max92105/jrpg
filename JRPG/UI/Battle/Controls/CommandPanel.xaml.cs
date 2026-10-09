// CommandPanel.xaml.cs
using JRPG.Utils;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Core.Utils;

namespace JRPG.UI.Battle.Controls
{
    public partial class CommandPanel : UserControl
    {
        public event Action<string> OnCommandSelected;
        public event Action<Skill> OnSkillSelected;
        public event Action<InventoryItem> OnItemSelected;

        public CommandPanel()
        {
            InitializeComponent();
            InitializeCommandPanelIcons();
        }

        private void InitializeCommandPanelIcons()
        {
            SetButtonIcon(img_Attack, "Attack.png");
            SetButtonIcon(img_Defend, "Defend.png");
            SetButtonIcon(img_Skill, "Skill.png");
            SetButtonIcon(img_Item, "Item.png");
            SetButtonIcon(img_Run, "Run.png");
        }

        private void SetButtonIcon(Image icon, string file)
        {
            icon.Source = ImageLoader.LoadIcon(file);
        }

        public void ShowSkills(IEnumerable<Skill> skills)
        {
            OptionListPanel.Children.Clear();

            foreach (var skill in skills)
            {
                var skillButton = new Button
                {
                    Content = new TextBlock
                    {
                        Text = skill.Name,
                        Foreground = Brushes.White,
                        FontSize = 14,
                        TextAlignment = TextAlignment.Center
                    },
                    Style = (Style)FindResource("CommandButtonStyle"),
                    Tag = skill
                };

                skillButton.Click += (s, e) =>
                {
                    OnSkillSelected?.Invoke(skill);
                    OptionListPanel.Visibility = Visibility.Collapsed;
                };

                OptionListPanel.Children.Add(skillButton);
            }
        }

        public void ShowItems(IEnumerable<InventoryItem> inventoryItems)
        {
            OptionListPanel.Children.Clear();

            foreach (var inventoryItem in inventoryItems)
            {
                var item = inventoryItem?.Item as Consumable;
                if (item == null)
                {
                    return;
                }

                var consumable = item;

                var itemButton = new Button
                {
                    Content = new TextBlock
                    {
                        Text = $"{consumable.Name} (x{inventoryItem.Quantity})",
                        Foreground = Brushes.White,
                        FontSize = 14,
                        TextAlignment = TextAlignment.Center
                    },
                    Style = (Style)FindResource("CommandButtonStyle"),
                    Tag = item
                };

                itemButton.Click += (s, e) =>
                {
                    OnItemSelected?.Invoke(inventoryItem);
                    OptionListPanel.Visibility = Visibility.Collapsed;
                };

                OptionListPanel.Children.Add(itemButton);
            }
        }

        private void Command_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string command)
            {
                if (command == "Skill" || command == "Item")
                {
                    // Raise base event but let parent handle showing skills
                    OnCommandSelected?.Invoke(command);
                }
                else
                {
                    // Always hide skill list on other command selections
                    OptionListPanel.Visibility = Visibility.Collapsed;
                    OnCommandSelected?.Invoke(command);
                }
            }
        }
    }
}
