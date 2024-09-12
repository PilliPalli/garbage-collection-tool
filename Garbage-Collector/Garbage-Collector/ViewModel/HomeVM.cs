using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
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

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
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
        public ICommand ExportLogsCommand { get; }

        public HomeVM()
        {
            LoadCleanupLogs();
            WelcomeMessage = $"Willkommen, {LoginVM.CurrentUserName}!";
            ClearStatisticsCommand = new RelayCommand(ClearStatistics);
            ExportLogsCommand = new RelayCommand(ExportLogs);
            CalculateTotals();
        }

        private void LoadCleanupLogs()
        {
            using (var context = new GarbageCollectorDbContext())
            {
                var logs = context.CleanupLogs
                                              .Where(log => log.UserId == LoginVM.CurrentUserId)
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

        private void ExportLogs(object obj)
        {
            var saveFileDialog = new SaveFileDialog
            {
                FileName = "cleanup_logs",
                DefaultExt = ".csv",
                Filter = "CSV-Dateien (.csv)|*.csv"
            };

            bool? result = saveFileDialog.ShowDialog();

            if (result == true)
            {
                string exportFilePath = saveFileDialog.FileName;

                try
                {
                    using (var writer = new StreamWriter(exportFilePath))
                    {
                       
                        writer.WriteLine("Datum,Gelöschte Dateien,Freigegebener Speicher (MB),Bereinigungstyp");

                        using (var context = new GarbageCollectorDbContext())
                        {
                           
                            var userLogs = context.CleanupLogs
                                                  .Where(log => log.UserId == LoginVM.CurrentUserId)
                                                  .OrderByDescending(log => log.CleanupDate)
                                                  .ToList();

                           
                            foreach (var log in userLogs)
                            {
                                writer.WriteLine($"{log.CleanupDate};{log.FilesDeleted};{log.SpaceFreedInMb};{log.CleanupType}");
                            }
                        }
                    }

                    StatusMessage = "Logs wurden erfolgreich exportiert.";
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Fehler beim Exportieren der Logs: {ex.Message}";
                }

                OnPropertyChanged(nameof(StatusMessage));
            }
        }



        private void CalculateTotals()
        {
            using (var context = new GarbageCollectorDbContext())
            {
                
                var userLogs = context.CleanupLogs
                                      .Where(log => log.UserId == LoginVM.CurrentUserId)
                                      .ToList();

                TotalDeletedFiles = userLogs.Sum(log => log.FilesDeleted).ToString();
                TotalFreedSpace = userLogs.Sum(log => log.SpaceFreedInMb).ToString("F2") + " MB";
            }
        }

    }
}
