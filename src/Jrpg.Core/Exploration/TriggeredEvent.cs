using Jrpg.Core.Events;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>An event about to run, with the object it belongs to (null for a cell event).</summary>
internal sealed record TriggeredEvent(MapEvent Event, PlacedObject? Source);
