namespace Jrpg.Editor.Sprites;

/// <summary>What a facing reads in the Character sprites window's setup (ADR 0054, decision 761).</summary>
internal enum FacingSource
{
    /// <summary>Its row, the walk's frames from column 0.</summary>
    Row,

    /// <summary>A sequence of cells picked on the sheet, overriding its row.</summary>
    Cells,

    /// <summary>Another facing's frames, flipped left to right.</summary>
    Mirror,
}
