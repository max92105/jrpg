using Jrpg.Core.Maps;

namespace Jrpg.Core.Tests.Builders;

internal sealed class MapLayoutBuilder
{
    private readonly List<TileLayer> _layers = [];

    /// <summary>Adds a layer above the previous ones (Z 0, 1, 2 and so on); each string is one row of single-digit tile ids.</summary>
    public MapLayoutBuilder WithLayer(params string[] rows)
    {
        _layers.Add(new TileLayer
        {
            Z = _layers.Count,
            Rows = [.. rows.Select(row => (IReadOnlyList<int>)[.. row.Select(digit => digit - '0')])],
        });
        return this;
    }

    public MapLayout Build() => new()
    {
        Id = "test_map",
        Width = _layers[0].Rows[0].Count,
        Height = _layers[0].Rows.Count,
        Layers = [.. _layers],
        Objects = [],
    };
}
