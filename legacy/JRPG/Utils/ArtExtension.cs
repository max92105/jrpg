using System;
using System.Windows.Markup;
using System.Windows.Media;

namespace JRPG.Utils
{
    /// <summary>
    /// Lets XAML ask for UI art by key:  Source="{utils:Art Menu/Inventory}"
    /// See UiArt for where the image comes from.
    /// </summary>
    [MarkupExtensionReturnType(typeof(ImageSource))]
    public class ArtExtension : MarkupExtension
    {
        public ArtExtension() { }

        public ArtExtension(string key)
        {
            Key = key;
        }

        [ConstructorArgument("key")]
        public string Key { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => UiArt.Get(Key);
    }
}
