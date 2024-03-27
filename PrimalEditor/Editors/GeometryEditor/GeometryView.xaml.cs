using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media.Media3D;

namespace PrimalEditor.Editors;

/// <summary>
/// Interaction logic for GeometryView.xaml
/// </summary>
public partial class GeometryView : UserControl
{
    public void SetGeometry(int index = -1)
    {
        if (DataContext is not MeshRenderer vm) return;

        if (vm.Meshes.Any() && viewport.Children.Count == 2)
        {
            viewport.Children.RemoveAt(1);
        }

        var meshIndex = 0;

        var modelGroup = new Model3DGroup();

        foreach (var mesh in vm.Meshes)
        {
            if (index != -1 && meshIndex != index)
            {
                ++meshIndex;

                continue;
            }

            var mesh3D = new MeshGeometry3D
            {
                Positions = mesh.Positions,
                Normals = mesh.Normals,
                TriangleIndices = mesh.Indices,
                TextureCoordinates = mesh.UVs
            };

            var diffuse = new DiffuseMaterial(mesh.Diffuse);
            var specular = new SpecularMaterial(mesh.Specular, 50);

            var matGroup = new MaterialGroup();

            matGroup.Children.Add(diffuse);
            matGroup.Children.Add(specular);

            var model = new GeometryModel3D(mesh3D, matGroup);

            modelGroup.Children.Add(model);

            var binding = new Binding(nameof(mesh.Diffuse)) { Source = mesh };

            BindingOperations.SetBinding(diffuse, DiffuseMaterial.BrushProperty, binding);

            if (meshIndex == index) break;
        }

        var visual = new ModelVisual3D() { Content = modelGroup };

        viewport.Children.Add(visual);
    }

    public GeometryView()
    {
        InitializeComponent();

        DataContextChanged += (s, e) => SetGeometry();
    }

    private Point _clickedPosition;
    private bool _capturedLeft;
    private bool _capturedRight;

    private void OnGridMouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _clickedPosition = e.GetPosition(this);
        _capturedLeft = true;

        Mouse.Capture(sender as UIElement);
    }

    private void OnGridMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        _capturedLeft = false;

        if (!_capturedRight) Mouse.Capture(null);
    }

    private void OnGridMouseMove(object sender, System.Windows.Input.MouseEventArgs e)
    {
        if (!_capturedLeft && !_capturedRight) return;

        var pos = e.GetPosition(this);
        var d = pos - _clickedPosition;

        if (_capturedLeft && !_capturedRight)
        {
            MoveCamera(d.X, d.Y, 0);
        }
        else if (!_capturedLeft && _capturedRight)
        {
            var vm = DataContext as MeshRenderer;
            var cameraPos = vm.CameraPosition;
            var yOffset = d.Y * 0.001 * Math.Sqrt(cameraPos.X * cameraPos.X + cameraPos.Z * cameraPos.Z);

            vm.CameraTarget = new(vm.CameraTarget.X, vm.CameraTarget.Y + yOffset, vm.CameraTarget.Z);
        }
    }

    private void OnGridMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
    {

    }

    private void OnGridMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {

    }

    private void OnGridMouseRightButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {

    }
}
