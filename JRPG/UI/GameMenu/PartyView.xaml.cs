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
using System.Windows.Media.Animation;

namespace JRPG.UI.GameMenu
{
    public partial class PartyView : UserControl
    {
        private readonly Dictionary<Character, Border> characterBlockMap = new Dictionary<Character, Border>();

        private Point _dragStartPoint;
        private bool _isDragging = false;

        private bool isAnimatingReorder = false;

        public PartyView()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            ActivePartyPanel.Children.Clear();
            FullRosterPanel.Children.Clear();

            // Render Active Party in defined order
            foreach (var character in GameSystems.Party.ActivePartyOrder)
            {
                if (!characterBlockMap.ContainsKey(character))
                    characterBlockMap[character] = CreateCharacterBlock(character);

                var block = characterBlockMap[character];
                var currentParent = VisualTreeHelper.GetParent(block) as Panel;
                currentParent?.Children.Remove(block);

                SetupDragDropHandlers(block, character);
                ActivePartyPanel.Children.Add(block);
            }

            // Render Reserve Members (all characters not in ActivePartyOrder)
            foreach (var character in GameSystems.Party.AllCharacters.Except(GameSystems.Party.ActivePartyOrder))
            {
                if (!characterBlockMap.ContainsKey(character))
                    characterBlockMap[character] = CreateCharacterBlock(character);

                var block = characterBlockMap[character];
                var currentParent = VisualTreeHelper.GetParent(block) as Panel;
                currentParent?.Children.Remove(block);

                SetupDragDropHandlers(block, character);
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
                Style = (Style)FindResource("HoverAnimatedBorder"),
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

            if (border.RenderTransform is TransformGroup group)
            {
                var clone = new TransformGroup();
                foreach (var t in group.Children)
                {
                    clone.Children.Add(t.CloneCurrentValue());
                }
                border.RenderTransform = clone;
            }

            return border;
        }

        private void SetupDragDropHandlers(Border border, Character character)
        {
            // Remove all existing handlers to avoid duplicates
            border.MouseLeftButtonDown -= OnDragMouseDown;
            border.MouseMove -= OnDragMouseMove;
            border.MouseLeftButtonUp -= OnDragMouseUp;
            border.MouseLeftButtonUp -= OnReserveClick;
            border.Drop -= OnDrop;
            border.DragOver -= OnDragOver;

            // LEFT SIDE (Active Party)
            if (character.IsActive && !character.IsMainCharacter)
            {
                border.AllowDrop = true;

                border.MouseLeftButtonDown += OnDragMouseDown;
                border.MouseMove += OnDragMouseMove;
                border.MouseLeftButtonUp += OnDragMouseUp;

                border.DragOver += OnDragOver;
                border.Drop += OnDrop;
            }
            // RIGHT SIDE (Reserve Roster)
            else if (!character.IsActive && !character.IsMainCharacter)
            {
                border.AllowDrop = false;
                border.MouseLeftButtonUp += OnReserveClick;
            }
        }

        private void OnDragMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            _dragStartPoint = e.GetPosition(null);
        }

        private void OnDragMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender is Border border)
            {
                var currentPos = e.GetPosition(null);
                Vector delta = currentPos - _dragStartPoint;

                if (!_isDragging && (Math.Abs(delta.X) > SystemParameters.MinimumHorizontalDragDistance ||
                                      Math.Abs(delta.Y) > SystemParameters.MinimumVerticalDragDistance))
                {
                    _isDragging = true;

                    if (border.Tag is Character character)
                    {
                        var data = new DataObject(typeof(Character), character);
                        DragDrop.DoDragDrop(border, data, DragDropEffects.Move);
                    }
                }
            }
        }

        private void OnDragMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isDragging && sender is Border border && border.Tag is Character character)
            {
                GameSystems.Party.RemoveFromParty(character);
                Refresh();
            }
        }

        private void OnReserveClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is Border border && border.Tag is Character character)
            {
                GameSystems.Party.AddToParty(character);
                Refresh();
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (isAnimatingReorder) return;
            if (!e.Data.GetDataPresent(typeof(Character))) return;

            var draggedChar = e.Data.GetData(typeof(Character)) as Character;
            var targetBorder = sender as Border;
            var targetChar = (Character)targetBorder?.Tag;

            if (draggedChar == null || draggedChar == targetChar) return;

            var targetIndex = ActivePartyPanel.Children.IndexOf(targetBorder);
            var draggedBlock = characterBlockMap[draggedChar];

            if (ActivePartyPanel.Children.Contains(draggedBlock))
            {
                int oldIndex = ActivePartyPanel.Children.IndexOf(draggedBlock);

                if (oldIndex != targetIndex)
                {
                    isAnimatingReorder = true;
                    AnimateSlideToPosition(draggedBlock, oldIndex, targetIndex);
                }
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

            GameSystems.Party.SetPartyOrder(newOrder);
        }

        private void AnimateSlideToPosition(Border draggedBlock, int oldIndex, int newIndex)
        {
            if (oldIndex == newIndex || oldIndex < 0 || newIndex < 0) return;

            var positionsBefore = ActivePartyPanel.Children
                .OfType<Border>()
                .ToDictionary(b => b, b => b.TransformToAncestor(this).Transform(new Point(0, 0)).Y);

            ActivePartyPanel.Children.RemoveAt(oldIndex);
            ActivePartyPanel.Children.Insert(newIndex, draggedBlock);
            ActivePartyPanel.UpdateLayout();

            int animCount = 0;
            int completed = 0;

            foreach (var block in ActivePartyPanel.Children.OfType<Border>())
            {
                var newTop = block.TransformToAncestor(this).Transform(new Point(0, 0)).Y;
                var oldTop = positionsBefore.ContainsKey(block) ? positionsBefore[block] : newTop;
                double offset = oldTop - newTop;

                // Ensure we’re animating Translate.Y inside the existing TransformGroup
                if (block.RenderTransform is TransformGroup group &&
                    group.Children.OfType<TranslateTransform>().FirstOrDefault() is TranslateTransform translate)
                {
                    if (Math.Abs(offset) > 0.1)
                    {
                        animCount++;

                        translate.BeginAnimation(TranslateTransform.YProperty, null);
                        translate.Y = offset;

                        var anim = new DoubleAnimation
                        {
                            To = 0,
                            Duration = TimeSpan.FromMilliseconds(300),
                            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                        };

                        anim.Completed += (s, e) =>
                        {
                            completed++;
                            if (completed == animCount)
                                isAnimatingReorder = false;
                        };

                        translate.BeginAnimation(TranslateTransform.YProperty, anim);
                    }
                    else
                    {
                        translate.BeginAnimation(TranslateTransform.YProperty, null);
                        translate.Y = 0;
                    }
                }
            }

            if (animCount == 0)
                isAnimatingReorder = false;
        }
    }
}
