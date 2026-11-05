using System.Windows;
using Jrpg.Core.Maps;

namespace Jrpg.Editor.Tiles;

/// <summary>
/// A sheet as the browser shows it: the cut its marks follow (the Cut panel's preview while it is open), its image's
/// size in its own pixels and the view's scale, which sizes the marks' lines.
/// </summary>
internal sealed record ShownSheet(TilesheetDefinition Cut, Size ImageSize, double Scale);
