using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using JRPG.Core.Battles.Models;
using JRPG.Core.Systems;
using JRPG.Core.Systems.Equipments.Models;
using JRPG.Core.Systems.Inventories.Models;
using JRPG.Core.Utils;
using JRPG.Enums;
using JRPG.Managers;
using JRPG.UI.GameMenu.Controls;
using JRPG.Utils;

namespace JRPG.UI.GameMenu
{
    /// <summary>
    /// The in-game menu shell (layout in GameMenuControl.xaml).
    /// Each nav tab has its own click handler, which builds a page and hands it to ShowPage.
    /// </summary>
    public partial class GameMenuControl : UserControl
    {
        private const string MenuTitle = "Menu";
        private const string MenuHint = "Choose a menu.";

        private static readonly Duration OpenDuration = TimeSpan.FromMilliseconds(150);
        private static readonly Duration PageTransitionDuration = TimeSpan.FromMilliseconds(220);
        private const double PageSlideDistance = 24;

        // How far the System sub menu tucks under its tab, so it reads as unfolding from it
        private const double SubMenuOverlap = 40;

        public Action<int> OnLoadSlotConfirmed { get; set; }
        public event Func<Task> ReturnToMainMenuRequested;

        // Shown in the footer when nothing more specific is hovered
        private string pageHint = MenuHint;
        private bool pageShowsPartySidebar;

        // The nav tab of the open page, so it can be re-selected when the System sub menu closes
        private MenuTab currentPageTab;

        public GameMenuControl()
        {
            InitializeComponent();

            MenuBackground.Source = ImageLoader.LoadBackground("GameMenu.png");
            ShowNoPage();
        }

        // ============ Open / close ============

        public bool IsOpen => Visibility == Visibility.Visible;

        public void Open()
        {
            Visibility = Visibility.Visible;
            Panel.SetZIndex(this, 1000);
            GameStateManager.SetState(SceneType.Menu);

            Header.Start();
            BeginAnimation(OpacityProperty, new DoubleAnimation(0, 1, OpenDuration));
        }

        /// <summary>Closes the menu and resets it, so it reopens on the main list next time.</summary>
        public void Close()
        {
            ShowNoPage();
            Header.Stop();

            Visibility = Visibility.Collapsed;
            Panel.SetZIndex(this, 0);
            GameStateManager.SetState(SceneType.Exploration);
        }

        private void CloseHint_Click(object sender, MouseButtonEventArgs e) => Close();

        // ============ Nav tabs ============

        private void Inventory_Click(object sender, RoutedEventArgs e)
        {
            var categories = new List<FilterOption<Type>>
            {
                new FilterOption<Type> { IconFile = "All.png", Label = "All Items", FilterValue = null },
                new FilterOption<Type> { IconFile = "Consumable.png", Label = "Consumables", FilterValue = typeof(Consumable) },
                new FilterOption<Type> { IconFile = "Armor.png", Label = "Armor", FilterValue = typeof(Armor) },
                new FilterOption<Type> { IconFile = "Weapon.png", Label = "Weapons", FilterValue = typeof(Weapon) }
            };

            ShowPage("Inventory", "Menu/Inventory", new InventoryView(), "Hover an item to read about it. Click to use it.",
                CreateCategoryTabs(categories));
        }

        private void Party_Click(object sender, RoutedEventArgs e)
        {
            // The page already shows the whole party, so the sidebar would repeat it
            ShowPage("Party", "Menu/Party", new PartyView(), "Choose who fights, and in what order.",
                showPartySidebar: false);
        }

        private void Equipment_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("Equipment", "Menu/Equipment", new EquipmentView(), "Choose a slot to change what is worn.",
                CreateCategoryTabs(CharacterFilters()));
        }

        private void Status_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("Status", "Menu/Status", new StatusView(), "Choose a character on the left.",
                CreateCategoryTabs(CharacterFilters()));
        }

        // ============ System sub menu ============

        private void System_Click(object sender, RoutedEventArgs e)
        {
            if (SystemSubMenu.Visibility == Visibility.Visible)
            {
                CloseSubMenu();
                return;
            }

            // Unfold just under the middle of the System tab, starting a little inside it
            Point anchor = SystemTab.TranslatePoint(
                new Point(SystemTab.ActualWidth - SubMenuOverlap, SystemTab.ActualHeight / 2), SubMenuLayer);
            Canvas.SetLeft(SystemSubMenu, anchor.X);
            Canvas.SetTop(SystemSubMenu, anchor.Y);

            SystemSubMenu.Visibility = Visibility.Visible;
            SubMenuDismissLayer.Visibility = Visibility.Visible;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            // Saving is handled inside the view
            var page = new SaveLoadView(SaveLoadView.SaveLoadMode.Save, slotIndex => Task.CompletedTask, () => { });

            ShowPage("Save", "Menu/Save", page, "Choose a file to save to.");
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            var page = new SaveLoadView(
                SaveLoadView.SaveLoadMode.Load,
                slotIndex =>
                {
                    OnLoadSlotConfirmed?.Invoke(slotIndex);
                    return Task.CompletedTask;
                },
                ShowNoPage);

            ShowPage("Load", "Menu/Load", page, "Choose a file to load.");
        }

        private void Options_Click(object sender, RoutedEventArgs e)
        {
            ShowPage("Options", "Menu/Options", new OptionsView(), "Nothing to set yet.");
        }

        private async void TitleScreen_Click(object sender, RoutedEventArgs e)
        {
            ShowNoPage();

            if (ReturnToMainMenuRequested != null)
                await ReturnToMainMenuRequested.Invoke();
        }

        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void SubMenuDismissLayer_Click(object sender, MouseButtonEventArgs e) => CloseSubMenu();

        private void CloseSubMenu()
        {
            SystemSubMenu.Visibility = Visibility.Collapsed;
            SubMenuDismissLayer.Visibility = Visibility.Collapsed;

            // The System tab only stays lit while one of its pages is open
            if (currentPageTab != SystemTab)
            {
                SystemTab.IsChecked = false;
                if (currentPageTab != null)
                    currentPageTab.IsChecked = true;
            }
        }

        // ============ Pages ============

        /// <summary>Shows a page: title in the header, icon as watermark, hint in the footer, category tabs on the rail.</summary>
        private void ShowPage(string title, string artKey, MenuPage page, string hint,
            IEnumerable<MenuTab> categories = null, bool showPartySidebar = true)
        {
            currentPageTab = NavPanel.Children.OfType<MenuTab>().FirstOrDefault(tab => tab.IsChecked == true);
            CloseSubMenu();
            SetNavCompact(true);

            Header.Title = title;
            PageWatermark.Source = UiArt.Get(artKey);
            pageHint = hint;
            ShowHint(null);

            page.HintRequested += ShowHint;
            page.SecondaryRequested += ShowSecondary;
            page.SecondaryClosed += HideSecondary;
            page.PartyChanged += PartySidebar.Refresh;

            GameMenuContent.Content = page;
            ContainerGameMenuContent.Visibility = Visibility.Visible;
            SetCategoryTabs(categories);

            pageShowsPartySidebar = showPartySidebar;
            PartySidebar.Refresh();
            HideSecondary();

            PlayPageTransition();
        }

        /// <summary>Back to the main menu: the full nav tabs over the background, nothing else.</summary>
        private void ShowNoPage()
        {
            currentPageTab = null;
            foreach (var tab in NavPanel.Children.OfType<MenuTab>())
                tab.IsChecked = false;
            CloseSubMenu();
            SetNavCompact(false);

            Header.Title = MenuTitle;
            pageHint = MenuHint;
            ShowHint(null);

            GameMenuContent.Content = null;
            ContainerGameMenuContent.Visibility = Visibility.Hidden;
            SetCategoryTabs(null);

            pageShowsPartySidebar = false;
            HideSecondary();
        }

        /// <summary>Shows a panel on the right, in place of the party (e.g. the equipment picker).</summary>
        private void ShowSecondary(UIElement panel)
        {
            SecondaryPanel.Content = panel;
            SecondaryPanelContainer.Visibility = Visibility.Visible;
            PartySidebar.Visibility = Visibility.Collapsed;
        }

        private void HideSecondary()
        {
            SecondaryPanel.Content = null;
            SecondaryPanelContainer.Visibility = Visibility.Collapsed;
            PartySidebar.Visibility = pageShowsPartySidebar ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>Shows a hint in the footer. Null or empty goes back to the page's own hint.</summary>
        private void ShowHint(string text)
        {
            HintText.Text = string.IsNullOrEmpty(text) ? pageHint : text;
        }

        /// <summary>Nav tabs show icon + label on the main menu, and only the icon while a page is open.</summary>
        private void SetNavCompact(bool isCompact)
        {
            foreach (var tab in NavPanel.Children.OfType<MenuTab>())
                tab.IsCompact = isCompact;
        }

        private void PlayPageTransition()
        {
            var ease = new CubicEase { EasingMode = EasingMode.EaseOut };

            PageSlide.BeginAnimation(TranslateTransform.XProperty,
                new DoubleAnimation(PageSlideDistance, 0, PageTransitionDuration) { EasingFunction = ease });
            ContainerGameMenuContent.BeginAnimation(OpacityProperty,
                new DoubleAnimation(0, 1, PageTransitionDuration));
        }

        // ============ Category tabs ============

        private void SetCategoryTabs(IEnumerable<MenuTab> tabs)
        {
            CategoryPanel.Children.Clear();

            foreach (var tab in tabs ?? Enumerable.Empty<MenuTab>())
                CategoryPanel.Children.Add(tab);

            // Hidden, not collapsed: the page keeps the same position whether or not it has categories
            CategoryRail.Visibility = CategoryPanel.Children.Count > 0 ? Visibility.Visible : Visibility.Hidden;
        }

        /// <summary>One tab per option. Clicking a tab filters the current page (it must be IFilterable).</summary>
        private List<MenuTab> CreateCategoryTabs<T>(IEnumerable<FilterOption<T>> options)
        {
            var tabs = new List<MenuTab>();

            foreach (var option in options)
            {
                string iconPath = option.PathResolver != null
                    ? option.PathResolver(option.IconFile)
                    : AssetPathResolver.GetIconsPath(option.IconFile);

                var tab = new MenuTab
                {
                    Style = (Style)FindResource("CategoryTabStyle"),
                    ToolTip = option.Label,
                    Icon = ImageLoader.Load(iconPath, 256)
                };

                tab.Click += (s, e) => (GameMenuContent.Content as IFilterable<T>)?.ApplyFilter(option.FilterValue);

                tabs.Add(tab);
            }

            // Pages open on their first category
            if (tabs.Count > 0)
                tabs[0].IsChecked = true;

            return tabs;
        }

        private static IEnumerable<FilterOption<Character>> CharacterFilters()
        {
            return GameSystems.Party.AllCharacters.Select(c => new FilterOption<Character>
            {
                FilterValue = c,
                IconFile = c.PortraitFile,
                Label = c.Name,
                PathResolver = AssetPathResolver.GetPortraitPath
            });
        }
    }
}
