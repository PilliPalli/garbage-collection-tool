using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Microsoft.VisualBasic.FileIO;
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
        private string _configFilePath = "config.json";

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
        public ICommand SaveConfigCommand { get; }
        public ICommand CleanJunkFilesCommand { get; }

        public CleanupVM()
        {
            LoadConfig(_configFilePath);
            CleanupCommand = new RelayCommand(async obj => await CleanupAsync());
            LoadConfigCommand = new RelayCommand(param => LoadConfig((string)param));
            SaveConfigCommand = new RelayCommand(param => SaveConfig(_configFilePath));
            CleanJunkFilesCommand = new RelayCommand(async obj => await CleanJunkFilesAsync());
            ProgressBarVisibility = Visibility.Collapsed;
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

        private void SaveConfig(string configFilePath)
        {
            try
            {
                var config = new AppConfig
                {
                    SearchPath = DirectoryPath,
                    FilePatterns = FilePatterns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList(),
                    OlderThanDays = OlderThanDays,
                };
                config.SaveToJson(configFilePath);
                StatusMessage = "Konfiguration erfolgreich gespeichert.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler beim Speichern der Konfiguration: {ex.Message}";
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

                    await Task.Run(() =>
                    {
                        foreach (var file in filesToDelete)
                        {
                            try
                            {
                                if (!IsFileLocked(file))
                                {
                                    FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                    ProgressValue++;
                                    StatusMessage = $"Verschoben: {file}";
                                }
                                else
                                {
                                    StatusMessage = $"Datei in Verwendung: {file}";
                                }
                            }
                            catch (UnauthorizedAccessException)
                            {
                                StatusMessage = $"Zugriff verweigert: {file}";
                            }
                            catch (Exception ex)
                            {
                                StatusMessage = $"Fehler beim Verschieben von {file}: {ex.Message}";
                            }
                        }
                    });

                    ProgressBarVisibility = Visibility.Collapsed;
                    StatusMessage = "Alle ausgewählten Dateien wurden verschoben.";
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

        private async Task CleanJunkFilesAsync()
        {
            string tempPath = Path.GetTempPath();
            var junkFiles = Directory.GetFiles(tempPath, "*.*", System.IO.SearchOption.AllDirectories)
                                     .Where(f => Path.GetExtension(f).Equals(".tmp") ||
                                                 Path.GetExtension(f).Equals(".log") ||
                                                 Path.GetExtension(f).Equals(".bak")).ToList();

            if (junkFiles.Any())
            {
                ProgressBarVisibility = Visibility.Visible;
                ProgressMaximum = junkFiles.Count;
                ProgressValue = 0;

                await Task.Run(() =>
                {
                    foreach (var file in junkFiles)
                    {
                        try
                        {
                            if (!IsFileLocked(file))
                            {
                                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                ProgressValue++;
                                StatusMessage = $"Gelöscht: {file}";
                            }
                            else
                            {
                                StatusMessage = $"Datei in Verwendung: {file}";
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            StatusMessage = $"Zugriff verweigert: {file}";
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"Fehler beim Löschen von {file}: {ex.Message}";
                        }
                    }
                });

                ProgressBarVisibility = Visibility.Collapsed;
                StatusMessage = "Junk-Dateien wurden gelöscht.";
            }
            else
            {
                StatusMessage = "Keine Junk-Dateien gefunden.";
            }
        }

        private bool IsFileLocked(string filePath)
        {
            try
            {
                using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    stream.Close();
                }
            }
            catch (IOException)
            {
                return true;
            }
            return false;
        }
    }
}