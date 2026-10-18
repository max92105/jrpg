namespace Jrpg.Editor.Maps.Editing;

/// <summary>One undoable change to a draft: a brush stroke, a fill, a placed object. Reverting undoes exactly what applying did.</summary>
internal interface IMapEdit
{
    void Apply(MapDraft draft);

    void Revert(MapDraft draft);
}
