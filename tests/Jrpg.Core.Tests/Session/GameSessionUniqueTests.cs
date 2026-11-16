using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Session;
using Jrpg.Core.Stats;
using Jrpg.Core.Tests.Builders;
using Jrpg.Core.Tests.Randomness;

namespace Jrpg.Core.Tests.Session;

// A unique saves as any piece, and its record is the truth on load (decision 868): the saved pieces below hold a Rare
// with Power 2, where the record gives Unique with Power 6 then crit 3.
public sealed class GameSessionUniqueTests
{
    private static readonly ItemDefinition _band =
        new ItemDefinitionBuilder().WithId("wolf_band").AsWorn(EquipmentKind.Ring).Build() with
        {
            IsUnique = true,
            FixedAffixes = [new RolledAffix("power", StatKind.Power, 6), new RolledAffix("crit_chance", StatKind.CritChance, 3)],
        };

    private static readonly SavedGear _savedApart = new("wolf_band", Rarity.Rare, [new SavedAffix("power", 2)]);

    private static readonly ContentCatalog _catalog = new ContentCatalogBuilder()
        .WithMembers(new PartyMemberBuilder().WithId("sarxian").BuildDefinition())
        .WithItems(_band)
        .WithAffixes(Affix("power", StatKind.Power), Affix("crit_chance", StatKind.CritChance))
        .WithLeader("sarxian")
        .Build();

    [Fact]
    public void Restore_SavedUniquePiece_IsRebuiltFromItsRecord()
    {
        var save = StartNew().Capture(DateTimeOffset.UnixEpoch) with { Gear = [_savedApart] };

        var restored = GameSession.Restore(save, _catalog, new FixedSequenceRandom());

        Assert.Equal([GearPiece.Unique(_band)], restored.Inventory.Gear);
    }

    [Fact]
    public void Save_ThenRestore_UniquePieceComesBackEqual()
    {
        var session = StartNew();
        session.Inventory.Add(_band, 2);
        Assert.True(session.Equipment.TryEquip(session.Party.Leader, GearPiece.Unique(_band), EquipmentSlot.FirstRing));

        var restored = GameSession.Restore(session.Capture(DateTimeOffset.UnixEpoch), _catalog, new FixedSequenceRandom());

        Assert.Equal(GearPiece.Unique(_band), restored.Party.Leader.EquippedIn(EquipmentSlot.FirstRing));
        Assert.Equal([GearPiece.Unique(_band)], restored.Inventory.Gear);
    }

    // Worn in the second ring slot, so a restore wearing it wherever it fits would put it in the first.
    [Fact]
    public void Restore_SavedUniqueWorn_KeepsItsSlot()
    {
        var save = StartNew().Capture(DateTimeOffset.UnixEpoch);
        var worn = new Dictionary<EquipmentSlot, SavedGear> { [EquipmentSlot.SecondRing] = _savedApart };
        save = save with { Members = [save.Members[0] with { Worn = worn }] };

        var leader = GameSession.Restore(save, _catalog, new FixedSequenceRandom()).Party.Leader;

        Assert.Equal(GearPiece.Unique(_band), leader.EquippedIn(EquipmentSlot.SecondRing));
        Assert.Null(leader.EquippedIn(EquipmentSlot.FirstRing));
    }

    private static GameSession StartNew() => GameSession.StartNew(_catalog, "sarxian", new FixedSequenceRandom());

    private static AffixDefinition Affix(string id, StatKind stat) =>
        new() { Id = id, Stat = stat, Min = 1, Max = 5, Kinds = [EquipmentKind.Ring], Weight = 10, GoldValue = 20 };
}
