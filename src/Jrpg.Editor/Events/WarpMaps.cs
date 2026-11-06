using Jrpg.Core.Content;
using Jrpg.Core.Maps;
using Jrpg.Editor.Maps;

namespace Jrpg.Editor.Events;

/// <summary>
/// The maps a warp can lead to: their list rows and layouts, and the catalog saying where the player stands.
/// </summary>
/// <param name="Entries">The map list's rows, in its order.</param>
/// <param name="Layouts">Each map's layout as it is now, unsaved changes included, by id.</param>
/// <param name="Catalog">The tiles and objects the layouts draw.</param>
internal sealed record WarpMaps(IReadOnlyList<MapEntry> Entries, IReadOnlyDictionary<string, MapLayout> Layouts, ContentCatalog Catalog);
