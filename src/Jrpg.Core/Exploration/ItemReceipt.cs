using Jrpg.Core.Items;

namespace Jrpg.Core.Exploration;

/// <summary>
/// What a give-item step actually did, so the message can tell the truth (ADR 0006). Copies that didn't fit because the
/// stack was full are lost (developer decision 29, placeholder).
/// </summary>
public sealed record ItemReceipt(ItemDefinition Item, int Added, int Lost);
