﻿using System;
using System.Windows;
using System.Windows.Controls;

using PrimalEditor.ContentToolsAPIStructs;
using PrimalEditor.DLLWrappers;
using PrimalEditor.Editors;
using PrimalEditor.Utilities.Controls;

namespace PrimalEditor.Content
{
    /// <summary>
    /// Interaction logic for PrimitiveMeshDialog.xaml
    /// </summary>
    public partial class PrimitiveMeshDialog : Window
    {
        public PrimitiveMeshDialog()
        {
            InitializeComponent();

            Loaded += (s, e) => UpdatePrimitive();
        }

        private void OnPrimitiveType_SelectionChanged(object sender, SelectionChangedEventArgs e) => UpdatePrimitive();
        private void OnScalarBox_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitive();

        private void OnSlider_ValueChanged(object sender, RoutedEventArgs e) => UpdatePrimitive();

        private float Value(ScalerBox scalarBox, float min)
        {
            float.TryParse(scalarBox.Value, out var result);

            return Math.Max(result, min);
        }

        private void UpdatePrimitive()
        {
            if (!IsInitialized) return;

            var primitiveType = (PrimitiveMeshType) primTypeComboBox.SelectedItem;
            var info = new PrimitiveInitInfo() { Type = primitiveType };

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
                    break;
                case PrimitiveMeshType.UvSphere:
                    break;
                case PrimitiveMeshType.IcoSphere:
                    break;
                case PrimitiveMeshType.Cylinder:
                    break;
                case PrimitiveMeshType.Capsule:
                    break;
                default:
                    break;
            }

            var geometry = new Geometry();

            ContentToolsAPI.CreatePrimitiveMesh(geometry, info);

            (DataContext as GeometryEditor).SetAsset(geometry);
        }
    }
}
