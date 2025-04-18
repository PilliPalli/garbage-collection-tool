using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Konscious.Security.Cryptography;
using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class AdministrationVM : ViewModelBase
    {
        private User _selectedUser;
        private Role _selectedRole;
        private string _statusMessage;

        public ObservableCollection<User> Users { get; set; }
        public ObservableCollection<Role> Roles { get; set; }



        public User SelectedUser
        {
            get => _selectedUser;
            set
            {
                if (_selectedUser != value)
                {
                    _selectedUser = value;
                    OnPropertyChanged();
                }
            }
        }

        public Role SelectedRole
        {
            get => _selectedRole;
            set
            {
                if (_selectedRole != value)
                {
                    _selectedRole = value;
                    OnPropertyChanged();
                }
            }
        }

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
        public ICommand AssignRoleCommand { get; set; }

        public AdministrationVM()
        {
            LoadUsersAndRoles();
            DeleteUserCommand = new RelayCommand(DeleteUser, CanModifyUser);
            ChangePasswordCommand = new RelayCommand(ChangePassword, CanModifyUser);
            AssignRoleCommand = new RelayCommand(AssignRole, CanModifyUser);
        }

        private void LoadUsersAndRoles()
        {
            using (var context = new GarbageCollectorDbContext())
            {
                Users = new ObservableCollection<User>(context.Users.ToList());
                Roles = new ObservableCollection<Role>(context.Roles.ToList());
            }
        }


        private void DeleteUser(object parameter)
        {
            if (parameter is User user)
            {
                using (var context = new GarbageCollectorDbContext())
                {
                    var userRoles = context.UserRoles.Where(ur => ur.UserId == user.UserId).ToList();
                    context.UserRoles.RemoveRange(userRoles);
                    context.SaveChanges();

                    context.Users.Remove(user);
                    context.SaveChanges();

                    Users.Remove(user);
                    Application.Current.Dispatcher.Invoke(async () =>
                   {
                       StatusMessage = "Benutzer erfolgreich gelöscht.";

                       if (user.UserId == LoginVM.CurrentUserId)
                       {
                           var navigationVM = new NavigationVM();
                           await Task.Delay(2000);
                           navigationVM.LogoutCommand.Execute(null);
                       }
                   });
                }
            }
        }

        private void ChangePassword(object parameter)
        {
            if (parameter is User user)
            {
                string newPassword = "CHANGE_ME_BEFORE_USE";
                user.PasswordHash = HashPassword(newPassword);

                using (var context = new GarbageCollectorDbContext())
                {
                    context.Users.Update(user);
                    context.SaveChanges();
                    Application.Current.Dispatcher.Invoke(() => {
                        StatusMessage = "Passwort erfolgreich zurückgesetzt.";
                    });
                }
            }
        }

        private void AssignRole(object parameter)
        {
            if (SelectedUser != null && SelectedRole != null)
            {
                using (var context = new GarbageCollectorDbContext())
                {
                    var userRole = context.UserRoles
                        .FirstOrDefault(ur => ur.UserId == SelectedUser.UserId);

                    if (userRole != null)
                    {

                        userRole.RoleId = SelectedRole.RoleId;
                        context.UserRoles.Update(userRole);
                    }
                    else
                    {

                        userRole = new UserRole
                        {
                            UserId = SelectedUser.UserId,
                            RoleId = SelectedRole.RoleId
                        };
                        context.UserRoles.Add(userRole);
                    }

                    context.SaveChanges();
                    LoadUsersAndRoles();
                    Application.Current.Dispatcher.Invoke(() => {
                        StatusMessage = "Rolle erfolgreich zugewiesen.";
                    });
                }
            }
        }



        private bool CanModifyUser(object parameter)
        {
            return SelectedUser != null && SelectedUser.Username != "admin";
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
    }

}
