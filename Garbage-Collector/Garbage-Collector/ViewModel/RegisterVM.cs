using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Garbage_Collector.View;
using Konscious.Security.Cryptography;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class RegisterVM : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _confirmPassword;

       

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
            var loginView = new Login();
            Application.Current.MainWindow = loginView;
            loginView.Show();

            if (parameter is Window registerWindow)
            {
                registerWindow.Close();
            }
        }


        private void ExecuteRegister(object parameter)
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
            {
               
                SnackbarService.Show("Benutzername und Passwort dürfen nicht leer sein.", "error");
                return;
            }

            if (Password != ConfirmPassword)
            {
                SnackbarService.Show("Passwörter stimmen nicht überein.", "error");
                return;
            }

            using (var context = new GarbageCollectorDbContext())
            {
                string normalizedUsername = Username.ToLower();

                if (context.Users.Any(u => u.Username == normalizedUsername))
                {
                    SnackbarService.Show("Benutzername bereits vergeben.", "error");
                    return;
                }

                var newUser = new User
                {
                    Username = Username,
                    PasswordHash = HashPassword(Password)
                };

                context.Users.Add(newUser);
                context.SaveChanges();

                UserRole userRole;

                // Wenn dies der erste Benutzer ist, weise die Admin-Rolle zu
                if (!context.UserRoles.Any())
                {
                    var adminRole = context.Roles.Single(r => r.RoleName == "Admin");
                    userRole = new UserRole
                    {
                        UserId = newUser.UserId,
                        RoleId = adminRole.RoleId
                    };
                }
                else
                {
                    var userRoleEntity = context.Roles.Single(r => r.RoleName == "User");
                    userRole = new UserRole
                    {
                        UserId = newUser.UserId,
                        RoleId = userRoleEntity.RoleId
                    };
                }

                context.UserRoles.Add(userRole);
                context.SaveChanges();
            }

            SnackbarService.Show("Registrierung erfolgreich", "success");
        }



        private static string HashPassword(string password)
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

        private void CloseWindow(object parameter)
        {
            if (parameter is Window window)
            {
                window.Close();
            }
        }
    }
}