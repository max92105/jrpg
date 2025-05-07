// CommandPanel.xaml.cs
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using JRPG.Core.Battles.Models;
using JRPG.Core.Utils;

namespace JRPG.UI.Battle.Controls
{
    public partial class CommandPanel : UserControl
    {
        public event Action<string> OnCommandSelected;
        public event Action<Skill> OnSkillSelected;

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
            icon.Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(file)));
        }

        public void ShowSkills(IEnumerable<Skill> skills)
        {
            SkillListPanel.Children.Clear();

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
                    SkillListPanel.Visibility = Visibility.Collapsed;
                };

                SkillListPanel.Children.Add(skillButton);
            }
        }

        private void Command_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string command)
            {
                if (command == "Skill")
                {
                    // Raise base event but let parent handle showing skills
                    OnCommandSelected?.Invoke(command);

                    // Optional: if you want to toggle collapse on second click
                    SkillListPanel.Visibility = SkillListPanel.Visibility == Visibility.Visible
                        ? Visibility.Collapsed
                        : Visibility.Visible;
                }
                else
                {
                    // Always hide skill list on other command selections
                    SkillListPanel.Visibility = Visibility.Collapsed;
                    OnCommandSelected?.Invoke(command);
                }
            }
        }
    }
}
