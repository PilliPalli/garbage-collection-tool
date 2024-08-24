using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Garbage_Collector.ViewModel;

namespace Garbage_Collector.View
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }
        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is LoginVM viewModel)
            {
                viewModel.Password = ((PasswordBox)sender).Password;
            }
        }


        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}