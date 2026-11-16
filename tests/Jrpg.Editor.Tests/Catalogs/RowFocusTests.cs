using System.Windows.Controls;
using System.Windows.Input;
using Jrpg.Editor.Catalogs;

namespace Jrpg.Editor.Tests.Catalogs;

// A typed row chosen when a box in it takes the focus, the focus event raised by the test: no window needed.
public sealed class RowFocusTests
{
    [Fact]
    public void ChoosesRow_BoxInTheRowFocused_ChoosesTheRow()
    {
        var isSelected = false;

        StaThread.Run(() =>
        {
            var row = new ListBoxItem();
            RowFocus.SetChoosesRow(row, true);

            Focus(row);

            isSelected = row.IsSelected;
        });

        Assert.True(isSelected);
    }

    [Fact]
    public void ChoosesRow_NotSet_LeavesTheRowUnchosen()
    {
        var isSelected = true;

        StaThread.Run(() =>
        {
            var row = new ListBoxItem();

            Focus(row);

            isSelected = row.IsSelected;
        });

        Assert.False(isSelected);
    }

    [Fact]
    public void ChoosesRow_SetThenCleared_LeavesTheRowUnchosen()
    {
        var isSelected = true;

        StaThread.Run(() =>
        {
            var row = new ListBoxItem();
            RowFocus.SetChoosesRow(row, true);
            RowFocus.SetChoosesRow(row, false);

            Focus(row);

            isSelected = row.IsSelected;
        });

        Assert.False(isSelected);
    }

    private static void Focus(ListBoxItem row) =>
        row.RaiseEvent(new KeyboardFocusChangedEventArgs(Keyboard.PrimaryDevice, 0, null, null)
        {
            RoutedEvent = Keyboard.PreviewGotKeyboardFocusEvent,
        });
}
