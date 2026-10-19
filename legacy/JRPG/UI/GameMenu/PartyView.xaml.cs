using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.UI.GameMenu.Controls;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace JRPG.UI.GameMenu
{
    /// <summary>
    /// Party page: who fights and in what order.
    /// Click an active member to send them to the reserve, click a reserve member to bring them in,
    /// drag active members to reorder them. The leader never moves.
    /// </summary>
    public partial class PartyView : MenuPage
    {
        private static readonly Duration ReorderDuration = TimeSpan.FromMilliseconds(300);

        private Point dragStartPoint;
        private bool isDragging;
        private bool isAnimatingReorder;

        public PartyView()
        {
            InitializeComponent();
            Refresh();
        }

        private void Refresh()
        {
            ActivePartyPanel.Children.Clear();
            ReservePanel.Children.Clear();

            foreach (var character in GameSystems.Party.ActivePartyOrder)
                ActivePartyPanel.Children.Add(CreateActiveCard(character));

            foreach (var character in GameSystems.Party.AllCharacters.Except(GameSystems.Party.ActivePartyOrder))
                ReservePanel.Children.Add(CreateReserveCard(character));

            ActiveCountText.Text = $"{GameSystems.Party.ActivePartyOrder.Count} / {GameSystems.Party.MaxPartySize}";
        }

        private PartyMemberCard CreateActiveCard(Character character)
        {
            var card = CreateCard(character);

            if (character.IsMainCharacter)
            {
                card.MouseEnter += (s, e) => ShowHint($"{character.Name} leads the party and always stays in it.");
                return card;
            }

            card.IsInteractive = true;
            card.AllowDrop = true;
            card.MouseEnter += (s, e) => ShowHint($"Click to move {character.Name} to the reserve. Drag to change the order.");
            card.MouseLeftButtonDown += OnDragMouseDown;
            card.MouseMove += OnDragMouseMove;
            card.MouseLeftButtonUp += OnActiveCardClick;
            card.DragOver += OnDragOver;
            card.Drop += OnDrop;
            return card;
        }

        private PartyMemberCard CreateReserveCard(Character character)
        {
            var card = CreateCard(character);
            card.IsInteractive = true;

            card.MouseEnter += (s, e) => ShowHint(IsPartyFull
                ? "The party is full. Send someone to the reserve first."
                : $"Click to add {character.Name} to the party.");

            card.MouseLeftButtonUp += (s, e) =>
            {
                GameSystems.Party.AddToParty(character);
                PartyHasChanged();
            };
            return card;
        }

        private PartyMemberCard CreateCard(Character character)
        {
            var card = new PartyMemberCard(character)
            {
                Margin = new Thickness(0, 0, 0, 10),
                RenderTransform = new TranslateTransform() // used by the reorder animation
            };

            card.MouseLeave += (s, e) => ShowHint(null);
            return card;
        }

        private bool IsPartyFull => GameSystems.Party.GetActivePartyMemberCount() >= GameSystems.Party.MaxPartySize;

        private void PartyHasChanged()
        {
            Refresh();
            NotifyPartyChanged();
        }

        // ============ Click / drag on active members ============

        private void OnDragMouseDown(object sender, MouseButtonEventArgs e)
        {
            isDragging = false;
            dragStartPoint = e.GetPosition(null);
        }

        private void OnDragMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || isDragging || !(sender is PartyMemberCard card))
                return;

            Vector moved = e.GetPosition(null) - dragStartPoint;
            if (Math.Abs(moved.X) > SystemParameters.MinimumHorizontalDragDistance ||
                Math.Abs(moved.Y) > SystemParameters.MinimumVerticalDragDistance)
            {
                isDragging = true;
                DragDrop.DoDragDrop(card, new DataObject(typeof(Character), card.Character), DragDropEffects.Move);
            }
        }

        private void OnActiveCardClick(object sender, MouseButtonEventArgs e)
        {
            // A drag also ends with a mouse up: only a real click sends the member to the reserve
            if (isDragging || !(sender is PartyMemberCard card))
                return;

            GameSystems.Party.RemoveFromParty(card.Character);
            PartyHasChanged();
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Handled = true;

            if (isAnimatingReorder || !(e.Data.GetData(typeof(Character)) is Character dragged))
                return;

            var target = (PartyMemberCard)sender;
            var draggedCard = ActivePartyPanel.Children.OfType<PartyMemberCard>().FirstOrDefault(c => c.Character == dragged);
            if (draggedCard == null || draggedCard == target)
                return;

            int oldIndex = ActivePartyPanel.Children.IndexOf(draggedCard);
            int newIndex = ActivePartyPanel.Children.IndexOf(target);
            if (oldIndex != newIndex)
                AnimateReorder(draggedCard, oldIndex, newIndex);
        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            var newOrder = ActivePartyPanel.Children.OfType<PartyMemberCard>().Select(c => c.Character).ToList();
            GameSystems.Party.SetPartyOrder(newOrder);
            NotifyPartyChanged();
        }

        /// <summary>Moves the dragged card to its new place, and slides every card that moved from its old spot.</summary>
        private void AnimateReorder(PartyMemberCard draggedCard, int oldIndex, int newIndex)
        {
            var cards = ActivePartyPanel.Children.OfType<PartyMemberCard>().ToList();
            var topsBefore = cards.ToDictionary(c => c, c => c.TranslatePoint(new Point(0, 0), ActivePartyPanel).Y);

            ActivePartyPanel.Children.RemoveAt(oldIndex);
            ActivePartyPanel.Children.Insert(newIndex, draggedCard);
            ActivePartyPanel.UpdateLayout();

            int running = 0;
            isAnimatingReorder = true;

            foreach (var card in cards)
            {
                double offset = topsBefore[card] - card.TranslatePoint(new Point(0, 0), ActivePartyPanel).Y;
                var slide = (TranslateTransform)card.RenderTransform;

                if (Math.Abs(offset) < 0.1)
                    continue;

                running++;
                var animation = new DoubleAnimation(offset, 0, ReorderDuration)
                {
                    EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
                };
                animation.Completed += (s, e) =>
                {
                    running--;
                    if (running == 0)
                        isAnimatingReorder = false;
                };
                slide.BeginAnimation(TranslateTransform.YProperty, animation);
            }

            if (running == 0)
                isAnimatingReorder = false;
        }
    }
}
