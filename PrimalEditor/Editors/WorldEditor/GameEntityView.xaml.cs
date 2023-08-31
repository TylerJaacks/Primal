using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

using PrimalEditor.Components;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// Interaction logic for GameEntityView.xaml
    /// </summary>
    public partial class GameEntityView : UserControl
    {
        private Action _undoAction;

        private string _propertyName;

        public static GameEntityView Instance { get; private set; }

        public GameEntityView()
        {
            InitializeComponent();

            DataContext = null;
            Instance = this;

            DataContextChanged += (_, __) =>
            {
                if (DataContext != null)
                {
                    ((MsEntity) DataContext).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
                }
            };
        }

        private Action GetRenameAction()
        {
            var vm = DataContext as MsEntity;
            var selection = vm?.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();

            return new Action(() =>
            {
                selection?.ForEach(item => item.entity.Name = item.Name);

                (DataContext as MsEntity)?.Refresh();
            });
        }

        private Action GetIsEnabledAction()
        {
            var vm = DataContext as MsEntity;
            var selection = vm?.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();
            return new Action(() =>
            {
                selection?.ForEach(item => item.entity.IsEnabled = item.IsEnabled);

                (DataContext as MsEntity)?.Refresh();
            });
        }

        private void AddComponent(ComponentType componentType, object data)
        {
            var creationFunction = ComponentFactory.GetCreationFunction(componentType);
            var vm = DataContext as MsEntity;

            var entityList = (from entity in vm?.SelectedEntities let component = creationFunction(entity, data) where entity.AddComponent(component) select (entity, component)).ToList();

            if (!entityList.Any()) return;

            vm.Refresh();

            Project.UndoRedo.Add(new UndoRedoAction(
                () =>
                {
                    entityList.ForEach(x => x.entity.RemoveComponent(x.component));
                    (DataContext as MsEntity)?.Refresh();
                },
                () =>
                {
                    entityList.ForEach(x => x.entity.AddComponent(x.component));
                    (DataContext as MsEntity)?.Refresh();
                },
                $"Added {componentType} component"
            ));
        }

        private void OnName_TextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            _propertyName = string.Empty;

            _undoAction = GetRenameAction();
        }

        private void OnName_TextBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if(_propertyName == nameof(MsEntity.Name) && _undoAction != null)
            {
                var redoAction = GetRenameAction();
                Project.UndoRedo.Add(new UndoRedoAction(_undoAction, redoAction, "Rename game entity"));
                _propertyName = null;
            }
            _undoAction = null;
        }

        private void OnIsEnabled_CheckBox_Click(object sender, RoutedEventArgs e)
        {
            var undoAction = GetIsEnabledAction();

            if (DataContext is not MsEntity vm) return;

            vm.IsEnabled = sender is CheckBox { IsChecked: true };

            var redoAction = GetIsEnabledAction();

            Project.UndoRedo.Add(new UndoRedoAction(undoAction, redoAction,
                vm.IsEnabled == true ? "Enable game entity" : "Disable game entity"));
        }

        private void OnAddScriptComponent_OnClick(object sender, RoutedEventArgs e)
        {
            AddComponent(ComponentType.Script, (sender as MenuItem)?.Header.ToString());
        }

        private void AddComponent_OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var menu = FindResource("AddComponentMenu") as ContextMenu;

            if (sender is ToggleButton btn)
            {
                btn.IsChecked = true;

                if (menu == null) return;

                menu.Placement = PlacementMode.Bottom;
                menu.PlacementTarget = btn;
                menu.MinHeight = btn.ActualWidth;
            }

            if (menu != null) menu.IsOpen = true;
        }
    }
}
