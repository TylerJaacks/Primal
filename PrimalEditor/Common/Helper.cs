using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace PrimalEditor.Common;

public static class VisualExtensions
{
    public static T FindVisualParent<T>(this DependencyObject dependencyObject) where T : DependencyObject
    {
        if (dependencyObject is not Visual) return null;

        var parent = VisualTreeHelper.GetParent(dependencyObject);

        while (parent != null)
        {
            if (parent is T type)
            {
                return (T)type;
            }

            parent = VisualTreeHelper.GetParent(parent);
        }

        return null;
    }
}

public static class ContentHelper
{
    public static string GetRandomString(int length = 8)
    {
        if (length <= 0) length = 8;

        var n = length / 11;
        var sb = new StringBuilder();

        for (int i = 0; i < n; i++)
        {
            sb.Append(Path.GetRandomFileName().Replace(".", ""));
        }

        return sb.ToString(0, length);
    }
}