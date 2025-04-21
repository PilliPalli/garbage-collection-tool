using Garbage_Collector.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Garbage_Collector.View
{
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
            passwordBox2.KeyDown += PasswordBox_KeyDown;    

        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterVM viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is RegisterVM viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {

            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void PasswordBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (DataContext is RegisterVM viewModel && viewModel.RegisterCommand.CanExecute(this))
                {
                    viewModel.RegisterCommand.Execute(this);
                }
            }
        }
    }
}