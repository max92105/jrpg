using System.Windows.Controls;

namespace Jrpg.Editor.Effects;

/// <summary>
/// The Effects page (<c>effects.json</c>, ADR 0045, decisions 976 and 983): the tiered effects left, the chosen one's
/// boxes, its amount at the shown Power and the skills naming it right. Its host shows the problem line and keeps or
/// drops the whole session.
/// </summary>
internal sealed partial class TieredEffectCatalogPage : UserControl
{
    public TieredEffectCatalogPage(TieredEffectCatalogViewModel effects)
    {
        InitializeComponent();
        DataContext = effects;
    }
}
