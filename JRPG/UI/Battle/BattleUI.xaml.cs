using JRPG.Core.Battles.Enum;
using JRPG.Core.Battles.Models;
using JRPG.Core.Utils;
using JRPG.Enums;
using JRPG.UI.Battle.Controls;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using Image = System.Windows.Controls.Image;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// Interaction logic for BattleUI.xaml
    /// </summary>
    public partial class BattleUI : UserControl
    {
        private BattleManager battleManager;
        private List<BattleActor> battleActors;

        private List<InventoryItem> inventory;

        private Dictionary<object, TranslateTransform> spriteOffsets = new Dictionary<object, TranslateTransform>();
        private Dictionary<object, Image> spriteImages = new Dictionary<object, Image>();

        //Targeting System
        private bool isTargeting = false;
        private TargetType? TargetType = null;
        private CombatAction? CombatAction = null;
        private object targetingParam;

        private List<BattleActor> currentlySelected = new List<BattleActor>();

        private BitmapImage targetArrowImage = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath("TargetArrow.png"))); // replace with your arrow filename
        private HashSet<Image> glowingSprites = new HashSet<Image>();
        private HashSet<Image> activeTargetArrows = new HashSet<Image>();

        public event Action OnBattleEnd;

        public BattleUI(List<BattleActor> battleActors , List<InventoryItem> inventory)
        {
            InitializeComponent();

            this.battleActors = battleActors;
            this.inventory = inventory;

            battleManager = new BattleManager(battleActors);

            HookCommandBox();

            UpdateUI();
            CheckNextState();
        }

        private void HookCommandBox()
        {
            CommandBox.OnSkillSelected += HandleSkillTargeting;
            CommandBox.OnItemSelected += HandleItemTargeting;
        }

        public void SetBackground(string filename)
        {
            BattleBackground.Source = new BitmapImage(new Uri(AssetPathResolver.GetBackgroundsPath(filename), UriKind.Absolute));
        }    

        private void UpdateUI()
        {
            UpdatePlayerUI();
            DrawSprites();          
        }

        private void UpdatePlayerUI()
        {
            UpdateCharacterPanel();
            UpdateCommandBox();
            UpdateTurnOrderDisplay();
        }    

        private void UpdateCommandBox()
        {
            if (!battleManager.CurrentTurn.IsPlayer)
            {
                CommandBox.Visibility = Visibility.Collapsed;
                return;
            }

            var activeEntry = battleManager.CurrentTurn;
            object player = (object)activeEntry.BattleActor;

            if (!spriteImages.TryGetValue(player, out var playerImage))
            {
                CommandBox.Visibility = Visibility.Collapsed;
                return;
            }

            // Get the position of the playerImage relative to the SpriteCanvas
            Point playerPosition = playerImage.TranslatePoint(new Point(0, 0), SpriteCanvas);

            double offsetX = -(playerImage.ActualWidth + 50); // sprite + movement
            double offsetY = (playerImage.ActualHeight - 300) / 2; // Center vertically

            Canvas.SetLeft(CommandBox, playerPosition.X + offsetX);
            Canvas.SetTop(CommandBox, playerPosition.Y + offsetY);

            CommandBox.Visibility = Visibility.Visible;
        }

        private void UpdateCharacterPanel()
        {
            CharacterPanelContainer.Children.Clear();

            foreach (Character character in battleActors.Where(battleActors => battleActors is Character))
            {
                var panel = new CharacterPanel
                {
                    Portrait = character.PortraitFile,
                    CharacterName = character.Name,
                    Level = character.Level,
                    CurrentHP = character.HP,
                    MaxHP = character.MaxHP,
                    CurrentMP = character.MP,
                    MaxMP = character.MaxMP,
                    IsActive = battleManager.CurrentTurn.IsPlayer && battleManager.CurrentTurn.BattleActor == character
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
            double verticalSpacing = spriteSize + 40;

            // Draw Enemies
            double enemyBaseY = 120;
            double enemyStartX = 200;
            var aliveEnemies = battleManager.GetAliveEnemies();

            for (int i = 0; i < aliveEnemies.Count; i++)
            {
                Enemy enemy = (Enemy)aliveEnemies[i];
                var image = CreateSpriteImage(AssetPathResolver.GetSpritesPath(enemy.BattleSpriteFile), enemy.SpriteFlip, enemy);

                image.MouseEnter += (s, e) =>
                {
                    if (!isTargeting) return;

                    if (TargetType == Core.Battles.Enum.TargetType.Enemy)
                    {
                        AddTargets(new List<BattleActor>() { enemy });
                    }
                };

                image.MouseLeftButtonUp += (s, e) =>
                {
                    if (!isTargeting || currentlySelected == null || !currentlySelected.Any()) 
                        return;

                    if (TargetType == Core.Battles.Enum.TargetType.Ally || TargetType == Core.Battles.Enum.TargetType.Self)
                        return; // Block clicks in non-enemy mode

                    SelectTarget();
                };

                Canvas.SetLeft(image, enemyStartX + (i * 50));
                Canvas.SetBottom(image, enemyBaseY + (i * verticalSpacing));
                SpriteCanvas.Children.Add(image);
            }

            // Draw Characters
            double characterBaseY = 120;
            double characterStartX = canvasWidth - (200 + spriteSize);
            var characters = battleActors.Where(battleActor => battleActor is Character).ToList();

            for (int i = 0; i < characters.Count; i++)
            {
                var character = characters[i];
                var image = CreateSpriteImage(AssetPathResolver.GetSpritesPath(character.BattleSpriteFile), false, character);

                image.MouseEnter += (s, e) =>
                {
                    if (!isTargeting) return;

                    if (TargetType == Core.Battles.Enum.TargetType.Ally || TargetType == Core.Battles.Enum.TargetType.Self)
                    {
                        // In "Self" mode, block hovering over other allies
                        if (TargetType == Core.Battles.Enum.TargetType.Self)
                            if (character != battleManager.CurrentTurn.BattleActor)
                                return;

                        AddTargets(new List<BattleActor>() { character });
                    }
                };

                image.MouseLeftButtonUp += (s, e) =>
                {
                    if (!isTargeting || currentlySelected == null)
                        return;

                    if (TargetType == Core.Battles.Enum.TargetType.Ally
                    || TargetType == Core.Battles.Enum.TargetType.Self
                    || TargetType == Core.Battles.Enum.TargetType.AllAllies)
                    {

                        // Self lock
                        if (TargetType != Core.Battles.Enum.TargetType.Self && character != battleManager.CurrentTurn.BattleActor)
                            return;

                        SelectTarget();
                    }
                };

                Canvas.SetLeft(image, characterStartX - (i * 50));
                Canvas.SetBottom(image, characterBaseY + (i * verticalSpacing));
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
            if (target == null || glowingSprites.Contains(target)) return;

            glowingSprites.Add(target);

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

        private void RemoveAllGlows()
        {
            foreach (var img in glowingSprites)
            {
                if (img.Effect is DropShadowEffect effect)
                {
                    effect.BeginAnimation(DropShadowEffect.BlurRadiusProperty, null);
                    effect.BeginAnimation(DropShadowEffect.OpacityProperty, null);
                }
                img.Effect = null;
            }

            glowingSprites.Clear();
        }

        private void AnimateStepForward(Action onComplete = null)
        {
            var activeEntry = battleManager.CurrentTurn;
            object actor = activeEntry.BattleActor;

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
            var activeEntry = battleManager.CurrentTurn;
            object actor = activeEntry.BattleActor;

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

            foreach (var turn in battleManager.TurnQueue)
            {
                // ❌ Skip dead enemies
                if (!turn.IsPlayer && turn.BattleActor.HP <= 0)
                    continue;

                var portraitFile = turn.BattleActor.PortraitFile;

                var img = new Image
                {
                    Width = 32,
                    Height = 32,
                    Source = new BitmapImage(new Uri(AssetPathResolver.GetPortraitPath(portraitFile)))
                };

                TurnOrderPanel.Children.Add(img);
            }
        }

        private void ShowHoverArrowFor(object target)
        {
            if (!spriteImages.TryGetValue(target, out var img)) return;

            var arrow = new Image
            {
                Width = 36,
                Height = 36,
                Source = new BitmapImage(new Uri(AssetPathResolver.GetIconsPath("TargetArrow.png"), UriKind.Absolute)),
                RenderTransformOrigin = new Point(0.5, 0.5),
                Opacity = 0,
                RenderTransform = new ScaleTransform(0.5, 0.5)
            };

            Point center = img.TranslatePoint(new Point(img.Width / 2, 0), SpriteCanvas);
            Canvas.SetLeft(arrow, center.X - (arrow.Width / 2));
            Canvas.SetTop(arrow, center.Y - arrow.Height - 4);

            SpriteCanvas.Children.Add(arrow);
            activeTargetArrows.Add(arrow);

            var scale = (ScaleTransform)arrow.RenderTransform;
            var grow = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150)) { EasingFunction = new BackEase { EasingMode = EasingMode.EaseOut } };
            var fade = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(150));

            scale.BeginAnimation(ScaleTransform.ScaleXProperty, grow);
            scale.BeginAnimation(ScaleTransform.ScaleYProperty, grow);
            arrow.BeginAnimation(UIElement.OpacityProperty, fade);
        }

        private void ClearAllTargetArrows()
        {
            foreach (var arrow in activeTargetArrows)
            {
                SpriteCanvas.Children.Remove(arrow);
            }
            activeTargetArrows.Clear();
        }

        private void HighlightSelectedInTurnOrder()
        {
            for (int i = 0; i < TurnOrderPanel.Children.Count; i++)
            {
                var aliveEntries = battleManager.TurnQueue.Where(entry => entry.BattleActor.HP > 0);

                if (TurnOrderPanel.Children[i] is Image img && i < aliveEntries.Count())
                {
                    var entry = aliveEntries.ElementAt(i);
                    img.Opacity = (currentlySelected.Contains(entry.BattleActor)) ? 1.0 : 0.5;
                }
            }
        }

        private void ShowFloatingText(string text, FloatingTextType type, Point position)
        {
            var floatingText = new FloatingText(text, type);

            Canvas.SetLeft(floatingText, position.X - 50); // center horizontally
            Canvas.SetTop(floatingText, position.Y);       // appears just above target

            EffectCanvas.Children.Add(floatingText);
        }

        private void ExitTargetingMode()
        {
            isTargeting = false;
            currentlySelected.Clear();
            ClearAllTargetArrows();
            RemoveAllGlows();
            HighlightSelectedInTurnOrder();
        }

        private void CheckNextState()
        {
            if (!battleManager.IsBattleOver())
            {
                if (battleManager.State == BattleState.EnemyTurn)
                {
                    AnimateStepForward(() =>
                    {
                        // Do the action once fully moved forward

                        var target = battleManager.GetAlivePlayers()[new Random().Next(battleManager.GetAlivePlayers().Count)];

                        currentlySelected.Add(target);

                        string result = battleManager.ExecuteCurrentTurnAction(Core.Battles.Enum.CombatAction.Attack, currentlySelected);

                        currentlySelected.Clear();

                        if (spriteImages.TryGetValue(target, out var targetImage))
                        {
                            var screenPos = targetImage.TranslatePoint(new Point(targetImage.Width / 2, 0), EffectCanvas);
                            ShowFloatingText(result, FloatingTextType.Damage, screenPos);
                        }

                        // Then move back AFTER the action
                        AnimateStepBack(() =>
                        {
                            if (battleManager.State == BattleState.Lost)
                            {
                                EndBattle("You were defeated.");
                            }
                            else
                            {
                                UpdatePlayerUI();

                                battleManager.AdvanceTurn();
                                CheckNextState();
                            }
                        });
                    });
                }
                else if (battleManager.State == BattleState.PlayerTurn)
                {
                    // Only move forward, action comes later
                    AnimateStepForward(() =>
                    {
                        UpdatePlayerUI();
                    });
                }
            }
            if (battleManager.State == BattleState.Won)
            {
                EndBattle("You won!");
            }
        }

        private void EndBattle(string message)
        {
            MessageBox.Show(message);

            if (battleManager.State == BattleState.Won)
            {
                //GrantBattleRewards(120, 50); // EXP and Gold reward
            }

            OnBattleEnd?.Invoke();
        }

        private void GrantBattleRewards(int expReward, int goldReward)
        {
            List<string> leveledUpNames = new List<string>();

            foreach (Character character in battleActors.Where(battleActor => battleActor is Character))
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

        #region Targeting System

        private void HandleCommandSelected(string command)
        {
            switch (command)
            {
                case "Attack": AttackButton_Click(null, null); break;
                case "Defend": DefendButton_Click(null, null); break;
                case "Skill":
                    var currentChar = battleManager.CurrentTurn.BattleActor;
                    if (currentChar.Skills != null && currentChar.Skills.Count > 0)
                    {
                        CommandBox.ShowSkills(currentChar.Skills);
                        CommandBox.OptionListPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show($"{currentChar.Name} has no skills!");
                    }
                    break;
                case "Item":
                    if (inventory != null && inventory.Any(i => i.Quantity > 0))
                    {
                        CommandBox.ShowItems(inventory);
                        CommandBox.OptionListPanel.Visibility = Visibility.Visible;
                    }
                    else
                        MessageBox.Show("No items available!");
                    break;
                case "Run": RunButton_Click(null, null); break;
            }
        }

        private void AttackButton_Click(object sender, RoutedEventArgs e)
        {
            CombatAction = Core.Battles.Enum.CombatAction.Attack;

            EnterTargetingMode(Core.Battles.Enum.TargetType.Enemy);
        }

        private void DefendButton_Click(object sender, RoutedEventArgs e)
        {
            currentlySelected.Add(battleManager.CurrentTurn.BattleActor);

            string result = battleManager.ExecuteCurrentTurnAction(Core.Battles.Enum.CombatAction.Defend, currentlySelected);

            MessageBox.Show(result);

            AnimateStepBack(() =>
            {
                currentlySelected.Clear();
                battleManager.AdvanceTurn();
                UpdateUI();
                CheckNextState();
            });
        }

        private void RunButton_Click(object sender, RoutedEventArgs e)
        {
            EndBattle("You ran away!");
        }

        private void HandleSkillTargeting(Skill skill)
        {
            targetingParam = skill;
            CombatAction = Core.Battles.Enum.CombatAction.Skill;

            EnterTargetingMode(skill.TargetType);
        }

        private void HandleItemTargeting(InventoryItem item)
        {
            targetingParam = item;
            CombatAction = Core.Battles.Enum.CombatAction.Item;

            EnterTargetingMode(item.TargetType);
        }

        private void EnterTargetingMode(TargetType targetType)
        {
            isTargeting = true;
            TargetType = targetType;

            //Default Target
            switch(targetType)
            {
                case Core.Battles.Enum.TargetType.Self:
                    AddTargets(new List<BattleActor>() { battleManager.CurrentTurn.BattleActor });

                    break;

                case Core.Battles.Enum.TargetType.Ally:
                    AddTargets(new List<BattleActor>() { battleManager.CurrentTurn.BattleActor });
                    break;

                case Core.Battles.Enum.TargetType.Enemy:
                    AddTargets(new List<BattleActor>() { battleManager.GetAliveEnemies().LastOrDefault() });
                    break;

                case Core.Battles.Enum.TargetType.AllAllies:
                    AddTargets(battleManager.GetAlivePlayers());
                    break;

                case Core.Battles.Enum.TargetType.AllEnemies:
                    AddTargets(battleManager.GetAliveEnemies());
                    break;
            }
        }

        private void AddTargets(List<BattleActor> actors)
        {
            currentlySelected.Clear();
            ClearAllTargetArrows();
            RemoveAllGlows();

            foreach (var actor in actors)
            {
                if (spriteImages.TryGetValue(actor, out var image))
                {
                    ApplyPulsingGlow(image);
                    ShowHoverArrowFor(actor);
                }

                currentlySelected.Add(actor);
            }

            HighlightSelectedInTurnOrder();
        }

        private void SelectTarget()
        {
            string result = battleManager.ExecuteCurrentTurnAction(CombatAction.Value, currentlySelected, targetingParam);

            targetingParam = null;

            if (result == null)
                return;

            foreach (var target in currentlySelected)
            {
                if (spriteImages.TryGetValue(target, out var targetImage))
                {
                    var screenPos = targetImage.TranslatePoint(new Point(targetImage.Width / 2, 0), EffectCanvas);
                    ShowFloatingText(result, FloatingTextType.Damage, screenPos);
                }
            }

            AnimateStepBack(() =>
            {
                ExitTargetingMode();
                battleManager.AdvanceTurn();
                UpdateUI();
                CheckNextState();
            });
        }
        #endregion
    }
}
