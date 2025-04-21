using Garbage_Collector.Model;
using System.Windows;

namespace Garbage_Collector
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                var config = AppConfig.LoadFromJson();

                using var context = new GarbageCollectorDbContext();
                if (!context.Database.CanConnect())
                {
                    MessageBox.Show("Verbindung zur Datenbank konnte nicht hergestellt werden.\nBitte überprüfen Sie den Connection String in der config.json.", "Verbindungsfehler", MessageBoxButton.OK, MessageBoxImage.Error);
                    Shutdown();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Start der Anwendung:\n{ex.Message}", "Startup-Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
                Shutdown();
            }
        }
    }
}

