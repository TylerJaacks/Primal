﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PrimalEditor.Dictionaries
{
    public partial class ControlTemplates :ResourceDictionary
    {
        private void OnTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var exp = textBox.GetBindingExpression(TextBox.TextProperty);
            
            if (exp == null) return;
            
            if (e.Key == Key.Enter)
            { 
                if (textBox.Tag is ICommand command && command.CanExecute(textBox.Text))
                {
                    command.Execute(textBox.Text);
                }
                else
                {
                    exp.UpdateSource();
                }

                Keyboard.ClearFocus();
                
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                exp.UpdateTarget();

                Keyboard.ClearFocus();
            }
        }

        private void OnTextBoxWithRenameKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            var exp = textBox?.GetBindingExpression(TextBox.TextProperty);

            if (exp == null) return;

            if (e.Key == Key.Enter)
            {
                if (textBox.Tag is ICommand command && command.CanExecute(textBox.Text))
                {
                    command.Execute(textBox.Text);
                }
                else
                {
                    exp.UpdateSource();
                }

                textBox.Visibility = Visibility.Collapsed;

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                exp.UpdateTarget();

                textBox.Visibility = Visibility.Collapsed;
            }
        }

        private void OnTextBoxWithRenameLostFocus(object sender, RoutedEventArgs e)
        {
            var textBox = sender as TextBox;
            var exp = textBox?.GetBindingExpression(TextBox.TextProperty);

            if (exp != null)
            {
                exp.UpdateTarget();

                textBox.Visibility = Visibility.Collapsed;
            }
        }

        private void OnCloseButtonClick(object sender, RoutedEventArgs e)
        {
            var window = (Window) ((FrameworkElement) sender).TemplatedParent;

            window.Close();
        }

        private void OnMaximizeRestoreButtonClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.WindowState = (window.WindowState == WindowState.Normal) ?
                WindowState.Maximized : WindowState.Normal;
        }

        private void OnMimimizeButtonClick(object sender, RoutedEventArgs e)
        {
            var window = (Window)((FrameworkElement)sender).TemplatedParent;

            window.WindowState = WindowState.Minimized;
        }
    }
}
