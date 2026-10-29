using Jrpg.Game.Ui;

namespace Jrpg.Game.Crafting;

/// <summary>One material and how many the party owns, measured for its row in the Forge's materials panel.</summary>
/// <param name="Name">The material's name.</param>
/// <param name="Count">How many are owned; 0 is listed too (decision 370: the costs name every material).</param>
internal sealed record MaterialCount(string Name, MeasuredText Count);
