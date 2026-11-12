using Jrpg.Game.Battle;
using Jrpg.Game.Input;
using Jrpg.Game.Tests.Input;

namespace Jrpg.Game.Tests.Battle;

// Presses on a given command menu: a fixed count, or until an entry is selected.
internal static class CommandMenuDriver
{
    public static void Press(CommandMenu menu, GameButton button, int times)
    {
        for (var press = 0; press < times; press++)
        {
            menu.Update(InputFrames.Pressing(button));
        }
    }

    // Bounded by the entries, so a list without the entry fails the test instead of looping forever.
    public static void SelectEntry(CommandMenu menu, CommandEntry entry)
    {
        for (var presses = 0; presses < CommandMenu.Entries.Length && menu.Selected != entry; presses++)
        {
            menu.Update(InputFrames.Pressing(GameButton.Down));
        }

        Assert.Equal(entry, menu.Selected);
    }
}
