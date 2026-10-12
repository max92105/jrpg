using JRPG.Core.Battles;
using JRPG.Core.Battles.Enums;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Core.Utils;
using JRPG.Enums;
using JRPG.UI.Battle.Controls;
using JRPG.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using WpfAnimatedGif;
using Image = System.Windows.Controls.Image;

namespace JRPG.UI.Battle
{
    /// <summary>
    /// The battle screen (layout in BattleUI.xaml). Turn rules live in BattleManager;
    /// this class draws the fight, asks the player for commands and targets, and plays the effects.
    /// </summary>
    public partial class BattleUI : UserControl
    {
        // ============ Layout ============
        // Positions are on the 1920 x 1080 design area (BattleScene in the XAML),
        // which is scaled to the real window size, so these numbers work on any screen.
        private const double CanvasHeight = 1080;
        private const double SpriteSize = 96;
        private const double RowSpacing = SpriteSize + 40;  // vertical distance between rows of sprites
        private const double RowShift = 50;                 // each row is shifted sideways, for depth
        private const double EnemyStartX = 300 + SpriteSize;
        private const double PartyStartX = 1920 - (400 + SpriteSize);
        private const double SpritesBottom = 120;
        private const double EnemyTagGap = 38;              // enemy tag sits this far under its sprite
        private const double StepForwardDistance = 50;

        private const double CommandGap = 40;               // between the command menu and the acting sprite
        private const double CommandMinTop = 260;           // stay under the turn order and party status
        private const double CommandMaxHeight = 420;        // keeps the menu on screen for the lowest row

        private const double TargetPointerSize = 44;
        private const double VictoryBlurRadius = 10;

        // ============ Timing ============
        private static readonly Duration StepDuration = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan EffectDelay = TimeSpan.FromMilliseconds(1000);
        private static readonly TimeSpan EffectDelayWithDeath = TimeSpan.FromMilliseconds(2000);
        private static readonly TimeSpan EscapedDisplayTime = TimeSpan.FromSeconds(1.5);

        /// <summary>The battle is over; tells the scene where to go next.</summary>
        public event Action<BattleExit> OnBattleEnd;

        private readonly BattleManager battleManager;

        private readonly Dictionary<Character, CharacterPanel> characterPanels = new Dictionary<Character, CharacterPanel>();
        private readonly Dictionary<BattleActor, TranslateTransform> spriteOffsets = new Dictionary<BattleActor, TranslateTransform>();
        private readonly Dictionary<BattleActor, Image> spriteImages = new Dictionary<BattleActor, Image>();
        private readonly Dictionary<Enemy, EnemyTag> enemyTags = new Dictionary<Enemy, EnemyTag>();

        // Targeting
        private bool isTargeting;
        private bool isTargetingLocked;
        private TargetType? actionTargetType;
        private CombatAction? pendingAction;
        private object pendingActionParam; // the Skill or InventoryItem being used
        private readonly List<BattleActor> currentlySelected = new List<BattleActor>();
        private readonly HashSet<Image> glowingSprites = new HashSet<Image>();
        private readonly HashSet<Image> targetPointers = new HashSet<Image>();

        // End of battle
        private bool hasEnded;
        private bool hasFinished;
        private DispatcherTimer escapedTimer;

        public BattleUI(List<BattleActor> battleActors)
        {
            InitializeComponent();

            battleManager = new BattleManager(battleActors);

            CommandBox.OnSkillSelected += HandleSkillTargeting;
            CommandBox.OnItemSelected += HandleItemTargeting;

            Victory.Continued += () => FinishBattle(BattleExit.BackToMap);
            Defeat.Chosen += FinishBattle;

            UpdateUI();
            CheckNextState();
        }

        public void SetBackground(string filename)
        {
            BattleBackground.Source = ImageLoader.LoadBackground(filename);
        }

        // ============ HUD ============

        private void UpdateUI()
        {
            DrawSprites();
            UpdatePlayerUI();
        }

        private void UpdatePlayerUI()
        {
            UpdateCharacterPanels();
            UpdateCommandBox();
            UpdateTurnOrder();
        }

        private void UpdateCharacterPanels()
        {
            var characters = GameSystems.Party.ActivePartyOrder;

            // Forget panels of characters who left the party
            foreach (var gone in characterPanels.Keys.Except(characters).ToList())
                characterPanels.Remove(gone);

            CharacterPanelContainer.Children.Clear();

            foreach (var character in characters)
            {
                if (!characterPanels.TryGetValue(character, out var panel))
                {
                    panel = new CharacterPanel();
                    characterPanels[character] = panel;
                }

                panel.Show(character, isActive: character == battleManager.CurrentTurn.BattleActor);
                CharacterPanelContainer.Children.Add(panel);
            }
        }

        private void UpdateTurnOrder()
        {
            var order = battleManager.TurnQueue
                .Select(turn => turn.BattleActor)
                .Where(actor => actor.HP > 0)
                .ToList();

            TurnOrder.Show(order);
        }

        /// <summary>Shows the command menu to the left of the acting character, or hides it on an enemy turn.</summary>
        private void UpdateCommandBox()
        {
            var actor = battleManager.CurrentTurn.BattleActor;

            if (!battleManager.CurrentTurn.IsPlayer || !spriteImages.TryGetValue(actor, out var sprite))
            {
                CommandBox.Visibility = Visibility.Collapsed;
                return;
            }

            double spriteLeft = Canvas.GetLeft(sprite) + spriteOffsets[actor].X;
            double spriteTop = CanvasHeight - Canvas.GetBottom(sprite) - SpriteSize;
            double top = Math.Min(spriteTop - SpriteSize, CanvasHeight - CommandMaxHeight);

            Canvas.SetLeft(CommandBox, spriteLeft - CommandBox.Width - CommandGap);
            Canvas.SetTop(CommandBox, Math.Max(CommandMinTop, top));

            CommandBox.ShowCommands(actor.Name);
            CommandBox.Visibility = Visibility.Visible;
        }

        // ============ Sprites ============

        private void DrawSprites()
        {
            SpriteCanvas.Children.Clear();
            EffectCanvas.Children.Clear();
            spriteOffsets.Clear();
            spriteImages.Clear();
            enemyTags.Clear();
            targetPointers.Clear();
            glowingSprites.Clear();

            // Enemies, on the left. SlotIndex places each one on its own row.
            foreach (Enemy enemy in battleManager.GetAliveEnemies())
            {
                var image = CreateSprite(AssetPathResolver.GetSpritesPath(enemy.BattleSpriteFile), enemy.SpriteFlip, enemy);

                image.MouseEnter += (s, e) =>
                {
                    if (isTargeting && actionTargetType == TargetType.Enemy)
                        AddTargets(new List<BattleActor> { enemy });
                };
                image.MouseLeftButtonUp += (s, e) =>
                {
                    // Ally-only skills can't be aimed at an enemy
                    if (actionTargetType != TargetType.Ally && actionTargetType != TargetType.Self)
                        TrySelectTarget();
                };

                double left = EnemyStartX + enemy.SlotIndex * RowShift;
                double bottom = SpritesBottom + enemy.SlotIndex * RowSpacing;
                PlaceOnCanvas(image, left, bottom);

                var tag = new EnemyTag(enemy);
                PlaceOnCanvas(tag, left, bottom - EnemyTagGap);
                enemyTags[enemy] = tag;
            }

            // Party, on the right. Drawn bottom row first, so the order is reversed.
            var characters = GameSystems.Party.ActivePartyOrder.Reverse<Character>().ToList();

            for (int row = 0; row < characters.Count; row++)
            {
                var character = characters[row];
                var image = CreateSprite(AssetPathResolver.GetSpritesPath(character.BattleSpriteFile), false, character);

                image.MouseEnter += (s, e) =>
                {
                    if (!isTargeting)
                        return;

                    bool canAim = actionTargetType == TargetType.Ally
                               || (actionTargetType == TargetType.Self && character == battleManager.CurrentTurn.BattleActor);
                    if (canAim)
                        AddTargets(new List<BattleActor> { character });
                };
                image.MouseLeftButtonUp += (s, e) =>
                {
                    bool canSelect = actionTargetType == TargetType.Ally
                                  || actionTargetType == TargetType.AllAllies
                                  || (actionTargetType == TargetType.Self && character == battleManager.CurrentTurn.BattleActor);
                    if (canSelect)
                        TrySelectTarget();
                };

                PlaceOnCanvas(image, PartyStartX - row * RowShift, SpritesBottom + row * RowSpacing);
            }
        }

        private void PlaceOnCanvas(UIElement element, double left, double bottom)
        {
            Canvas.SetLeft(element, left);
            Canvas.SetBottom(element, bottom);
            SpriteCanvas.Children.Add(element);
        }

        private Image CreateSprite(string path, bool flip, BattleActor owner)
        {
            var offset = new TranslateTransform();

            var transforms = new TransformGroup();
            transforms.Children.Add(new ScaleTransform(flip ? -1 : 1, 1, SpriteSize / 2, SpriteSize / 2));
            transforms.Children.Add(offset);

            var image = new Image
            {
                Width = SpriteSize,
                Height = SpriteSize,
                Source = ImageLoader.Load(path, 256),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = transforms
            };

            spriteOffsets[owner] = offset;
            spriteImages[owner] = image;
            return image;
        }

        /// <summary>Top-center of a sprite, on the canvas, including its step-forward offset.</summary>
        private Point SpriteTopCenter(BattleActor actor)
        {
            var sprite = spriteImages[actor];
            double left = Canvas.GetLeft(sprite) + spriteOffsets[actor].X;
            double top = CanvasHeight - Canvas.GetBottom(sprite) - SpriteSize;
            return new Point(left + SpriteSize / 2, top);
        }

        // ============ Turn flow ============

        private void CheckNextState()
        {
            if (!battleManager.IsBattleOver())
            {
                if (battleManager.State == BattleState.EnemyTurn)
                    PlayEnemyTurn();
                else if (battleManager.State == BattleState.PlayerTurn)
                    AnimateStepForward(UpdatePlayerUI); // the player picks a command next
            }

            if (battleManager.State == BattleState.Won)
                ShowVictory();

            if (battleManager.State == BattleState.Lost)
                ShowDefeat();
        }

        private void PlayEnemyTurn()
        {
            AnimateStepForward(() =>
            {
                var alivePlayers = battleManager.GetAlivePlayers();
                currentlySelected.Add(alivePlayers[new Random().Next(alivePlayers.Count)]);

                var results = battleManager.ExecuteCurrentTurnAction(CombatAction.Attack, currentlySelected);

                PlayTurnActionEffect(results, () => AnimateStepBack(() =>
                {
                    currentlySelected.Clear();
                    UpdatePlayerUI();

                    battleManager.AdvanceTurn();
                    CheckNextState();
                }));
            });
        }

        /// <summary>After a player's action: step back, next turn.</summary>
        private void FinishPlayerTurn()
        {
            AnimateStepBack(() =>
            {
                isTargetingLocked = false;
                ExitTargetingMode();
                battleManager.AdvanceTurn();
                UpdateUI();
                CheckNextState();
            });
        }

        private void AnimateStepForward(Action onComplete = null)
        {
            var turn = battleManager.CurrentTurn;
            if (!spriteOffsets.TryGetValue(turn.BattleActor, out var offset))
            {
                onComplete?.Invoke();
                return;
            }

            // Players step left, towards the enemies; enemies step right
            double target = turn.IsPlayer ? -StepForwardDistance : StepForwardDistance;

            var step = new DoubleAnimation { To = target, Duration = StepDuration, FillBehavior = FillBehavior.Stop };
            step.Completed += (s, e) =>
            {
                offset.X = target; // keep the final position, so stepping back starts from here
                onComplete?.Invoke();
            };

            offset.BeginAnimation(TranslateTransform.XProperty, step);
        }

        private void AnimateStepBack(Action onComplete = null)
        {
            if (!spriteOffsets.TryGetValue(battleManager.CurrentTurn.BattleActor, out var offset))
            {
                onComplete?.Invoke();
                return;
            }

            var step = new DoubleAnimation { To = 0, Duration = StepDuration };
            step.Completed += (s, e) =>
            {
                offset.X = 0;
                onComplete?.Invoke();
            };

            offset.BeginAnimation(TranslateTransform.XProperty, step);
        }

        // ============ Commands ============

        private void HandleCommandSelected(string command)
        {
            var actor = battleManager.CurrentTurn.BattleActor;

            switch (command)
            {
                case "Attack":
                    pendingAction = CombatAction.Attack;
                    EnterTargetingMode(TargetType.Enemy);
                    break;

                case "Skill":
                    if (actor.Skills != null && actor.Skills.Count > 0)
                        CommandBox.ShowSkills(actor);
                    else
                        Banner.Show($"{actor.Name} has no skills.");
                    break;

                case "Item":
                    if (GameSystems.Inventory.Items.Any(i => i.Item is Consumable && i.Quantity > 0))
                        CommandBox.ShowItems(GameSystems.Inventory.Items);
                    else
                        Banner.Show("No items to use.");
                    break;

                case "Defend":
                    Defend();
                    break;

                case "Run":
                    ShowEscaped();
                    break;
            }
        }

        private void Defend()
        {
            var actor = battleManager.CurrentTurn.BattleActor;
            currentlySelected.Add(actor);
            battleManager.ExecuteCurrentTurnAction(CombatAction.Defend, currentlySelected);

            CommandBox.Visibility = Visibility.Collapsed;
            Banner.Show($"{actor.Name} defends.");

            AnimateStepBack(() =>
            {
                currentlySelected.Clear();
                battleManager.AdvanceTurn();
                UpdateUI();
                CheckNextState();
            });
        }

        private void HandleSkillTargeting(Skill skill)
        {
            pendingActionParam = skill;
            pendingAction = CombatAction.Skill;
            EnterTargetingMode(skill.TargetType);
        }

        private void HandleItemTargeting(InventoryItem inventoryItem)
        {
            pendingActionParam = inventoryItem;
            pendingAction = CombatAction.Item;
            EnterTargetingMode(((Consumable)inventoryItem.Item).TargetType);
        }

        // ============ Targeting ============

        private void EnterTargetingMode(TargetType targetType)
        {
            isTargeting = true;
            actionTargetType = targetType;
            CommandBox.ShowMessage("Choose a target. Right-click to cancel.");

            // Start on a sensible target, so a single click confirms
            switch (targetType)
            {
                case TargetType.Self:
                case TargetType.Ally:
                    AddTargets(new List<BattleActor> { battleManager.CurrentTurn.BattleActor });
                    break;
                case TargetType.Enemy:
                    AddTargets(new List<BattleActor> { battleManager.GetAliveEnemies().LastOrDefault() });
                    break;
                case TargetType.AllAllies:
                    AddTargets(battleManager.GetAlivePlayers());
                    break;
                case TargetType.AllEnemies:
                    AddTargets(battleManager.GetAliveEnemies());
                    break;
            }
        }

        private void ExitTargetingMode()
        {
            isTargeting = false;
            currentlySelected.Clear();
        }

        /// <summary>Right-click: cancel the target choice, or go back to the main command list.</summary>
        private void BattleUI_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isTargetingLocked || hasEnded || battleManager.State != BattleState.PlayerTurn)
                return;

            if (isTargeting)
            {
                ClearTargetMarks();
                ExitTargetingMode();
                pendingActionParam = null;
                TurnOrder.Highlight(currentlySelected);
                CommandBox.ShowMessage(null);
            }
            else
            {
                CommandBox.ShowCommands();
            }
        }

        private void AddTargets(List<BattleActor> actors)
        {
            currentlySelected.Clear();
            ClearTargetMarks();

            foreach (var actor in actors.Where(a => a != null))
            {
                if (spriteImages.TryGetValue(actor, out var image))
                {
                    ApplyPulsingGlow(image);
                    ShowTargetPointer(actor);
                }

                currentlySelected.Add(actor);
            }

            TurnOrder.Highlight(currentlySelected);
        }

        private void TrySelectTarget()
        {
            if (isTargeting && !isTargetingLocked && currentlySelected.Any())
                SelectTarget();
        }

        private void SelectTarget()
        {
            isTargetingLocked = true; // until the whole turn has played out

            string actionName = (pendingActionParam as Skill)?.Name ?? (pendingActionParam as InventoryItem)?.Item.Name;
            var results = battleManager.ExecuteCurrentTurnAction(pendingAction.Value, currentlySelected, pendingActionParam);
            pendingActionParam = null;

            if (results == null)
            {
                isTargetingLocked = false; // nothing happened, let the player choose again
                return;
            }

            if (actionName != null)
                Banner.Show(actionName);

            ClearTargetMarks();
            CommandBox.Visibility = Visibility.Collapsed;

            PlayTurnActionEffect(results, FinishPlayerTurn);
        }

        /// <summary>The bobbing hand above a target.</summary>
        private void ShowTargetPointer(BattleActor target)
        {
            Point topCenter = SpriteTopCenter(target);

            var bob = new TranslateTransform();
            var transforms = new TransformGroup();
            transforms.Children.Add(new RotateTransform(90)); // the art points right; turn it to point down
            transforms.Children.Add(bob);

            var pointer = new Image
            {
                Width = TargetPointerSize,
                Height = TargetPointerSize,
                Source = UiArt.Get("Battle/Pointer"),
                RenderTransformOrigin = new Point(0.5, 0.5),
                RenderTransform = transforms,
                IsHitTestVisible = false,
                Effect = (Effect)FindResource("Effect.TextOutline")
            };

            Canvas.SetLeft(pointer, topCenter.X - TargetPointerSize / 2);
            Canvas.SetTop(pointer, topCenter.Y - TargetPointerSize - 6);
            SpriteCanvas.Children.Add(pointer);
            targetPointers.Add(pointer);

            bob.BeginAnimation(TranslateTransform.YProperty, new DoubleAnimation(0, -8, TimeSpan.FromMilliseconds(450))
            {
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            });
        }

        private void ApplyPulsingGlow(Image target)
        {
            if (glowingSprites.Contains(target))
                return;

            glowingSprites.Add(target);

            var glow = new DropShadowEffect
            {
                Color = (Color)FindResource("Color.Accent"),
                BlurRadius = 20,
                ShadowDepth = 0,
                Opacity = 0.8
            };
            target.Effect = glow;

            var pulse = TimeSpan.FromMilliseconds(800);
            glow.BeginAnimation(DropShadowEffect.BlurRadiusProperty,
                new DoubleAnimation(20, 40, pulse) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
            glow.BeginAnimation(DropShadowEffect.OpacityProperty,
                new DoubleAnimation(0.8, 0.4, pulse) { AutoReverse = true, RepeatBehavior = RepeatBehavior.Forever });
        }

        /// <summary>Removes every target pointer and glow.</summary>
        private void ClearTargetMarks()
        {
            foreach (var pointer in targetPointers)
                SpriteCanvas.Children.Remove(pointer);
            targetPointers.Clear();

            foreach (var image in glowingSprites)
                image.Effect = null;
            glowingSprites.Clear();
        }

        // ============ Effects ============

        /// <summary>Damage numbers, attack animation and deaths for one action, then onFinished.</summary>
        private void PlayTurnActionEffect(List<ActionResult> results, Action onFinished)
        {
            var attacker = battleManager.CurrentTurn.BattleActor;

            foreach (var result in results)
            {
                if (!spriteImages.ContainsKey(result.Target))
                    continue;

                Point topCenter = SpriteTopCenter(result.Target);
                ShowFloatingText(result.EffectFinalValue.ToString(), result.EffectType, topCenter);
                PlayVFX(attacker.AttackVFXFile, new Point(topCenter.X, topCenter.Y + 30));

                if (result.Target is Enemy enemy && enemyTags.TryGetValue(enemy, out var tag))
                    tag.Refresh(enemy);
            }

            // Party HP / MP change right away, not at the end of the turn
            UpdateCharacterPanels();

            var deadActors = currentlySelected.Where(t => t.HP <= 0).ToList();
            foreach (var dead in deadActors)
                AnimateDeath(dead);

            var delay = new DispatcherTimer { Interval = deadActors.Count > 0 ? EffectDelayWithDeath : EffectDelay };
            delay.Tick += (s, e) =>
            {
                delay.Stop();
                onFinished();
            };
            delay.Start();
        }

        private void ShowFloatingText(string text, EffectType type, Point position)
        {
            var floatingText = new FloatingText(text, type);
            Canvas.SetLeft(floatingText, position.X - FloatingText.HalfWidth);
            Canvas.SetTop(floatingText, position.Y - 30);
            EffectCanvas.Children.Add(floatingText);
        }

        private void PlayVFX(string vfxName, Point position)
        {
            var vfxImage = new Image
            {
                Width = SpriteSize,
                Height = SpriteSize,
                RenderTransformOrigin = new Point(0.5, 0.5)
            };

            var gif = new BitmapImage(new Uri(AssetPathResolver.GetVFXPath(vfxName), UriKind.Absolute));
            ImageBehavior.SetAnimatedSource(vfxImage, gif);
            ImageBehavior.SetRepeatBehavior(vfxImage, new RepeatBehavior(1)); // play once

            Canvas.SetLeft(vfxImage, position.X - vfxImage.Width / 2);
            Canvas.SetTop(vfxImage, position.Y - vfxImage.Height / 2);
            EffectCanvas.Children.Add(vfxImage);

            var fade = new DoubleAnimation(1.0, 0.0, TimeSpan.FromMilliseconds(800));
            fade.Completed += (s, e) => EffectCanvas.Children.Remove(vfxImage);
            vfxImage.BeginAnimation(OpacityProperty, fade);
        }

        /// <summary>White flash, a pause, then the sprite (and its enemy tag) fade out.</summary>
        private void AnimateDeath(BattleActor actor)
        {
            if (!spriteImages.TryGetValue(actor, out var image))
                return;

            image.Effect = new DropShadowEffect { Color = Colors.White, BlurRadius = 0, ShadowDepth = 0, Opacity = 1.0 };

            var pause = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            pause.Tick += (s, e) =>
            {
                pause.Stop();

                var fadeOut = new DoubleAnimation
                {
                    To = 0,
                    Duration = TimeSpan.FromMilliseconds(1000),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                };
                fadeOut.Completed += (s2, e2) => image.Effect = null;

                image.BeginAnimation(OpacityProperty, fadeOut);
                if (actor is Enemy enemy && enemyTags.TryGetValue(enemy, out var tag))
                    tag.BeginAnimation(OpacityProperty, fadeOut);
            };
            pause.Start();
        }

        // ============ End of battle ============

        private void ShowVictory()
        {
            if (!BeginEnding())
                return;

            // EXP is given here, split between the members who fought; the screen animates it
            var gains = battleManager.ShareExperience();

            // Like the reference: the battle stays visible behind the results,
            // background blurred, sprites sharp, only the left side darkened
            HideHud();
            BattleBackground.Effect = new BlurEffect { Radius = VictoryBlurRadius, RenderingBias = RenderingBias.Performance };
            ShowOutcomeLayer((Brush)FindResource("Brush.VictoryShade"));

            Victory.Show(battleManager.TotalExpReward, gains);
        }

        private void ShowDefeat()
        {
            if (!BeginEnding())
                return;

            HideHud();
            ShowOutcomeLayer((Brush)FindResource("Brush.Scrim"));
            Defeat.Show();
        }

        /// <summary>Hides the fight's turn order, party status and banner.</summary>
        private void HideHud()
        {
            TurnOrder.Visibility = Visibility.Collapsed;
            CharacterPanelContainer.Visibility = Visibility.Collapsed;
            Banner.Visibility = Visibility.Collapsed;
        }

        private void ShowEscaped()
        {
            if (!BeginEnding())
                return;

            ShowOutcomeLayer((Brush)FindResource("Brush.Scrim"));
            EscapedScreen.Visibility = Visibility.Visible;

            escapedTimer = new DispatcherTimer { Interval = EscapedDisplayTime };
            escapedTimer.Tick += (s, e) => FinishBattle(BattleExit.BackToMap);
            escapedTimer.Start();
        }

        private void EscapedScreen_Click(object sender, MouseButtonEventArgs e) => FinishBattle(BattleExit.BackToMap);

        /// <summary>Stops the fight UI. False if the battle had already ended.</summary>
        private bool BeginEnding()
        {
            if (hasEnded)
                return false;
            hasEnded = true;

            CommandBox.Visibility = Visibility.Collapsed;
            ClearTargetMarks();
            return true;
        }

        /// <param name="shade">What darkens the battle behind the screen.</param>
        private void ShowOutcomeLayer(Brush shade)
        {
            OutcomeLayer.Background = shade;
            OutcomeLayer.Visibility = Visibility.Visible;
            OutcomeLayer.BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(300)));
        }

        /// <summary>Hands control back to the scene, once.</summary>
        private void FinishBattle(BattleExit exit)
        {
            if (hasFinished)
                return;
            hasFinished = true;

            escapedTimer?.Stop();
            OnBattleEnd?.Invoke(exit);
        }
    }
}
