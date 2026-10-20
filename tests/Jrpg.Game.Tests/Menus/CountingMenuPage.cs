using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Tests.Menus;

/// <summary>A page that only counts how often the menu opened it.</summary>
internal sealed class CountingMenuPage : IMenuPage
{
    public int OpenCount { get; private set; }

    public string TitleMessageId => MessageIds.MenuStatus;

    public bool LeavesOnCancel => true;

    public void Open() => OpenCount++;

    public SceneRequest? Update(InputFrame input) => null;

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
    }
}
