using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;

namespace MapEditor.Controls
{
    /// <summary>
    /// Interaction logic for ZEditPopup.xaml
    /// </summary>
    public partial class ZEditPopup : Window
    {
        public int Result { get; private set; }

        public ZEditPopup(int currentZ)
        {
            InitializeComponent();
            ZInput.Text = currentZ.ToString();
            ZInput.Focus();
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(ZInput.Text, out int z))
            {
                Result = z;
                DialogResult = true;
            }
            else
            {
                MessageBox.Show("Invalid number.");
            }
        }
    }
}
