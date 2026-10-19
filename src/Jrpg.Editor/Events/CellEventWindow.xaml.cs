using System.Windows;

namespace Jrpg.Editor.Events;

/// <summary>The event dialog for one cell; OK keeps the edited event, Cancel drops it.</summary>
internal sealed partial class CellEventWindow : Window
{
    private readonly CellEventViewModel _cellEvent;

    public CellEventWindow(CellEventViewModel cellEvent)
    {
        InitializeComponent();
        DataContext = _cellEvent = cellEvent;
    }

    // An event without steps, or with a number left half typed, keeps the dialog open, saying what is missing.
    private void OnOkClicked(object sender, RoutedEventArgs e)
    {
        ProblemText.Text = _cellEvent.Problem;
        if (_cellEvent.Problem is null)
        {
            DialogResult = true;
        }
    }
}
