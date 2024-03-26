using System.Windows;
using System.Windows.Controls;
using PrimalEditor.Content;
using PrimalEditor.GameDev;
using PrimalEditor.Utilities.Controls;

namespace PrimalEditor.Editors
{
    /// <summary>
    /// Interaction logic for WorldEditorView.xaml
    /// </summary>
    public partial class WorldEditorView : UserControl
    {
        public WorldEditorView()
        {
            InitializeComponent();

            Loaded += OnWorldEditorViewLoaded;
        }

        private void OnWorldEditorViewLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnWorldEditorViewLoaded;
            
            Focus();
        }

        private void NewScriptsButton_OnClick(object sender, RoutedEventArgs e)
        {
            new NewScriptDialog().Show();
        }

        private void BuildButton_OnClick(object sender, RoutedEventArgs e)
        {

        }

        private void OnCreatePrimitiveMesh_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new PrimitiveMeshDialog();

            dlg.ShowDialog();
        }
    }
}
