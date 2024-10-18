using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class SettingsVM : ViewModelBase
    {
        private AppConfig _config;
        private string _oldPassword;
        private string _newPassword;
        private string _confirmPassword;
        private string _statusMessage;
        private bool _isError;


        public bool DeleteDirectly
        {
            get => _config.DeleteDirectly;
            set
            {
                if (_config.DeleteDirectly != value)
                {
                    _config.DeleteDirectly = value;
                    OnPropertyChanged();
                }
            }
        }
        public string OldPassword
        {
            get => _oldPassword;
            set
            {
                _oldPassword = value; OnPropertyChanged();
            }
        }

        public string NewPassword
        {
            get => _newPassword;
            set
            {
                _newPassword = value; OnPropertyChanged();
            }
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set
            {
                _confirmPassword = value; OnPropertyChanged();
            }
        }


        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }



        public bool IsError
        {
            get => _isError;
            set
            {
                _isError = value;
                OnPropertyChanged();
            }
        }




        public ICommand ChangePasswordCommand { get; }

        public SettingsVM()
        {
            _config = AppConfig.LoadFromJson();
            ChangePasswordCommand = new RelayCommand(ExecuteChangePassword);

        }

        private void ExecuteChangePassword(object parameter)
        {
            if (string.IsNullOrWhiteSpace(OldPassword))
            {
                IsError = true;
                StatusMessage = "Altes Passwort darf nicht leer sein.";

                return;
            }

            if (string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword))
            {
                IsError = true;
                StatusMessage = "Das neue Passwort und die Bestätigung dürfen nicht leer sein.";
                return;
            }

            if (NewPassword != ConfirmPassword)
            {
                IsError = true;
                StatusMessage = "Die neuen Passwörter stimmen nicht überein.";
                return;
            }

            using (var context = new GarbageCollectorDbContext())
            {
                var user = context.Users.SingleOrDefault(u => u.UserId == LoginVM.CurrentUserId);

                if (user != null && VerifyPassword(OldPassword, user.PasswordHash))
                {

                    user.PasswordHash = HashPassword(NewPassword);
                    context.SaveChanges();
                    IsError = false;
                    StatusMessage = "Passwort erfolgreich geändert!";
                }
                else
                {
                    IsError = true;
                    StatusMessage = "Altes Passwort ist falsch.";
                }
            }
        }


        private bool VerifyPassword(string password, string storedHash)
        {
            byte[] hashBytes = Convert.FromBase64String(storedHash);

            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                Iterations = 4,
                MemorySize = 65536
            };

            byte[] hash = argon2.GetBytes(32);

            for (int i = 0; i < 32; i++)
            {
                if (hash[i] != hashBytes[16 + i])
                {
                    return false;
                }
            }

            return true;
        }


        private string HashPassword(string password)
        {

            byte[] salt = new byte[16];
            RandomNumberGenerator.Fill(salt);

            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8,
                Iterations = 4,
                MemorySize = 65536
            };

            byte[] hash = argon2.GetBytes(32);

            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes);
        }
    }
}
