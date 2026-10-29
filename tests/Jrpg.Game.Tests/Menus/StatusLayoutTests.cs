using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class StatusLayoutTests
{
    [Fact]
    public void Resources_EveryClassInContent_FitBesideSkills()
    {
        var mostResources = StartMapScenarios.BelowClosedDoor.Classes.Values.Max(characterClass => characterClass.Resources.Count);

        Assert.True(
            mostResources <= StatusResourcesView.MostResources,
            $"A class has {mostResources} resources; the Status page's Resources section has room for {StatusResourcesView.MostResources}.");
    }

    [Fact]
    public void ResourcesHeight_MostResources_FitsTheSkillsRow()
    {
        Assert.True(StatusResourcesView.Height <= StatusSheetView.SkillsHeight, $"Resources need {StatusResourcesView.Height} px of {StatusSheetView.SkillsHeight}.");
    }

    // The narrower Skills column wraps longer descriptions to two lines (decision 373): every class's skills, with
    // their descriptions as content words them, must all show whole.
    [Fact]
    public void Skills_EveryClassInContent_AllShowBesideResources()
    {
        var catalog = StartMapScenarios.BelowClosedDoor;
        var section = StatusSheetView.SkillsSectionIn(MenuLayout.SingleColumn(MenuLayout.PageArea(TabFold.RailWidth)));
        var font = RepositoryScreenText.Text.Fonts.FontAt(StatusSkillsView.DescriptionPixels);
        int HeightOf(string skillId) =>
            StatusSkillsView.DescriptionLayout(font, section.Width - StatusSkillsView.Indent, catalog.Skills[skillId].Description).Size.Y;

        var hidden = catalog.Classes.Values
            .Where(characterClass => StatusSkillsView.HairlineTopsOf([.. characterClass.SkillIds.Select(HeightOf)]).Any(top => top > section.Height))
            .Select(characterClass => characterClass.Id);

        Assert.Empty(hidden);
    }

    // The page keeps room for three two-line skills: their last hairline is at 266, so 267 down to under it.
    [Fact]
    public void RoomForThreeSkills_ThreeTwoLineSkills_EndsUnderTheLastHairline()
    {
        Assert.Equal(StatusSkillsView.RoomForThreeSkills, StatusSkillsView.HairlineTopsOf([36, 36, 36])[^1] + 1);
    }

    // One-line descriptions (18 px) stack 60 px apart from 92; two-line ones (36 px) 78 apart from 110.
    [Theory]
    [InlineData(18, new[] { 92, 152, 212 })]
    [InlineData(36, new[] { 110, 188, 266 })]
    public void HairlineTopsOf_ThreeSkills_StackUnderTheBand(int descriptionHeight, int[] expected)
    {
        Assert.Equal(expected, StatusSkillsView.HairlineTopsOf([descriptionHeight, descriptionHeight, descriptionHeight]));
    }
}
