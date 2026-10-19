using System;
using System.Windows;
using System.Windows.Controls;

namespace JRPG.UI.GameMenu
{
    /// <summary>
    /// Base class of the pages shown in the game menu (Inventory, Equipment...).
    /// A page never talks to the menu directly: it calls these helpers,
    /// and GameMenuControl listens to the matching events.
    /// </summary>
    public class MenuPage : UserControl
    {
        public event Action<string> HintRequested;
        public event Action<UIElement> SecondaryRequested;
        public event Action SecondaryClosed;
        public event Action PartyChanged;

        /// <summary>Shows a hint in the menu footer. Null goes back to the page's default hint.</summary>
        protected void ShowHint(string text) => HintRequested?.Invoke(text);

        /// <summary>Shows a panel on the right of the page, in place of the party (e.g. the equipment picker).</summary>
        protected void ShowSecondary(UIElement panel) => SecondaryRequested?.Invoke(panel);

        protected void HideSecondary() => SecondaryClosed?.Invoke();

        /// <summary>Tells the menu the party changed (HP, members...), so it can refresh the sidebar.</summary>
        protected void NotifyPartyChanged() => PartyChanged?.Invoke();
    }
}
