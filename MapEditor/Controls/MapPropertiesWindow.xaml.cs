using System.Windows;
using System.Windows.Controls;

namespace MapEditor.Controls
{
    public partial class MapPropertiesWindow : Window
    {
        private readonly int originalWidth;
        private readonly int originalHeight;

        public string MapName { get; private set; }
        public int MapWidth { get; private set; }
        public int MapHeight { get; private set; }
        public bool RandomEncounters { get; private set; }

        public MapPropertiesWindow(string title, string name, int width, int height, bool canRename = true, bool randomEncounters = true)
        {
            InitializeComponent();

            Title = title;
            EncountersBox.IsChecked = randomEncounters;

            if (!canRename)
            {
                NamePanel.Visibility = Visibility.Collapsed;
                RenameHint.Visibility = Visibility.Visible;
            }
            originalWidth = width;
            originalHeight = height;

            NameBox.Text = name;
            WidthBox.Text = width.ToString();
            HeightBox.Text = height.ToString();

            WidthBox.TextChanged += Size_TextChanged;
            HeightBox.TextChanged += Size_TextChanged;

            Loaded += (s, e) =>
            {
                var first = canRename ? NameBox : WidthBox;
                first.Focus();
                first.SelectAll();
            };
        }

        private void Size_TextChanged(object sender, TextChangedEventArgs e)
        {
            bool shrinking = (int.TryParse(WidthBox.Text, out var w) && w < originalWidth)
                          || (int.TryParse(HeightBox.Text, out var h) && h < originalHeight);
            ShrinkWarning.Visibility = shrinking ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(WidthBox.Text, out int width) || !int.TryParse(HeightBox.Text, out int height) ||
                width <= 0 || height <= 0 || width > 1000 || height > 1000)
            {
                MessageBox.Show(this, "Width and height must be whole numbers between 1 and 1000.", "Invalid size",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MapName = string.IsNullOrWhiteSpace(NameBox.Text) ? "New Map" : NameBox.Text.Trim();
            MapWidth = width;
            MapHeight = height;
            RandomEncounters = EncountersBox.IsChecked == true;
            DialogResult = true;
        }
    }
}
