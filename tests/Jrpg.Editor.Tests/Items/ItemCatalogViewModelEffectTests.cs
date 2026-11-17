using Jrpg.Core.Effects;
using Jrpg.Core.Stats;
using Jrpg.Editor.Items;

namespace Jrpg.Editor.Tests.Items;

// A consumable's effect through the Items list's problem line (decision 881): Data's rules re-checked as a box is
// typed; and the strip note (decision 442): the chosen item's own strip when the effects folder lacks it.
public sealed class ItemCatalogViewModelEffectTests
{
    private readonly ItemCatalogViewModel _catalog = new(ItemFixtures.Catalog, [], ["Blast.png", "Spark.png"]);

    [Fact]
    public void Problem_DamageWithoutAType_IsDatasMessage()
    {
        _catalog.Choose("tonic");

        _catalog.SelectedItem!.Effect!.Kind = EffectKind.Damage;

        Assert.Equal("items.json: item 'tonic': effect.damageType is required for a damage effect.", _catalog.Problem);
    }

    [Fact]
    public void Problem_HealTurnedDamageOnAnEnemyInBattle_IsNull()
    {
        _catalog.Choose("tonic");
        var tonic = _catalog.SelectedItem!;
        tonic.IsUsableFromMenu = false;

        tonic.Effect!.Kind = EffectKind.Damage;
        tonic.Effect.DamageType = DamageType.Fire;
        tonic.Effect.Targeting = Targeting.SingleEnemy;

        Assert.Null(_catalog.Problem);
    }

    [Fact]
    public void Problem_HealTurnedRestOnOneAlly_IsDatasRestRule()
    {
        _catalog.Choose("tonic");

        _catalog.SelectedItem!.Effect!.Kind = EffectKind.Rest;

        Assert.Equal(
            "items.json: item 'tonic': effect.kind rest needs isUsableFromMenu true and effect.targeting allAllies; "
            + "a rest is used from the menu on the whole party.",
            _catalog.Problem);
    }

    [Fact]
    public void Problem_HealTurnedRestOnAllAllies_IsNull()
    {
        _catalog.Choose("tonic");
        var effect = _catalog.SelectedItem!.Effect!;

        effect.Kind = EffectKind.Rest;
        effect.Targeting = Targeting.AllAllies;

        Assert.Null(_catalog.Problem);
    }

    [Fact]
    public void Problem_BombTurnedHeal_IsDatasHitsRule()
    {
        _catalog.Choose("firebomb");
        var effect = _catalog.SelectedItem!.Effect!;

        effect.Kind = EffectKind.RestoreHp;
        effect.Targeting = Targeting.AllAllies;

        Assert.Equal(
            "items.json: item 'firebomb': effect.hits above 1 is only for a damage effect (was 2 on RestoreHp).",
            _catalog.Problem);
    }

    [Fact]
    public void MissingStripNote_StripNotInTheFolder_NamesIt()
    {
        _catalog.Choose("firebomb");

        _catalog.SelectedItem!.EffectFile = "Flame.png";

        Assert.Equal("Flame.png isn't in art/effects: the game will refuse to start until it is.", _catalog.MissingStripNote);
    }

    [Fact]
    public void MissingStripNote_StripFixed_IsCleared()
    {
        _catalog.Choose("firebomb");
        _catalog.SelectedItem!.EffectFile = "Flame.png";
        Assert.NotEmpty(_catalog.MissingStripNote);

        _catalog.SelectedItem.EffectFile = "Spark.png";

        Assert.Empty(_catalog.MissingStripNote);
    }

    // No strip plays its user's attack strip, which the game's start checks on the user.
    [Fact]
    public void MissingStripNote_StripEmptied_IsEmpty()
    {
        _catalog.Choose("firebomb");
        _catalog.SelectedItem!.EffectFile = "Flame.png";

        _catalog.SelectedItem.EffectFile = " ";

        Assert.Empty(_catalog.MissingStripNote);
    }

    [Fact]
    public void MissingStripNote_StripInAnotherCase_IsEmpty()
    {
        _catalog.Choose("firebomb");

        _catalog.SelectedItem!.EffectFile = "blast.PNG";

        Assert.Empty(_catalog.MissingStripNote);
    }

    [Fact]
    public void MissingStripNote_OtherItemChosen_IsRecomputed()
    {
        _catalog.Choose("firebomb");
        _catalog.SelectedItem!.EffectFile = "Flame.png";
        Assert.NotEmpty(_catalog.MissingStripNote);

        _catalog.Choose("tonic");

        Assert.Empty(_catalog.MissingStripNote);
    }

    [Fact]
    public void MissingStripNote_StripTyped_IsRaised()
    {
        _catalog.Choose("firebomb");
        var raised = new List<string?>();
        _catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.SelectedItem!.EffectFile = "Flame.png";

        Assert.Contains(nameof(ItemCatalogViewModel.MissingStripNote), raised);
    }

    [Fact]
    public void MissingStripNote_OtherItemChosen_IsRaised()
    {
        var raised = new List<string?>();
        _catalog.PropertyChanged += (_, e) => raised.Add(e.PropertyName);

        _catalog.Choose("firebomb");

        Assert.Contains(nameof(ItemCatalogViewModel.MissingStripNote), raised);
    }
}
