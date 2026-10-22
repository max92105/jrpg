using Jrpg.Core.Events;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>
/// An event about to run, with the object it belongs to (null for a cell event). A struct: the interact prompt looks for
/// one every update.
/// </summary>
internal readonly record struct TriggeredEvent(MapEvent Event, PlacedObject? Source);
