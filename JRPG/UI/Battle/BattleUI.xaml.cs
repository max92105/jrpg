using JRPG.Core.Battles.Enum;
using JRPG.Core.Battles.Models;
using JRPG.Core.Utils;
using JRPG.Enums;
using JRPG.UI.Battle.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
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

        private bool isTargeting = false;
        private string targetingAction = null;
        private Dictionary<Enemy, Image> enemyTargetArrows = new Dictionary<Enemy, Image>();
        private Enemy currentlySelectedEnemy = null;
        private BitmapImage targetArrowImage = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath("TargetArrow.png"))); // replace with your arrow filename

        private Image hoverArrow = null;
        private Enemy lastHoveredEnemy;
        private Image glowingEnemyImage;

        private readonly DropShadowEffect glowEffect = new DropShadowEffect
        {
            Color = Colors.White,
            BlurRadius = 20,
            ShadowDepth = 0,
            Opacity = 0.8
        };

        public event Action OnBattleEnd;

        public BattleUI(List<Character> characters, List<Enemy> enemies, List<InventoryItem> inventory)
        {
            InitializeComponent();
            this.characters = characters;
            this.enemies = enemies;
            this.inventory = inventory;
            battle = new BattleManager(characters, enemies);

            UpdateUI();
            CheckNextState();
        }

        private void UpdateUI()
        {
            CommandBox.IsEnabled = (battle.State == BattleState.PlayerTurn);
            CommandBox.Visibility = (battle.State == BattleState.PlayerTurn) ? Visibility.Visible : Visibility.Collapsed;
            UpdateOnlyPLayerUI();
            DrawSprites();
            UpdateTurnOrderDisplay();
        }

        private void UpdateOnlyPLayerUI()
        {
            RefreshPanelsOnly();
            UpdateCommandBoxPosition();
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

        private void UpdateCommandBoxPosition()
        {
            if (!battle.CurrentTurn.IsPlayer)
            {
                CommandBox.Visibility = Visibility.Collapsed;
                return;
            }

            var activeEntry = battle.CurrentTurn;
            object player = (object)activeEntry.Character;

            if (!spriteImages.TryGetValue(player, out var playerImage))
            {
                CommandBox.Visibility = Visibility.Collapsed;
                return;
            }

            // Get the position of the playerImage relative to the SpriteCanvas
            Point playerPosition = playerImage.TranslatePoint(new Point(0, 0), SpriteCanvas);

            double offsetX = - (playerImage.ActualWidth + 50); // sprite + movement
            double offsetY = (playerImage.ActualHeight - 300) / 2; // Center vertically

            Canvas.SetLeft(CommandBox, playerPosition.X + offsetX);
            Canvas.SetTop(CommandBox, playerPosition.Y + offsetY);

            CommandBox.Visibility = Visibility.Visible;
        }

        private void RefreshPanelsOnly()
        {
            CommandBox.IsEnabled = (battle.State == BattleState.PlayerTurn);
            CommandBox.Visibility = (battle.State == BattleState.PlayerTurn) ? Visibility.Visible : Visibility.Collapsed;
            UpdateCharacterPanel();
            UpdateTurnOrderDisplay();
        }

        private void DrawSprites()
        {
            SpriteCanvas.Children.Clear();
            EffectCanvas.Children.Clear();
            spriteOffsets.Clear();
            spriteImages.Clear();

            double canvasWidth = SpriteCanvas.ActualWidth > 0 ? SpriteCanvas.ActualWidth : 1920;
            double spriteSize = 96;
            double verticalSpacing = spriteSize + 40;

            // Draw Enemies
            double enemyBaseY = 120;
            double enemyStartX = 200;
            var aliveEnemies = battle.GetAliveEnemies();

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                var enemy = aliveEnemies[i];
                var image = CreateSpriteImage(AssetPathResolver.GetSpritesPath(enemypath), true, enemy);

                // Mouse hover = show target arrow
                image.MouseEnter += (s, e) =>
                {
                    if (!isTargeting) return;
                    ShowHoverArrowFor(enemy);
                    currentlySelectedEnemy = enemy;
                    HighlightSelectedEnemyInTurnOrder();
                };

                // Click = confirm target
                image.MouseLeftButtonUp += (s, e) =>
                {
                    if (!isTargeting || currentlySelectedEnemy == null) return;

                    string result = battle.ExecuteCurrentTurnAction(targetingAction, currentlySelectedEnemy);
                    MessageBox.Show(result);

                    AnimateStepBack(() =>
                    {
                        ExitTargetingMode();
                        battle.AdvanceTurn();
                        UpdateUI();
                        CheckNextState();
                    });
                };

                Canvas.SetLeft(image, enemyStartX + (i * 50));
                Canvas.SetBottom(image, enemyBaseY + (i * verticalSpacing));
                SpriteCanvas.Children.Add(image);
            }

            // Draw Players
            double playerBaseY = 120;
            double playerStartX = canvasWidth - (200 + spriteSize);

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

            // Prepare glow effect
            var glow = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.White,
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            image.MouseEnter += (s, e) =>
            {
                if (isTargeting && owner is Enemy enemy)
                {
                    if (glowingEnemyImage != null && glowingEnemyImage != image)
                        glowingEnemyImage.Effect = null;

                    glowingEnemyImage = image;
                    ApplyPulsingGlow(image);
                    ShowHoverArrowFor(enemy);
                }
            };

            // Do not remove the glow on mouse leave anymore

            spriteOffsets[owner] = translate;
            spriteImages[owner] = image;
            return image;
        }

        private void ApplyPulsingGlow(Image target)
        {
            var glow = new System.Windows.Media.Effects.DropShadowEffect
            {
                Color = Colors.White,
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            target.Effect = glow;

            // Animate the glow's BlurRadius and Opacity in a loop
            var blurAnim = new DoubleAnimation
            {
                From = 20,
                To = 40,
                Duration = TimeSpan.FromMilliseconds(800),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            var opacityAnim = new DoubleAnimation
            {
                From = 0.8,
                To = 0.4,
                Duration = TimeSpan.FromMilliseconds(800),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            glow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.BlurRadiusProperty, blurAnim);
            glow.BeginAnimation(System.Windows.Media.Effects.DropShadowEffect.OpacityProperty, opacityAnim);
        }

        private void AnimateStepForward(Action onComplete = null)
        {
            var activeEntry = battle.CurrentTurn;
            object actor = activeEntry.IsPlayer ? (object)activeEntry.Character : activeEntry.Enemy;

            if (!spriteOffsets.TryGetValue(actor, out var translate))
            {
                onComplete?.Invoke();
                return;
            }

            double target = activeEntry.IsPlayer ? -50 : 50;

            var slideIn = new DoubleAnimation
            {
                To = target,
                Duration = TimeSpan.FromMilliseconds(200),
                FillBehavior = FillBehavior.Stop
            };

            slideIn.Completed += (s, e) =>
            {
                // ✅ force the final value so that step back animates from correct position
                translate.X = target;
                onComplete?.Invoke();
            };

            translate.BeginAnimation(TranslateTransform.XProperty, slideIn);
        }

        private void AnimateStepBack(Action onComplete = null)
        {
            var activeEntry = battle.CurrentTurn;
            object actor = activeEntry.IsPlayer ? (object)activeEntry.Character : activeEntry.Enemy;

            if (!spriteOffsets.TryGetValue(actor, out var translate))
            {
                onComplete?.Invoke();
                return;
            }

            var slideOut = new DoubleAnimation
            {
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200),
                FillBehavior = FillBehavior.HoldEnd
            };

            slideOut.Completed += (s, e) =>
            {
                translate.X = 0; // ensure reset
                onComplete?.Invoke();
            };

            translate.BeginAnimation(TranslateTransform.XProperty, slideOut);
        }

        private void UpdateTurnOrderDisplay()
        {
            TurnOrderPanel.Children.Clear();

            foreach (var turn in battle.TurnQueue)
            {
                // ❌ Skip dead characters
                if (turn.IsPlayer && turn.Character.HP <= 0)
                    continue;

                // ❌ Skip dead enemies
                if (!turn.IsPlayer && turn.Enemy.CurrentHP <= 0)
                    continue;

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

        private void ShowHoverArrowFor(Enemy enemy)
        {
            currentlySelectedEnemy = enemy;

            // Remove the old one if it exists
            if (hoverArrow != null)
            {
                SpriteCanvas.Children.Remove(hoverArrow);
                hoverArrow = null;
            }

            if (!spriteImages.TryGetValue(enemy, out var img))
                return;

            hoverArrow = new Image
            {
                Width = 36,
                Height = 36,
                Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath("TargetArrow.png"), UriKind.Absolute)),
                RenderTransformOrigin = new Point(0.5, 0.5),
                Opacity = 0,
                RenderTransform = new ScaleTransform(0.5, 0.5)
            };

            Point center = img.TranslatePoint(new Point(img.Width / 2, 0), SpriteCanvas);
            Canvas.SetLeft(hoverArrow, center.X - (hoverArrow.Width / 2));
            Canvas.SetTop(hoverArrow, center.Y - hoverArrow.Height - 4);

            SpriteCanvas.Children.Add(hoverArrow);

            // Animate "pop"
            var scale = (ScaleTransform)hoverArrow.RenderTransform;
            var grow = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut } };
            var fade = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150));

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
            hoverArrow.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private void HideHoverArrow()
        {
            if (hoverArrow != null)
            {
                SpriteCanvas.Children.Remove(hoverArrow);
                hoverArrow = null;
            }
        }

        private void HighlightSelectedEnemyInTurnOrder()
        {
            for (int i = 0; i < TurnOrderPanel.Children.Count; i++)
            {
                if (TurnOrderPanel.Children[i] is Image img && i < battle.TurnQueue.Count)
                {
                    var entry = battle.TurnQueue.ElementAt(i);
                    img.Opacity = (entry.Enemy == currentlySelectedEnemy) ? 1.0 : 0.5;
                }
            }
        }

        private void ExitTargetingMode()
        {
            isTargeting = false;
            currentlySelectedEnemy = null;
            HideHoverArrow();
            HighlightSelectedEnemyInTurnOrder();
        }

        private void EnterTargetingMode(string action)
        {
            isTargeting = true;
            targetingAction = action;

            // Hide command menu
            CommandBox.Visibility = Visibility.Collapsed;

            // Choose default enemy (e.g., last one alive)
            var enemies = battle.GetAliveEnemies();
            var defaultEnemy = enemies.LastOrDefault();
            if (defaultEnemy == null) return;

            ShowHoverArrowFor(defaultEnemy); // ✅ Shows arrow immediately
            ApplyPulsingGlow(spriteImages[defaultEnemy]); // ✅ Glow on selection
            HighlightSelectedEnemyInTurnOrder();
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            EnterTargetingMode("Attack");
        }

        private void DefendButton_Click(object sender, RoutedEventArgs e)
        {
            string result = battle.ExecuteCurrentTurnAction("Defend");

            MessageBox.Show(result);

            AnimateStepBack(() =>
            {
                battle.AdvanceTurn();
                UpdateUI();
                CheckNextState();
            });
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

                AnimateStepBack(() =>
                {
                    var item = inventory.FirstOrDefault(i => i.Name == itemName);
                    if (item != null)
                    {
                        item.Quantity--;
                        if (item.Quantity <= 0)
                            inventory.Remove(item);
                    }

                    battle.AdvanceTurn();
                    UpdateUI();
                    CheckNextState();
                    window.Close();
                });               
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
                    MessageBox.Show(result);

                    AnimateStepBack(() =>
                    {
                        battle.AdvanceTurn();
                        UpdateUI();
                        CheckNextState();
                        window.Close();
                    });
                }
                else
                {
                    var target = battle.GetAliveEnemies().First(e => e.Name == selected);
                    result = battle.ExecuteCurrentTurnAction("SkillEnemy", Tuple.Create(skillName, target));
                    MessageBox.Show(result);

                    AnimateStepBack(() =>
                    {
                        battle.AdvanceTurn();
                        UpdateUI();
                        CheckNextState();
                        window.Close();
                    });
                }
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
            {
                AnimateStepForward(() =>
                {
                    // Do the action once fully moved forward
                    string result = battle.ExecuteCurrentTurnAction("Auto");
                    MessageBox.Show(result);

                    // Then move back AFTER the action
                    AnimateStepBack(() =>
                    {
                        if (battle.State == BattleState.Lost)
                        {
                            EndBattle("You were defeated.");
                        }
                        else
                        {
                            battle.AdvanceTurn();
                            CheckNextState();
                        }
                    });
                });
            }
            else if (battle.State == BattleState.PlayerTurn)
            {
                // Only move forward, action comes later
                AnimateStepForward(() =>
                {
                    UpdateOnlyPLayerUI();
                });
            }
            else if (battle.State == BattleState.Won)
            {
                EndBattle("You won!");
            }
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
