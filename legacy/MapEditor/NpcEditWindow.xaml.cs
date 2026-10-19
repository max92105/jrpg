using System.Collections.Generic;
using System.Windows;

namespace MapEditor
{
    public partial class NpcEditWindow : Window
    {
        public Dictionary<string, string> ResultMeta { get; private set; }

        public NpcEditWindow(Dictionary<string, string> existingMeta = null)
        {
            InitializeComponent();

            if (existingMeta != null)
            {
                if (existingMeta.TryGetValue("name", out var name))
                    NameBox.Text = name;
                if (existingMeta.TryGetValue("dialogue", out var dialogue))
                    DialogueBox.Text = dialogue;
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            ResultMeta = new Dictionary<string, string>
            {
                ["npc"] = "true",
                ["name"] = NameBox.Text.Trim(),
                ["dialogue"] = DialogueBox.Text.Trim()
            };
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
