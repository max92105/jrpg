using JRPG.Core.Maps.Models;
using JRPG.Core.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MapEditor.Controls
{
    /// <summary>
    /// Interaction logic for PropertyEditor.xaml
    /// </summary>
    public partial class PropertyEditor : UserControl
    {
        public PropertyEditor()
        {
            InitializeComponent();
        }

        public event Action<TileDefinition> RequestEditTileCollision;
        public event Action<ObjectDefinition> RequestEditObjectCollision;

        public void LoadTileDefinition(TileDefinition def, Action saveCallback)
        {
            FieldsPanel.Children.Clear();

            AddText("Name", def.Name, v => def.Name = v);
            AddText("Sprite", def.Sprite, v => def.Sprite = v);
            AddText("Tilesheet", def.Tilesheet, v => def.Tilesheet = v);
            AddText("Frame Duration", def.FrameDuration.ToString(), v =>
            {
                if (double.TryParse(v, out var d)) def.FrameDuration = d;
            });
            AddText("Sheet X", def.SheetX?.ToString() ?? "", v =>
            {
                if (int.TryParse(v, out var i)) def.SheetX = i;
            });
            AddText("Sheet Y", def.SheetY?.ToString() ?? "", v =>
            {
                if (int.TryParse(v, out var i)) def.SheetY = i;
            });

            AddCheckbox("Walkable", def.IsWalkable, b => def.IsWalkable = b);

            var btnTileCollision = new Button
            {
                Content = "Edit Tile Collision",
                Margin = new Thickness(0, 10, 0, 0)
            };
            btnTileCollision.Click += (s, e) => RequestEditTileCollision?.Invoke(def);
            FieldsPanel.Children.Add(btnTileCollision);

            void Save() => saveCallback?.Invoke();
            void AddText(string label, string value, Action<string> update)
            {
                var box = new TextBox { Text = value, Margin = new Thickness(0, 5, 0, 10) };
                box.TextChanged += (s, e) => { update(box.Text); Save(); };
                FieldsPanel.Children.Add(new TextBlock { Text = label, Foreground = Brushes.White });
                FieldsPanel.Children.Add(box);
            }

            void AddCheckbox(string label, bool value, Action<bool> update)
            {
                var cb = new CheckBox { Content = label, IsChecked = value, Foreground = Brushes.White };
                cb.Checked += (s, e) => { update(true); Save(); };
                cb.Unchecked += (s, e) => { update(false); Save(); };
                FieldsPanel.Children.Add(cb);
            }
        }

        public void LoadObjectDefinition(ObjectDefinition def, Action saveCallback)
        {
            FieldsPanel.Children.Clear();
            AddText("Name", def.Name, v => def.Name = v);
            AddText("Category", def.Category, v => def.Category = v);
            AddText("Width", def.Width.ToString(), v => { if (int.TryParse(v, out var w)) def.Width = w; });
            AddText("Height", def.Height.ToString(), v => { if (int.TryParse(v, out var h)) def.Height = h; });

            var btnObjectCollision = new Button
            {
                Content = "Edit Object Collision",
                Margin = new Thickness(0, 10, 0, 0)
            };
            btnObjectCollision.Click += (s, e) => RequestEditObjectCollision?.Invoke(def);
            FieldsPanel.Children.Add(btnObjectCollision);

            void Save() => saveCallback?.Invoke();
            void AddText(string label, string value, Action<string> update)
            {
                var box = new TextBox { Text = value, Margin = new Thickness(0, 5, 0, 10) };
                box.TextChanged += (s, e) => { update(box.Text); Save(); };
                FieldsPanel.Children.Add(new TextBlock { Text = label, Foreground = Brushes.White });
                FieldsPanel.Children.Add(box);
            }
        }
    }
}
