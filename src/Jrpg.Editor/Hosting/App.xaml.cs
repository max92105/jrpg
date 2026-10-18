using System.Windows;
using Jrpg.Data.Content;
using Jrpg.Editor.Shell;

namespace Jrpg.Editor.Hosting;

/// <summary>Starts the editor on the content folder the game uses (decision 77): the repository's in Debug, or <c>--content</c>.</summary>
internal sealed partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var contentRoot = ContentRootLocator.Find(e.Args, typeof(App).Assembly);
        new Shell.MainWindow(new MainViewModel(contentRoot, Shell.MainWindow.AskToDiscard)).Show();
    }
}
