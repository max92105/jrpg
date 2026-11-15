using System.Windows.Media.Imaging;
using Jrpg.Editor.GifImport;
using Jrpg.Editor.Tiles;

namespace Jrpg.Editor.Battle;

/// <summary>
/// What the Enemies window's Art tab copies pictures in with, reads its battle sprite from and plays its loop on.
/// </summary>
/// <param name="Import">
/// Copies chosen images into an art folder, by folder name, a GIF for the battlers as a strip by the options, and says
/// what it did (the shell's import).
/// </param>
/// <param name="BattlerImageOf">A battle sprite read whole by file name; null when the folder lacks it.</param>
/// <param name="AnimationTime">The editor's animation clock, which the battle sprite's loop plays on.</param>
internal sealed record EnemyArtSources(
    Func<string, IReadOnlyList<string>, GifImportOptions, SheetImportResult> Import,
    Func<string, BitmapSource?> BattlerImageOf,
    TileAnimationTime AnimationTime);
