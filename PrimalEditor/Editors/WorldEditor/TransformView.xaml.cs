using System;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using PrimalEditor.Components;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;

using static PrimalEditor.Editors.GameEntityView;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// Interaction logic for TransformView.xaml
    /// </summary>
    public partial class TransformView : UserControl
    {
        private Action _undoAction = null;

        private bool _propertyChanged = false;

        public TransformView()
        {
            InitializeComponent();

            Loaded += OnTransformViewLoaded;
        }

        private void OnTransformViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnTransformViewLoaded;

            ((MSTransform) DataContext).PropertyChanged += (s, _) => _propertyChanged = true;
        }

        private Action GetAction(Func<Transform, (Transform transform, Vector3)> selector, Action<(Transform transform, Vector3)> forEachAction)
        {
            _propertyChanged = false;

            if (DataContext is not MSTransform msTransform)
            {
                _undoAction = null;
                _propertyChanged = false;

                return null;
            }

            var selection = msTransform.SelectedComponents.Select(selector).ToList();

            return () =>
            {
                selection.ForEach(forEachAction);

                (Instance.DataContext as MsEntity)?.GetMsComponent<MSTransform>().Refresh();
            };
        }

        private void RecordActions(Action redoAction, string name)
        {
            Debug.Assert(_undoAction != null);

            _propertyChanged = false;

            Project.UndoRedo.Add(new UndoRedoAction(_undoAction, redoAction, name));
        }

        private Action GetPositionAction() => GetAction((x) => (x, x.Position), (x) => x.transform.Position = x.Item2);
        private Action GetRotationAction() => GetAction((x) => (x, x.Rotation), (x) => x.transform.Rotation = x.Item2);
        private Action GetScaleAction() => GetAction((x) => (x, x.Scale), (x) => x.transform.Scale = x.Item2);

        private void PositionVectorBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;

            _undoAction = GetPositionAction();
        }

        private void PositionVectorBox_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_propertyChanged) return;

            RecordActions(GetPositionAction(), "Position Changed.");
        }

        private void PositionVectorBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyChanged && _undoAction != null)
            {
                PositionVectorBox_OnPreviewMouseLeftButtonUp(sender, null);
            }
        }

        private void RotationVectorBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;

            _undoAction = GetRotationAction();
        }

        private void RotationVectorBox_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_propertyChanged) return;

            RecordActions(GetRotationAction(), "Rotation Changed.");
        }

        private void RotationVectorBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyChanged && _undoAction != null)
            {
                RotationVectorBox_OnPreviewMouseLeftButtonUp(sender, null);
            }
        }

        private void ScaleVectorBox_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _propertyChanged = false;

            _undoAction = GetScaleAction();
        }

        private void ScaleVectorBox_OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_propertyChanged) return;

            RecordActions(GetScaleAction(), "Scale Changed.");
        }

        private void ScaleVectorBox_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_propertyChanged && _undoAction != null)
            {
                ScaleVectorBox_OnPreviewMouseLeftButtonUp(sender, null);
            }
        }
    }
}
