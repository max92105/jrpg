namespace Jrpg.Editor.Shell;

/// <summary>One step in the undo order (decision 503): the catalogs' or which open map's.</summary>
internal readonly record struct UndoStep
{
    private UndoStep(UndoTarget target, string? mapId)
    {
        Target = target;
        MapId = mapId;
    }

    /// <summary>The history the step went to.</summary>
    public UndoTarget Target { get; }

    /// <summary>The open map whose history holds the step; null for the catalogs.</summary>
    public string? MapId { get; }

    public static UndoStep Catalogs { get; } = new(UndoTarget.Catalogs, null);

    public static UndoStep OnMap(string mapId) => new(UndoTarget.Map, mapId);
}
