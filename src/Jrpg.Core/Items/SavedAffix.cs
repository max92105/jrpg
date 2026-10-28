namespace Jrpg.Core.Items;

/// <summary>A rolled affix as a save keeps it: its stat comes from the affix pool on load (ADR 0029).</summary>
public sealed record SavedAffix(string AffixId, int Value);
