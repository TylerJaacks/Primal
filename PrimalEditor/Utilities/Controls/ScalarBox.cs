using System.Windows;

namespace PrimalEditor.Utilities.Controls;

public class ScalarBox : NumberBox
{
    static ScalarBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ScalarBox), 
            new FrameworkPropertyMetadata(typeof(ScalarBox)));
    }
}