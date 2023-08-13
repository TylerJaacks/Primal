using System.IO;
using System.Windows;

namespace PrimalEditor
{
    public partial class EnginePathDialog : Window
    {
        public string PrimalPath { get; private set; }

        public EnginePathDialog()
        {
            InitializeComponent();

            Owner = Application.Current.MainWindow;
        }

        private void OkButton_OnClick(object sender, RoutedEventArgs e)
        {
            var path = PathTextBox.Text;

            MessageTextBlock.Text = string.Empty;

            if (string.IsNullOrEmpty(path))
            {
                MessageTextBlock.Text = "Invalid path.";
            }
            else if (path.IndexOfAny(Path.GetInvalidPathChars()) != -1)
            {
                MessageTextBlock.Text = "Invalid character(s) used in path.";
            }
            else if (!Directory.Exists(Path.Combine(path, @"Engine\EngineAPI\")))
            {
                MessageTextBlock.Text = "Unable to find the Engine at the specified location.";
            }

            if (!string.IsNullOrWhiteSpace(MessageTextBlock.Text)) return;

            if (!Path.EndsInDirectorySeparator(path)) path += @"\";

            PrimalPath = path;

            DialogResult = true;

            Close();
        }
    }
}
