using System.ComponentModel;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;

namespace Jrpg.Editor.Workspaces;

/// <summary>
/// The Database half of the main window, shown in place of the map while Database is chosen (decision 925): the page
/// list, and the shown page built anew for each session of the workspace, since a session starts again on every draft
/// change (decision 921).
/// </summary>
internal sealed partial class DatabaseWorkspaceView : UserControl
{
    private DatabaseWorkspaceViewModel? _workspace;
    private DatabasePageViews? _pageViews;

    public DatabaseWorkspaceView() => InitializeComponent();

    /// <summary>Shows the workspace's page sessions from now on, each page's view built by the views given.</summary>
    public void Host(DatabaseWorkspaceViewModel workspace, DatabasePageViews pageViews)
    {
        (_workspace, _pageViews) = (workspace, pageViews);
        workspace.PropertyChanged += OnWorkspaceChanged;
    }

    private void OnWorkspaceChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DatabaseWorkspaceViewModel.Session))
        {
            ShowSession(_workspace!.Session);
        }
    }

    // The automation name lets the smoke script find the page (decision 933). A page that held the keyboard focus hands
    // it to the new one, so the keys typed next still reach the page (decision 940).
    private void ShowSession(PageSession? session)
    {
        var hadFocus = PageHost.IsKeyboardFocusWithin;
        if (session is null)
        {
            PageHost.Content = null;
            return;
        }

        var page = _pageViews!.ViewOf(session.Edited);
        AutomationProperties.SetName(page, $"{_workspace!.ShownPage!.Title} page");
        if (hadFocus)
        {
            page.Loaded += TakeFocus;
        }

        PageHost.Content = page;
    }

    private static void TakeFocus(object sender, RoutedEventArgs e)
    {
        var page = (FrameworkElement)sender;
        page.Loaded -= TakeFocus;
        page.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
    }
}
