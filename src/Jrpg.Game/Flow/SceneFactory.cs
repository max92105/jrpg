using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Core.Session;
using Jrpg.Game.Battle;
using Jrpg.Game.Exploration;
using Jrpg.Game.Hosting;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Saving;
using Jrpg.Game.Title;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Flow;

/// <summary>
/// Builds the scenes from what they share, so the transitions that swap scenes don't each carry every scene's
/// dependencies.
/// </summary>
internal sealed class SceneFactory
{
    private readonly ContentCatalog _catalog;
    private readonly SaveSlots _saveSlots;
    private readonly PlayerSettings _settings;
    private readonly ScreenText _text;
    private readonly MapArt _art;
    private readonly UiTextures _ui = new();

    /// <param name="catalog">The content, as loaded.</param>
    /// <param name="text">Messages and fonts for everything a scene writes.</param>
    /// <param name="saveSlots">The save files the menu writes and loads.</param>
    /// <param name="settings">The player's options: the Options page edits them, dialogue reads the text speed.</param>
    public SceneFactory(ContentCatalog catalog, ScreenText text, SaveSlots saveSlots, PlayerSettings settings)
    {
        _catalog = catalog;
        _saveSlots = saveSlots;
        _settings = settings;
        _text = text;
        _art = new MapArt(catalog, MapView.CellPixels);
    }

    public TitleScene CreateTitle() => new(_text, _saveSlots, _ui);

    /// <param name="session">The playthrough the header reports on.</param>
    public GameMenuScene CreateGameMenu(GameSession session) =>
        new(_text, _ui, new MenuHeader(_text, session), [new InventoryPage(_text, _ui, session, _catalog), new PartyPage(_text, _ui, session, _catalog), new EquipmentPage(_text, _ui, session, _catalog), new StatusPage(_text, _ui, session, _catalog), new OptionsPage(_text, _ui, _settings), CreateSystemPage(session)]);

    /// <param name="session">The playthrough whose active battle the screen shows.</param>
    public BattleScene CreateBattle(GameSession session) => new(session, _catalog.Items, _text, _ui);

    public ExplorationScene CreateExploration(ExplorationState exploration) =>
        new(exploration, _catalog, _art, new EventPresenter(exploration, _text, _settings));

    private SystemPage CreateSystemPage(GameSession session) =>
        new(_text, _ui, new SavePage(_saveSlots, session, _text, _ui), new LoadPage(_saveSlots, _text, _ui));
}
