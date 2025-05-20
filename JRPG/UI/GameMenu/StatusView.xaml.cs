using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace JRPG.UI.GameMenu
{
    public partial class StatusView : UserControl, IFilterable<Character>
    {
        private Character selectedCharacter;

        public StatusView(Character selectedCharacter = null)
        {
            InitializeComponent();

            if (selectedCharacter != null)
            {
                ApplyFilter(selectedCharacter);
            }
            else
            {
                var firstCharacter = GameSystems.Party.AllCharacters.FirstOrDefault();
                if (firstCharacter != null)
                {
                    ApplyFilter(firstCharacter);
                }
            }
        }

        public void ApplyFilter(Character character)
        {
            if (character == null) return;
            selectedCharacter = character;
            RefreshUI();
        }

        private void RefreshUI()
        {
            if (selectedCharacter == null) return;

            CharacterNameBlock.Text = selectedCharacter.Name;

            LevelBlock.Text = $"{selectedCharacter.Level}";
            CurrentXPBlock.Text = $"{selectedCharacter.EXP}";
            XPToNextLevelBlock.Text = $"{selectedCharacter.EXPToNextLevel}";
            ClassBlock.Text = $"{selectedCharacter.Class}";

            UpdateXpBar();

            // Clear and rebuild stats
            cStatsPanel.Load(selectedCharacter);

            SkillsPanel.Children.Clear();

            var skills = selectedCharacter.Skills.ToList();
            for (int i = 0; i < skills.Count; i += 2)
            {
                var left = skills[i];
                var right = (i + 1 < skills.Count) ? skills[i + 1] : null;

                AddSkillRow(
                    leftSkillName: left.Name,
                    leftIcon: left.IconFile,
                    rightSkillName: right?.Name,
                    rightIcon: right?.IconFile
                );
            }
        }

        private void UpdateXpBar()
        {
            // Set max before animating
            XpBar.Maximum = selectedCharacter.EXPToNextLevel;

            // Reset any previous animations and start from 0
            XpBar.BeginAnimation(ProgressBar.ValueProperty, null);
            XpBar.Value = 0;

            var animation = new DoubleAnimation
            {
                From = 0,
                To = selectedCharacter.EXP,
                Duration = TimeSpan.FromMilliseconds(400),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            XpBar.BeginAnimation(ProgressBar.ValueProperty, animation);
        }

        private void AddSkillRow(string leftSkillName, string leftIcon = null, string rightSkillName = null, string rightIcon = null)
        {
            var rowGrid = new Grid
            {
                Margin = new Thickness(0, 4, 0, 4),
                VerticalAlignment = VerticalAlignment.Center
            };

            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) }); // Left icon
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Left name
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(20) }); // Spacer
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(24) }); // Right icon
            rowGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }); // Right name

            var leftIconImg = VisualFactory.CreateIcon(leftIcon);
            Grid.SetColumn(leftIconImg, 0);
            rowGrid.Children.Add(leftIconImg);

            var leftLabel = VisualFactory.CreateText(leftSkillName);
            leftLabel.Margin = new Thickness(4, 0, 0, 0);
            Grid.SetColumn(leftLabel, 1);
            rowGrid.Children.Add(leftLabel);

            var rightIconImg = VisualFactory.CreateIcon(rightIcon);
            Grid.SetColumn(rightIconImg, 3);
            rowGrid.Children.Add(rightIconImg);

            var rightLabel = VisualFactory.CreateText(rightSkillName ?? "");
            rightLabel.Margin = new Thickness(4, 0, 0, 0);
            Grid.SetColumn(rightLabel, 4);
            rowGrid.Children.Add(rightLabel);

            SkillsPanel.Children.Add(rowGrid);
        }
    }
}
