using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.UI.GameMenu.Controls;
using JRPG.Utils;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace JRPG.UI.GameMenu
{
    /// <summary>Status page: profile, experience, parameters and skills. The category tabs pick the character.</summary>
    public partial class StatusView : MenuPage, IFilterable<Character>
    {
        private static readonly Duration XpBarFillDuration = TimeSpan.FromMilliseconds(400);

        private Character character;

        public StatusView()
        {
            InitializeComponent();
            ApplyFilter(GameSystems.Party.AllCharacters.FirstOrDefault());
        }

        public void ApplyFilter(Character newCharacter)
        {
            if (newCharacter == null)
                return;

            character = newCharacter;
            Refresh();
        }

        private void Refresh()
        {
            CharacterNameText.Text = character.Name;
            ClassText.Text = character.Class;
            PortraitImage.Source = ImageLoader.LoadPortrait(character.PortraitFile);

            LevelText.Text = character.Level.ToString();
            ExpText.Text = character.EXP.ToString();
            NextLevelText.Text = Math.Max(0, character.EXPToNextLevel - character.EXP).ToString();
            AnimateXpBar();

            Stats.Load(character);
            ShowSkills();
        }

        private void AnimateXpBar()
        {
            XpBar.Maximum = character.EXPToNextLevel;
            XpBar.BeginAnimation(ProgressBar.ValueProperty, new DoubleAnimation(0, character.EXP, XpBarFillDuration)
            {
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            });
        }

        private void ShowSkills()
        {
            SkillsPanel.Children.Clear();

            foreach (var skill in character.Skills)
            {
                SkillsPanel.Children.Add(new ListRow
                {
                    Icon = ImageLoader.LoadIcon(skill.IconFile),
                    Text = skill.Name,
                    Value = skill.MPCost.ToString(),
                    Margin = new Thickness(0, 0, 16, 0)
                });
            }

            NoSkillsText.Visibility = SkillsPanel.Children.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
