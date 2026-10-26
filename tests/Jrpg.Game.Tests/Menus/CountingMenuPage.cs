using Jrpg.Game.Input;
using Jrpg.Game.Menus;
using Jrpg.Game.Rendering;
using Jrpg.Game.Scenes;
using Jrpg.Game.Ui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Jrpg.Game.Tests.Menus;

/// <summary>A page that only counts how often the menu opened it, and records the order of its calls.</summary>
internal sealed class CountingMenuPage : IMenuPage
{
    public int OpenCount { get; private set; }

    public List<string> Calls { get; } = [];

    public TimeSpan LastAdvance { get; private set; }

    public string TitleMessageId => MessageIds.MenuStatus;

    public string Hint => "Counting page hint";

    public bool ShowsPartySidebar { get; init; } = true;

    public bool LeavesOnCancel => true;

    public void Open() => OpenCount++;

    public void Advance(TimeSpan delta)
    {
        LastAdvance = delta;
        Calls.Add(nameof(Advance));
    }

    public SceneRequest? Update(InputFrame input)
    {
        Calls.Add(nameof(Update));
        return null;
    }

    public void Draw(SpriteBatch batch, TextureLibrary textures, Rectangle area, float opacity)
    {
    }
}
