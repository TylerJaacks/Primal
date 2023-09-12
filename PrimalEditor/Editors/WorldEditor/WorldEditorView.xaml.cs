using System.Windows;
using System.Windows.Controls;

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
    }
}
