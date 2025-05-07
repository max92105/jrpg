using JRPG.Core.Battles.Enum;
using JRPG.Core.Battles.Models;
using JRPG.UI.Battle;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace JRPG
{
    public partial class MainWindow : Window
    {
        public Game game;
        public DialogueManager dialogueManager;

        private DateTime sessionStartTime;
        private int totalPlayTimeSeconds = 0;

        //Inventory System
        private List<InventoryItem> playerInventory = new List<InventoryItem>();
        private InventoryItem selectedItem;

        //Charater Management System
        private List<Character> allCharacters = new List<Character>();
        private List<Character> activePartyOrder = new List<Character>();
        private Character draggedCharacter = null;

        //Equipment System
        private List<string> availableWeapons = new List<string>();
        private List<string> availableArmor = new List<string>();

        private Dictionary<string, int> weaponAttackBonus = new Dictionary<string, int>();
        private Dictionary<string, int> armorDefenseBonus = new Dictionary<string, int>();

        private Dictionary<string, List<string>> weaponAllowedClasses = new Dictionary<string, List<string>>();
        private Dictionary<string, List<string>> armorAllowedClasses = new Dictionary<string, List<string>>();

        private Dictionary<string, int> weaponStock = new Dictionary<string, int>();
        private Dictionary<string, int> armorStock = new Dictionary<string, int>();

        public MainWindow()
        {
            InitializeComponent();

            WindowStyle = WindowStyle.None;
            WindowState = WindowState.Maximized;
            ResizeMode = ResizeMode.NoResize;
            Topmost = true;
        }

        private void InitializeInventory()
        {
            playerInventory.Add(new InventoryItem { Name = "Potion", Quantity = 5, Description = "Restores a small amount of HP." });
            playerInventory.Add(new InventoryItem { Name = "Hi-Potion", Quantity = 2, Description = "Restores a medium amount of HP." });
            playerInventory.Add(new InventoryItem { Name = "Elixir", Quantity = 1, Description = "Fully restores HP and MP." });
            playerInventory.Add(new InventoryItem { Name = "Antidote", Quantity = 7, Description = "Cures poison." });
            playerInventory.Add(new InventoryItem { Name = "Phoenix Down", Quantity = 3, Description = "Revives a fallen ally." });
        }

        private void InitializeCharacters()
        {
            allCharacters.Add(new Character
            {
                Name = "Alaric",
                PortraitFile = "Alaric.png",
                Class = "Warrior",
                Level = 10,
                HP = 230,
                MaxHP = 230,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 15,
                BaseDefense = 8,
                EXP = 250,
                EXPToNextLevel = 500,
                IsActive = true,
                IsMainCharacter = true,
                Skills = new List<Skill> 
                {
                    new Skill { Name = "Fireball", MPCost = 10, TargetType = TargetType.Enemy },
                    new Skill { Name = "Heal", MPCost = 8, TargetType = TargetType.Ally },
                    new Skill { Name = "Recovery", MPCost = 8, TargetType = TargetType.Self }
                }
            });

            allCharacters.Add(new Character
            {
                Name = "Selene",
                Class = "Mage",
                Level = 9,
                HP = 120,
                MaxHP = 120,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 5,
                BaseDefense = 3,
                EXP = 150,
                EXPToNextLevel = 400,
            });

            allCharacters.Add(new Character
            {
                Name = "Dorian",
                Class = "Thief",
                Level = 11,
                HP = 190,
                MaxHP = 190,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 12,
                BaseDefense = 5,
                EXP = 300,
                EXPToNextLevel = 600,
            });

            allCharacters.Add(new Character
            {
                Name = "Lyra",
                Class = "Healer",
                Level = 8,
                HP = 150,
                MaxHP = 150,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 4,
                BaseDefense = 6,
                EXP = 100,
                EXPToNextLevel = 300,
            });

            allCharacters.Add(new Character
            {
                Name = "Balthazar",
                Class = "Knight",
                Level = 12,
                HP = 300,
                MaxHP = 300,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 18,
                BaseDefense = 12,
                EXP = 100,
                EXPToNextLevel = 300,
            });

            allCharacters.Add(new Character
            {
                Name = "Fiora",
                Class = "Ranger",
                Level = 10,
                HP = 170,
                MaxHP = 170,
                MP = 30,
                MaxMP = 30,
                BaseAttack = 13,
                BaseDefense = 5,
                EXP = 100,
                EXPToNextLevel = 300,
            });
        }

        private void InitializeEquipment()
        {
            availableWeapons.Add("Bronze Sword");
            availableWeapons.Add("Iron Sword");
            availableWeapons.Add("Magic Staff");

            availableArmor.Add("Leather Armor");
            availableArmor.Add("Chain Mail");
            availableArmor.Add("Wizard Robe");

            weaponAttackBonus["Bronze Sword"] = 5;
            weaponAttackBonus["Iron Sword"] = 8;
            weaponAttackBonus["Magic Staff"] = 6;

            armorDefenseBonus["Leather Armor"] = 3;
            armorDefenseBonus["Chain Mail"] = 6;
            armorDefenseBonus["Wizard Robe"] = 4;

            weaponAllowedClasses["Bronze Sword"] = new List<string> { "Warrior", "Knight", "Thief" };
            weaponAllowedClasses["Iron Sword"] = new List<string> { "Warrior", "Knight" };
            weaponAllowedClasses["Magic Staff"] = new List<string> { "Mage", "Healer" };

            armorAllowedClasses["Leather Armor"] = new List<string> { "Warrior", "Knight", "Thief", "Ranger" };
            armorAllowedClasses["Chain Mail"] = new List<string> { "Warrior", "Knight" };
            armorAllowedClasses["Wizard Robe"] = new List<string> { "Mage", "Healer" };

            weaponStock["Bronze Sword"] = 2;
            weaponStock["Iron Sword"] = 1;
            weaponStock["Magic Staff"] = 1;

            armorStock["Leather Armor"] = 3;
            armorStock["Chain Mail"] = 1;
            armorStock["Wizard Robe"] = 2;
        }

        public void StartBattle()
        {
            Fade(1, 0.5, () =>
            {
                List<Enemy> enemies = new List<Enemy>()
                {
                    new Enemy("Goblin", 40, 10, 2, "goblin.png", "PlaceholderEnemy.png"),
                    new Enemy("Goblin", 40, 10, 2, "goblin.png", "PlaceholderEnemy.png"),
                    new Enemy("Goblin", 40, 10, 2, "goblin.png", "PlaceholderEnemy.png")
                };

                var battleUI = new BattleUI(activePartyOrder, enemies, playerInventory);
                battleUI.OnBattleEnd += EndBattle;

                RootGrid.Children.Add(battleUI);

                Fade(0, 0.5);
            });
        }

        private void EndBattle()
        {
            Fade(1, 0.5, () =>
            {
                var battleUI = RootGrid.Children.OfType<BattleUI>().FirstOrDefault();
                if (battleUI != null)
                    RootGrid.Children.Remove(battleUI); // Only remove BattleUI

                Focus();
                Keyboard.Focus(GameCanvas);

                Fade(0, 0.5); // Fade back to game world
            });
        }

        private void Fade(double toOpacity, double durationSec, Action onComplete = null)
        {
            FadeRect.Visibility = Visibility.Visible;
            FadeRect.Width = ActualWidth;
            FadeRect.Height = ActualHeight;

            var storyboard = new Storyboard();
            var animation = new DoubleAnimation
            {
                To = toOpacity,
                Duration = TimeSpan.FromSeconds(durationSec),
                FillBehavior = FillBehavior.Stop
            };

            Storyboard.SetTarget(animation, FadeRect);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(animation);

            storyboard.Completed += (s, e) =>
            {
                FadeRect.Opacity = toOpacity;
                if (toOpacity == 0)
                    FadeRect.Visibility = Visibility.Collapsed;

                onComplete?.Invoke();
            };

            storyboard.Begin();
        }

        private async Task FadeInOut(Action onFadeOutComplete)
        {
            FadeRect.Visibility = Visibility.Visible;

            // Fade In to Black
            for (double opacity = 0; opacity <= 1; opacity += 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8); // ~60fps
            }

            onFadeOutComplete?.Invoke();

            // Fade Out to Clear
            for (double opacity = 1; opacity >= 0; opacity -= 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8);
            }

            FadeRect.Visibility = Visibility.Collapsed;
        }

        private async Task FadeToBlack()
        {
            FadeRect.Visibility = Visibility.Visible;

            for (double opacity = 0; opacity <= 1; opacity += 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8); // ~30 FPS
            }
        }

        private async Task FadeFromBlack()
        {
            for (double opacity = 1; opacity >= 0; opacity -= 0.03)
            {
                FadeRect.Opacity = opacity;
                await Task.Delay(8);
            }

            FadeRect.Visibility = Visibility.Collapsed;
        }

        public void ShowDialogue(List<string> pages, string portraitPath = null, string speaker = null, Action callback = null)
        {
            dialogueManager.Show(pages, portraitPath, speaker, callback);
        }

        private UIElement CreatePlaceholderPage(string text)
        {
            return new TextBlock
            {
                Text = text,
                Foreground = Brushes.White,
                FontSize = 32,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.Key == Key.Escape && StartScreen.Visibility != Visibility.Visible)
            {
                if (GameMenu.Visibility == Visibility.Visible)
                {
                    GameMenu.Visibility = Visibility.Collapsed;
                    Panel.SetZIndex(GameMenu, 0);
                }
                else
                {
                    GameMenu.Visibility = Visibility.Visible;
                    Panel.SetZIndex(GameMenu, 1000);
                }

            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (dialogueManager.IsActive && e.Key == Key.Space)
            {
                dialogueManager.Skip();
                return;
            }

            if (!dialogueManager.IsActive && RootGrid.Children.Contains(GameCanvas))
                game.OnKeyDown(e.Key);
        }

        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (!dialogueManager.IsActive && RootGrid.Children.Contains(GameCanvas))
                game.OnKeyUp(e.Key);
        }

        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // Fade to black first
            await FadeToBlack();

            // Switch screens AFTER full black
            StartScreen.Visibility = Visibility.Collapsed;
            GameCanvas.Visibility = Visibility.Visible;

            // Small delay to make sure WPF refreshes layout
            await Task.Delay(500);

            // Then fade back in
            await FadeFromBlack();
        }

        private void ResumeButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenu.Visibility = Visibility.Collapsed;
        }

        private async void ReturnToMenuButton_Click(object sender, RoutedEventArgs e)
        {
            await FadeInOut(() =>
            {
                GameMenu.Visibility = Visibility.Collapsed;
                GameCanvas.Visibility = Visibility.Collapsed;
                StartScreen.Visibility = Visibility.Visible;
            });
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void InventoryButton_Click(object sender, RoutedEventArgs e)
        {
            var mainGrid = new Grid();
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(7, GridUnitType.Star) });
            mainGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(3, GridUnitType.Star) });

            var itemListStack = new StackPanel();

            // Description Block
            TextBlock descriptionBlock = new TextBlock
            {
                Text = "Select an item to see its description.",
                FontSize = 18,
                Foreground = Brushes.White,
                Margin = new Thickness(10),
                TextWrapping = TextWrapping.Wrap,
                VerticalAlignment = VerticalAlignment.Top
            };

            // Wrap description inside a Border
            Border descriptionBorder = new Border
            {
                BorderBrush = Brushes.White,
                BorderThickness = new Thickness(2),
                CornerRadius = new CornerRadius(10),
                Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                Margin = new Thickness(10),
                Child = descriptionBlock
            };

            foreach (var item in playerInventory)
            {
                var itemRow = new Border
                {
                    Background = Brushes.Transparent,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(0),
                    Margin = new Thickness(5),
                    CornerRadius = new CornerRadius(5)
                };

                var itemStack = new StackPanel { Orientation = Orientation.Horizontal };
                itemRow.Child = itemStack;

                itemStack.Children.Add(new TextBlock
                {
                    Text = item.Name,
                    FontSize = 24,
                    Foreground = Brushes.White,
                    Width = 200,
                    VerticalAlignment = VerticalAlignment.Top
                });

                itemStack.Children.Add(new TextBlock
                {
                    Text = "x" + item.Quantity,
                    FontSize = 24,
                    Foreground = Brushes.White,
                    Width = 100,
                    HorizontalAlignment = HorizontalAlignment.Right
                });

                // Highlight + description on hover
                itemRow.MouseEnter += (s, args) =>
                {
                    itemRow.Background = new SolidColorBrush(Color.FromRgb(80, 80, 80));
                    descriptionBlock.Text = item.Description;
                };

                itemRow.MouseLeave += (s, args) =>
                {
                    itemRow.Background = Brushes.Transparent;
                };

                itemRow.MouseLeftButtonDown += async (s, args) =>
                {
                    selectedItem = item;
                    PopupQuestion.Text = $"Use {item.Name}?";
                    await FadeInPopup();
                };

                itemListStack.Children.Add(itemRow);
            }

            ScrollViewer scroll = new ScrollViewer
            {
                Content = itemListStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                Margin = new Thickness(10),
                Background = Brushes.Transparent
            };

            Grid.SetRow(scroll, 0);
            Grid.SetRow(descriptionBorder, 1);

            mainGrid.Children.Add(scroll);
            mainGrid.Children.Add(descriptionBorder);

            GameMenuContent.Content = mainGrid;
        }

        private async void PopupYes_Click(object sender, RoutedEventArgs e)
        {
            if (selectedItem != null)
            {
                selectedItem.Quantity--;

                if (selectedItem.Quantity <= 0)
                {
                    playerInventory.Remove(selectedItem);
                }

                selectedItem = null;
                await FadeOutPopup();

                // Refresh Inventory page
                InventoryButton_Click(null, null);
            }
        }

        private async void PopupNo_Click(object sender, RoutedEventArgs e)
        {
            selectedItem = null;
            await FadeOutPopup();
        }

        private async Task FadeInPopup()
        {
            PopupConfirmation.Visibility = Visibility.Visible;
            PopupConfirmation.Opacity = 0;

            for (double opacity = 0; opacity <= 1; opacity += 0.05)
            {
                PopupConfirmation.Opacity = opacity;
                await Task.Delay(10);
            }

            PopupConfirmation.Opacity = 1;
        }

        private async Task FadeOutPopup()
        {
            for (double opacity = 1; opacity >= 0; opacity -= 0.05)
            {
                PopupConfirmation.Opacity = opacity;
                await Task.Delay(10);
            }

            PopupConfirmation.Visibility = Visibility.Collapsed;
        }

        private void PartyButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshActivePartyOrder();

            var mainStack = new StackPanel();

            // Active Party Preview
            var activePartyStack = new StackPanel { Orientation = Orientation.Vertical, Margin = new Thickness(10) };

            for (int i = 0; i < activePartyOrder.Count; i++)
            {
                var member = activePartyOrder[i];

                var memberBorder = new Border
                {
                    Background = member.IsMainCharacter ? Brushes.DarkSlateBlue : Brushes.DarkGreen,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(5),
                    Padding = new Thickness(10)
                };

                memberBorder.MouseMove += (s, mouseEvent) =>
                {
                    if (mouseEvent.LeftButton == MouseButtonState.Pressed && !member.IsMainCharacter)
                    {
                        draggedCharacter = member;
                        memberBorder.AllowDrop = false; // 👈 disable drop on self
                        memberBorder.Opacity = 0.6;

                        DragDrop.DoDragDrop(memberBorder, member, DragDropEffects.Move);

                        memberBorder.Opacity = 1.0;
                        memberBorder.AllowDrop = true; // 👈 re-enable after drag
                    }
                };

                memberBorder.AllowDrop = true;

                memberBorder.DragEnter += (s, dragEvent) =>
                {
                    if (!member.IsMainCharacter)
                        memberBorder.Background = Brushes.DarkOliveGreen; // 👈 Highlight drop target
                };

                memberBorder.DragLeave += (s, dragEvente) =>
                {
                    if (!member.IsMainCharacter)
                        memberBorder.Background = Brushes.DarkGreen; // 👈 Restore color
                };

                memberBorder.Drop += (s, dragEvent) =>
                {
                    if (draggedCharacter != null && draggedCharacter != member)
                    {
                        SwapPartyMembers(draggedCharacter, member);
                        draggedCharacter = null;
                    }
                };

                var memberStack = new StackPanel();

                memberStack.Children.Add(new TextBlock
                {
                    Text = member.Name,
                    FontSize = 18,
                    Foreground = Brushes.White,
                    TextAlignment = TextAlignment.Center,
                });

                memberStack.Children.Add(new TextBlock
                {
                    Text = $"{member.Class} (Lv{member.Level})",
                    FontSize = 14,
                    Foreground = Brushes.LightGray,
                    TextAlignment = TextAlignment.Center,

                });

                memberStack.Children.Add(new TextBlock
                {
                    Text = $"HP: {member.HP}",
                    FontSize = 14,
                    Foreground = Brushes.LightGray,
                    TextAlignment = TextAlignment.Center,
                });

                memberBorder.Child = memberStack;
                activePartyStack.Children.Add(memberBorder);
            }

            mainStack.Children.Add(activePartyStack);

            // --- Full Roster Scrollable ---
            var rosterStack = new StackPanel();

            foreach (var character in allCharacters)
            {
                var characterRow = new Border
                {
                    Background = character.IsMainCharacter ? Brushes.DarkSlateBlue : (character.IsActive ? Brushes.Green : Brushes.Transparent),
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    Margin = new Thickness(5),
                    CornerRadius = new CornerRadius(5)
                };

                var characterStack = new StackPanel { Orientation = Orientation.Horizontal };
                characterRow.Child = characterStack;

                var textStack = new StackPanel();

                textStack.Children.Add(new TextBlock
                {
                    Text = character.Name,
                    FontSize = 20,
                    Foreground = Brushes.White,
                    Margin = new Thickness(5, 5, 0, 5)
                });

                textStack.Children.Add(new TextBlock
                {
                    Text = $"{character.Class} (Lv{character.Level}) - HP: {character.HP}",
                    FontSize = 14,
                    Foreground = Brushes.LightGray,
                    Margin = new Thickness(5, 5, 0, 5)
                });

                characterStack.Children.Add(textStack);

                characterRow.MouseLeftButtonDown += (s, args) =>
                {
                    if (character.IsMainCharacter)
                    {
                        MessageBox.Show($"{character.Name} cannot be removed from the party!");
                        return;
                    }

                    if (!character.IsActive && GetActivePartyCount() < 4)
                    {
                        character.IsActive = true;
                    }
                    else if (character.IsActive)
                    {
                        character.IsActive = false;
                    }
                    else if (GetActivePartyCount() >= 4 && !character.IsActive)
                    {
                        MessageBox.Show("Party is full! Maximum 4 members.");
                    }

                    PartyButton_Click(null, null); // Refresh
                };

                rosterStack.Children.Add(characterRow);
            }

            var rosterScroll = new ScrollViewer
            {
                Content = rosterStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };

            mainStack.Children.Add(rosterScroll);

            GameMenuContent.Content = mainStack;
        }

        private int GetActivePartyCount()
        {
            return allCharacters.Count(c => c.IsActive);
        }

        private void RefreshActivePartyOrder()
        {
            // Always clear and rebuild order
            activePartyOrder = allCharacters.Where(c => c.IsActive)
                                            .OrderBy(c => c.IsMainCharacter ? 0 : 1) // Main character always first
                                            .ThenBy(c => activePartyOrder.IndexOf(c)) // Maintain relative order when possible
                                            .ToList();
        }

        private void SwapPartyMembers(Character source, Character target)
        {
            int sourceIndex = activePartyOrder.IndexOf(source);
            int targetIndex = activePartyOrder.IndexOf(target);

            if (sourceIndex == -1 || targetIndex == -1)
                return;

            // Swap them
            activePartyOrder[sourceIndex] = target;
            activePartyOrder[targetIndex] = source;

            PartyButton_Click(null, null); // Refresh screen
        }

        private void EquipmentButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshActivePartyOrder();

            var mainStack = new StackPanel();

            foreach (var character in activePartyOrder)
            {
                var characterBorder = new Border
                {
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(5),
                    Padding = new Thickness(10)
                };

                var characterStack = new StackPanel();

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"{character.Name} - {character.Class}",
                    FontSize = 20,
                    Foreground = Brushes.White
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"Weapon: {character.EquippedWeapon ?? "None"}",
                    FontSize = 16,
                    Foreground = Brushes.LightGray,
                    Margin = new Thickness(0, 5, 0, 0)
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"Armor: {character.EquippedArmor ?? "None"}",
                    FontSize = 16,
                    Foreground = Brushes.LightGray,
                    Margin = new Thickness(0, 2, 0, 0)
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"ATK: {character.GetTotalAttack(weaponAttackBonus)}  DEF: {character.GetTotalDefense(armorDefenseBonus)}",
                    FontSize = 16,
                    Foreground = Brushes.LightBlue,
                    Margin = new Thickness(0, 5, 0, 0)
                });

                characterBorder.Child = characterStack;

                characterBorder.MouseLeftButtonDown += (s, args) =>
                {
                    OpenEquipmentOptions(character);
                };

                mainStack.Children.Add(characterBorder);
            }

            GameMenuContent.Content = new ScrollViewer
            {
                Content = mainStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };
        }

        private void OpenEquipmentOptions(Character character)
        {
            var menuStack = new StackPanel();

            var title = new TextBlock
            {
                Text = $"Equip {character.Name}",
                FontSize = 24,
                Foreground = Brushes.White,
                Margin = new Thickness(0, 0, 0, 20)
            };
            menuStack.Children.Add(title);

            var equipWeaponButton = new Button
            {
                Content = "Equip Weapon",
                Margin = new Thickness(0, 0, 0, 10)
            };
            equipWeaponButton.Click += (s, e) =>
            {
                ShowWeaponSelection(character);
            };
            menuStack.Children.Add(equipWeaponButton);

            var equipArmorButton = new Button
            {
                Content = "Equip Armor"
            };
            equipArmorButton.Click += (s, e) =>
            {
                ShowArmorSelection(character);
            };
            menuStack.Children.Add(equipArmorButton);

            GameMenuContent.Content = menuStack;
        }

        private void ShowWeaponSelection(Character character)
        {
            var weaponStack = new StackPanel();

            // Unequip button first
            var unequipButton = new Button
            {
                Content = "None (Unequip Weapon)",
                Margin = new Thickness(0, 0, 0, 10)
            };
            unequipButton.Click += (s, e) =>
            {
                if (character.EquippedWeapon != null && weaponStock.ContainsKey(character.EquippedWeapon))
                {
                    weaponStock[character.EquippedWeapon]++; // 🔥 Return to stock!
                }

                character.EquippedWeapon = null;
                EquipmentButton_Click(null, null); // Refresh
            };

            weaponStack.Children.Add(unequipButton);

            // List weapons
            foreach (var weapon in availableWeapons)
            {
                if (!weaponAllowedClasses.ContainsKey(weapon) || !weaponAllowedClasses[weapon].Contains(character.Class))
                    continue;

                if (weaponStock.ContainsKey(weapon) && weaponStock[weapon] <= 0)
                    continue; // 🔥 Skip weapons with no stock

                var currentAttack = character.BaseAttack + (character.EquippedWeapon != null && weaponAttackBonus.ContainsKey(character.EquippedWeapon) ? weaponAttackBonus[character.EquippedWeapon] : 0);
                var newAttack = character.BaseAttack + (weaponAttackBonus.ContainsKey(weapon) ? weaponAttackBonus[weapon] : 0);

                var weaponButton = new Button
                {
                    Content = $"{weapon} (ATK: {newAttack}) {StatChangePreview(currentAttack, newAttack)}  (x{weaponStock[weapon]})",
                    Margin = new Thickness(0, 0, 0, 10)
                };

                weaponButton.Click += (s, e) =>
                {
                    if (weaponStock.ContainsKey(weapon) && weaponStock[weapon] > 0)
                    {
                        if (character.EquippedWeapon != null && weaponStock.ContainsKey(character.EquippedWeapon))
                            weaponStock[character.EquippedWeapon]++;

                        character.EquippedWeapon = weapon;
                        weaponStock[weapon]--;
                    }
                    EquipmentButton_Click(null, null);
                };

                weaponStack.Children.Add(weaponButton);
            }

            GameMenuContent.Content = new ScrollViewer
            {
                Content = weaponStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        private void ShowArmorSelection(Character character)
        {
            var armorStack = new StackPanel();

            // Unequip button first
            var unequipButton = new Button
            {
                Content = "None (Unequip Armor)",
                Margin = new Thickness(0, 0, 0, 10)
            };
            unequipButton.Click += (s, e) =>
            {
                if (character.EquippedArmor != null && armorStock.ContainsKey(character.EquippedArmor))
                {
                    armorStock[character.EquippedArmor]++; // 🔥 Return to stock!
                }

                character.EquippedArmor = null;
                EquipmentButton_Click(null, null); // Refresh
            };

            armorStack.Children.Add(unequipButton);

            // List armors
            foreach (var armor in availableArmor)
            {
                if (!armorAllowedClasses.ContainsKey(armor) || !armorAllowedClasses[armor].Contains(character.Class))
                    continue;

                if (armorStock.ContainsKey(armor) && armorStock[armor] <= 0)
                    continue; // 🔥 Skip armor with no stock

                var currentDefense = character.BaseDefense + (character.EquippedArmor != null && armorDefenseBonus.ContainsKey(character.EquippedArmor) ? armorDefenseBonus[character.EquippedArmor] : 0);
                var newDefense = character.BaseDefense + (armorDefenseBonus.ContainsKey(armor) ? armorDefenseBonus[armor] : 0);

                var armorButton = new Button
                {
                    Content = $"{armor} (DEF: {newDefense}) {StatChangePreview(currentDefense, newDefense)}  (x{armorStock[armor]})",
                    Margin = new Thickness(0, 0, 0, 10)
                };

                armorButton.Click += (s, e) =>
                {
                    if (armorStock.ContainsKey(armor) && armorStock[armor] > 0)
                    {
                        if (character.EquippedArmor != null && armorStock.ContainsKey(character.EquippedArmor))
                            armorStock[character.EquippedArmor]++;

                        character.EquippedArmor = armor;
                        armorStock[armor]--;
                    }
                    EquipmentButton_Click(null, null);
                };

                armorStack.Children.Add(armorButton);
            }

            GameMenuContent.Content = new ScrollViewer
            {
                Content = armorStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
        }

        private string StatChangePreview(int current, int after)
        {
            if (after > current)
                return $"▲ (+{after - current})";
            else if (after < current)
                return $"▼ ({after - current})";
            else
                return "(no change)";
        }

        private void StatusButton_Click(object sender, RoutedEventArgs e)
        {
            RefreshActivePartyOrder(); // Always refresh

            var mainStack = new StackPanel();

            foreach (var character in activePartyOrder)
            {
                var characterBorder = new Border
                {
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(1),
                    CornerRadius = new CornerRadius(5),
                    Margin = new Thickness(5),
                    Padding = new Thickness(10)
                };

                var characterStack = new StackPanel();

                // Basic Info
                characterStack.Children.Add(new TextBlock
                {
                    Text = $"{character.Name} - {character.Class} (Lv {character.Level})",
                    FontSize = 24,
                    Foreground = Brushes.White,
                    Margin = new Thickness(0, 0, 0, 10)
                });

                // HP
                characterStack.Children.Add(new TextBlock
                {
                    Text = $"HP: {character.HP}",
                    FontSize = 18,
                    Foreground = Brushes.LightGreen
                });

                // Attack
                characterStack.Children.Add(new TextBlock
                {
                    Text = $"ATK: {character.GetTotalAttack(weaponAttackBonus)} (Base {character.BaseAttack})",
                    FontSize = 18,
                    Foreground = Brushes.LightBlue
                });

                // Defense
                characterStack.Children.Add(new TextBlock
                {
                    Text = $"DEF: {character.GetTotalDefense(armorDefenseBonus)} (Base {character.BaseDefense})",
                    FontSize = 18,
                    Foreground = Brushes.LightBlue
                });

                // Equipped Weapon
                characterStack.Children.Add(new TextBlock
                {
                    Text = $"Weapon: {character.EquippedWeapon ?? "None"}",
                    FontSize = 18,
                    Foreground = Brushes.LightGray
                });

                // Equipped Armor
                characterStack.Children.Add(new TextBlock
                {
                    Text = $"Armor: {character.EquippedArmor ?? "None"}",
                    FontSize = 18,
                    Foreground = Brushes.LightGray
                });

                characterStack.Children.Add(new TextBlock
                {
                    Text = $"EXP: {character.EXP} / {character.EXPToNextLevel}",
                    FontSize = 18,
                    Foreground = Brushes.Yellow,
                    Margin = new Thickness(0, 5, 0, 0)
                });

                var expBar = new ProgressBar
                {
                    Minimum = 0,
                    Maximum = character.EXPToNextLevel,
                    Value = character.EXP,
                    Height = 15,
                    Margin = new Thickness(0, 5, 0, 0)
                };

                characterStack.Children.Add(expBar);

                characterStack.Children.Add(new TextBlock
                {
                    Text = "Skills:",
                    FontSize = 20,
                    Foreground = Brushes.LightSkyBlue,
                    Margin = new Thickness(0, 10, 0, 0)
                });

                foreach (var skill in character.Skills)
                {
                    characterStack.Children.Add(new TextBlock
                    {
                        Text = $"- {skill}",
                        FontSize = 16,
                        Foreground = Brushes.White
                    });
                }

                characterBorder.Child = characterStack;
                mainStack.Children.Add(characterBorder);
            }

            GameMenuContent.Content = new ScrollViewer
            {
                Content = mainStack,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                Margin = new Thickness(10)
            };
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var slot = AskSaveSlot();
            if (slot == -1) return;

            var sessionSeconds = (int)(DateTime.Now - sessionStartTime).TotalSeconds;
            var totalSeconds = totalPlayTimeSeconds + sessionSeconds;

            SaveGameManager.SaveGame(slot, allCharacters, playerInventory, weaponStock, armorStock, game.currentMapFile, game.playerX, game.playerY, activePartyOrder, totalSeconds);
            MessageBox.Show($"Game Saved in Slot {slot}!");

            sessionStartTime = DateTime.Now;
            totalPlayTimeSeconds = totalSeconds;
        }

        private int AskSaveSlot()
        {
            var window = new Window
            {
                Title = "Select Save Slot",
                Width = 400,
                Height = 300,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                Background = System.Windows.Media.Brushes.Black,
                Owner = Application.Current.MainWindow
            };

            var stack = new StackPanel { Margin = new Thickness(20) };

            for (int i = 1; i <= 3; i++)
            {
                var slotPath = Path.Combine("Saves", $"save{i}.json");
                string display = $"Slot {i}: (Empty)";

                if (File.Exists(slotPath))
                {
                    try
                    {
                        string json = File.ReadAllText(slotPath);
                        var saveData = JsonSerializer.Deserialize<SaveData>(json);

                        if (saveData != null)
                        {
                            var playtime = TimeSpan.FromSeconds(saveData.PlayTimeSeconds);
                            string timeFormatted = $"{(int)playtime.TotalHours}h {playtime.Minutes}m {playtime.Seconds}s";

                            display = $"Slot {i}: Lv {saveData.PartyAverageLevel} - {saveData.LocationName}\n" +
                                      $"Saved: {saveData.SaveTimestamp:yyyy-MM-dd HH:mm} — Playtime: {timeFormatted}";
                        }
                    }
                    catch
                    {
                        display = $"Slot {i}: (Corrupt)";
                    }
                }

                var button = new Button
                {
                    Content = display,
                    Width = 300,
                    Height = 50,
                    Margin = new Thickness(5),
                    Tag = i
                };

                button.Click += (s, e) =>
                {
                    window.Tag = (int)((Button)s).Tag;
                    window.Close();
                };

                stack.Children.Add(button);
            }

            window.Content = stack;
            window.ShowDialog();

            if (window.Tag is int slot)
                return slot;
            return -1;
        }

        private void OptionsButton_Click(object sender, RoutedEventArgs e)
        {
            GameMenuContent.Content = CreatePlaceholderPage("Options Page");
        }

        private void CloseGameMenu_Click(object sender, RoutedEventArgs e)
        {
            GameMenu.Visibility = Visibility.Collapsed;
        }

        private async void LoadButton_Click(object sender, RoutedEventArgs e)
        {
            var slot = AskSaveSlot();
            if (slot == -1) return;

            if (!SaveGameManager.SaveExists(slot))
            {
                MessageBox.Show("No save file found in that slot!");
                return;
            }

            if (!SaveGameManager.SaveExists(slot))
            {
                MessageBox.Show("No save file found!");
                return;
            }

            try
            {
                var saveData = SaveGameManager.LoadGame(slot);

                allCharacters = saveData.Characters;
                playerInventory = saveData.Inventory;
                weaponStock = saveData.WeaponStock;
                armorStock = saveData.ArmorStock;
                game.currentMapFile = saveData.CurrentMap;
                game.LoadFromFile(game.currentMapFile);

                game.playerX = saveData.PlayerX;
                game.playerY = saveData.PlayerY;

                activePartyOrder = allCharacters.Where(c => saveData.ActivePartyOrder.Contains(c.Name)).ToList();
                //totalGold = saveData.Gold; Not Implemented Yet
                totalPlayTimeSeconds = saveData.PlayTimeSeconds;

                MessageBox.Show("Game Loaded!");

                // Fade to black first
                await FadeToBlack();

                // Switch screens AFTER full black
                StartScreen.Visibility = Visibility.Collapsed;
                GameCanvas.Visibility = Visibility.Visible;

                // Small delay to make sure WPF refreshes layout
                await Task.Delay(500);

                // Then fade back in
                await FadeFromBlack();

                sessionStartTime = DateTime.Now;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load save: {ex.Message}");
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dialogueManager = new DialogueManager(DialogueUI.TextElement, DialogueUI.Container, DialogueUI.Portrait, DialogueUI.Speaker, DialogueUI.ContinueIcon);
            game = new Game(GameCanvas);

            InitializeInventory();
            InitializeCharacters();
            InitializeEquipment();
            RefreshActivePartyOrder();

            GameCanvas.Visibility = Visibility.Collapsed; // Hide gameplay at start

            CompositionTarget.Rendering += (s, eventArgs) =>
            {
                if (StartScreen.Visibility == Visibility.Visible || GameMenu.Visibility == Visibility.Visible)
                    return; // Don't update world if Start Menu or Game Menu open

                if (!dialogueManager.IsActive)
                    game.Update();

                game.Render();
            };

            Loaded += (s, routedEvent) => Keyboard.Focus(GameCanvas);
        }
    }
}