using System.IO;
using System.Windows;
using System.Windows.Threading;
using Jrpg.Data.Content;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Hosting;

/// <summary>
/// Starts the editor on the content folder the game uses (decision 77): the repository's in Debug, or <c>--content</c>.
/// An exception nothing caught on the UI thread no longer closes the editor (ADR 0050): it is written, the developer is
/// asked, and the draft stays; before the main window has loaded it is written and still ends the editor.
/// </summary>
internal sealed partial class App : Application
{
    private MainViewModel? _viewModel;
    private UnexpectedErrors? _errors;
    private UnexpectedErrorLog? _errorLog;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var contentRoot = ContentRootLocator.Find(e.Args, typeof(App).Assembly);
        var reports = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Jrpg", "Editor", "Errors");
        _errorLog = new UnexpectedErrorLog(reports, contentRoot, Console.Error);
        _errors = new UnexpectedErrors(_errorLog, Ask);
        DispatcherUnhandledException += OnUnexpectedError;
        _viewModel = new MainViewModel(contentRoot, Shell.MainWindow.Ask, SaveChangesWindow.Ask);
        new Shell.MainWindow(_viewModel).Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _errors?.AddRepeatCounts();
        base.OnExit(e);
    }

    // The editor isn't Core: the wall clock names the report, as the game's save stamp does.
    private void OnUnexpectedError(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        if (MainWindow is not { IsLoaded: true })
        {
            _errorLog!.Write(e.Exception, DateTimeOffset.Now);
            return;
        }

        e.Handled = true;
        if (_errors!.Handle(e.Exception, DateTimeOffset.Now) == UnexpectedErrorAnswer.CloseEditor)
        {
            // After this handler returns, so the close question doesn't open inside it.
            Dispatcher.BeginInvoke(MainWindow.Close);
        }
    }

    private UnexpectedErrorAnswer Ask(UnexpectedErrorReport report)
    {
        var text = UnexpectedErrorQuestion.Text(report, _viewModel?.UnsavedChanges);
        var answer = MessageBox.Show(text, UnexpectedErrorQuestion.Title, MessageBoxButton.YesNo, MessageBoxImage.Error);
        return answer == MessageBoxResult.No ? UnexpectedErrorAnswer.CloseEditor : UnexpectedErrorAnswer.KeepWorking;
    }
}
