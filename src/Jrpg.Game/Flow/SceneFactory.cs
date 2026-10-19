using Jrpg.Core.Content;
using Jrpg.Core.Exploration;
using Jrpg.Game.Exploration;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
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
    private readonly ScreenText _text;
    private readonly MapArt _art;
    private readonly UiTextures _ui = new();

    /// <param name="catalog">The content as played, without random battles.</param>
    /// <param name="text">Messages and fonts for everything a scene writes.</param>
    public SceneFactory(ContentCatalog catalog, ScreenText text)
    {
        _catalog = catalog;
        _text = text;
        _art = new MapArt(catalog, MapView.CellPixels);
    }

    public TitleScene CreateTitle() => new(_text);

    public GameMenuScene CreateGameMenu() => new(_text, _ui, [new SystemPage(_text)]);

    public ExplorationScene CreateExploration(ExplorationState exploration) => new(exploration, _catalog, _art, _text);
}
