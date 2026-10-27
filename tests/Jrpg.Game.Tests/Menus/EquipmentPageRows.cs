using Jrpg.Core.Items;
using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using static Jrpg.Game.Tests.Input.InputFrames;

namespace Jrpg.Game.Tests.Menus;

/// <summary>
/// The Equipment page's rows by what they are, so a test never counts them: a fresh page starts on the first.
/// </summary>
internal static class EquipmentPageRows
{
    /// <summary>The row after the last slot.</summary>
    public static int RemoveAll => EquipmentSheet.Slots.Length;

    public static int Of(EquipmentSlot slot) => EquipmentSheet.Slots.IndexOf(slot);

    /// <summary>Presses Down <paramref name="rows"/> times.</summary>
    public static void MoveDown(EquipmentPage page, int rows)
    {
        for (var step = 0; step < rows; step++)
        {
            page.Update(Pressing(GameButton.Down));
        }
    }
}
