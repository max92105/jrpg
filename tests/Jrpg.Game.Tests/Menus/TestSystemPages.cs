using Jrpg.Core.Session;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Saving;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

/// <summary>A System page whose save files are never written, for tests that don't save or load.</summary>
internal static class TestSystemPages
{
    public static SystemPage Create(GameSession session, UiTextures ui)
    {
        var slots = UnwrittenSaveSlots.Create();
        var text = RepositoryScreenText.Text;
        return new SystemPage(text, ui, new SavePage(slots, session, text, ui), new LoadPage(slots, text, ui));
    }
}
