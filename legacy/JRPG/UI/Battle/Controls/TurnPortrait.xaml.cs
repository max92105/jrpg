using JRPG.Core.Battles.Models;
using JRPG.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>A battle actor's portrait in a diamond frame.</summary>
    public partial class TurnPortrait : UserControl
    {
        private bool isCurrent;

        public TurnPortrait()
        {
            InitializeComponent();
        }

        public TurnPortrait(BattleActor actor) : this()
        {
            Show(actor);
        }

        public BattleActor Actor { get; private set; }

        public void Show(BattleActor actor)
        {
            Actor = actor;
            PortraitImage.Source = ImageLoader.LoadPortrait(actor.PortraitFile);
            EnemyTint.Visibility = actor is Enemy ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>Lit with a double frame: this actor is acting now.</summary>
        public bool IsCurrent
        {
            get => isCurrent;
            set
            {
                isCurrent = value;
                CurrentFrame.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
                Frame.Stroke = (Brush)FindResource(value ? "Brush.Selection.Stroke" : "Brush.Stroke.Strong");
            }
        }
    }
}
