using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Utils;
using JRPG.UI.Battle.Controls;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JRPG.UI.GameMenu
{
    public partial class PartyView : UserControl
    {
        private Character draggedCharacter;
        private readonly Dictionary<Character, Border> characterBlockMap = new Dictionary<Character, Border>();

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
                if (!characterBlockMap.ContainsKey(character))
                {
                    characterBlockMap[character] = CreateCharacterBlock(character);
                }

                var block = characterBlockMap[character];

                // Detach from previous parent
                var currentParent = VisualTreeHelper.GetParent(block) as Panel;
                currentParent?.Children.Remove(block);

                // Add to the right panel
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
                AllowDrop = true,
                Tag = character.IsMainCharacter
            };

            if (!character.IsMainCharacter)
            {
                bool isDragging = false;
                Point dragStart = new Point();

                border.MouseLeftButtonDown += (s, e) =>
                {
                    isDragging = false;
                    dragStart = e.GetPosition(null);
                };

                border.MouseMove += (s, e) =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        Point current = e.GetPosition(null);
                        Vector delta = current - dragStart;

                        if (!isDragging && (Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                            Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance))
                        {
                            isDragging = true;
                            draggedCharacter = character;
                            DragDrop.DoDragDrop(border, character, DragDropEffects.Move);
                        }
                    }
                };

                border.Drop += (s, e) =>
                {
                    if (draggedCharacter != null && draggedCharacter != character)
                    {
                        GameSystems.Party.SwapPartyMembers(draggedCharacter, character);
                        draggedCharacter = null;
                        Refresh();
                    }
                };

                border.MouseLeftButtonUp += (s, e) =>
                {
                    if (!isDragging)
                    {
                        if (character.IsActive)
                            GameSystems.Party.RemoveFromParty(character);
                        else
                            GameSystems.Party.AddToParty(character);

                        Refresh();
                    }
                };
            }

            return border;
        }
    }
}
