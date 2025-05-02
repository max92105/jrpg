using JRPG.Core.Utils;
using System;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>
    /// Interaction logic for CharacterPanel.xaml
    /// </summary>
    public partial class CharacterPanel : UserControl
    {
        private int hp, maxHp, mp, maxMp;

        public CharacterPanel()
        {
            InitializeComponent();
        }

        public string Portrait
        {
            set
            {
                if (!string.IsNullOrEmpty(value) && File.Exists(AssetPathResolver.GetPortraitPath(value)))
                {
                    PortraitImage.Source = new BitmapImage(new Uri(AssetPathResolver.GetPortraitPath(value)));
                }
            }
        }

        public string CharacterName
        {
            get => NameText.Text;
            set => NameText.Text = value;
        }

        public int Level
        {
            set => LevelText.Text = $"Lv {value}";
        }

        public int CurrentHP
        {
            get => hp;
            set { hp = value; UpdateHPBar(); }
        }

        public int MaxHP
        {
            get => maxHp;
            set { maxHp = value; UpdateHPBar(); }
        }

        public int CurrentMP
        {
            get => mp;
            set { mp = value; UpdateMPBar(); }
        }

        public int MaxMP
        {
            get => maxMp;
            set { maxMp = value; UpdateMPBar(); }
        }

        public bool IsActive
        {
            set => BorderBrush = value ? Brushes.Gold : Brushes.White;
        }

        private void UpdateHPBar()
        {
            HPBar.Maximum = maxHp;
            HPBar.Value = Math.Min(hp, maxHp);
        }

        private void UpdateMPBar()
        {
            MPBar.Maximum = maxMp;
            MPBar.Value = Math.Min(mp, maxMp);
        }
    }
}
