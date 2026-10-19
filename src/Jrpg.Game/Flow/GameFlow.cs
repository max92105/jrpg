using Jrpg.Core.Content;
using Jrpg.Core.Session;
using Jrpg.Game.Exploration;
using Jrpg.Game.Input;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Flow;

/// <summary>
/// Owns the screens and the one playthrough (ADR 0008). Its update never touches the graphics device, so it can run
/// headless in tests; drawing is separate.
/// </summary>
internal sealed class GameFlow : IScene
{
    private readonly SceneStack _scenes = new();

    public GameFlow(ContentCatalog catalog)
    {
        var playedCatalog = EncounterFreeContent.Of(catalog);
        var art = new MapArt(playedCatalog, MapView.CellPixels);
        Session = new SessionTransitions(playedCatalog, _scenes, art).StartNewGame();
    }

    public GameSession Session { get; }

    /// <summary>Play time counts every update, menus and pause included (developer decision 35).</summary>
    public void Update(TimeSpan delta, InputFrame input)
    {
        Session.AddPlayTime(delta);
        _scenes.Update(delta, input);
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures) => _scenes.Draw(batch, textures);
}
