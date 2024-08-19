using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Microsoft.VisualBasic.FileIO;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class CleanupVM : ViewModelBase
    {
        private AppConfig _config;

        public string DirectoryPath
        {
            get
            {
                return _config?.SearchPath ?? string.Empty;
            }
            set
            {
                if (_config != null)
                {
                    _config.SearchPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public int OlderThanDays
        {
            get
            {
                return _config?.OlderThanDays ?? 0;
            }
            set
            {
                if (_config != null)
                {
                    _config.OlderThanDays = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FilePatterns
        {
            get
            {
                return _config != null ? string.Join(", ", _config.FilePatterns) : string.Empty;
            }
            set
            {
                if (_config != null)
                {
                    _config.FilePatterns = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(p => p.Trim())
                                                .ToList();
                    OnPropertyChanged();
                }
            }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set { _progressBarVisibility = value; OnPropertyChanged(); }
        }

        private int _progressValue;
        public int ProgressValue
        {
            get { return _progressValue; }
            set { _progressValue = value; OnPropertyChanged(); }
        }

        private int _progressMaximum;
        public int ProgressMaximum
        {
            get { return _progressMaximum; }
            set { _progressMaximum = value; OnPropertyChanged(); }
        }

        private bool _areButtonsEnabled = true;
        public bool AreButtonsEnabled
        {
            get { return _areButtonsEnabled; }
            set { _areButtonsEnabled = value; OnPropertyChanged(); }
        }

        public ICommand CleanupCommand { get; }
        public ICommand LoadConfigCommand { get; }
        public ICommand CleanJunkFilesCommand { get; }
        public ICommand RemoveDuplicateFilesCommand { get; }

        public CleanupVM()
        {
            _config = AppConfig.LoadFromJson("config.json");
            CleanupCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(CleanupAsync));
            LoadConfigCommand = new RelayCommand(param => LoadConfig((string)param));
            CleanJunkFilesCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(CleanJunkFilesAsync));
            RemoveDuplicateFilesCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(RemoveDuplicateFilesAsync));
            ProgressBarVisibility = Visibility.Collapsed;
        }

        private async Task ExecuteWithButtonDisable(Func<Task> action)
        {
            AreButtonsEnabled = false;
            try
            {
                await action();
            }
            finally
            {
                AreButtonsEnabled = true;
            }
        }

        private void LoadConfig(string configFilePath)
        {
            try
            {
                _config = AppConfig.LoadFromJson(configFilePath);
                OnPropertyChanged(nameof(DirectoryPath));
                OnPropertyChanged(nameof(FilePatterns));
                OnPropertyChanged(nameof(OlderThanDays));
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

                await Task.Run((Action)(() =>
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
                }));

                ProgressBarVisibility = Visibility.Collapsed;
                StatusMessage = "Junk-Dateien wurden gelöscht.";
            }
            else
            {
                StatusMessage = "Keine Junk-Dateien gefunden.";
            }
        }

        private async Task RemoveDuplicateFilesAsync()
        {
            if (Directory.Exists(DirectoryPath))
            {
                var files = Directory.GetFiles(DirectoryPath, "*.*", System.IO.SearchOption.AllDirectories);
                var fileHashes = new Dictionary<string, List<string>>();

                ProgressBarVisibility = Visibility.Visible;
                ProgressMaximum = files.Length;
                ProgressValue = 0;

                bool duplicatesFound = false; // Variable to track if duplicates were found

                await Task.Run((Action)(() =>
                {
                    foreach (var file in files)
                    {
                        try
                        {
                            if (!IsFileLocked(file))
                            {
                                string fileHash = ComputeFileHash(file);

                                if (!fileHashes.ContainsKey(fileHash))
                                {
                                    fileHashes[fileHash] = new List<string>();
                                }
                                fileHashes[fileHash].Add(file);

                                ProgressValue++;
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
                            StatusMessage = $"Fehler beim Verarbeiten von {file}: {ex.Message}";
                        }
                    }

                    ProgressValue = 0;
                    ProgressMaximum = fileHashes.Where(kvp => kvp.Value.Count > 1).Sum(kvp => kvp.Value.Count - 1);

                    foreach (var hash in fileHashes.Keys)
                    {
                        var duplicateFiles = fileHashes[hash];
                        if (duplicateFiles.Count > 1)
                        {
                            duplicatesFound = true; // Set to true if duplicates are found

                            for (int i = 1; i < duplicateFiles.Count; i++)
                            {
                                try
                                {
                                    FileSystem.DeleteFile(duplicateFiles[i], UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                    StatusMessage = $"Duplikat gelöscht: {duplicateFiles[i]}";
                                    ProgressValue++;
                                }
                                catch (UnauthorizedAccessException)
                                {
                                    StatusMessage = $"Zugriff verweigert: {duplicateFiles[i]}";
                                }
                                catch (Exception ex)
                                {
                                    StatusMessage = $"Fehler beim Löschen von {duplicateFiles[i]}: {ex.Message}";
                                }
                            }
                        }
                    }
                }));

                ProgressBarVisibility = Visibility.Collapsed;

                if (duplicatesFound)
                {
                    StatusMessage = "Duplikate wurden entfernt.";
                }
                else
                {
                    StatusMessage = "Keine Duplikate gefunden.";
                }
            }
            else
            {
                StatusMessage = "Der angegebene Pfad existiert nicht.";
            }
        }


        private static string ComputeFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLower();
                }
            }
        }

        private static bool IsFileLocked(string filePath)
        {
            try
            {
                using (var stream = File.Open(filePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
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