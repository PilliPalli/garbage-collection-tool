using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Garbage_Collector.Utilities;
using Garbage_Collector.View;
using Garbage_Collector.Model;
using Konscious.Security.Cryptography;

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
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Username cannot be empty.";
            }
            else if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Password cannot be empty.";
            }
            else if (ValidateCredentials(Username, Password))
            {
                var mainWindow = new MainWindow();
                Application.Current.MainWindow = mainWindow;
                mainWindow.Show();

                if (parameter is Window loginWindow)
                {
                    loginWindow.Close();
                }
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }
        }

        private bool ValidateCredentials(string username, string password)
        {
            // Keine leeren Passwörter zulassen
            if (string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            using (var context = new GarbageCollectorDbContext())
            {
                // Benutzer nach Groß-/Kleinschreibung ignorierend abfragen
                var user = context.Users.SingleOrDefault(u => u.Username.ToLower() == username.ToLower());
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
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            // Extrahiere das Salt aus dem gespeicherten Hash
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            // Verwende Argon2id zur Passwortüberprüfung mit dem extrahierten Salt
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                Iterations = 4,
                MemorySize = 65536
            };

            byte[] hash = argon2.GetBytes(32);

            // Vergleiche die Hashes
            for (int i = 0; i < 32; i++)
            {
                if (hash[i] != hashBytes[16 + i])
                {
                    return false;
                }
            }

            return true;
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