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

        //Targeting System
        private bool isTargeting = false;
        private string targetingAction = null;
        private Skill targetingSkill;
        private InventoryItem targetingItem;
        private Dictionary<Enemy, Image> enemyTargetArrows = new Dictionary<Enemy, Image>();
        private Enemy currentlySelectedEnemy = null;
        private Character currentlySelectedAlly = null;
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

            CommandBox.OnSkillSelected += HandleSkillTargeting;

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

                    if (targetingAction == "SkillAlly" || targetingAction == "SkillSelf" ||
                        targetingAction == "ItemAlly" || targetingAction == "ItemSelf")
                        return; // Block enemy hover in Ally/Self item modes

                    currentlySelectedAlly = null;
                    currentlySelectedEnemy = enemy;

                    HighlightSelectedEnemyInTurnOrder();
                    ShowHoverArrowFor(enemy);
                    ApplyPulsingGlow(image);
                };

                // Click = confirm target
                image.MouseLeftButtonUp += (s, e) =>
                {
                    if (!isTargeting || currentlySelectedEnemy == null) return;

                    // ❌ Block enemy clicks in Self or Ally targeting
                    if (targetingAction == "SkillSelf" || targetingAction == "SkillAlly")
                        return;

                    string result;
                    if (targetingAction == "Attack")
                    {
                        result = battle.ExecuteCurrentTurnAction("Attack", currentlySelectedEnemy);
                    }
                    else if (targetingAction == "SkillEnemy" && targetingSkill != null)
                    {
                        result = battle.ExecuteCurrentTurnAction("SkillEnemy", Tuple.Create(targetingSkill, currentlySelectedEnemy));
                    }
                    else
                    {
                        return; // Unsupported
                    }

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

                image.MouseEnter += (s, e) =>
                {
                    if (!isTargeting) return;

                    if (targetingAction == "SkillAlly" || targetingAction == "SkillSelf")
                    {
                        if (targetingAction == "SkillSelf" && player != battle.CurrentTurn.Character)
                            return; // ❌ Disallow hover over other allies in Self mode

                        currentlySelectedEnemy = null;
                        currentlySelectedAlly = player;

                        HighlightSelectedAllyInTurnOrder();
                        ShowHoverArrowFor(player);
                        ApplyPulsingGlow(image);
                    }
                };

                image.MouseLeftButtonUp += (s, e) =>
                {
                    if (!isTargeting || targetingAction != "SkillAlly" && targetingAction != "SkillSelf")
                        return;

                    // Block changing selection in SkillSelf mode
                    if (targetingAction == "SkillSelf" && player != battle.CurrentTurn.Character)
                        return;

                    string result = battle.ExecuteCurrentTurnAction(targetingAction, Tuple.Create(targetingSkill, player));
                    MessageBox.Show(result);

                    AnimateStepBack(() =>
                    {
                        ExitTargetingMode();
                        battle.AdvanceTurn();
                        UpdateUI();
                        CheckNextState();
                    });
                };

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

        private void ApplyPulsingGlow(Image target)
        {
            // Clear old glow
            if (glowingEnemyImage != null && glowingEnemyImage != target)
            {
                glowingEnemyImage.Effect = null;
                glowingEnemyImage.BeginAnimation(DropShadowEffect.BlurRadiusProperty, null);
                glowingEnemyImage.BeginAnimation(DropShadowEffect.OpacityProperty, null);
            }

            glowingEnemyImage = target;

            var glow = new DropShadowEffect
            {
                Color = Colors.White,
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 0.8
            };

            target.Effect = glow;

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

            glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty, blurAnim);
            glow.BeginAnimation(DropShadowEffect.OpacityProperty, opacityAnim);
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

        private void ShowHoverArrowFor(object target)
        {
            if (!spriteImages.TryGetValue(target, out var img))
                return;

            // Remove old arrow
            if (hoverArrow != null)
            {
                SpriteCanvas.Children.Remove(hoverArrow);
                hoverArrow = null;
            }

            hoverArrow = new Image
            {
                Width = 36,
                Height = 36,
                Source = targetArrowImage,
                RenderTransformOrigin = new Point(0.5, 0.5),
                Opacity = 0,
                RenderTransform = new ScaleTransform(0.5, 0.5)
            };

            Point center = img.TranslatePoint(new Point(img.Width / 2, 0), SpriteCanvas);
            Canvas.SetLeft(hoverArrow, center.X - (hoverArrow.Width / 2));
            Canvas.SetTop(hoverArrow, center.Y - hoverArrow.Height - 4);

            SpriteCanvas.Children.Add(hoverArrow);

            // Animate
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

        private void HighlightSelectedAllyInTurnOrder()
        {
            for (int i = 0; i < TurnOrderPanel.Children.Count; i++)
            {
                if (TurnOrderPanel.Children[i] is Image img && i < battle.TurnQueue.Count)
                {
                    var entry = battle.TurnQueue.ElementAt(i);
                    img.Opacity = (entry.Character == currentlySelectedAlly) ? 1.0 : 0.5;
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

            if (action == "SkillSelf")
            {
                var self = battle.CurrentTurn.Character;

                if (!spriteImages.TryGetValue(self, out var img)) return;

                currentlySelectedEnemy = null; // Not using enemy in this case

                // Show arrow and glow over the player
                ShowHoverArrowFor(self);
                ApplyPulsingGlow(img);

                return; // skip enemy default logic
            }

            if (targetingAction == "SkillAlly" || targetingAction == "SkillSelf")
            {
                var defaultAlly = battle.CurrentTurn.Character;

                currentlySelectedAlly = defaultAlly;
                ShowHoverArrowFor(defaultAlly);
                ApplyPulsingGlow(spriteImages[defaultAlly]);
                HighlightSelectedAllyInTurnOrder();
            }
            else if (targetingAction == "Attack" || targetingAction == "SkillEnemy")
            {
                var defaultEnemy = battle.GetAliveEnemies().LastOrDefault();
                currentlySelectedEnemy = defaultEnemy;
                ShowHoverArrowFor(defaultEnemy);
                ApplyPulsingGlow(spriteImages[defaultEnemy]);
                HighlightSelectedEnemyInTurnOrder();
            }
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

        private void HandleItemTargeting(InventoryItem item)
        {
            targetingItem = item;

            switch (item.TargetType)
            {
                case TargetType.Enemy:
                    EnterTargetingMode("ItemEnemy");
                    break;

                case TargetType.Ally:
                    EnterTargetingMode("ItemAlly");
                    break;

                case TargetType.Self:
                    EnterTargetingMode("ItemSelf");
                    break;

                case TargetType.AllEnemies:
                case TargetType.AllAllies:
                    ExecuteTargetedItem(item, null); // Instant AOE
                    break;
            }
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
                case "Skill":
                    var currentChar = battle.CurrentTurn.Character;
                    if (currentChar.Skills != null && currentChar.Skills.Count > 0)
                        CommandBox.ShowSkills(currentChar.Skills);
                    else
                    {
                        MessageBox.Show($"{currentChar.Name} has no skills!");
                    }
                    break;
                case "Item": UseItemButton_Click(null, null); break;
                case "Run": RunButton_Click(null, null); break;
            }
        }

        private void HandleSkillTargeting(Skill skill)
        {
            targetingSkill = skill;

            switch (skill.TargetType)
            {
                case TargetType.Enemy:
                    EnterTargetingMode("SkillEnemy");
                    break;
                case TargetType.Ally:
                    EnterTargetingMode("SkillAlly");
                    break;
                case TargetType.Self:
                    targetingSkill = skill;
                    EnterTargetingMode("SkillSelf");
                    break;
                    //case TargetType.AllEnemies:
                    //    ExecuteAoESkill(skill, battle.GetAliveEnemies());
                    //    break;
                    //case TargetType.AllAllies:
                    //    ExecuteAoESkill(skill, characters.Where(c => c.HP > 0));
                    //    break;
            }
        }
    }
}
