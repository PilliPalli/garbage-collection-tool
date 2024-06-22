using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class CleanupVM : ViewModelBase
    {
        private string _directoryPath;
        private int _olderThanDays;
        private string _filePatterns;
        private string _statusMessage;
        private Visibility _progressBarVisibility;
        private int _progressValue;
        private int _progressMaximum;

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

        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set { _progressBarVisibility = value; OnPropertyChanged(); }
        }

        public int ProgressValue
        {
            get { return _progressValue; }
            set { _progressValue = value; OnPropertyChanged(); }
        }

        public int ProgressMaximum
        {
            get { return _progressMaximum; }
            set { _progressMaximum = value; OnPropertyChanged(); }
        }

        public ICommand CleanupCommand { get; }
        public ICommand LoadConfigCommand { get; }

        public CleanupVM()
        {
            LoadConfig("config.json");
            CleanupCommand = new RelayCommand(async obj => await CleanupAsync());
            LoadConfigCommand = new RelayCommand(param => LoadConfig((string)param));
            ProgressBarVisibility = Visibility.Collapsed; // Initial visibility of ProgressBar is collapsed
        }

        private void LoadConfig(string configFilePath)
        {
            try
            {
                string fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFilePath);
                var config = AppConfig.LoadFromJson(fullPath);
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

        private async Task CleanupAsync()
        {
            if (Directory.Exists(DirectoryPath))
            {
                var patterns = FilePatterns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
                var filesToDelete = new List<string>();

                foreach (var pattern in patterns)
                {
                    filesToDelete.AddRange(Directory.GetFiles(DirectoryPath, pattern, System.IO.SearchOption.AllDirectories)
                        .Where(file => (DateTime.Now - File.GetCreationTime(file)).TotalDays > OlderThanDays));
                }

                if (filesToDelete.Any())
                {
                    ProgressBarVisibility = Visibility.Visible;
                    ProgressMaximum = filesToDelete.Count;
                    ProgressValue = 0;

                    foreach (var file in filesToDelete)
                    {
                        try
                        {
                            await Task.Run(() => FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin));
                            ProgressValue++;
                            StatusMessage = $"Verschoben: {file}";
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"Fehler beim Verschieben von {file}: {ex.Message}";
                        }
                    }

                    ProgressBarVisibility = Visibility.Collapsed;
                    StatusMessage = "Alle ausgewählten Dateien wurden in den Papierkorb verschoben.";
                }
                else
                {
                    StatusMessage = "Keine Dateien gefunden, die verschoben werden müssen.";
                }
            }
            else
            {
                StatusMessage = "Der angegebene Pfad existiert nicht.";
            }
        }
    }
}