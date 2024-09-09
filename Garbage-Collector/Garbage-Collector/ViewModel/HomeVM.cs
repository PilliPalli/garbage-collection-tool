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
        private string _welcomeMessage;
        private string _totalDeletedFiles;
        private string _totalFreedSpace;

        public ObservableCollection<CleanupLog> CleanupLogs
        {
            get => _cleanupLogs;
            set
            {
                _cleanupLogs = value;
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

        public string TotalDeletedFiles
        {
            get => _totalDeletedFiles;
            set
            {
                _totalDeletedFiles = value;
                OnPropertyChanged();
            }
        }

        public string TotalFreedSpace
        {
            get => _totalFreedSpace;
            set
            {
                _totalFreedSpace = value;
                OnPropertyChanged();
            }
        }

        public ICommand ClearStatisticsCommand { get; }

        public HomeVM()
        {
            LoadCleanupLogs();
            WelcomeMessage = $"Willkommen, {LoginVM.CurrentUserName}!";
            ClearStatisticsCommand = new RelayCommand(ClearStatistics);
            CalculateTotals();
        }

        private void LoadCleanupLogs()
        {
            using (var context = new GarbageCollectorDbContext())
            {
                var logs = context.CleanupLogs
                                              .OrderByDescending(log => log.CleanupDate) 
                                              .Take(5)
                                              .ToList();

                CleanupLogs = new ObservableCollection<CleanupLog>(logs);
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
            CalculateTotals();
        }

        private void CalculateTotals()
        {
            using (var context = new GarbageCollectorDbContext())
            {
                var allLogs = context.CleanupLogs.ToList();
                TotalDeletedFiles = allLogs.Sum(log => log.FilesDeleted).ToString();
                TotalFreedSpace = allLogs.Sum(log => log.SpaceFreedInMb).ToString("F2") + " MB";
            }
        }
    }
}
