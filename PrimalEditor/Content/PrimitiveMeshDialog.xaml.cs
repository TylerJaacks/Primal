using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PrimalEditor.ContentToolsAPIStructs;
using PrimalEditor.DLLWrappers;
using PrimalEditor.Editors;
using PrimalEditor.Utilities.Controls;

namespace PrimalEditor.Content;

/// <summary>
/// Interaction logic for PrimitiveMeshDialog.xaml
/// </summary>
public partial class PrimitiveMeshDialog : Window
{
    private static readonly List<ImageBrush> Textures = new();

    public PrimitiveMeshDialog()
    {
        InitializeComponent();

        Loaded += (s, e) => UpdatePrimitive();
    }

    static PrimitiveMeshDialog()
    {
        LoadTextures();
    }

    private void OnPrimitiveType_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitive();
    private void OnScalarBox_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitive();

    private void OnSlider_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitive();

    private static float Value(NumberBox scalarBox, float min)
    {
        float.TryParse(scalarBox.Value, out var result);

        return Math.Max(result, min);
    }

    private void UpdatePrimitive()
    {
        if (!IsInitialized) return;

        var primitiveType = (PrimitiveMeshType) primTypeComboBox.SelectedItem;
        var info = new PrimitiveInitInfo() { Type = primitiveType };
        var smoothingAngle = 0;

        switch (primitiveType)
        {
            case PrimitiveMeshType.Plane:
                {
                    info.SegmentX = (int) xSliderPlane.Value;
                    info.SegmentZ = (int) zSliderPlane.Value;

                    info.Size.X = Value(widthScalarBoxPlane, 0.001f);
                    info.Size.Z = Value(lengthScalarBoxPlane, 0.001f);

                    break;
                }
            case PrimitiveMeshType.Cube:
                return;
            case PrimitiveMeshType.UvSphere:
                {
                    info.SegmentX = (int)xSliderUvSphere.Value;
                    info.SegmentY = (int)ySliderUvSphere.Value;

                    info.Size.X = Value(xScalarBoxUvSphere, 0.001f);
                    info.Size.Z = Value(yScalarBoxUvSphere, 0.001f);
                    info.Size.Z = Value(zScalarBoxUvSphere, 0.001f);

                    smoothingAngle = (int)angleSliderUvSphere.Value;

                    break;
                }
            case PrimitiveMeshType.IcoSphere:
                return;
            case PrimitiveMeshType.Cylinder:
                return;
            case PrimitiveMeshType.Capsule:
                return;
            default:
                break;
        }

        var geometry = new Geometry();

        geometry.ImportSettings.SmoothingAngle = smoothingAngle;

        ContentToolsAPI.CreatePrimitiveMesh(geometry, info);

        (DataContext as GeometryEditor)?.SetAsset(geometry);

        OnTexturedCheckBoxClick(textureCheckBox, null);
    }

    private static void LoadTextures()
    {
        var uris = new List<Uri>
        {
            new("pack://application:,,,/Resources/PrimitiveMeshView/PlaneTexture.png"),
            new("pack://application:,,,/Resources/PrimitiveMeshView/PlaneTexture.png"),
            new("pack://application:,,,/Resources/PrimitiveMeshView/Checkermap.png"),
        };

        Textures.Clear();

        foreach (var resource in uris.Select(Application.GetResourceStream))
        {
            using var reader = new BinaryReader(resource.Stream);

            var data = reader.ReadBytes((int)resource.Stream.Length);

            var imageSource = (BitmapSource)new ImageSourceConverter().ConvertFrom(data);

            imageSource?.Freeze();

            var brush = new ImageBrush(imageSource)
            {
                Transform = new ScaleTransform(1, -1, 0.5, 0.5),
                ViewportUnits = BrushMappingMode.Absolute
            };

            Textures.Add(brush);
        }
    }

    private void OnTexturedCheckBoxClick(object sender, RoutedEventArgs e)
    {
        Brush brush = Brushes.White;

        if (sender is CheckBox { IsChecked: true })
        {
            brush = Textures[(int)primTypeComboBox.SelectedItem];
        }

        var vm = DataContext as GeometryEditor;

        foreach (var mesh in vm?.MeshRenderer.Meshes!)
        {
            mesh.Diffuse = brush;
        }
    }
}
