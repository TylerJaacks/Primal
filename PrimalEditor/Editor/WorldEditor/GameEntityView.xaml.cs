using PrimalEditor.Components;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.Editor;

public partial class GameEntityView : UserControl
{
    public static GameEntityView Instance { get; private set; }

    private Action _undoAction;

    private string _propertyName;

    public GameEntityView()
    {
        InitializeComponent();

        DataContext = null;
        Instance = this;

        DataContextChanged += (_, __) =>
        {
            if (DataContext != null)
            {
                ((MSEntity)DataContext).PropertyChanged += (s, e) => _propertyName = e.PropertyName;
            }
        };
    }

    private Action GetRenameAction()
    {
        var vm = DataContext as MSEntity;

        var selection = vm?.SelectedEntities.Select(entity => (entity, entity.Name)).ToList();

        return new Action(() =>
        {
            selection?.ForEach(item => item.entity.Name = item.Name);

            (DataContext as MSEntity)?.Refresh();
        });
    }

    private Action GetEnableAction()
    {
        var vm = DataContext as MSEntity;

        var selection = vm?.SelectedEntities.Select(entity => (entity, entity.IsEnabled)).ToList();

        return new Action(() =>
        {
            selection?.ForEach(item => item.entity.IsEnabled = item.IsEnabled);

            (DataContext as MSEntity)?.Refresh();
        });
    }

    private void Name_OnKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        _undoAction = GetRenameAction();
    }

    private void Name_OnLostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        if (_propertyName == nameof(MSEntity.Name) && _undoAction != null)
        {
            var redoAction = GetRenameAction();

            Project.UndoRedo.Add(new UndoRedoAction("Rename game entity.", _undoAction, redoAction));

            _propertyName = null;
        }

        _undoAction = null;
    }

    private void IsEnable_OnClick(object sender, RoutedEventArgs e)
    {
        var undoAction = GetEnableAction();

        var vm = DataContext as MSEntity;

        vm.IsEnabled = (sender as CheckBox)?.IsChecked == true;

        var redoAction = GetEnableAction();

        Project.UndoRedo.Add(new UndoRedoAction(vm.IsEnabled == true ? "Enable game entity" : "Disable game entity", undoAction, redoAction));
    }
}
