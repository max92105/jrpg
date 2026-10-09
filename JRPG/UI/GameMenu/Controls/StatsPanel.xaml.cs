using JRPG.Core.Battles.Models;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Utils;
using JRPG.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JRPG.UI.GameMenu.Controls
{
    public partial class StatsPanel : UserControl
    {
        public StatsPanel()
        {
            InitializeComponent();
        }

        public void Load(Character character)
        {
            StatsRoot.Children.Clear();

            if (character == null)
                return;

            AddStatRow("Maximum HP", character.HP, null, "Maximum MP", character.MP, null);
            AddStatRow("Attack", character.GetTotalAttack(), "Attack.png", "Defense", character.GetTotalDefense(), "Defend.png");
        }

        public void PreviewStatChange(Character character, Equipment equipment, bool isWeapon)
        {
            if (character == null || equipment == null)
                return;

            int baseStat = isWeapon ? character.BaseAttack : character.BaseDefense;
            int currentStat = isWeapon ? character.GetTotalAttack() : character.GetTotalDefense();
            int newBonus = isWeapon
                ? ((Weapon)equipment).AttackBonus
                : ((Armor)equipment).DefenseBonus;

            int newStat = baseStat + newBonus;
            int diff = newStat - currentStat;
            string label = isWeapon ? "Attack" : "Defense";

            foreach (var grid in StatsRoot.Children.OfType<Grid>())
            {
                int[] labelColumns = { 1, 5 };
                int[] valueColumns = { 2, 6 };

                for (int i = 0; i < labelColumns.Length; i++)
                {
                    var labelBlock = grid.Children.OfType<TextBlock>()
                        .FirstOrDefault(tb => Grid.GetColumn(tb) == labelColumns[i] && tb.Text == label);

                    if (labelBlock == null)
                        continue;

                    var valueBlock = grid.Children.OfType<TextBlock>()
                        .FirstOrDefault(tb => Grid.GetColumn(tb) == valueColumns[i]);

                    if (valueBlock != null)
                    {
                        valueBlock.Inlines.Clear();
                        valueBlock.Inlines.Add(new Run($"{currentStat} ") { Foreground = Brushes.White });
                        valueBlock.Inlines.Add(new Run($"→ {newStat} ") { Foreground = Brushes.White });

                        if (diff != 0)
                        {
                            string arrow = diff > 0 ? "▲" : "▼";
                            string sign = diff > 0 ? "+" : "";
                            Brush color = diff > 0 ? Brushes.LightGreen : Brushes.IndianRed;

                            valueBlock.Inlines.Add(new Run($"{arrow} ({sign}{diff})") { Foreground = color });
                        }
                        else
                        {
                            valueBlock.Inlines.Add(new Run("(No Change)") { Foreground = Brushes.Gray });
                        }
                    }

                    break; // Only one match needed
                }
            }
        }

        private void AddStatRow(
            string leftLabel, int leftValue, string leftIcon = null,
            string rightLabel = null, int? rightValue = null, string rightIcon = null)
        {
            var rowGrid = new Grid
            {
                Margin = new Thickness(0, 4, 0, 4),
                VerticalAlignment = VerticalAlignment.Center
            };

            for (int i = 0; i < 7; i++)
            {
                var width = (i == 0 || i == 4) ? new GridLength(24) :
                            (i == 3) ? new GridLength(10) :
                            new GridLength(1, GridUnitType.Star);
                rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = width });
            }

            AddElement(rowGrid, CreateIcon(leftIcon), 0);
            AddElement(rowGrid, CreateText(leftLabel), 1);
            AddElement(rowGrid, CreateText(leftValue.ToString(), 18, Brushes.White, HorizontalAlignment.Center), 2);
            AddElement(rowGrid, CreateIcon(rightIcon), 4);

            if (!string.IsNullOrEmpty(rightLabel))
                AddElement(rowGrid, CreateText(rightLabel), 5);

            if (rightValue.HasValue)
                AddElement(rowGrid, CreateText(rightValue.Value.ToString(), 18, Brushes.White, HorizontalAlignment.Center), 6);

            StatsRoot.Children.Add(rowGrid);
        }

        private static void AddElement(Grid grid, UIElement element, int column)
        {
            Grid.SetColumn(element, column);
            grid.Children.Add(element);
        }

        private static Image CreateIcon(string path)
        {
            return new Image
            {
                Width = 24,
                Height = 24,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = string.IsNullOrEmpty(path) ? 0 : 1,
                Source = string.IsNullOrEmpty(path) ? null : ImageLoader.LoadIcon(path)
            };
        }

        private static TextBlock CreateText(string text, int fontSize = 16, Brush color = null, HorizontalAlignment align = HorizontalAlignment.Left)
        {
            return new TextBlock
            {
                Text = text,
                FontSize = fontSize,
                FontWeight = FontWeights.Light,
                Foreground = color ?? Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = align,
                Margin = new Thickness(0, 0, 4, 0)
            };
        }
    }
}
