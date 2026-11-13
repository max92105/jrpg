using Jrpg.Editor.Trade;

namespace Jrpg.Editor.Shell;

/// <summary>
/// The Database menu's windows over the draft (ADR 0045, decision 583): the shops, beside the quests.
/// </summary>
internal sealed partial class MainViewModel
{
    public ShopEditingViewModel? ShopEditing { get; }

    /// <summary>Whether Database > Shops... can open its window: false while the content didn't load.</summary>
    public bool HasShopEditing => ShopEditing is not null;
}
