using Jrpg.Core.Battle;
using Jrpg.Core.Content;
using Jrpg.Core.Party;
using Jrpg.Data.Content;

namespace Jrpg.Data.Art;

/// <summary>
/// The battle art the startup check reads: party members' and enemies' battle sprites, an animated enemy's strip split
/// into its frames (ADR 0047), enemies' portraits, attack effects and their strips.
/// </summary>
public static partial class ArtReferenceCheck
{
    private static void AddBattleArtProblems(
        List<string> problems, ContentCatalog catalog, string artRoot, IEnumerable<PartyMemberDefinition> members)
    {
        var memberBattlers = members.Select(member => (member.BattleSpriteFile, $"party member '{member.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Battlers, memberBattlers);
        var enemies = catalog.Enemies.Values.OrderBy(enemy => enemy.Id, StringComparer.Ordinal);
        var enemyBattlers = enemies.Select(enemy => (enemy.BattleSpriteFile, $"enemy '{enemy.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Battlers, enemyBattlers);
        AddPartialBattlerStrips(problems, artRoot, enemies);
        var enemyPortraits = enemies.Select(enemy => (enemy.PortraitFile, $"enemy '{enemy.Id}'"));
        AddMissingByExactName(problems, artRoot, ArtFolders.Portraits, enemyPortraits);
        var attackEffects = members.Select(member => (File: member.AttackEffectFile, NamedBy: $"party member '{member.Id}'"))
            .Concat(enemies.Select(enemy => (File: enemy.AttackEffectFile, NamedBy: $"enemy '{enemy.Id}'")))
            .ToList();
        AddMissingByExactName(problems, artRoot, ArtFolders.Effects, attackEffects);
        AddPartialFrameStrips(problems, artRoot, attackEffects.Select(effect => effect.File));
    }

    // An animated enemy's strip is its loop's frames side by side (ADR 0047): its width must split into them evenly.
    private static void AddPartialBattlerStrips(List<string> problems, string artRoot, IEnumerable<EnemyDefinition> enemies)
    {
        foreach (var enemy in enemies)
        {
            var file = enemy.BattleSpriteFile;
            if (enemy.IdleAnimation is not { } idle || ReadSize(artRoot, ArtFolders.Battlers, file).Size is not { } size)
            {
                continue;
            }

            if (!SplitsIntoFrames(size.Width, idle.FrameCount))
            {
                problems.Add(
                    $"{ExactFileName.ShownPath(ArtFolders.Battlers, file)}: a {size.Width}x{size.Height} strip "
                    + $"doesn't split into {idle.FrameCount} frames. Named by enemy '{enemy.Id}'.");
            }
        }
    }

    private static bool SplitsIntoFrames(int width, int frameCount) => width % frameCount == 0;

    // An effect strip is square frames side by side (decision 57), so its width must be a whole number of its heights.
    private static void AddPartialFrameStrips(List<string> problems, string artRoot, IEnumerable<string> files)
    {
        foreach (var file in files.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal))
        {
            if (ReadSize(artRoot, ArtFolders.Effects, file).Size is not { } size)
            {
                continue;
            }

            if (!IsRowOfSquareFrames(size))
            {
                problems.Add(
                    $"{ExactFileName.ShownPath(ArtFolders.Effects, file)}: a {size.Width}x{size.Height} strip "
                    + "isn't a row of whole square frames.");
            }
        }
    }

    /// <summary>Whether an effect strip this size is whole square frames side by side (decision 57).</summary>
    internal static bool IsRowOfSquareFrames((int Width, int Height) size) => size.Height > 0 && size.Width % size.Height == 0;
}
