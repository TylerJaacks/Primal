using PrimalEditor.GameProject;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using PrimalEditor.GameDev;

namespace PrimalEditor.Editors
{
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
    }
}
