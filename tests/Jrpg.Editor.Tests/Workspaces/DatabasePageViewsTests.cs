using System.Windows.Controls;
using Jrpg.Editor.Workspaces;

namespace Jrpg.Editor.Tests.Workspaces;

// A TextBlock stands for a page's view: building one needs an STA thread, as any WPF element does.
public sealed class DatabasePageViewsTests
{
    [Fact]
    public void ViewOf_RegisteredCatalog_BuildsItsView()
    {
        StaThread.Run(() =>
        {
            var pageViews = new DatabasePageViews();
            pageViews.Register<NotesCatalog>(notes => new TextBlock { Text = notes.ChosenKey });
            var notes = new NotesCatalog();
            notes.Choose("first");

            var view = pageViews.ViewOf(notes);

            Assert.Equal("first", ((TextBlock)view).Text);
        });
    }

    [Fact]
    public void ViewOf_UnregisteredCatalog_ThrowsNamingIt()
    {
        StaThread.Run(() =>
        {
            var thrown = Assert.Throws<InvalidOperationException>(() => new DatabasePageViews().ViewOf(new NotesCatalog()));

            Assert.Contains(nameof(NotesCatalog), thrown.Message);
        });
    }
}
