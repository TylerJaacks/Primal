using System.Windows;

namespace PrimalEditor.Utilities.Controls;

public class ScalerBox : NumberBox
{
    static ScalerBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ScalerBox), 
            new FrameworkPropertyMetadata(typeof(ScalerBox)));
    }
}