using System.Windows;
using System.Windows.Controls;

namespace Garbage_Collector.Utilities
{

    public class Btn : RadioButton
    {
        static Btn()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Btn), new FrameworkPropertyMetadata(typeof(Btn)));
        }
    }

}
