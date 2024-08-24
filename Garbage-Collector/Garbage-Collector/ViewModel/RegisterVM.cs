using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Garbage_Collector.View;

namespace Garbage_Collector.ViewModel
{
    public class RegisterVM : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(); }
        }

        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set { _confirmPassword = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CloseCommand { get; }

        public RegisterVM()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void ExecuteRegister(object parameter)
        {
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(Password))
            {
                ErrorMessage = "Username and Password cannot be empty.";
                return;
            }

            if (Password != ConfirmPassword)
            {
                ErrorMessage = "Passwords do not match.";
                return;
            }

            using (var context = new GarbageCollectorDbContext())
            {
                if (context.Users.Any(u => u.Username == Username))
                {
                    ErrorMessage = "Username already exists.";
                    return;
                }

                var newUser = new User
                {
                    Username = Username,
                    PasswordHash = HashPassword(Password)
                };

                context.Users.Add(newUser);
                context.SaveChanges();
            }

            MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Schließe das aktuelle Registrierungsfenster
            if (parameter is Window registerWindow)
            {
                registerWindow.Close();
            }

            // Öffne das Login-Fenster
            var loginView = new Login();
            Application.Current.MainWindow = loginView;
            loginView.Show();
        }



        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                var builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void CloseWindow(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}