using System.Windows;

namespace Jrpg.Editor.Objects;

/// <summary>
/// The New object dialog. Create hands the id and category to the window's create step, which returns false when no
/// object was made (a field has a problem), and the dialog then stays open.
/// </summary>
internal sealed partial class NewObjectWindow : Window
{
    private readonly NewObjectViewModel _newObject;
    private readonly Func<NewObjectViewModel, bool> _create;

    /// <param name="newObject">The id and category, edited here.</param>
    /// <param name="create">Makes the object; false when it didn't.</param>
    public NewObjectWindow(NewObjectViewModel newObject, Func<NewObjectViewModel, bool> create)
    {
        InitializeComponent();
        DataContext = _newObject = newObject;
        _create = create;
        Loaded += (_, _) => IdBox.Focus();
    }

    private void OnCreateClicked(object sender, RoutedEventArgs e)
    {
        if (_create(_newObject))
        {
            DialogResult = true;
        }
    }
}
