using System.Windows.Controls;

namespace JRPG
{
    /// <summary>
    /// Interaction logic for DialogBox.xaml
    /// </summary>
    public partial class DialogueBox : UserControl
    {
        public DialogueBox()
        {
            InitializeComponent();
        }

        public TextBlock TextElement => DialogueText;
        public Border Container => DialogueContainer;
        public Image Portrait => PortraitImage;
    }
}
