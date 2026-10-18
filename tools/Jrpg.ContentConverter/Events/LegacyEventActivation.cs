namespace Jrpg.ContentConverter.Events;

/// <summary>The legacy editor's event activations, kept apart from Core's so a rename there can't break reading old maps.</summary>
internal enum LegacyEventActivation
{
    Interact,
    Touch,
}
