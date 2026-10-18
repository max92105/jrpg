namespace Jrpg.Data.Art;

/// <summary>
/// The folder each kind of art lives in under the art root (ADR 0009). Until the content cutover (slice 10) the art root
/// is the legacy <c>Assets/</c> folder, so these are its folder names.
/// </summary>
public static class ArtFolders
{
    public const string Tilesheets = "Tiles";

    /// <summary>Map sprites; legacy keeps its one sprite, <c>player.png</c>, at the root.</summary>
    public const string FieldSprites = "";
}
