namespace Jrpg.Core.Battle;

/// <summary>
/// One effect strip's pace (ADR 0048, decision 654): how long each frame shows, the frame its hit lands on and the
/// swing an attack plays before it, by the strip's file name in <c>effects/</c>. A strip with no record plays at
/// <see cref="UnlistedFrameTime"/>, its hit on frame 0, no swing. A projectile, flying from the attacker to its target,
/// would be another strip named here (decision 665).
/// </summary>
public sealed record EffectStripDefinition
{
    /// <summary>The strip's file name in the effects folder, exact case.</summary>
    public required string File { get; init; }

    /// <summary>How long each square frame shows, in whole milliseconds from 1.</summary>
    public required int FrameMilliseconds { get; init; }

    /// <summary>The frame, from 0, that shows at the moment the hit lands.</summary>
    public int HitFrame { get; init; }

    /// <summary>
    /// The strip an Attack plays in front of its attacker before this one lands (decision 657); none when null.
    /// </summary>
    public string? SwingFile { get; init; }

    /// <summary>
    /// How long each frame of a strip no record lists shows: decision 57's 80 ms, every strip's pace before ADR 0048.
    /// </summary>
    public static TimeSpan UnlistedFrameTime { get; } = TimeSpan.FromMilliseconds(80);

    /// <summary><see cref="FrameMilliseconds"/> as a time, what the battle's clock compares.</summary>
    public TimeSpan FrameTime => TimeSpan.FromMilliseconds(FrameMilliseconds);

    /// <summary>How long the strip plays before its hit frame shows.</summary>
    public TimeSpan HitLead => HitFrame * FrameTime;

    /// <param name="file">The strip's file name in the effects folder.</param>
    /// <param name="listed">The records by file name, from <c>effect-strips.json</c>.</param>
    /// <returns>Its record, or the unlisted pace with its hit on frame 0 and no swing.</returns>
    public static EffectStripDefinition Of(string file, IReadOnlyDictionary<string, EffectStripDefinition> listed) =>
        listed.TryGetValue(file, out var record)
            ? record
            : new EffectStripDefinition { File = file, FrameMilliseconds = (int)UnlistedFrameTime.TotalMilliseconds };
}
