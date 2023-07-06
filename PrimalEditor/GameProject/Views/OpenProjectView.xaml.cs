using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.GameProject
{
    /// <summary>
    /// Interaction logic for OpenProject.xaml
    /// </summary>
    public partial class OpenProjectView : UserControl
    {
        public OpenProjectView()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                var item =
                    ProjectsListBox.ItemContainerGenerator?.ContainerFromIndex(ProjectsListBox.SelectedIndex) as
                        ListBoxItem;

                item?.Focus();
            };
        }

        private void OpenSelectedProject()
        {
            var project = OpenProject.Open(ProjectsListBox.SelectedItem as ProjectData);

            var dialogResult = false;

            var win = Window.GetWindow(this);

            if (project != null)
            {
                dialogResult = true;
                win.DataContext = project;
            }

            win.DialogResult = dialogResult;
            win.Close();
        }

        private void OnOpen_Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            OpenSelectedProject();
        }

        public void OnListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            OpenSelectedProject();
        }
    }
}
