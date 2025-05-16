using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Utils;
using JRPG.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.UI.GameMenu
{
    public partial class EquipmentView : UserControl, IFilterable<Character>
    {
        private Character selectedCharacter;

        public EquipmentView(Character selectedCharacter = null)
        {
            InitializeComponent();

            WeaponNameBlock.MouseLeftButtonUp += WeaponRow_Click;
            ArmorNameBlock.MouseLeftButtonUp += ArmorRow_Click;

            if (selectedCharacter != null)
            {
                ApplyFilter(selectedCharacter);           
            }
            else
            {
                // Default selection
                var firstCharacter = GameSystems.Party.AllCharacters.FirstOrDefault();
                if (firstCharacter != null)
                {
                    ApplyFilter(firstCharacter);
                }
            }
        }

        public void ApplyFilter(Character character)
        {
            if (character == null) return;

            selectedCharacter = character;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (selectedCharacter == null) return;

            CharacterNameBlock.Text = selectedCharacter.Name;
            WeaponNameBlock.Text = selectedCharacter.EquippedWeapon?.Name ?? "None";
            ArmorNameBlock.Text = selectedCharacter.EquippedArmor?.Name ?? "None";

            StatsPanel.Children.Clear();
            StatsPanel.Children.Add(CreateStatText("HP", selectedCharacter.HP));
            StatsPanel.Children.Add(CreateStatText("MP", selectedCharacter.MP));
            StatsPanel.Children.Add(CreateStatText("ATK", selectedCharacter.GetTotalAttack()));
            StatsPanel.Children.Add(CreateStatText("DEF", selectedCharacter.GetTotalDefense()));
            StatsPanel.Children.Add(CreateStatText("Level", selectedCharacter.Level));
        }

        private TextBlock CreateStatText(string label, int value)
        {
            return new TextBlock
            {
                Text = $"{label}: {value}",
                FontSize = 16,
                Foreground = System.Windows.Media.Brushes.White,
                Margin = new Thickness(2)
            };
        }

        public void PreviewStats(Equipment equipment, bool isWeapon)
        {
            if (selectedCharacter == null) return;

            int baseStat = isWeapon ? selectedCharacter.BaseAttack : selectedCharacter.BaseDefense;
            int currentStat = isWeapon ? selectedCharacter.GetTotalAttack() : selectedCharacter.GetTotalDefense();
            int newBonus = isWeapon
                ? ((Weapon)equipment).AttackBonus
                : ((Armor)equipment).DefenseBonus;

            int newStat = baseStat + newBonus;
            int diff = newStat - currentStat;

            string label = isWeapon ? "ATK" : "DEF";

            foreach (var child in StatsPanel.Children.OfType<TextBlock>())
            {
                if (child.Text.StartsWith(label))
                {
                    child.Inlines.Clear();

                    child.Inlines.Add(new Run($"{label}: {currentStat} ") { Foreground = Brushes.White });
                    child.Inlines.Add(new Run($"→ {newStat} ") { Foreground = Brushes.White });

                    if (diff != 0)
                    {
                        string arrow = diff > 0 ? "▲" : "▼";
                        string sign = diff > 0 ? "+" : "";
                        Brush color = diff > 0 ? Brushes.LightGreen : Brushes.IndianRed;

                        child.Inlines.Add(new Run($"{arrow} ({sign}{diff})") { Foreground = color });
                    }
                    else
                    {
                        child.Inlines.Add(new Run("(No Change)") { Foreground = Brushes.Gray });
                    }
                }
            }
        }

        private void WeaponRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedCharacter == null) return;

            var view = new EquipmentSelectionView(selectedCharacter, isWeapon: true);
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowSecondary(view);
        }

        private void ArmorRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedCharacter == null) return;

            var view = new EquipmentSelectionView(selectedCharacter, isWeapon: false);
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowSecondary(view);
        }
    }
}