using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Parties;
using JRPG.Core.Utils;
using JRPG.UI.Battle.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace JRPG.UI.GameMenu
{
    public partial class PartyView : UserControl
    {
        private Character draggedCharacter;

        public PartyView()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            ActivePartyPanel.Children.Clear();
            FullRosterPanel.Children.Clear();

            // Display only active members in the left panel
            foreach (var character in GameSystems.Party.ActivePartyOrder)
            {
                ActivePartyPanel.Children.Add(CreateCharacterBlock(character, isActiveParty: true));
            }

            // Display only non-active members in the right panel
            foreach (var character in GameSystems.Party.AllCharacters)
            {
                if (!character.IsActive)
                    FullRosterPanel.Children.Add(CreateCharacterBlock(character, isActiveParty: false));
            }
        }

        private Border CreateCharacterBlock(Character character, bool isActiveParty)
        {
            var panel = new CharacterPanel();
            panel.CharacterName = character.Name;
            panel.CurrentHP = character.HP;
            panel.MaxHP = character.MaxHP;
            panel.CurrentMP = character.MP;
            panel.MaxMP = character.MaxMP;
            panel.SetPortrait(AssetPathResolver.GetPortraitPath(character.PortraitFile));

            var border = new Border
            {
                Background = Brushes.Transparent, // Removed all conditional background coloring
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(6),
                Margin = new Thickness(5),
                Padding = new Thickness(8),
                Child = panel,
                Effect = character.IsMainCharacter
                    ? new DropShadowEffect
                    {
                        Color = Colors.White,
                        BlurRadius = 8,
                        ShadowDepth = 0,
                        Opacity = 0.3
                    }
                    : null,
                AllowDrop = true
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
