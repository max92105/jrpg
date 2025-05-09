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

        public string CharacterName
        {
            get => NameText.Text;
            set => NameText.Text = value;
        }

        public int CurrentHP
        {
            get => hp;
            set { hp = Math.Max(0, value); UpdateHP(); }
        }

        public int MaxHP
        {
            get => maxHp;
            set { maxHp = Math.Max(1, value); UpdateHP(); }
        }

        public int CurrentMP
        {
            get => mp;
            set { mp = Math.Max(0, value); UpdateMP(); }
        }

        public int MaxMP
        {
            get => maxMp;
            set { maxMp = Math.Max(1, value); UpdateMP(); }
        }

        private void UpdateHP()
        {
            HPBar.Maximum = maxHp;
            HPBar.Value = Math.Min(hp, maxHp);
            HPText.Text = $"{hp} / {maxHp}";
        }

        private void UpdateMP()
        {
            MPBar.Maximum = maxMp;
            MPBar.Value = Math.Min(mp, maxMp);
            MPText.Text = $"{mp} / {maxMp}";
        }
    }
}
