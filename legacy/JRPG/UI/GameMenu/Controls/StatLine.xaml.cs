using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>One stat with an optional preview of a new value (cyan if it goes up, grey if down).</summary>
    public partial class StatLine : UserControl
    {
        private int currentValue;

        public StatLine()
        {
            InitializeComponent();
        }

        public string Label
        {
            get => LabelText.Text;
            set => LabelText.Text = value;
        }

        public ImageSource Icon
        {
            get => IconImage.Source;
            set => IconImage.Source = value;
        }

        public void Show(int value)
        {
            currentValue = value;
            ValueText.Text = value.ToString();
            ClearPreview();
        }

        public void ShowPreview(int newValue)
        {
            string brushKey = newValue > currentValue ? "Brush.Positive"
                            : newValue < currentValue ? "Brush.Negative"
                            : "Brush.Text.Secondary";

            PreviewText.Text = newValue.ToString();
            PreviewText.Foreground = (Brush)FindResource(brushKey);
            ArrowText.Foreground = PreviewText.Foreground;

            ArrowText.Visibility = Visibility.Visible;
            PreviewText.Visibility = Visibility.Visible;
        }

        public void ClearPreview()
        {
            ArrowText.Visibility = Visibility.Hidden;
            PreviewText.Visibility = Visibility.Hidden;
        }
    }
}
