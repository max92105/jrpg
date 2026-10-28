using Jrpg.Core.Items;
using Jrpg.Core.Party;
using Jrpg.Game.Menus;
using Jrpg.Game.Tests.Exploration;
using Jrpg.Game.Tests.Ui;
using Jrpg.Game.Ui;

namespace Jrpg.Game.Tests.Menus;

[Collection(RepositoryScreenText.CollectionName)]
public sealed class EquipmentChoiceViewTests
{
    private static readonly ItemDefinition _sword = StartMapScenarios.BelowClosedDoor.Items["bronze_sword"];

    // Each owned row's name takes its rarity's color when the choice opens (decision 362); a plain one the body text's.
    [Fact]
    public void Show_RarePiece_RowIsRare()
    {
        var view = new EquipmentChoiceView(RepositoryScreenText.Text, new UiTextures());
        var choice = new EquipmentChoice([GearPiece.Plain(_sword), GearPiece.Rolled(_sword, Rarity.Rare, [])], []);

        view.Show(choice, "Weapon");

        Assert.Equal([UiTheme.TextPrimary, UiTheme.RarityRare], view.OwnedColors);
    }


    // Another member's piece stays dimmed as a row that takes from them, unless it rolled.
    [Fact]
    public void Show_RareTakeablePiece_TakesItsColorPlainStaysDimmed()
    {
        var view = new EquipmentChoiceView(RepositoryScreenText.Text, new UiTextures());
        var wearer = StartMapScenarios.StartWithWholeParty(StartMapScenarios.BelowClosedDoor).Party.MemberById("zanyr");
        var choice = new EquipmentChoice([], [new WornItem(wearer, GearPiece.Plain(_sword)), new WornItem(wearer, GearPiece.Rolled(_sword, Rarity.Rare, []))]);

        view.Show(choice, "Weapon");

        Assert.Equal([UiTheme.TextSecondary, UiTheme.RarityRare], view.WornColors);
    }
}
