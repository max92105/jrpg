using System.Collections.Frozen;
using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Data.Items;

namespace Jrpg.Data.Content;

public static class ContentCatalogLoader
{
    private const string ItemsFile = "items.json";

    /// <summary>Loads and validates every content file under <paramref name="contentRoot"/>.</summary>
    /// <exception cref="ContentValidationException">A file is malformed or breaks a content rule.</exception>
    public static ContentCatalog Load(string contentRoot)
    {
        var items = ContentFileReader.Read<IReadOnlyList<ItemDefinition>>(Path.Combine(contentRoot, ItemsFile));
        ItemCatalogValidator.Validate(ItemsFile, items);

        return new ContentCatalog(items.ToFrozenDictionary(item => item.Id));
    }
}
