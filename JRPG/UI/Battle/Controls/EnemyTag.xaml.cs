using JRPG.Core.Battles.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>An enemy's name and HP bar, shown under its sprite.</summary>
    public partial class EnemyTag : UserControl
    {
        private static readonly Duration BarDuration = TimeSpan.FromMilliseconds(300);

        public EnemyTag(Enemy enemy)
        {
            InitializeComponent();

            NameText.Text = enemy.Name;
            HPBar.Maximum = Math.Max(1, enemy.MaxHP);
            HPBar.Value = enemy.HP;
        }

        /// <summary>Slides the bar to the enemy's current HP.</summary>
        public void Refresh(Enemy enemy)
        {
            HPBar.BeginAnimation(ProgressBar.ValueProperty, new DoubleAnimation(enemy.HP, BarDuration));
        }
    }
}
