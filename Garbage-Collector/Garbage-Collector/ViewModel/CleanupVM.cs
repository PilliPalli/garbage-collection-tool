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
            get => _config?.SearchPath ?? string.Empty;
            set
            {
                if (_config != null && _config.SearchPath != value)
                {
                    _config.SearchPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public int OlderThanDays
        {
            get => _config?.OlderThanDays ?? 0;
            set
            {
                if (_config != null && _config.OlderThanDays != value)
                {
                    _config.OlderThanDays = value;
                    OnPropertyChanged();
                }
            }
        }

        public string FilePatterns
        {
            get => _config != null ? string.Join(", ", _config.FilePatterns) : string.Empty;
            set
            {
                if (_config != null)
                {
                    var patterns = value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                        .Select(p => p.Trim())
                                        .ToList();
                    if (!_config.FilePatterns.SequenceEqual(patterns))
                    {
                        _config.FilePatterns = patterns;
                        OnPropertyChanged();
                    }
                }
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

        private Visibility _progressBarVisibility = Visibility.Collapsed;
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set
            {
                if (_progressBarVisibility != value)
                {
                    _progressBarVisibility = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _progressValue;
        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                if (_progressValue != value)
                {
                    _progressValue = value;
                    OnPropertyChanged();
                }
            }
        }

        private int _progressMaximum;
        public int ProgressMaximum
        {
            get => _progressMaximum;
            set
            {
                if (_progressMaximum != value)
                {
                    _progressMaximum = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _areButtonsEnabled = true;
        public bool AreButtonsEnabled
        {
            get => _areButtonsEnabled;
            set
            {
                if (_areButtonsEnabled != value)
                {
                    _areButtonsEnabled = value;
                    OnPropertyChanged();
                }
            }
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
                StatusMessage = "Konfiguration erfolgreich geladen";
                OnPropertyChanged(nameof(StatusMessage));
            }
            catch (Exception ex)
            {
               
                StatusMessage = $"Fehler beim Laden der Konfiguration: {ex.Message}";
                OnPropertyChanged(nameof(StatusMessage));
            }
        }

        private List<string> GetFilesSafely(string path, string pattern)
        {
            var files = new List<string>();

            try
            {
                files.AddRange(Directory.GetFiles(path, pattern, System.IO.SearchOption.TopDirectoryOnly));

                foreach (var directory in Directory.GetDirectories(path))
                {
                    try
                    {
                        files.AddRange(GetFilesSafely(directory, pattern));
                    }
                    catch (UnauthorizedAccessException)
                    {
                        StatusMessage = $"Zugriff verweigert: {directory}";
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = $"Zugriff verweigert: {path}";
            }

            return files;
        }

        private async Task CleanupAsync()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                StatusMessage = "Der angegebene Pfad existiert nicht.";
                return;
            }

            var patterns = FilePatterns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(p => p.Trim())
                                       .ToList();
            var filesToDelete = new List<string>();
            bool filesProcessed = false;

            await Task.Run(() =>
            {
                foreach (var pattern in patterns)
                {
                    var files = GetFilesSafely(DirectoryPath, pattern);
                    filesToDelete.AddRange(files.Where(file => (DateTime.Now - File.GetCreationTime(file)).TotalDays > OlderThanDays));
                }
            });

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
                                if (_config.DeleteDirectly)
                                {
                                    // Datei direkt löschen
                                    File.Delete(file);
                                }
                                else
                                {
                                    // Datei in den Papierkorb verschieben
                                    FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                }

                                ProgressValue++;
                                StatusMessage = $"Verschoben: {file}";
                                filesProcessed = true;
                            }
                            else
                            {
                                StatusMessage = $"Zugriff verweigert: {file}";
                            }
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"Fehler beim Verschieben von {file}: {ex.Message}";
                        }
                    }
                });

                ProgressBarVisibility = Visibility.Collapsed;
                StatusMessage = filesProcessed ? "Alle ausgewählten Dateien verschoben" : "Keine Dateien zum Verschieben gefunden";
            }
            else
            {
                StatusMessage = "Keine Dateien zum Verschieben gefunden";
            }
        }

        private async Task CleanJunkFilesAsync()
        {
            string tempPath = Path.GetTempPath();
            var junkFiles = Directory.GetFiles(tempPath, "*.*", System.IO.SearchOption.AllDirectories)
                                     .Where(f => Path.GetExtension(f).ToLower() == ".tmp" ||
                                                 Path.GetExtension(f).ToLower() == ".log" ||
                                                 Path.GetExtension(f).ToLower() == ".bak")
                                     .ToList();

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
                                if (_config.DeleteDirectly)
                                {
                                    File.Delete(file);
                                }
                                else
                                {
                                    FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                }

                                ProgressValue++;
                                StatusMessage = $"Gelöscht: {file}";
                            }
                            else
                            {
                                StatusMessage = $"Zugriff verweigert: {file}";
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            StatusMessage = $"Zugriff verweigert: {file}";
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"Fehler beim Löschen {file}: {ex.Message}";
                        }
                    }
                });

                ProgressBarVisibility = Visibility.Collapsed;
                StatusMessage = "Alle Junk-Dateien gelöscht.";
            }
            else
            {
                StatusMessage = "Keine Junk-Dateien gefunden.";
            }
        }

        private async Task RemoveDuplicateFilesAsync()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                StatusMessage = "Der angegebene Pfad existiert nicht.";
                return;
            }

            var fileHashes = new Dictionary<string, List<string>>();
            var filesToDelete = new List<string>();
            bool filesProcessed = false;

            var patterns = FilePatterns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(p => p.Trim())
                                       .ToList();

            await Task.Run(() =>
            {
                try
                {
                    foreach (var pattern in patterns)
                    {
                        var files = GetFilesSafely(DirectoryPath, pattern);

                        foreach (var file in files)
                        {
                            try
                            {
                                string fileHash = ComputeFileHash(file);

                                if (!fileHashes.ContainsKey(fileHash))
                                {
                                    fileHashes[fileHash] = new List<string>();
                                }
                                fileHashes[fileHash].Add(file);
                            }
                            catch (Exception ex)
                            {
                                StatusMessage = $"Fehler beim Verarbeiten von {file}: {ex.Message}";
                            }
                        }
                    }

                    foreach (var hash in fileHashes.Keys)
                    {
                        var duplicateFiles = fileHashes[hash];
                        if (duplicateFiles.Count > 1)
                        {
                            for (int i = 1; i < duplicateFiles.Count; i++)
                            {
                                filesToDelete.Add(duplicateFiles[i]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    StatusMessage = $"Fehler beim Durchsuchen des Verzeichnisses: {ex.Message}";
                }
            });

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
                            if (_config.DeleteDirectly)
                            {
                                File.Delete(file);
                            }
                            else
                            {
                                FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                            }

                            ProgressValue++;
                            filesProcessed = true;
                            StatusMessage = $"Duplikat gelöscht: {file}";
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"Fehler beim Löschen von {file}: {ex.Message}";
                        }
                    }
                });

                ProgressBarVisibility = Visibility.Collapsed;
                StatusMessage = filesProcessed ? "Duplikate wurden entfernt." : "Keine Duplikate gefunden.";
            }
            else
            {
                StatusMessage = "Keine Duplikate gefunden.";
            }
        }



        private string ComputeFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filePath))
                {
                    byte[] hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLower();
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
