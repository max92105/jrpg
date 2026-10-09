using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace MapEditor.Controls
{
    /// <summary>Asks for the name and category of an object being created from tiles.</summary>
    public partial class NewObjectWindow : Window
    {
        public string ObjectName { get; private set; }
        public string Category { get; private set; }

        public NewObjectWindow(int tileCount, IEnumerable<string> existingCategories, string defaultCategory)
        {
            InitializeComponent();

            InfoText.Text = $"Creating an object from {tileCount} tile{(tileCount == 1 ? "" : "s")}.";

            foreach (var category in existingCategories.Where(c => !string.IsNullOrWhiteSpace(c)).Distinct().OrderBy(c => c))
                CategoryBox.Items.Add(category);

            CategoryBox.Text = string.IsNullOrWhiteSpace(defaultCategory) ? "General" : defaultCategory;

            Loaded += (s, e) => NameBox.Focus();
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(NameBox.Text))
            {
                MessageBox.Show(this, "Give the object a name.", "New Object", MessageBoxButton.OK, MessageBoxImage.Warning);
                NameBox.Focus();
                return;
            }

            ObjectName = NameBox.Text.Trim();
            Category = string.IsNullOrWhiteSpace(CategoryBox.Text) ? "General" : CategoryBox.Text.Trim();
            DialogResult = true;
        }
    }
}
