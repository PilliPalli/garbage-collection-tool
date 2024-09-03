using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class HomeVM : ViewModelBase
    {
        private ObservableCollection<CleanupLog> _cleanupLogs;
        private CleanupLog _selectedCleanupLog;
        private string _welcomeMessage;

        public ObservableCollection<CleanupLog> CleanupLogs
        {
            get => _cleanupLogs;
            set
            {
                _cleanupLogs = value;
                OnPropertyChanged();
            }
        }

        public CleanupLog SelectedCleanupLog
        {
            get => _selectedCleanupLog;
            set
            {
                _selectedCleanupLog = value;
                OnPropertyChanged();
            }
        }

        public string WelcomeMessage
        {
            get => _welcomeMessage;
            set
            {
                _welcomeMessage = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClearStatisticsCommand { get; }

        public HomeVM()
        {
            LoadCleanupLogs();
            WelcomeMessage = $"Willkommen, {LoginVM.CurrentUserName}!";
            ClearStatisticsCommand = new RelayCommand(ClearStatistics);
        }

        private void LoadCleanupLogs()
        {
            using (var context = new GarbageCollectorDbContext())
            {
                CleanupLogs = new ObservableCollection<CleanupLog>(context.CleanupLogs.OrderByDescending(log => log.CleanupDate).ToList());
            }
        }

        private void ClearStatistics(object obj)
        {
            using (var context = new GarbageCollectorDbContext())
            {
                context.CleanupLogs.RemoveRange(context.CleanupLogs);
                context.SaveChanges();
            }
            CleanupLogs.Clear();
        }
    }
}
