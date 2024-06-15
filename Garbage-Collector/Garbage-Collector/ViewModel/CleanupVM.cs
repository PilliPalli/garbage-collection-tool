using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class CleanupVM : ViewModelBase
    {
        private string _directoryPath;
        private int _olderThanDays;
        private string _filePatterns;
        private string _statusMessage;

        public string DirectoryPath
        {
            get { return _directoryPath; }
            set { _directoryPath = value; OnPropertyChanged(); }
        }

        public int OlderThanDays
        {
            get { return _olderThanDays; }
            set { _olderThanDays = value; OnPropertyChanged(); }
        }

        public string FilePatterns
        {
            get { return _filePatterns; }
            set { _filePatterns = value; OnPropertyChanged(); }
        }

        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand CleanupCommand { get; }
        public ICommand LoadConfigCommand { get; }

        public CleanupVM()
        {
            LoadConfig("config.json");
            CleanupCommand = new RelayCommand(Cleanup);
            LoadConfigCommand = new RelayCommand(param => LoadConfig((string)param));
        }

        private void LoadConfig(string configFilePath)
        {
            try
            {
                var config = AppConfig.LoadFromJson(configFilePath);
                DirectoryPath = config.SearchPath;
                FilePatterns = string.Join(", ", config.FilePatterns);
                OlderThanDays = config.OlderThanDays;
                StatusMessage = "Konfiguration erfolgreich geladen.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler beim Laden der Konfiguration: {ex.Message}";
            }
        }

        private void Cleanup(object obj)
        {
            if (Directory.Exists(DirectoryPath))
            {
                var patterns = FilePatterns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
                foreach (var pattern in patterns)
                {
                    var files = Directory.GetFiles(DirectoryPath, pattern, System.IO.SearchOption.AllDirectories)
                        .Where(file => (DateTime.Now - File.GetCreationTime(file)).TotalDays > OlderThanDays);

                    if (files.Any())
                    {
                        foreach (var file in files)
                        {
                            try
                            {
                                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                StatusMessage = $"Verschoben: {file}";
                            }
                            catch (Exception ex)
                            {
                                StatusMessage = $"Fehler beim Verschieben von {file}: {ex.Message}";
                            }
                        }
                        StatusMessage = "Alle ausgewählten Dateien wurden in den Papierkorb verschoben.";
                    }
                    else
                    {
                        StatusMessage = "Keine Dateien gefunden, die verschoben werden müssen.";
                    }
                }
            }
            else
            {
                StatusMessage = "Der angegebene Pfad existiert nicht.";
            }
        }
    }
}