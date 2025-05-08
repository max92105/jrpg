using JRPG.Core.Battles.Models;
using JRPG.Core.Utils;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>
    /// Interaction logic for EnemyPanel.xaml
    /// </summary>
    public partial class EnemyPanel : UserControl
    {
        public EnemyPanel()
        {
            InitializeComponent();
        }

        public void SetEnemy(Enemy enemy, bool isSelected)
        {
            EnemyName.Text = enemy.Name;
            EnemyHPBar.Maximum = enemy.MaxHP;
            EnemyHPBar.Value = enemy.HP;

            if (!string.IsNullOrEmpty(enemy.PortraitFile) && File.Exists(AssetPathResolver.GetPortraitPath(enemy.PortraitFile)))
            {
                PortraitImage.Source = new BitmapImage(new Uri(AssetPathResolver.GetPortraitPath(enemy.PortraitFile)));
            }

            Background = isSelected
                ? new SolidColorBrush(Color.FromRgb(180, 60, 60))
                : new SolidColorBrush(Color.FromRgb(60, 0, 0));
        }
    }
}
