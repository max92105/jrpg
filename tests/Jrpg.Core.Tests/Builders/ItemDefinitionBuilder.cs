using Jrpg.Core.Items;

namespace Jrpg.Core.Tests.Builders;

internal sealed class ItemDefinitionBuilder
{
    private string _id = "potion";
    private int _maxStack = 99;

    public ItemDefinitionBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

    public ItemDefinitionBuilder WithMaxStack(int maxStack)
    {
        _maxStack = maxStack;
        return this;
    }

    public ItemDefinition Build() => new()
    {
        Id = _id,
        Name = "Test Item",
        Description = "An item for tests.",
        IconFile = "icon.png",
        MaxStack = _maxStack,
        IsUsableFromMenu = false,
    };
}
