using System;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Garbage_Collector.Model;

namespace Garbage_Collector.View
{
    public partial class Register : Window
    {
        public Register()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;
            string confirmPassword = ConfirmPasswordBox.Password;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ErrorMessageTextBlock.Text = "Username and Password cannot be empty.";
                return;
            }

            if (password != confirmPassword)
            {
                ErrorMessageTextBlock.Text = "Passwords do not match.";
                return;
            }

            using (var context = new GarbageCollectorDbContext())
            {
                if (context.Users.Any(u => u.Username == username))
                {
                    ErrorMessageTextBlock.Text = "Username already exists.";
                    return;
                }

                var newUser = new User
                {
                    Username = username,
                    PasswordHash = HashPassword(password)
                };

                context.Users.Add(newUser);
                context.SaveChanges();
            }

            MessageBox.Show("Registration successful!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

            // Optional: Nach erfolgreicher Registrierung zurück zum Login-Fenster
            var loginView = new Login();
            loginView.Show();
            this.Close();
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

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}