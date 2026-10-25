using Jrpg.Core.Statuses;

namespace Jrpg.Core.Effects;

/// <summary>What a skill or consumable does, how strongly, and to whom.</summary>
/// <param name="Kind">What it changes.</param>
/// <param name="Power">How much: HP for damage and healing; unused by a status-only effect.</param>
/// <param name="Targeting">Whom it may be aimed at.</param>
/// <param name="Status">Granted to each target still standing after the effect; null when none (ADR 0021).</param>
public sealed record Effect(EffectKind Kind, int Power, Targeting Targeting, StatusGrant? Status = null);
