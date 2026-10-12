using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>
    /// One line of a menu list. Set what you need, leave the rest empty:
    ///   new ListRow { Icon = ..., Text = "Potion", Value = "5", IsClickable = true }
    /// </summary>
    public partial class ListRow : UserControl
    {
        private const double DimmedIconOpacity = 0.45;
        private static readonly GridLength DetailWidth = new GridLength(1.2, GridUnitType.Star);

        private bool isDimmed;
        private bool isClickable;

        public ListRow()
        {
            InitializeComponent();
        }

        public ImageSource Icon
        {
            get => IconImage.Source;
            set => IconImage.Source = value;
        }

        public string Text
        {
            get => MainText.Text;
            set => MainText.Text = value;
        }

        /// <summary>Small boxed tag before the text, e.g. "E" for equipped. Null hides it.</summary>
        public string Badge
        {
            get => BadgeText.Text;
            set
            {
                BadgeText.Text = value;
                BadgeBorder.Visibility = string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
            }
        }

        /// <summary>Second column, e.g. the item equipped in a slot. Null hides the column.</summary>
        public string Detail
        {
            get => DetailText.Text;
            set
            {
                DetailText.Text = value;
                DetailColumn.Width = string.IsNullOrEmpty(value) ? new GridLength(0) : DetailWidth;
            }
        }

        public ImageSource DetailIcon
        {
            get => DetailIconImage.Source;
            set => DetailIconImage.Source = value;
        }

        /// <summary>Right-aligned number or short text (quantity, MP cost).</summary>
        public string Value
        {
            get => ValueText.Text;
            set => ValueText.Text = value;
        }

        /// <summary>Keeps the row lit, e.g. the slot being changed.</summary>
        public bool IsSelected
        {
            get => SelectedHighlight.Visibility == Visibility.Visible;
            set => SelectedHighlight.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>Greys the row out, for things that can't be used right now.</summary>
        public bool IsDimmed
        {
            get => isDimmed;
            set
            {
                isDimmed = value;

                var brush = (Brush)FindResource(value ? "Brush.Text.Disabled" : "Brush.Text.Primary");
                MainText.Foreground = brush;
                DetailText.Foreground = brush;
                ValueText.Foreground = brush;
                IconImage.Opacity = value ? DimmedIconOpacity : 1.0;
            }
        }

        /// <summary>Shows the hand cursor. Clicks are handled by whoever creates the row.</summary>
        public bool IsClickable
        {
            get => isClickable;
            set
            {
                isClickable = value;
                Cursor = value ? Cursors.Hand : null;
            }
        }
    }
}
