using JRPG.Core.Maps;
using JRPG.Core.Triggers.Models;
using JRPG.Core.Utils;
using Microsoft.Win32;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Trigger = JRPG.Core.Triggers.Models.Trigger;

namespace MapEditor
{
    public partial class TriggerEditWindow : Window
    {
        public Trigger Result { get; private set; }

        public TriggerEditWindow(Trigger existing = null)
        {
            InitializeComponent();

            if (existing is TriggerWarp warp)
            {
                TriggerTypeBox.SelectedIndex = 1;
                WarpMapBox.Text = warp.TargetMap;
                WarpXBox.Text = warp.TargetX.ToString();
                WarpYBox.Text = warp.TargetY.ToString();
            }
            else if (existing is TriggerDialogue dialogue)
            {
                DialogueSpeakerBox.Text = dialogue.SpeakerName ?? "";
                DialoguePortraitBox.Text = dialogue.PortraitPath ?? "";

                DialogueLinesBox.Items.Clear();
                foreach (var line in dialogue.Lines)
                    DialogueLinesBox.Items.Add(line);
            }
            else if (existing is TriggerItem item)
            {
                TriggerTypeBox.SelectedIndex = 3;
                ItemBox.Text = item.ItemId;
            }
            else
            {
                TriggerTypeBox.SelectedIndex = 0;
            }
        }

        private void TriggerTypeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            WarpPanel.Visibility = Visibility.Collapsed;
            DialoguePanel.Visibility = Visibility.Collapsed;
            ItemPanel.Visibility = Visibility.Collapsed;

            string selected = (TriggerTypeBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            if (selected == "Warp") WarpPanel.Visibility = Visibility.Visible;
            else if (selected == "Dialogue") DialoguePanel.Visibility = Visibility.Visible;
            else if (selected == "Item") ItemPanel.Visibility = Visibility.Visible;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            string selected = (TriggerTypeBox.SelectedItem as ComboBoxItem)?.Tag?.ToString();

            try
            {
                switch (selected)
                {
                    case "Warp":
                        if (string.IsNullOrWhiteSpace(WarpMapBox.Text)) throw new Exception("Target map required");
                        if (!int.TryParse(WarpXBox.Text, out int x)) throw new Exception("Invalid X");
                        if (!int.TryParse(WarpYBox.Text, out int y)) throw new Exception("Invalid Y");

                        Result = new TriggerWarp
                        {
                            TargetMap = WarpMapBox.Text.Trim(),
                            TargetX = x,
                            TargetY = y
                        };
                        break;

                    case "Dialogue":
                        var dialogue = new TriggerDialogue
                        {
                            SpeakerName = DialogueSpeakerBox.Text.Trim(),
                            PortraitPath = DialoguePortraitBox.Text.Trim(),
                            Lines = DialogueLinesBox.Items.Cast<string>().ToList()
                        };
                        Result = dialogue;
                        break;

                    case "Item":
                        if (string.IsNullOrWhiteSpace(ItemBox.Text)) throw new Exception("Item name required");

                        Result = new TriggerItem
                        {
                            ItemId = ItemBox.Text.Trim()
                        };
                        break;
                }

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void SelectMapFile_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Map Files (*.json)|*.json",
                Title = "Select Target Map",
                InitialDirectory = Path.Combine(AssetPathResolver.GetPath(MapConstant.MapFolderPath)),
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.SafeFileName;
                string baseDir = Directory.GetCurrentDirectory();

                // Store a relative path if possible
                if (selectedPath.StartsWith(baseDir))
                {
                    selectedPath = selectedPath.Substring(baseDir.Length).TrimStart('\\', '/');
                }

                WarpMapBox.Text = selectedPath.Replace("\\", "/"); // Normalize for consistency
            }
        }

        private void SelectPortrait_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "PNG Files (*.png)|*.png",
                Title = "Select Portrait",
                InitialDirectory = AssetPathResolver.GetAssetsRootPath()
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.SafeFileName;
                string baseDir = Directory.GetCurrentDirectory();

                if (selectedPath.StartsWith(baseDir))
                {
                    selectedPath = selectedPath.Substring(baseDir.Length).TrimStart('\\', '/');
                }

                DialoguePortraitBox.Text = selectedPath.Replace("\\", "/");
            }
        }

        private void AddDialogueLine_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(NewDialogueLineBox.Text))
            {
                DialogueLinesBox.Items.Add(NewDialogueLineBox.Text.Trim());
                NewDialogueLineBox.Text = "";
            }
        }

        private void RemoveDialogueLine_Click(object sender, RoutedEventArgs e)
        {
            if (DialogueLinesBox.SelectedItem != null)
            {
                DialogueLinesBox.Items.Remove(DialogueLinesBox.SelectedItem);
            }
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}