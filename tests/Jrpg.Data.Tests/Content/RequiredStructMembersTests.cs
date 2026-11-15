using System.Text.Json;
using Jrpg.Core.Sprites;
using Jrpg.Data.Content;

namespace Jrpg.Data.Tests.Content;

// A struct's members are required through the contract modifier (decision 768): without it, a cell missing its row
// would read as row 0.
public sealed class RequiredStructMembersTests
{
    [Fact]
    public void Read_CellWithoutRow_IsRefusedNamingIt()
    {
        var exception = Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<FacingSequence>("""{ "cells": [ { "column": 2 } ] }""", ContentJsonOptions.Strict));

        Assert.Contains("'row'", exception.Message, StringComparison.Ordinal);
    }
}
