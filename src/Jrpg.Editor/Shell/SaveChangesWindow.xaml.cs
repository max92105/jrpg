using System.Windows;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The question when the editor closes with unsaved changes: Save all, Don't save or Cancel (decision 501).
/// </summary>
internal sealed partial class SaveChangesWindow : Window
{
    private UnsavedChangesAnswer _answer = UnsavedChangesAnswer.Cancel;

    private SaveChangesWindow(string question)
    {
        InitializeComponent();
        QuestionText.Text = question;
    }

    /// <param name="question">What is unsaved, as <see cref="MainViewModel"/> words it.</param>
    public static UnsavedChangesAnswer Ask(string question)
    {
        var window = new SaveChangesWindow(question) { Owner = Application.Current.MainWindow };
        window.ShowDialog();
        return window._answer;
    }

    private void OnSaveAllClicked(object sender, RoutedEventArgs e) => CloseWith(UnsavedChangesAnswer.SaveAll);

    private void OnDiscardClicked(object sender, RoutedEventArgs e) => CloseWith(UnsavedChangesAnswer.Discard);

    private void CloseWith(UnsavedChangesAnswer answer)
    {
        _answer = answer;
        Close();
    }
}
