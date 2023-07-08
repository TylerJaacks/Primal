using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;


namespace PrimalEditor.GameProject
{
    public partial class ProjectBrowserDialog : Window
    {
        private readonly CubicEase _easing = new() { EasingMode = EasingMode.EaseInOut };

        public ProjectBrowserDialog()
        {
            InitializeComponent();

            Loaded += OnProjectBrowserDialogLoaded;
        }

        private void OnProjectBrowserDialogLoaded(object sender, RoutedEventArgs e)
        {
            Loaded -= OnProjectBrowserDialogLoaded;

            if (OpenProject.Projects.Any()) return;

            openProjectButton.IsEnabled = false;
            openProjectView.Visibility = Visibility.Hidden;

            OnToggleButtonClick(createProjectButton, new RoutedEventArgs());
        }

        private void OnToggleButtonClick(object sender, RoutedEventArgs e)
        {
            if (Equals(sender, openProjectButton))
            {
                if (createProjectButton.IsChecked == true)
                {
                    createProjectButton.IsChecked = false;

                    AnimateToOpenProject();

                    openProjectView.IsEnabled = true;
                    newProjectView.IsEnabled = false;
                }

                openProjectButton.IsChecked = true;
            }
            else
            {
                if (openProjectButton.IsChecked == true)
                {
                    openProjectButton.IsChecked = false;

                    AnimateToCreateProject();

                    openProjectView.IsEnabled = false;
                    newProjectView.IsEnabled = true;
                }

                createProjectButton.IsChecked = true;
            }
        }

        private void AnimateToCreateProject()
        {
            var highlightAnimation = new DoubleAnimation(200, 400, new Duration(TimeSpan.FromSeconds(0.2)));

            highlightAnimation.Completed += (s, e) =>
            {
                var animation = new ThicknessAnimation(new Thickness(0, 0, 0, 0), new Thickness(-1600, 0, 0, 0),
                    new Duration(TimeSpan.FromSeconds(0.5)));

                animation.EasingFunction = _easing;

                browserContent.BeginAnimation(MarginProperty, animation);
            };

            HighlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
        }

        private void AnimateToOpenProject()
        {
            var highlightAnimation = new DoubleAnimation(400, 200, new Duration(TimeSpan.FromSeconds(0.2)));

            highlightAnimation.Completed += (s, e) =>
            {
                var animation = new ThicknessAnimation(new Thickness(-1600, 0, 0, 0), new Thickness(0),
                    new Duration(TimeSpan.FromSeconds(0.5)));

                animation.EasingFunction = _easing;

                browserContent.BeginAnimation(MarginProperty, animation);
            };

            HighlightRect.BeginAnimation(Canvas.LeftProperty, highlightAnimation);
        }
    }
}
