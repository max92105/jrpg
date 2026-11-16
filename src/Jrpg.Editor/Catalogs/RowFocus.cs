using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jrpg.Editor.Catalogs;

/// <summary>
/// A database page's typed row (a drop entry, a skill's cost or grant, a status's part) chosen as soon as a box in it
/// takes the focus, so Add, Remove and the moves act on the row being typed in. Set on the row's container from the
/// list's item style. The focus event rather than a trigger on <c>IsKeyboardFocusWithin</c>: the trigger lets go as
/// soon as a button takes the focus, and a move rebuilds the row's container, so the button would act on another row.
/// </summary>
internal static class RowFocus
{
    public static readonly DependencyProperty ChoosesRowProperty = DependencyProperty.RegisterAttached(
        "ChoosesRow",
        typeof(bool),
        typeof(RowFocus),
        new PropertyMetadata(false, OnChoosesRowChanged));

    public static bool GetChoosesRow(ListBoxItem row) => (bool)row.GetValue(ChoosesRowProperty);

    public static void SetChoosesRow(ListBoxItem row, bool value) => row.SetValue(ChoosesRowProperty, value);

    private static void OnChoosesRowChanged(DependencyObject target, DependencyPropertyChangedEventArgs e)
    {
        var row = (ListBoxItem)target;
        row.PreviewGotKeyboardFocus -= ChooseRow;
        if ((bool)e.NewValue)
        {
            row.PreviewGotKeyboardFocus += ChooseRow;
        }
    }

    private static void ChooseRow(object sender, KeyboardFocusChangedEventArgs e) => ((ListBoxItem)sender).IsSelected = true;
}
