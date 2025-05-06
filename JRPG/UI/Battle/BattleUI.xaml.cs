using JRPG.Core.Battles.Enum;
using JRPG.Core.Battles.Models;
using JRPG.Core.Utils;
using JRPG.Enums;
using JRPG.UI.Battle.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class BattleUI : UserControl
    {
        private BattleManager battle;
        private List<Character> characters;
        private List<Enemy> enemies = new List<Enemy>();
        private List<InventoryItem> inventory;

        private int selectedEnemyIndex = -1;

        private Dictionary<object, TranslateTransform> spriteOffsets = new Dictionary<object, TranslateTransform>();
        private Dictionary<object, Image> spriteImages = new Dictionary<object, Image>();

        private string enemypath = "PlaceholderEnemy.png";
        private string playerpath = "PlaceholderCharacter.png";

        public event Action OnBattleEnd;

        public BattleUI(List<Character> characters, List<Enemy> enemies, List<InventoryItem> inventory)
        {
            InitializeComponent();
            this.characters = characters;
            this.enemies = enemies;
            this.inventory = inventory;
            battle = new BattleManager(characters, enemies);

            UpdateUI();
        }

        private void UpdateUI()
        {
            CommandBox.IsEnabled = (battle.State == BattleState.PlayerTurn);
            CommandBox.Visibility = (battle.State == BattleState.PlayerTurn) ? Visibility.Visible : Visibility.Collapsed;
            UpdateCharacterPanel();
            DrawSprites();
            AnimateTurnIndicator();
            UpdateTurnOrderDisplay();
        }

        private void UpdateCharacterPanel()
        {
            CharacterPanelContainer.Children.Clear();

            foreach (var c in characters)
            {
                var panel = new CharacterPanel
                {
                    Portrait = c.PortraitFile,
                    CharacterName = c.Name,
                    Level = c.Level,
                    CurrentHP = c.HP,
                    MaxHP = c.MaxHP,
                    CurrentMP = c.MP,
                    MaxMP = c.MaxMP,
                    IsActive = battle.CurrentTurn.IsPlayer && battle.CurrentTurn.Character == c
                };
                CharacterPanelContainer.Children.Add(panel);
            }
        }

        private void DrawSprites()
        {
            SpriteCanvas.Children.Clear();
            EffectCanvas.Children.Clear();
            spriteOffsets.Clear();
            spriteImages.Clear();

            double canvasWidth = SpriteCanvas.ActualWidth > 0 ? SpriteCanvas.ActualWidth : 1920;
            double spriteSize = 96;
            double verticalSpacing = spriteSize + 20;

            // Draw Enemies
            double enemyBaseY = 20;
            double enemyStartX = 200;
            var aliveEnemies = battle.GetAliveEnemies();

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                var enemy = aliveEnemies[i];
                var image = CreateSpriteImage(AssetPathResolver.GetSpritesPath(enemypath), true, enemy);

                Canvas.SetLeft(image, enemyStartX + (i * 50));
                Canvas.SetBottom(image, enemyBaseY + (i * verticalSpacing));
                SpriteCanvas.Children.Add(image);
            }

            // Draw Players
            double playerBaseY = 20;
            double playerStartX = canvasWidth - 200;

            for (int i = 0; i < characters.Count; i++)
            {
                var player = characters[i];
                var image = CreateSpriteImage(AssetPathResolver.GetSpritesPath(playerpath), false, player);

                Canvas.SetLeft(image, playerStartX - (i * 50));
                Canvas.SetBottom(image, playerBaseY + (i * verticalSpacing));
                SpriteCanvas.Children.Add(image);
            }
        }

        private Image CreateSpriteImage(string path, bool flip, object owner)
        {
            var translate = new TranslateTransform();
            var rotate = new RotateTransform(0, 48, 48);
            var scale = new ScaleTransform(flip ? -1 : 1, 1, 48, 48);

            var transformGroup = new TransformGroup();
            transformGroup.Children.Add(scale);
            transformGroup.Children.Add(rotate);
            transformGroup.Children.Add(translate);

            var image = new Image
            {
                Width = 96,
                Height = 96,
                Source = new BitmapImage(new Uri(path, UriKind.Absolute)),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = transformGroup
            };

            spriteOffsets[owner] = translate;
            spriteImages[owner] = image;
            return image;
        }

        private void AnimateTurnIndicator()
        {
            var activeEntry = battle.CurrentTurn;
            object actor = activeEntry.IsPlayer ? (object)activeEntry.Character : activeEntry.Enemy;

            if (spriteOffsets.TryGetValue(actor, out var transform))
            {
                var slide = new DoubleAnimation
                {
                    To = activeEntry.IsPlayer ? -50 : 50,
                    Duration = TimeSpan.FromMilliseconds(300),
                    AutoReverse = true,
                    RepeatBehavior = new RepeatBehavior(1)
                };
                transform.BeginAnimation(TranslateTransform.XProperty, slide);
            }
        }

        private void UpdateTurnOrderDisplay()
        {
            TurnOrderPanel.Children.Clear();
            foreach (var turn in battle.TurnQueue)
            {
                var path = turn.IsPlayer ? playerpath : turn.Enemy.PortraitPath;
                var img = new Image
                {
                    Width = 32,
                    Height = 32,
                    Source = new BitmapImage(new Uri(AssetPathResolver.GetSpritesPath(path)))
                };
                TurnOrderPanel.Children.Add(img);
            }
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedEnemyIndex == -1)
            {
                MessageBox.Show("Select an enemy to attack!");
                return;
            }

            var target = battle.GetAliveEnemies()[selectedEnemyIndex];
            string result = battle.ExecuteCurrentTurnAction("Attack", target);

            MessageBox.Show(result);

            battle.AdvanceTurn();
            CheckNextState();
            UpdateUI();

            selectedEnemyIndex = -1;
        }

        private void DefendButton_Click(object sender, RoutedEventArgs e)
        {
            string result = battle.ExecuteCurrentTurnAction("Defend");
            MessageBox.Show(result);

            battle.AdvanceTurn();
            CheckNextState();
            UpdateUI();
        }

        private void UseItemButton_Click(object sender, RoutedEventArgs e)
        {
            if (inventory == null || !inventory.Any(i => i.Quantity > 0))
            {
                MessageBox.Show("No items available!");
                return;
            }

            var window = CreateSelectionWindow("Select Item", inventory.Select(i => $"{i.Name} (x{i.Quantity})"));

            var list = (ListBox)window.Content;
            list.SelectionChanged += (s, ev) =>
            {
                if (list.SelectedItem == null) return;

                string selected = list.SelectedItem.ToString();
                string itemName = selected.Split('(')[0].Trim();

                string result = battle.ExecuteCurrentTurnAction("UseItem", itemName);
                MessageBox.Show(result);
                //ShowFloatingText(result, FloatingTextType.Heal, CharacterSpriteStack.Children[0]);

                var item = inventory.FirstOrDefault(i => i.Name == itemName);
                if (item != null)
                {
                    item.Quantity--;
                    if (item.Quantity <= 0)
                        inventory.Remove(item);
                }

                battle.AdvanceTurn();
                CheckNextState();
                UpdateUI();
                window.Close();
            };

            window.ShowDialog();
        }

        private void SkillButton_Click(object sender, RoutedEventArgs e)
        {
            var currentChar = battle.CurrentTurn.Character;
            if (currentChar.Skills == null || currentChar.Skills.Count == 0)
            {
                MessageBox.Show($"{currentChar.Name} has no skills!");
                return;
            }

            var window = CreateSelectionWindow("Select Skill", currentChar.Skills.Select(s => s));
            var list = (ListBox)window.Content;

            list.SelectionChanged += (s, ev) =>
            {
                if (list.SelectedItem == null) return;

                string selectedSkill = list.SelectedItem.ToString();
                window.Close();
                SelectSkillTarget(selectedSkill);
            };

            window.ShowDialog();
        }

        private void SelectSkillTarget(string skillName)
        {
            bool isHeal = skillName.ToLower().Contains("heal");
            var targets = isHeal
                ? characters.Where(c => c.HP > 0).Select(c => c.Name)
                : battle.GetAliveEnemies().Select(e => e.Name);

            var window = CreateSelectionWindow("Select Target", targets);
            var list = (ListBox)window.Content;

            list.SelectionChanged += (s, ev) =>
            {
                if (list.SelectedItem == null) return;

                string selected = list.SelectedItem.ToString();
                string result = "";

                if (isHeal)
                {
                    var target = characters.First(c => c.Name == selected);
                    result = battle.ExecuteCurrentTurnAction("SkillAlly", Tuple.Create(skillName, target));

                    var index = characters.IndexOf(target);
                    //ShowFloatingText("50", FloatingTextType.Heal, CharacterSpriteStack.Children[index]);
                }
                else
                {
                    var target = battle.GetAliveEnemies().First(e => e.Name == selected);
                    result = battle.ExecuteCurrentTurnAction("SkillEnemy", Tuple.Create(skillName, target));

                    //ShowFloatingText(result, FloatingTextType.Damage, EnemySpriteStack.Children[index]);
                }

                battle.AdvanceTurn();
                CheckNextState();
                UpdateUI();
                window.Close();
            };

            window.ShowDialog();
        }

        private Window CreateSelectionWindow(string title, IEnumerable<string> items)
        {
            var list = new ListBox
            {
                Background = Brushes.Black,
                Foreground = Brushes.White,
                ItemsSource = items.ToList()
            };

            return new Window
            {
                Title = title,
                Width = 300,
                Height = 400,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = Brushes.Black,
                Owner = Application.Current.MainWindow,
                Content = list
            };
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            EndBattle("You ran away!");
        }

        private void EnemyTurn()
        {
            string result = battle.ExecuteCurrentTurnAction("Auto"); // "Auto" is a placeholder; ignored for enemies
            MessageBox.Show(result);

            if (battle.State == BattleState.Lost)
            {
                EndBattle("You were defeated.");
                return;
            }

            battle.AdvanceTurn();
            CheckNextState();
            UpdateUI();
        }

        private void CheckNextState()
        {
            if (battle.State == BattleState.EnemyTurn)
                EnemyTurn();
            else if (battle.State == BattleState.Won)
                EndBattle("You won!");
        }

        private void EndBattle(string message)
        {
            MessageBox.Show(message);

            if (battle.State == BattleState.Won)
            {
                GrantBattleRewards(120, 50); // EXP and Gold reward
            }

            OnBattleEnd?.Invoke();
        }

        private void GrantBattleRewards(int expReward, int goldReward)
        {
            List<string> leveledUpNames = new List<string>();

            foreach (var character in characters)
            {
                int oldLevel = character.Level;
                character.GainEXP(expReward);
                if (character.Level > oldLevel)
                    leveledUpNames.Add(character.Name);
            }

            string message = $"Victory!\nGained {expReward} EXP and {goldReward} Gold.";

            if (leveledUpNames.Count > 0)
            {
                message += "\n\nLevel Up!\n";
                foreach (var name in leveledUpNames)
                    message += $"- {name}\n";
            }

            MessageBox.Show(message);
        }       

        private void HandleCommandSelected(string command)
        {
            switch (command)
            {
                case "Attack": AttackButton_Click(null, null); break;
                case "Defend": DefendButton_Click(null, null); break;
                case "Skill": SkillButton_Click(null, null); break;
                case "Item": UseItemButton_Click(null, null); break;
                case "Run": RunButton_Click(null, null); break;
            }
        }
    }
}
