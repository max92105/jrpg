using JRPG.Core.Battles.Models;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>Who acts now (big diamond) and who acts next (small diamonds).</summary>
    public partial class TurnOrderBar : UserControl
    {
        private const int MaxUpcoming = 8;
        private const double UpcomingSize = 66;
        private const double DimmedOpacity = 0.35;

        public TurnOrderBar()
        {
            InitializeComponent();
        }

        /// <param name="order">Actors in turn order; the first one is acting now.</param>
        public void Show(IReadOnlyList<BattleActor> order)
        {
            UpcomingPanel.Children.Clear();

            if (order.Count == 0)
                return;

            CurrentPortrait.Show(order[0]);

            foreach (var actor in order.Skip(1).Take(MaxUpcoming))
            {
                UpcomingPanel.Children.Add(new TurnPortrait(actor)
                {
                    Width = UpcomingSize,
                    Height = UpcomingSize,
                    Margin = new Thickness(0, 0, -6, 0) // diamonds touch, like beads on the line
                });
            }
        }

        /// <summary>While choosing targets, dims everyone who isn't targeted. An empty list lights everyone.</summary>
        public void Highlight(ICollection<BattleActor> targets)
        {
            foreach (var portrait in UpcomingPanel.Children.OfType<TurnPortrait>().Append(CurrentPortrait))
            {
                bool isLit = targets.Count == 0 || targets.Contains(portrait.Actor);
                portrait.Opacity = isLit ? 1.0 : DimmedOpacity;
            }
        }
    }
}
