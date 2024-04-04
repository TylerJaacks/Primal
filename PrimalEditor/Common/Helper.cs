using System;
using System.IO;
using System.Security.Cryptography;
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

    public static object SanitizeFileName(string name)
    {
        var path = new StringBuilder(name.Substring(0, name.LastIndexOf(Path.DirectorySeparatorChar) + 1));
        var file = new StringBuilder(name[(name.LastIndexOf(Path.DirectorySeparatorChar) + 1)..]);

        foreach (var c in Path.GetInvalidPathChars())
        {
            path.Replace(c, '_');
        }

        foreach (var c in Path.GetInvalidFileNameChars())
        {
            file.Replace(c, '_');
        }

        return path.Append(file).ToString();
    }

    internal static byte[] ComputeHash(byte[] data, int offset = 0, int count = 0)
    {
        if (data?.Length > 0)
        {
            using var sha256 = SHA256.Create();

            return sha256.ComputeHash(data, offset, count > 0 ? count : data.Length);
        }

        return null;
    }
}