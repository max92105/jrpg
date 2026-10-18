using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Randomness;
using Jrpg.Core.World;

namespace Jrpg.Core.Exploration;

/// <summary>
/// What exploration reads and changes outside itself: the content, the party's items, the world's changes, and the seeded
/// random source for battle rolls.
/// </summary>
internal sealed record ExplorationContext(ContentCatalog Catalog, Inventory Inventory, WorldState World, IRandom Random);
