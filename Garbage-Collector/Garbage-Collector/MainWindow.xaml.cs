using Garbage_Collector.View;
using System.Windows;

namespace Garbage_Collector
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

        }
        private void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            // Öffne das Login-Fenster
            var loginWindow = new Login();
            loginWindow.Show();

            // Schließe das Hauptfenster
            this.Close();
        }

        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}