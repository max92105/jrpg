using System;
using System.Windows;
using System.Windows.Controls;

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
        }

        private void OnCommandClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                string command = btn.Content.ToString();
                if (command.Contains(" ")) 
                    command = command.Split(' ')[1];
                OnCommandSelected?.Invoke(command);
            }
        }
    }
}
