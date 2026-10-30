using Jrpg.Game.Ui;

namespace Jrpg.Game.Exploration;

/// <summary>What a map event's pages, stills and the inn's question are drawn with, passed as one.</summary>
/// <param name="Text">Messages and fonts.</param>
/// <param name="Ui">The shared textures: the question's window and the coin.</param>
internal sealed record EventLook(ScreenText Text, UiTextures Ui);
