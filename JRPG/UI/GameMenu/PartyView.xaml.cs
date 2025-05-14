using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Parties;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JRPG.UI.GameMenu
{
    public partial class PartyView : UserControl
    {
        private PartyManager partyManager;
        private Character draggedCharacter;

        public PartyView(PartyManager partyManager)
        {
            InitializeComponent();
            this.partyManager = partyManager;
            Refresh();
        }

        private void Refresh()
        {
            ActivePartyPanel.Children.Clear();
            FullRosterPanel.Children.Clear();

            foreach (var character in partyManager.ActivePartyOrder)
            {
                ActivePartyPanel.Children.Add(CreateCharacterBlock(character, isActiveParty: true));
            }

            foreach (var character in partyManager.AllCharacters)
            {
                FullRosterPanel.Children.Add(CreateCharacterBlock(character, isActiveParty: false));
            }
        }

        private Border CreateCharacterBlock(Character character, bool isActiveParty)
        {
            var border = new Border
            {
                Background = character.IsMainCharacter ? Brushes.DarkSlateBlue : (character.IsActive ? Brushes.DarkGreen : Brushes.Transparent),
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(1),
                CornerRadius = new CornerRadius(5),
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                AllowDrop = true
            };

            var stack = new StackPanel();

            stack.Children.Add(new TextBlock
            {
                Text = character.Name,
                FontSize = 18,
                Foreground = Brushes.White,
                TextAlignment = TextAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"{character.Class} (Lv{character.Level})",
                FontSize = 14,
                Foreground = Brushes.LightGray,
                TextAlignment = TextAlignment.Center
            });

            stack.Children.Add(new TextBlock
            {
                Text = $"HP: {character.HP}",
                FontSize = 14,
                Foreground = Brushes.LightGray,
                TextAlignment = TextAlignment.Center
            });

            border.Child = stack;

            if (!character.IsMainCharacter)
            {
                border.MouseMove += (s, e) =>
                {
                    if (e.LeftButton == MouseButtonState.Pressed)
                    {
                        draggedCharacter = character;
                        DragDrop.DoDragDrop(border, character, DragDropEffects.Move);
                    }
                };

                border.Drop += (s, e) =>
                {
                    if (draggedCharacter != null && draggedCharacter != character)
                    {
                        partyManager.SwapPartyMembers(draggedCharacter, character);
                        draggedCharacter = null;
                        Refresh();
                    }
                };

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

                border.MouseLeftButtonUp += (s, e) =>
                {
                    if (!isDragging)
                    {
                        // Only do add/remove if it was a click, not a drag
                        if (character.IsActive)
                            partyManager.RemoveFromParty(character);
                        else
                            partyManager.AddToParty(character);

                        Refresh();
                    }
                };
            }

            return border;
        }
    }
}
