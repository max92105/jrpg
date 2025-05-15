using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using JRPG.Core.Systems;

namespace JRPG.UI.GameMenu
{
    public partial class StatusView : UserControl
    {
        public StatusView()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            StatusPanel.Children.Clear();

            foreach (var character in GameSystems.Party.ActivePartyOrder)
            {
                var characterBorder = new Border
                {
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(5),
                    Padding = new Thickness(10)
                };

                var characterStack = new StackPanel();

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"{character.Name} - {character.Class} (Lv {character.Level})",
                    FontSize = 24,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"HP: {character.HP}",
                    FontSize = 18,
                    Foreground = Brushes.LightGreen
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"ATK: {character.GetTotalAttack()} (Base {character.BaseAttack})",
                    FontSize = 18,
                    Foreground = Brushes.LightBlue
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"DEF: {character.GetTotalDefense()} (Base {character.BaseDefense})",
                    FontSize = 18,
                    Foreground = Brushes.LightBlue
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"Weapon: {character.EquippedWeapon?.Name ?? "None"}",
                    FontSize = 18,
                    Foreground = Brushes.LightGray
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"Armor: {character.EquippedArmor?.Name ?? "None"}",
                    FontSize = 18,
                    Foreground = Brushes.LightGray
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"EXP: {character.EXP} / {character.EXPToNextLevel}",
                    FontSize = 18,
                    Foreground = Brushes.Yellow,
                    Margin = new Thickness(0, 5, 0, 0)
                });

                characterStack.Children.Add(new ProgressBar
                {
                    Minimum = 0,
                    Maximum = character.EXPToNextLevel,
                    Value = character.EXP,
                    Height = 15,
                    Margin = new Thickness(0, 5, 0, 0)
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = "Skills:",
                    FontSize = 20,
                    Foreground = Brushes.LightSkyBlue,
                    Margin = new Thickness(0, 10, 0, 0)
                });

                foreach (var skill in character.Skills)
                {
                    characterStack.Children.Add(new TextBlock
                    {
                        Text = $"- {skill}",
                        FontSize = 16,
                        Foreground = Brushes.White
                    });
                }

                characterBorder.Child = characterStack;
                StatusPanel.Children.Add(characterBorder);
            }
        }
    }
}
