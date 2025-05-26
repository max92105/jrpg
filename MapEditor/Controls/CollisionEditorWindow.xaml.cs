using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MapEditor.Controls
{
    public partial class CollisionEditorWindow : Window
    {
        private readonly List<Point> polygon;

        public List<Point> Result => Viewer.GetCollisionPolygon();

        public CollisionEditorWindow(List<Point> existing)
        {
            InitializeComponent();
            polygon = new List<Point>(existing);
        }

        public void SetImageAndPolygon(ImageSource image, List<Point> existing)
        {
            Viewer.SetImage(image);
            Viewer.EnableCollisionPolygonEdit(existing);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
