using Jrpg.Core.Items;

namespace Jrpg.Core.Content;

/// <summary>All loaded content definitions, by id.</summary>
public sealed record ContentCatalog(IReadOnlyDictionary<string, ItemDefinition> Items);
