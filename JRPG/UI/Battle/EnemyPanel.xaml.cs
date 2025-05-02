using JRPG.Core.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace JRPG
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
            EnemyHPBar.Value = enemy.CurrentHP;

            if (!string.IsNullOrEmpty(enemy.PortraitPath) && File.Exists(AssetPathResolver.GetPathPortrait(enemy.PortraitPath)))
            {
                PortraitImage.Source = new BitmapImage(new Uri(AssetPathResolver.GetPathPortrait(enemy.PortraitPath)));
            }

            Background = isSelected
                ? new SolidColorBrush(Color.FromRgb(180, 60, 60))
                : new SolidColorBrush(Color.FromRgb(60, 0, 0));
        }
    }
}
