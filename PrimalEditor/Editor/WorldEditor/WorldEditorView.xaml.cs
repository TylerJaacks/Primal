using PrimalEditor.GameProject;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Editor
{
    /// <summary>
    /// Interaction logic for WorldEditorView.xaml
    /// </summary>
    public partial class WorldEditorView : UserControl
    {
        public WorldEditorView()
        {
            InitializeComponent();

            Loaded += OnWorldViewLoaded;
        }

        private void OnWorldViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWorldViewLoaded;

            if (Project.Current == null)
                return;

            Focus();

            ((INotifyCollectionChanged)Project.Current.Scenes).CollectionChanged += (s, e) => Focus();
        }
    }
}
