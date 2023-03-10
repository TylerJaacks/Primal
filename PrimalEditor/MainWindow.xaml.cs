using PrimalEditor.GameProject;
using System.Windows;

namespace PrimalEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnMainWindowLoaded;
            Closing += OnMainWindowClosed;
        }

        private void OnMainWindowClosed(object sender, System.EventArgs e)
        {
            Closing -= OnMainWindowClosed;

            Project.Current?.Unload();
        }

        private void OnMainWindowLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnMainWindowLoaded;
            OpenProjectBrowserDialog();
        }

        private void OpenProjectBrowserDialog()
        {
            var projectBrowserDialog = new ProjectBrowserDialog();

            if (projectBrowserDialog.ShowDialog() == false || projectBrowserDialog.DataContext == null)
            {
                Application.Current.Shutdown();
            }
            else
            {
                Project.Current?.Unload();

                DataContext = projectBrowserDialog.DataContext;
            }
        }
    }
}
