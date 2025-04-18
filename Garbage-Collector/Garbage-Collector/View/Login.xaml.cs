using Garbage_Collector.Utilities;
using Garbage_Collector.ViewModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Garbage_Collector.View
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
            SnackbarService.Initialize(this);
            passwordBox2.KeyDown += PasswordBox_KeyDown;
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginVM viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
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
                if (DataContext is LoginVM viewModel && viewModel.LoginCommand.CanExecute(this))
                {
                    viewModel.LoginCommand.Execute(this);
                }
            }
        }
    }
}