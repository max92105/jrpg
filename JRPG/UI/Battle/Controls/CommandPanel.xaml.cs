using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using JRPG.Core.Utils;

namespace JRPG.UI.Battle.Controls
{
    /// <summary>
    /// Interaction logic for CommandPanel.xaml
    /// </summary>
    public partial class CommandPanel : UserControl
    {
        public event Action<string> OnCommandSelected;

        public CommandPanel()
        {
            InitializeComponent();
            InitializeCommandPanelIcons();
        }

        private void InitializeCommandPanelIcons()
        {
            SetButtonIcon(img_Attack, "Attack.png");
            SetButtonIcon(img_Defend, "Defend.png");
            SetButtonIcon(img_Skill, "Skill.png");
            SetButtonIcon(img_Item, "Item.png");
            SetButtonIcon(img_Run, "Run.png");
        }

        private void SetButtonIcon(Image icon, string file)
        {
            icon.Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath(file)));
        }

        private void Command_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string command)
            {
                OnCommandSelected?.Invoke(command);
            }
        }
    }
}
