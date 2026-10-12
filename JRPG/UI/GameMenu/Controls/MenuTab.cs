using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace JRPG.UI.GameMenu.Controls
{
    /// <summary>
    /// A selectable menu tab with an icon. Tabs sharing a GroupName work like radio buttons:
    /// only one stays selected. Its look comes from NavTabStyle or CategoryTabStyle (MenuStyles.xaml).
    /// </summary>
    public class MenuTab : RadioButton
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register(nameof(Icon), typeof(ImageSource), typeof(MenuTab), new PropertyMetadata(null));

        public static readonly DependencyProperty IsCompactProperty =
            DependencyProperty.Register(nameof(IsCompact), typeof(bool), typeof(MenuTab), new PropertyMetadata(false));

        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>Icon only, no label. Nav tabs shrink like this while a page is open.</summary>
        public bool IsCompact
        {
            get => (bool)GetValue(IsCompactProperty);
            set => SetValue(IsCompactProperty, value);
        }
    }
}
