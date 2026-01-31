using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using PrimalEditor.GameProject;
using PrimalEditor.Utilities;
using Window = System.Windows.Window;


namespace PrimalEditor.GameDev
{
    public partial class NewScriptDialog : Window
    {
        private static readonly string _cppCode =
@"#include ""{0}.h""

namespace {1} {{

    REGISTER_SCRIPT({0});

    void {0}::begin_play()
    {{

    }}

    void {0}::update(float dt)
    {{

    }}
}}";

        private static readonly string _hCode =
@"#pragma once
  
namespace {1} {{

    class {0} : public primal::script::entity_script
    {{
    public:
        constexpr explicit {0}(primal::game_entity::entity entity) : primal::script::entity_script(entity) {{ }}

        void begin_play() override;

        void update(float dt) override;
    }};
}}
";

        private static readonly string Namespace = GetNamespaceFromProjectName();

        private static string GetNamespaceFromProjectName()
        {
            var projectName = Project.Current.Name.Trim();

            if (string.IsNullOrEmpty(projectName)) return string.Empty;

            projectName = Regex.Replace(projectName, @"^[A-Za-z_][A-za-z0-9_]*", "");

            return projectName;
        }

        public NewScriptDialog()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;

            ScriptPathTextBox.Text = @"GameCode\";
        }

        private bool Validate()
        {
            var isValid = false;

            var name = ScriptNameTextBox.Text.Trim();
            var path = ScriptPathTextBox.Text.Trim();

            var errorMessage = string.Empty;

            var nameRegex = new Regex(@"^[A-Za-z_][A-za-z0-9_]*");

            if (string.IsNullOrEmpty(name))
                errorMessage = "Type in a script name.";
            else if (!nameRegex.IsMatch(name))
                errorMessage = "Invalid character(s) use in script name.";
            else if (string.IsNullOrEmpty(path))
                errorMessage = "Select a valid script folder.";
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
                errorMessage = "Invalid characters(s) used in the script path.";

            else if (name.IndexOfAny(Path.GetInvalidFileNameChars()) != -1 || name.Any(char.IsWhiteSpace))
                errorMessage = "Invalid character(s) used in script name.";
            else if (!Path.GetFullPath(Path.Combine(Project.Current.Path, path))
                         .Contains(Path.Combine(Project.Current.Path, @"GameCode\")))
                errorMessage = "Script must be added to (a sub-folder of) GameCode.";
            else if (File.Exists(
                             Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.cpp"))) ||
                         File.Exists(Path.GetFullPath(Path.Combine(Path.Combine(Project.Current.Path, path), $"{name}.h"))))
                errorMessage = $"Script already exists with the name {name}.";
            else
                isValid = true;

            if (!isValid)
                MessageTextBlock.Foreground = FindResource("Editor.RedBrush") as Brush;
            else
                MessageTextBlock.Foreground = FindResource("Editor.FontBrush") as Brush;

            MessageTextBlock.Text = errorMessage;

            return isValid;
        }

        private void ScriptNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            if (!Validate()) return;

            var name = ScriptNameTextBox.Text.Trim();

            MessageTextBlock.Text = $"{name}.h and {name}.cpp will be added to {Project.Current.Name}";
        }

        private void ScriptPathTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            Validate();
        }

        private async void Create_OnClick(object sender, RoutedEventArgs e)
        {
            if (!Validate()) return;

            IsEnabled = false;

            BusyAnimation.Opacity = 0;
            BusyAnimation.Visibility = Visibility.Visible;

            var fadeIn = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(500)));

            BusyAnimation.BeginAnimation(OpacityProperty, fadeIn);

            try
            {
                var name = ScriptNameTextBox.Text;
                var path = Path.GetFullPath(Path.Combine(Project.Current.Path, ScriptPathTextBox.Text.Trim()));
                var solution = Project.Current.Solution;
                var projectName = Project.Current.Name;

                await Task.Run(() => CreateScript(name, path, solution, projectName));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);

                Logger.Log(MessageType.Error, $"Failed to create script {ScriptNameTextBox.Text}");
            }
            finally
            {
                var fadeOut = new DoubleAnimation(1, 0, new Duration(TimeSpan.FromMilliseconds(200)));

                fadeOut.Completed += (s, eventArgs) =>
                {
                    BusyAnimation.Opacity = 0;
                    BusyAnimation.Visibility = Visibility.Hidden;
                    
                    Close();
                };

                BusyAnimation.BeginAnimation(OpacityProperty, fadeOut);
            }
        }

        private static void CreateScript(string name, string path, string solution, string projectName)
        {
            if (!Directory.Exists(path)) Directory.CreateDirectory(path);

            var cpp = Path.GetFullPath(Path.Combine(path, $"{name}.cpp"));
            var h = Path.GetFullPath(Path.Combine(path, $"{name}.h"));

            using (var sw = File.CreateText(cpp))
            {
                sw.Write(string.Format(_cppCode, name, Namespace));
            }

            using (var sw = File.CreateText(h))
            {
                sw.Write(string.Format(_hCode, name, Namespace));
            }

            var files = new string[] { cpp, h };

            for (var i = 0; i < 3; i++)
            {
                if (!VisualStudio.AddFilesToSolution(solution, projectName, files))
                    Thread.Sleep(1000);
                else
                    break;
            }
        }
    }
}
