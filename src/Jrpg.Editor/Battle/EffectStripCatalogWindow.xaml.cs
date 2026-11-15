using System.Windows;

namespace Jrpg.Editor.Battle;

/// <summary>
/// The Effect strips window (decision 659, ADR 0045): OK keeps the session's records when nothing would be refused,
/// Cancel drops them. The problem line shows live and greys OK while it shows.
/// </summary>
internal sealed partial class EffectStripCatalogWindow : Window
{
    public EffectStripCatalogWindow(EffectStripCatalogViewModel strips)
    {
        InitializeComponent();
        DataContext = strips;
    }

    private void OnOkClicked(object sender, RoutedEventArgs e) => DialogResult = true;
}
