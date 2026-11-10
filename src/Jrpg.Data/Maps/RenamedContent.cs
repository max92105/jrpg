using Jrpg.Core.Content;

namespace Jrpg.Data.Maps;

/// <summary>What a map id rename changed, so the writer writes only that (decision 530).</summary>
/// <param name="Content">The content with the map and every reference to it renamed.</param>
/// <param name="ChangedMapIds">The other maps whose warp steps led to the renamed map.</param>
/// <param name="IsNewGameChanged">Whether the new game started on the renamed map.</param>
internal sealed record RenamedContent(ContentCatalog Content, IReadOnlyList<string> ChangedMapIds, bool IsNewGameChanged);
