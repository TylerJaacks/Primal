using PrimalEditor.Components;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Editor
{
    /// <summary>
    /// Interaction logic for ProjectLayout.xaml
    /// </summary>
    public partial class ProjectLayoutView : UserControl
    {
        public ProjectLayoutView()
        {
            InitializeComponent();
        }

        private void OnAddGameEntityToSceneButton_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            var vm = btn?.DataContext as Scene;

            vm?.AddGameEntityCommand.Execute(new GameEntity(vm)
            {
                Name = "Empty Game Entity"
            });
        }

        private void OnGameEntitySelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            var newSelection = listBox?.SelectedItems.Cast<GameEntity>().ToList();
            var previousSelection = newSelection?.Except(e.AddedItems.Cast<GameEntity>()).Concat(e.RemovedItems.Cast<GameEntity>()).ToList();

            Project.UndoRedo.Add(new UndoRedoAction(
                $"Selection has been changed.",
                () =>
                {
                    listBox?.UnselectAll();

                    previousSelection!.ForEach(x => (listBox.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem).IsSelected = true);
                },
                () =>
                {
                    listBox?.UnselectAll();

                    newSelection!.ForEach(x => ((listBox!.ItemContainerGenerator.ContainerFromItem(x) as ListBoxItem)!).IsSelected = true);
                }
            ));

            MSGameEntity msEntity = null;

            if (newSelection!.Any())
            {
                msEntity = new MSGameEntity(newSelection);
            }

            GameEntityView.Instance.DataContext = msEntity;
        }
    }
}
