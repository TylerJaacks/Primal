using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.GameProject
{
    /// <summary>
    /// Interaction logic for NewProject.xaml
    /// </summary>
    public partial class NewProjectView : UserControl
    {
        public NewProjectView()
        {
            InitializeComponent();
        }

        private void OnCreate_Button_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as NewProject;

            var projectPath = vm?.CreateProject(TemplateListBox.SelectedItem as ProjectTemplate);

            var dialogResult = false;

            var win = Window.GetWindow(this);

            if (!string.IsNullOrEmpty(projectPath))
            {
                dialogResult = true;

                var project = OpenProject.Open(new ProjectData()
                {
                    ProjectName = vm.ProjectName,
                    ProjectPath = projectPath,
                });

                win.DataContext = project;
            }

            if (win == null) return;

            win.DialogResult = dialogResult;
            win.Close();
        }
    }
}
