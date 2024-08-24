using System.Linq;
using System.Windows;
using System.Windows.Input;
using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Garbage_Collector.View;

namespace Garbage_Collector.ViewModel
{
    public class LoginVM : ViewModelBase
    {
        private string _username;
        private string _password;
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

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand LoginCommand { get; }
        public ICommand RegisterCommand { get; }
        public ICommand CloseCommand { get; }

        public LoginVM()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
            RegisterCommand = new RelayCommand(OpenRegister);
            CloseCommand = new RelayCommand(CloseWindow);
        }

        private void ExecuteLogin(object parameter)
        {
            if (ValidateCredentials(Username, Password))
            {
                // Öffne das Hauptfenster
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();

                // Schließe das Login-Fenster
                if (parameter is Window loginWindow)
                {
                    loginWindow.Close();
                }
            }
            else
            {
                ErrorMessage = "Invalid username or password";
            }
        }


        private bool ValidateCredentials(string username, string password)
        {
            using (var context = new GarbageCollectorDbContext())
            {
                var user = context.Users.SingleOrDefault(u => u.Username == username);
                if (user != null)
                {
                    return VerifyPassword(password, user.PasswordHash);
                }
                else
                {
                    return false;
                }
            }
        }

        private bool VerifyPassword(string password, string storedHash)
        {
            string hash = HashPassword(password);
            return hash == storedHash;
        }

        private string HashPassword(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var builder = new System.Text.StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private void OpenRegister(object parameter)
        {
            // Öffne das Registrierungsfenster
            var registerView = new Register();
            Application.Current.MainWindow = registerView;
            registerView.Show();

            // Schließe das aktuelle Login-Fenster
            if (parameter is Window loginWindow)
            {
                loginWindow.Close();
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