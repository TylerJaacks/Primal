using System.Linq;
using System.Windows;

namespace PrimalEditor.GameProject
{
    /// <summary>
    /// Interaction logic for ProjectBrowser.xaml
    /// </summary>
    public partial class ProjectBrowserDialog : Window
    {
        public ProjectBrowserDialog()
        {
            InitializeComponent();

            Loaded += OnProjectBrowserDialogLoaded;
        }

        private void OnProjectBrowserDialogLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnProjectBrowserDialogLoaded;

            if (!OpenProject.Projects.Any())
            {
                OpenProjectButton.IsEnabled = false;

                OpenProjectView.Visibility = Visibility.Hidden;
                OnToggleButton_Click(NewProjectButton, new RoutedEventArgs());
            }
        }

        private void OnToggleButton_Click(object sender, RoutedEventArgs e)
        {
            if (Equals(sender, OpenProjectButton))
            {
                if (NewProjectButton.IsChecked == true)
                {
                    NewProjectButton.IsChecked = false;

                    BrowserContent.Margin = new Thickness(0);
                }

                OpenProjectButton.IsChecked = true;
            }
            else
            {
                if (OpenProjectButton.IsChecked == true)
                {
                    OpenProjectButton.IsChecked = false;

                    BrowserContent.Margin = new Thickness(-800, 0, 0, 0);
                }

                NewProjectButton.IsChecked = true;
            }
        }
    }
}
