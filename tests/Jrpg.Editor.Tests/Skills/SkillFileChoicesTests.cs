using Jrpg.Editor.Skills;

namespace Jrpg.Editor.Tests.Skills;

// What a file box of the Skills page offers: the empty entry, the folder's files, then the named files it lacks, so a
// skill naming a missing file shows that name rather than the previous skill's.
public sealed class SkillFileChoicesTests
{
    [Fact]
    public void Of_FolderFiles_ListsThemAfterTheEmptyEntry()
    {
        var choices = SkillFileChoices.Of(["Attack.png", "Skill.png"], []);

        Assert.Equal(["", "Attack.png", "Skill.png"], choices);
    }

    [Fact]
    public void Of_NamedFileInTheFolder_ListsItOnce()
    {
        var choices = SkillFileChoices.Of(["Attack.png", "Skill.png"], ["Skill.png"]);

        Assert.Equal(["", "Attack.png", "Skill.png"], choices);
    }

    // Named in the opposite order and twice, so a missing sort or distinct fails.
    [Fact]
    public void Of_NamedFilesTheFolderLacks_AddsEachOnceInOrdinalOrderAfterTheFolder()
    {
        var choices = SkillFileChoices.Of(["Skill.png"], ["Zap.png", "Bolt.png", "Zap.png"]);

        Assert.Equal(["", "Skill.png", "Bolt.png", "Zap.png"], choices);
    }

    // Ordinal, as the Effect strips page compares: a name differing only in case is listed beside the folder's file.
    [Fact]
    public void Of_NamedFileDifferingInCase_AddsIt()
    {
        var choices = SkillFileChoices.Of(["Skill.png"], ["skill.png"]);

        Assert.Equal(["", "Skill.png", "skill.png"], choices);
    }

    [Fact]
    public void Of_EmptyName_AddsNoSecondEmptyEntry()
    {
        var choices = SkillFileChoices.Of(["Skill.png"], ["", ""]);

        Assert.Equal(["", "Skill.png"], choices);
    }
}
