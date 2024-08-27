using Garbage_Collector.Model;
using Garbage_Collector.Utilities;

namespace Garbage_Collector.ViewModel
{
    public class SettingsVM : ViewModelBase
    {
        private AppConfig _config;

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

        public SettingsVM()
        {
            _config = AppConfig.LoadFromJson(); // Lädt Konfigurationsdatei
        }
    }
}
