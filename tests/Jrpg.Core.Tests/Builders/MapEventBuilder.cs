using Jrpg.Core.Events;

namespace Jrpg.Core.Tests.Builders;

internal sealed class MapEventBuilder
{
    private EventStep[] _steps = [];
    private bool _isOnce;
    private string? _onlyWhenObjectId;

    public MapEventBuilder WithSteps(params EventStep[] steps)
    {
        _steps = steps;
        return this;
    }

    public MapEventBuilder AsOnce()
    {
        _isOnce = true;
        return this;
    }

    public MapEventBuilder OnlyWhenObjectIs(string objectId)
    {
        _onlyWhenObjectId = objectId;
        return this;
    }

    public MapEvent Build() => new()
    {
        Id = "chest_0",
        Activation = EventActivation.Interact,
        Prompt = "Open",
        IsOnce = _isOnce,
        OnlyWhenObjectId = _onlyWhenObjectId,
        Steps = _steps,
    };
}
