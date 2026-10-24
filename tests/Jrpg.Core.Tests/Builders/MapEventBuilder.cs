using Jrpg.Core.Events;

namespace Jrpg.Core.Tests.Builders;

internal sealed class MapEventBuilder
{
    private string _id = "chest_0";
    private EventActivation _activation = EventActivation.Interact;
    private EventStep[] _steps = [];
    private bool _isOnce;
    private string? _onlyWhenObjectId;
    private string? _onlyWhenLeaderId;

    public MapEventBuilder WithId(string id)
    {
        _id = id;
        return this;
    }

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

    public MapEventBuilder AsTouch()
    {
        _activation = EventActivation.Touch;
        return this;
    }

    public MapEventBuilder AsArrive()
    {
        _activation = EventActivation.Arrive;
        return this;
    }

    public MapEventBuilder OnlyWhenLeaderIs(string memberId)
    {
        _onlyWhenLeaderId = memberId;
        return this;
    }

    public MapEventBuilder OnlyWhenObjectIs(string objectId)
    {
        _onlyWhenObjectId = objectId;
        return this;
    }

    public MapEvent Build() => new()
    {
        Id = _id,
        Activation = _activation,
        Prompt = _activation == EventActivation.Interact ? "Open" : null,
        IsOnce = _isOnce,
        OnlyWhenObjectId = _onlyWhenObjectId,
        OnlyWhenLeaderId = _onlyWhenLeaderId,
        Steps = _steps,
    };
}
