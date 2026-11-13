using Jrpg.Core.Geometry;

namespace Jrpg.Core.Exploration;

/// <summary>Where an NPC stands or walks now and which way it looks (ADR 0044), as the Game draws it.</summary>
/// <param name="Position">Its top-left corner in map cells; between cells while it walks.</param>
/// <param name="Facing">What it faces (decision 577).</param>
/// <param name="Walked">How long it has walked in its current walk, for its walk frame; zero while standing.</param>
/// <param name="IsWalking">Whether a cutscene walk is moving it now.</param>
public readonly record struct NpcPose(Point2D Position, Facing Facing, TimeSpan Walked, bool IsWalking);
