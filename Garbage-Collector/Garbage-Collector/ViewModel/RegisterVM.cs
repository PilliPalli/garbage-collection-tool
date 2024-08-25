using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Garbage_Collector.Utilities;
using Garbage_Collector.View;
using Garbage_Collector.Model;
using Konscious.Security.Cryptography;

namespace Garbage_Collector.ViewModel
{
    public class RegisterVM : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _confirmPassword;
        private string _errorMessage;
        private string _successMessage;

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

        public string SuccessMessage
        {
            get => _successMessage;
            set { _successMessage = value; OnPropertyChanged(); }
        }

        public ICommand RegisterCommand { get; }
        public ICommand CloseCommand { get; }
        public ICommand BackToLoginCommand { get; }

        public RegisterVM()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister);
            CloseCommand = new RelayCommand(CloseWindow);
            BackToLoginCommand = new RelayCommand(BackToLogin);
        }

        private void BackToLogin(object parameter)
        {
            // Öffne das Login-Fenster
            var loginView = new Login();
            Application.Current.MainWindow = loginView;
            loginView.Show();

            // Schließe das aktuelle Register-Fenster
            if (parameter is Window registerWindow)
            {
                registerWindow.Close();
            }
        }


        private void ExecuteRegister(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
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
                string normalizedUsername = Username.ToLower();

                if (context.Users.Any(u => u.Username == normalizedUsername))
                {
                    ErrorMessage = "Username already exists.";
                    return;
                }

                var newUser = new User
                {
                    Username = Username, // Speichere den Benutzernamen in der Originalschreibweise
                    PasswordHash = HashPassword(Password)
                };

                context.Users.Add(newUser);
                context.SaveChanges();
            }

            SuccessMessage = "Registration successful";
            // Keine automatische Rückkehr zum Login-Fenster
        }




        private string HashPassword(string password)
        {
            // Erzeuge ein Salt
            byte[] salt = new byte[16];

            RandomNumberGenerator.Fill(salt);

            // Verwende Argon2id für das Passwort-Hashing
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8, // Anzahl der Threads
                Iterations = 4,          // Anzahl der Iterationen
                MemorySize = 65536       // Speichergröße in KB
            };

            byte[] hash = argon2.GetBytes(32); // 256-bit Hash

            // Kombiniere das Salt und den Hash für die Speicherung
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes); // In Base64 umwandeln zur Speicherung
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