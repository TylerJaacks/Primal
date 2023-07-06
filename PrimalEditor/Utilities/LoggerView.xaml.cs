using System.Windows;
using System.Windows.Controls;

namespace PrimalEditor.Utilities
{
    /// <summary>
    /// Interaction logic for LoggerView.xaml
    /// </summary>
    public partial class LoggerView : UserControl
    {
        public LoggerView()
        {
            InitializeComponent();

            Loaded += (s, e) =>
            {
                Logger.Log(MessageType.Info, "Test Info");
                Logger.Log(MessageType.Warning, "Test Warning");
                Logger.Log(MessageType.Error, "Test Error");
            };
        }

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Clear();
        }

        private void ToggleInfo_OnClick(object sender, RoutedEventArgs e)
        {
            var filter = 0x0;

            if (toggleInfo.IsChecked == true)
                filter |= 0x1;
            if (toggleWarning.IsChecked == true)
                filter |= 0x2;
            if (toggleError.IsChecked == true)
                filter |= 0x4;

            Logger.SetMessageFilter(filter);
        }
    }
}
