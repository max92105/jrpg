// In PartyView.xaml.cs
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Utils;
using JRPG.UI.Battle.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JRPG.UI.GameMenu
{
    public partial class PartyView : UserControl
    {
        private readonly Dictionary<Character, Border> characterBlockMap = new Dictionary<Character, Border>();
        private Character draggedCharacter;
        private int draggedIndex = -1;

        public PartyView()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            ActivePartyPanel.Children.Clear();
            FullRosterPanel.Children.Clear();

            foreach (var character in GameSystems.Party.AllCharacters)
            {
                // Get or create the UI block
                if (!characterBlockMap.ContainsKey(character))
                {
                    characterBlockMap[character] = CreateCharacterBlock(character);
                }

                var block = characterBlockMap[character];
                var currentParent = VisualTreeHelper.GetParent(block) as Panel;
                currentParent?.Children.Remove(block);

                // Re-apply interaction behavior
                SetupDragDropHandlers(block, character);

                if (character.IsActive)
                    ActivePartyPanel.Children.Add(block);
                else
                    FullRosterPanel.Children.Add(block);
            }
        }

        private Border CreateCharacterBlock(Character character)
        {
            var panel = new CharacterPanel
            {
                CharacterName = character.Name,
                CurrentHP = character.HP,
                MaxHP = character.MaxHP,
                CurrentMP = character.MP,
                MaxMP = character.MaxMP
            };
            panel.SetPortrait(AssetPathResolver.GetPortraitPath(character.PortraitFile));

            var border = new Border
            {
                Style = (Style)FindResource("HoverAnimatedCharacterBorder"),
                Background = Brushes.Transparent,
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(0, 10, 0, 0),
                Padding = new Thickness(5),
                Child = panel,
                Tag = character
            };

            if (character.IsActive && !character.IsMainCharacter)
            {
                border.MouseLeftButtonUp += (s, e) =>
                {
                    if (!character.IsMainCharacter)
                    {
                        GameSystems.Party.AddToParty(character);
                        Refresh();
                    }
                };
            }

            return border;
        }

        private void SetupDragDropHandlers(Border border, Character character)
        {
            // Remove all previous handlers
            border.MouseLeftButtonUp -= OnReserveClick;
            border.MouseMove -= OnDragMove;
            border.Drop -= OnDrop;
            border.DragOver -= OnDragOver;

            if (character.IsActive && !character.IsMainCharacter)
            {
                border.AllowDrop = true;
                border.MouseMove += OnDragMove;
                border.DragOver += OnDragOver;
                border.Drop += OnDrop;
            }
            else if (!character.IsMainCharacter)
            {
                border.MouseLeftButtonUp += OnReserveClick;
            }
        }

        private void OnDragMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Border border)
            {
                var character = (Character)border.Tag;
                var data = new DataObject(typeof(Character), character);
                DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Character))) return;

            var draggedChar = e.Data.GetData(typeof(Character)) as Character;
            var targetBorder = sender as Border;
            var targetChar = (Character)targetBorder?.Tag;

            if (draggedChar == null || draggedChar == targetChar) return;

            var targetIndex = ActivePartyPanel.Children.IndexOf(targetBorder);
            var block = characterBlockMap[draggedChar];

            if (ActivePartyPanel.Children.Contains(block))
            {
                ActivePartyPanel.Children.Remove(block);
                ActivePartyPanel.Children.Insert(targetIndex, block);
            }

            e.Handled = true;
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(typeof(Character))) return;

            var newOrder = ActivePartyPanel.Children
                .OfType<Border>()
                .Select(b => (Character)b.Tag)
                .ToList();

            //GameSystems.Party.SwapPartyMembers(draggedCharacter, character);
        }

        private void OnReserveClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Character character)
            {
                GameSystems.Party.AddToParty(character);
                Refresh();
            }
        }
    }
}
