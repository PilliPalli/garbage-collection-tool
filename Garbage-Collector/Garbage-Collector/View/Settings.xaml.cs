using Garbage_Collector.ViewModel;
using System.Windows;
using System.Windows.Controls;

namespace Garbage_Collector.View
{
    /// <summary>
    /// Interaktionslogik für Settings.xaml
    /// </summary>
    public partial class Settings : UserControl
    {
        public Settings()
        {
            InitializeComponent();
        }

        private void OldPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)        {
            if (DataContext is SettingsVM viewModel)
            {
                viewModel.OldPassword = ((PasswordBox)sender).Password;
            }
        }

        private void NewPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsVM viewModel)
            {
                viewModel.NewPassword = ((PasswordBox)sender).Password;
            }
        }

        private void ConfirmPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is SettingsVM viewModel)
            {
                viewModel.ConfirmPassword = ((PasswordBox)sender).Password;
            }
        }

    }
}
