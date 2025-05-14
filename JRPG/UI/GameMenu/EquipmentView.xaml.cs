using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Equipments;
using JRPG.Core.Systems.Parties;
using JRPG.Core.Utils;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.UI.GameMenu
{
    /// <summary>
    /// Interaction logic for EquipmentView.xaml
    /// </summary>

    public partial class EquipmentView : UserControl
    {
        private PartyManager partyManager;
        private EquipmentManager equipmentManager;
        private Character selectedCharacter;

        public EquipmentView(PartyManager partyManager, EquipmentManager equipmentManager, Character defaultSelectedCharacter = null)
        {
            InitializeComponent();
            this.partyManager = partyManager;
            this.equipmentManager = equipmentManager;

            WeaponNameBlock.MouseLeftButtonUp += WeaponRow_Click;
            ArmorNameBlock.MouseLeftButtonUp += ArmorRow_Click;

            PopulatePortraits();

            if (defaultSelectedCharacter != null)
            {
                SelectCharacter(defaultSelectedCharacter);
            }
            else
            {
                SelectCharacter(partyManager.AllCharacters[0]);
            }
        }

        private void PopulatePortraits()
        {
            PortraitPanel.Children.Clear();

            foreach (var character in partyManager.AllCharacters)
            {
                var image = new Image
                {
                    Source = new BitmapImage(new Uri(AssetPathResolver.GetPortraitPath(character.PortraitFile))),
                    Width = 32,
                    Height = 32,
                    Margin = new Thickness(5),
                    Cursor = Cursors.Hand
                };

                image.MouseLeftButtonDown += (s, e) =>
                {
                    SelectCharacter(character);
                };

                PortraitPanel.Children.Add(image);
            }
        }

        private void SelectCharacter(Character character)
        {
            selectedCharacter = character;

            CharacterNameBlock.Text = character.Name;

            WeaponNameBlock.Text = character.EquippedWeapon?.Name ?? "None";
            ArmorNameBlock.Text = character.EquippedArmor?.Name ?? "None";

            StatsPanel.Children.Clear();
            StatsPanel.Children.Add(CreateStatText("HP", character.HP));
            StatsPanel.Children.Add(CreateStatText("MP", character.MP));
            StatsPanel.Children.Add(CreateStatText("ATK", character.GetTotalAttack()));
            StatsPanel.Children.Add(CreateStatText("DEF", character.GetTotalDefense()));
            StatsPanel.Children.Add(CreateStatText("Level", character.Level));
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

        private void WeaponRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedCharacter == null) return;

            var view = new EquipmentSelectionView(selectedCharacter, equipmentManager, partyManager, isWeapon: true);
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowContent(view);
        }

        private void ArmorRow_Click(object sender, MouseButtonEventArgs e)
        {
            if (selectedCharacter == null) return;

            var view = new EquipmentSelectionView(selectedCharacter, equipmentManager, partyManager, isWeapon: false);
            ((MainWindow)Application.Current.MainWindow).GameMenuControl.ShowContent(view);
        }
    }
}
