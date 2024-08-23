using System.Linq;
using System.Windows;

using Garbage_Collector.Model;

namespace Garbage_Collector.View
{
    public partial class Login : Window
    {
        public Login()
        {
            InitializeComponent();
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            string username = UsernameTextBox.Text;
            string password = PasswordBox.Password;

            if (ValidateCredentials(username, password))
            {
                // Öffne die Hauptanwendung nach erfolgreichem Login
                MainWindow mainWindow = new MainWindow();
                mainWindow.Show();

                // Schließe das Login-Fenster
                this.Close();
            }
            else
            {
                ErrorMessageTextBlock.Text = "Invalid username or password";
            }
        }

        private bool ValidateCredentials(string username, string password)
        {
            using (var context = new GarbageCollectorDbContext())
            {
                var user = context.Users.SingleOrDefault(u => u.Username == username);

                if (user != null)
                {
                    // Überprüfen, ob das eingegebene Passwort dem gespeicherten Hash entspricht
                    return VerifyPassword(password, user.PasswordHash);
                }
                else
                {
                    return false; // Benutzername existiert nicht
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

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            var registerView = new Register();
            registerView.Show();
            this.Close();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}