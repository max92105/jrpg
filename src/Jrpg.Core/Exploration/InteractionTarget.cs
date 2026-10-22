using Jrpg.Core.Events;
using Jrpg.Core.Maps;

namespace Jrpg.Core.Exploration;

/// <summary>The object the player faces and the interact event the confirm button would start on it.</summary>
public sealed record InteractionTarget(PlacedObject Placed, MapEvent Event);
