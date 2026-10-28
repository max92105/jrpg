using Jrpg.Core.Content;
using Jrpg.Core.Items;
using Jrpg.Core.Loot;
using Jrpg.Core.Party;
using Jrpg.Core.Progression;
using Jrpg.Core.Stats;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

public sealed class GameRulesValidatorTests
{
    private static readonly GameRules _rules = new(
        new PartyRules(MaxActiveMembers: 4),
        new ExperienceTable([100]),
        new ExplorationRules { WalkSpeed = 3, PlayerRadius = 0.1, InteractReach = 0.6 },
        new StatRules(
            HpPercentPer100Constitution: 100,
            BaseAccuracy: 90,
            BaseCritChance: 5,
            BaseDodge: 0,
            BaseCrowdControlResistance: 0,
            DamagePercentPer100Power: 100,
            CritDamagePercentPer100Power: 50,
            CritChancePer100Agility: 25,
            DodgePer100Agility: 20,
            PhysicalResistancePer100Constitution: 25,
            MagicalResistancePer100Will: 25,
            StatusPercentPer100Intelligence: 100,
            IntelligencePerExtraStatusTurn: 25,
            UnarmedDamage: 18,
            UnarmedDamageType: DamageType.Blunt),
        new LootRules(new Dictionary<Rarity, RarityRule>
        {
            [Rarity.Common] = new(Weight: 650, MinAffixes: 0, MaxAffixes: 0),
            [Rarity.Magic] = new(Weight: 280, MinAffixes: 1, MaxAffixes: 1),
            [Rarity.Rare] = new(Weight: 70, MinAffixes: 2, MaxAffixes: 3),
        }));

    // The rarity table is checked with the rest of the file, so a broken one stops the start, not the first drop.
    [Fact]
    public void Validate_NegativeRarityWeight_NamesTheField()
    {
        var rarities = new Dictionary<Rarity, RarityRule>(_rules.Loot.Rarities) { [Rarity.Magic] = new(Weight: -1, MinAffixes: 1, MaxAffixes: 1) };
        var rules = _rules with { Loot = new LootRules(rarities) };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: loot.rarities.magic.weight must be at least 0 (was -1).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeHpPercentPerConstitution_NamesTheField()
    {
        var rules = _rules with { Stats = _rules.Stats with { HpPercentPer100Constitution = -1 } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: stats.hpPercentPer100Constitution must be at least 0 (was -1).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeBaseAccuracy_NamesTheField()
    {
        var rules = _rules with { Stats = _rules.Stats with { BaseAccuracy = -5 } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: stats.baseAccuracy must be at least 0 (was -5).", exception.Message);
    }

    [Fact]
    public void Validate_NegativeDodgePerAgility_NamesTheField()
    {
        var rules = _rules with { Stats = _rules.Stats with { DodgePer100Agility = -1 } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: stats.dodgePer100Agility must be at least 0 (was -1).", exception.Message);
    }

    [Fact]
    public void Validate_ZeroIntelligencePerExtraStatusTurn_NamesTheField()
    {
        var rules = _rules with { Stats = _rules.Stats with { IntelligencePerExtraStatusTurn = 0 } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: stats.intelligencePerExtraStatusTurn must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_ZeroUnarmedDamage_NamesTheField()
    {
        var rules = _rules with { Stats = _rules.Stats with { UnarmedDamage = 0 } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: stats.unarmedDamage must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_ZeroMaxActiveMembers_ThrowsNamingTheField()
    {
        var rules = _rules with { Party = new PartyRules(MaxActiveMembers: 0) };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: party.maxActiveMembers must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_MoreThanFiveActiveMembers_NamesTheLimit()
    {
        var rules = _rules with { Party = new PartyRules(MaxActiveMembers: 6) };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: party.maxActiveMembers can be at most 5 (was 6).", exception.Message);
    }

    [Fact]
    public void Validate_FiveActiveMembers_Passes()
    {
        var rules = _rules with { Party = new PartyRules(MaxActiveMembers: 5) };

        var exception = Record.Exception(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Null(exception);
    }

    [Fact]
    public void Validate_EmptyExperienceTable_ThrowsNamingTheField()
    {
        var rules = _rules with { Experience = new ExperienceTable([]) };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: experience.experienceToNextLevel must have at least one entry.", exception.Message);
    }

    [Fact]
    public void Validate_ZeroExperienceEntry_ThrowsNamingTheIndex()
    {
        var rules = _rules with { Experience = new ExperienceTable([100, 0]) };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: experience.experienceToNextLevel[1] must be at least 1 (was 0).", exception.Message);
    }

    [Fact]
    public void Validate_ZeroWalkSpeed_ThrowsNamingTheField()
    {
        var rules = _rules with { Exploration = _rules.Exploration with { WalkSpeed = 0 } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Equal("rules.json: exploration.walkSpeed must be above 0 (was 0).", exception.Message);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void Validate_WalkSpeedNotANumber_ThrowsNamingTheField(double walkSpeed)
    {
        var rules = _rules with { Exploration = _rules.Exploration with { WalkSpeed = walkSpeed } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.StartsWith("rules.json: exploration.walkSpeed must be above 0", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_PlayerRadiusJustUnderHalfACell_DoesNotThrow()
    {
        var rules = _rules with { Exploration = _rules.Exploration with { PlayerRadius = 0.49 } };

        var exception = Record.Exception(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.Null(exception);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(0.5)]
    [InlineData(double.NaN)]
    public void Validate_PlayerRadiusNotInsideOneCell_ThrowsNamingTheField(double radius)
    {
        var rules = _rules with { Exploration = _rules.Exploration with { PlayerRadius = radius } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.StartsWith("rules.json: exploration.playerRadius must be above 0 and below 0.5", exception.Message, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(double.NaN)]
    public void Validate_InteractReachNotPositive_ThrowsNamingTheField(double reach)
    {
        var rules = _rules with { Exploration = _rules.Exploration with { InteractReach = reach } };

        var exception = Assert.Throws<ContentValidationException>(() => GameRulesValidator.Validate("rules.json", rules));

        Assert.StartsWith("rules.json: exploration.interactReach must be above 0", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Validate_ValidRules_DoesNotThrow()
    {
        var exception = Record.Exception(() => GameRulesValidator.Validate("rules.json", _rules));

        Assert.Null(exception);
    }
}
