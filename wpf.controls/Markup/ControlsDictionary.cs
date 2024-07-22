using System.Windows.Markup;
using System.Windows;

namespace wpf.controls.Markup;

[Localizability(LocalizationCategory.Ignore)]
[Ambient]
[UsableDuringInitialization(true)]
public class ControlsDictionary : ResourceDictionary
{
    private const string DictionaryUri = "pack://application:,,,/wpf.controls;component/Resources/wpf.controls.xaml";

    public ControlsDictionary()
    {
        Source = new Uri(DictionaryUri, UriKind.Absolute);
    }
}
