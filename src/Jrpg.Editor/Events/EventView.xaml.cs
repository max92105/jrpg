using System.Windows.Controls;

namespace Jrpg.Editor.Events;

/// <summary>
/// Shows the <see cref="EventViewModel"/> it is given, for the object and the cell event dialogs alike: its conditions,
/// its steps and the chosen step's fields. The rows only an object's event has follow
/// <see cref="EventViewModel.IsObjectEvent"/>.
/// </summary>
internal sealed partial class EventView : UserControl
{
    public EventView() => InitializeComponent();
}
