using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class CleanupVM : ViewModelBase
    {
        private AppConfig _config;
        private System.Timers.Timer _cleanupTimer;
        private System.Timers.Timer _countdownTimer; // Countdown Timer für den nächsten Cleanup
        private bool _isSchedulerRunning;
        private int _intervalInMinutes = 30;
        private DateTime _nextCleanupTime;
        private string _schedulerStatus;
        private string _timeUntilNextCleanup;
        private string _statusMessage;
    

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
        public int IntervalInMinutes
        {
            get => _intervalInMinutes;
            set
            {
                if (_intervalInMinutes != value)
                {
                    _intervalInMinutes = value;
                    OnPropertyChanged();
                    RestartTimer();
                }
            }
        }

       
        public string SchedulerStatus
        {
            get => _schedulerStatus;
            set
            {
                if (_schedulerStatus != value)
                {
                    _schedulerStatus = value;
                    OnPropertyChanged();
                }
            }
        }

       
        public string TimeUntilNextCleanup
        {
            get => _timeUntilNextCleanup;
            set
            {
                if (_timeUntilNextCleanup != value)
                {
                    _timeUntilNextCleanup = value;
                    OnPropertyChanged();
                }
            }
        }

       
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
        public ICommand StartSchedulerCommand { get; }
        public ICommand StopSchedulerCommand { get; }

        public CleanupVM()
        {
            _config = AppConfig.LoadFromJson("config.json");
            CleanupCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(CleanupAsync));
          //  LoadConfigCommand = new RelayCommand(param => LoadConfig((string)param));
            CleanJunkFilesCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(CleanJunkFilesAsync));
            RemoveDuplicateFilesCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(RemoveDuplicateFilesAsync));
            StartSchedulerCommand = new RelayCommand(obj => StartScheduler());
            StopSchedulerCommand = new RelayCommand(obj => StopScheduler());
            ProgressBarVisibility = Visibility.Collapsed;

            _countdownTimer = new System.Timers.Timer(1000); // Jede Sekunde aktualisieren
            _countdownTimer.Elapsed += CountdownElapsed;
        }

        private void StartScheduler()
        {
            if (_isSchedulerRunning || IntervalInMinutes <= 0)
                return;

            _cleanupTimer = new System.Timers.Timer(TimeSpan.FromMinutes(IntervalInMinutes).TotalMilliseconds);
            _cleanupTimer.Elapsed += OnCleanupTimeElapsed;
            _cleanupTimer.Start();
            _isSchedulerRunning = true;

            _nextCleanupTime = DateTime.Now.AddMinutes(IntervalInMinutes);
            _countdownTimer.Start(); // Countdown starten

            SchedulerStatus = $"Scheduler läuft alle {IntervalInMinutes} Minuten";
        }

        private void StopScheduler()
        {
            if (!_isSchedulerRunning)
                return;

            _cleanupTimer.Stop();
            _cleanupTimer.Dispose();
            _countdownTimer.Stop();
            _isSchedulerRunning = false;
            SchedulerStatus = "Scheduler ist nicht aktiv";
            TimeUntilNextCleanup = string.Empty;
        }

        private async void OnCleanupTimeElapsed(object sender, ElapsedEventArgs e)
        {
            await Task.Run(() => CleanupAsync());
            Debug.WriteLine($"Scheduler DeleteDirectly: {_config.DeleteDirectly}");
            _nextCleanupTime = DateTime.Now.AddMinutes(IntervalInMinutes); // Nach dem Cleanup die nächste Zeit setzen
            _countdownTimer.Start(); // Countdown nach dem Cleanup erneut starten
        }

        private void RestartTimer()
        {
            if (_isSchedulerRunning)
            {
                StopScheduler();
                StartScheduler();
            }
        }

        private void CountdownElapsed(object sender, ElapsedEventArgs e)
        {
            TimeSpan timeRemaining = _nextCleanupTime - DateTime.Now;
            if (timeRemaining.TotalSeconds > 0)
            {
                TimeUntilNextCleanup = $"{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
            }
            else
            {
                TimeUntilNextCleanup = "00:00";
                _countdownTimer.Stop();
            }
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

        //private void LoadConfig(string configFilePath)
        //{
        //    try
        //    {
        //        _config = AppConfig.LoadFromJson(configFilePath);
        //        OnPropertyChanged(nameof(DirectoryPath));
        //        OnPropertyChanged(nameof(FilePatterns));
        //        OnPropertyChanged(nameof(OlderThanDays));
        //        StatusMessage = "Konfiguration erfolgreich geladen";
                
        //        OnPropertyChanged(nameof(StatusMessage));
        //    }
        //    catch (Exception ex)
        //    {
        //        StatusMessage = $"Fehler beim Laden der Konfiguration: {ex.Message}";
        //        OnPropertyChanged(nameof(StatusMessage));
        //    }
        //}

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

            // Berechnung des gesamten freigegebenen Speicherplatzes
            double totalFreedSpace = 0;

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
                                // Berechne Dateigröße vor dem Löschen
                                var fileInfo = new FileInfo(file);
                                totalFreedSpace += fileInfo.Length / (1024.0 * 1024.0); // Konvertiert in MB

                                if (_config.DeleteDirectly)
                                {
                                    File.Delete(file);
                                }
                                else
                                {
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

                // Logge den gesamten freigegebenen Speicher
                await LogCleanupAsync(filesToDelete.Count, totalFreedSpace, "Standard");

                StatusMessage = filesProcessed ? "Alle ausgewählten Dateien verschoben" : "Keine Dateien zum Verschieben gefunden";
            }
            else
            {
                StatusMessage = "Keine Dateien zum Verschieben gefunden.";
            }
        }




        private async Task CleanJunkFilesAsync()
        {
            string tempPath = Path.GetTempPath();
            var junkFiles = Directory.GetFiles(tempPath, "*.*", System.IO.SearchOption.AllDirectories)
                                     .Where(f => IsJunkFile(f))
                                     .ToList();

            if (junkFiles.Any())
            {
                ProgressBarVisibility = Visibility.Visible;
                ProgressMaximum = junkFiles.Count;
                ProgressValue = 0;

                await Task.Run(() =>
                {
                    Parallel.ForEach(junkFiles, file =>
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
                    });
                });

                ProgressBarVisibility = Visibility.Collapsed;
                await LogCleanupAsync(junkFiles.Count, CalculateFreedSpace(junkFiles), "Junk");
                StatusMessage = "Alle Junk-Dateien gelöscht.";
            }
            else
            {
                StatusMessage = "Keine Junk-Dateien gefunden.";
            }
        }

        private bool IsJunkFile(string filePath)
        {
            string[] junkExtensions = { ".tmp", ".log", ".bak", ".old", ".dmp", ".swp" };
            return junkExtensions.Contains(Path.GetExtension(filePath).ToLower());
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

            double totalFreedSpace = 0;

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
                            if (!IsFileLocked(file))
                            {

                                var fileInfo = new FileInfo(file);
                                totalFreedSpace += fileInfo.Length / (1024.0 * 1024.0); // Konvertiere in MB

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
                            else
                            {
                                StatusMessage = $"Zugriff verweigert: {file}";
                            }
                        }
                        catch (Exception ex)
                        {
                            StatusMessage = $"Fehler beim Löschen von {file}: {ex.Message}";
                        }
                    }
                });

                ProgressBarVisibility = Visibility.Collapsed;


                await LogCleanupAsync(filesToDelete.Count, totalFreedSpace, "Duplicates");

                StatusMessage = filesProcessed ? "Duplikate wurden entfernt." : "Keine Duplikate gefunden.";
            }
            else
            {
                StatusMessage = "Keine Duplikate gefunden.";
            }
        }


        private async Task LogCleanupAsync(int filesDeleted, double spaceFreed, string cleanupType)
        {
            using (var context = new GarbageCollectorDbContext())
            {
                var cleanupLog = new CleanupLog
                {
                    UserId = LoginVM.CurrentUserId,
                    FilesDeleted = filesDeleted,
                    SpaceFreedInMb = Math.Round(spaceFreed, 2),
                    CleanupType = cleanupType
                };

                context.CleanupLogs.Add(cleanupLog);
                await context.SaveChangesAsync();
            }
        }

        private double CalculateFreedSpace(List<string> files)
        {
            double spaceFreed = 0;

            foreach (var file in files)
            {
                if (File.Exists(file))
                {
                    var fileInfo = new FileInfo(file);
                    spaceFreed += Math.Round(fileInfo.Length / (1024.0 * 1024.0), 2);
                }
                else
                {

                    Console.WriteLine($"Datei nicht gefunden: {file}");
                }
            }

            return spaceFreed;
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
