using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Konscious.Security.Cryptography;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class AdministrationVM : ViewModelBase
    {
        public ObservableCollection<User> Users { get; set; }
        private User _selectedUser;
        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged(); // Benachrichtige die UI über die Änderung
                }
            }
        }
        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {

                _statusMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand DeleteUserCommand { get; set; }
        public ICommand ChangePasswordCommand { get; set; }

        public AdministrationVM()
        {
            LoadUsers();
            DeleteUserCommand = new RelayCommand(DeleteUser, CanModifyUser);
            ChangePasswordCommand = new RelayCommand(ChangePassword, CanModifyUser);
        }

        private void LoadUsers()
        {
            using (var context = new GarbageCollectorDbContext())
            {
                Users = new ObservableCollection<User>(context.Users.ToList());
            }
        }

        private void DeleteUser(object parameter)
        {
            if (parameter is User user)
            {
                using (var context = new GarbageCollectorDbContext())
                {
                    context.Users.Remove(user);
                    context.SaveChanges();
                    Users.Remove(user);
                    StatusMessage = "User removed";
                }
            }
        }

        private void ChangePassword(object parameter)
        {
            if (parameter is User user)
            {
                string newPassword = "CHANGE_ME_BEFORE_USE"; // Setze das neue Passwort auf ein Standardpasswort
                user.PasswordHash = HashPassword(newPassword);

                using (var context = new GarbageCollectorDbContext())
                {
                    context.Users.Update(user);
                    context.SaveChanges();
                    StatusMessage = $"{user.Username}'s password resetted";
                }
            }
        }

        private bool CanModifyUser(object parameter)
        {
            return SelectedUser != null && SelectedUser.Username != "admin"; // Admin-Konto soll nicht modifizierbar sein
        }

        private string HashPassword(string password)
        {
            // Erzeugt ein Salt
            byte[] salt = new byte[16];

            RandomNumberGenerator.Fill(salt);

            // Verwendet Argon2id für das Passwort-Hashing
            var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
            {
                Salt = salt,
                DegreeOfParallelism = 8, // Anzahl der Threads
                Iterations = 4,          // Anzahl der Iterationen
                MemorySize = 65536       // Speichergröße in KB
            };

            byte[] hash = argon2.GetBytes(32); // 256-bit Hash

            // Kombiniert das Salt und den Hash für die Speicherung
            byte[] hashBytes = new byte[48];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 32);

            return Convert.ToBase64String(hashBytes); // In Base64 umwandeln zur Speicherung
        }
    }
}
