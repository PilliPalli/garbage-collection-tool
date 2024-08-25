using Garbage_Collector.View;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

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
            // Öffnet das Login-Fenster
            var loginWindow = new Login();
            loginWindow.Show();

            // Schließt das Hauptfenster
            this.Close();
        }
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
           
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
        private void CloseApp_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}