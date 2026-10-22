using Jrpg.Core.Items;

namespace Jrpg.Core.Party;

/// <summary>An item a member wears, such as another member's sword that could be taken from them.</summary>
public sealed record WornItem(PartyMember Wearer, ItemDefinition Item);
