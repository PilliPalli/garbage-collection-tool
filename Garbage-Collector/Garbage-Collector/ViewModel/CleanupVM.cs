using Garbage_Collector.Model;
using Garbage_Collector.Utilities;
using Microsoft.VisualBasic.FileIO;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace Garbage_Collector.ViewModel
{
    public class CleanupVM : ViewModelBase
    {
        private AppConfig _config;
        private System.Timers.Timer _cleanupTimer;
        private System.Timers.Timer _countdownTimer; 
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
        public ICommand SearchDirectoryPathCommand { get; }

        public CleanupVM()
        {
            _config = AppConfig.LoadFromJson("config.json");
            CleanupCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(CleanupAsync));
            CleanJunkFilesCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(CleanJunkFilesAsync));
            RemoveDuplicateFilesCommand = new RelayCommand(async obj => await ExecuteWithButtonDisable(RemoveDuplicateFilesAsync));
            StartSchedulerCommand = new RelayCommand(obj => StartScheduler());
            StopSchedulerCommand = new RelayCommand(obj => StopScheduler());
            SearchDirectoryPathCommand = new RelayCommand(obj => SearchDirectoryPath());
            ProgressBarVisibility = Visibility.Collapsed;

            _countdownTimer = new System.Timers.Timer(1000); 
            _countdownTimer.Elapsed += CountdownElapsed;

        }

        private void SearchDirectoryPath()
        {
            var dialog = new Microsoft.Win32.OpenFolderDialog();
            dialog.Title = "Verzeichnis für Bereinigung auswählen";

            bool? result = dialog.ShowDialog();

            if (result == true && !string.IsNullOrWhiteSpace(dialog.FolderName))
            {
                DirectoryPath = dialog.FolderName;
            }
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
            _countdownTimer.Start(); 

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
            SchedulerStatus = string.Empty;
            TimeUntilNextCleanup = string.Empty;
        }

        private async void OnCleanupTimeElapsed(object sender, ElapsedEventArgs e)
        {
            await Task.Run(() => CleanupAsync());
           
            _nextCleanupTime = DateTime.Now.AddMinutes(IntervalInMinutes); 
            _countdownTimer.Start(); 
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

            Application.Current.Dispatcher.Invoke(() =>
            {
                if (timeRemaining.TotalSeconds > 0)
                {
                    TimeUntilNextCleanup = $"{timeRemaining.Minutes:D2}:{timeRemaining.Seconds:D2}";
                }
                else
                {
                    TimeUntilNextCleanup = "00:00";
                    _countdownTimer.Stop();
                }
            });
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

        private List<string> GetFilesToProcess()
        {
            var patterns = FilePatterns.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                       .Select(p => p.Trim())
                                       .ToList();

            var filesToProcess = new List<string>();

            foreach (var pattern in patterns)
            {
                filesToProcess.AddRange(GetFilesSafely(DirectoryPath, pattern)); 
            }

            return filesToProcess;
        }

        private List<string> GetFilesSafely(string path, string pattern)
        {
            var files = new List<string>();
            var searchOption = _config.DeleteRecursively
                ? System.IO.SearchOption.AllDirectories
                : System.IO.SearchOption.TopDirectoryOnly;

            try
            {
               
                if (!pattern.Contains("^") && !pattern.Contains("$") &&
                    !pattern.Contains("[") && !pattern.Contains("]") && !pattern.Contains("\\"))
                {
                    files.AddRange(Directory.GetFiles(path, pattern, searchOption));
                }
                else
                {
                    
                    var allFiles = Directory.GetFiles(path, "*", searchOption);
                    var regex = new Regex(pattern);
                    foreach (var file in allFiles)
                    {
                        var fileName = Path.GetFileName(file);
                        if (regex.IsMatch(fileName))
                        {
                            files.Add(file);
                        }
                    }
                }
            }
            catch (UnauthorizedAccessException)
            {
                StatusMessage = $"Zugriff verweigert: {path}";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler beim Durchsuchen: {ex.Message}";
            }

            return files;
        }

        private async Task DeleteFilesAsync(List<string> filesToDelete, string operationType)
        {
            if (filesToDelete == null || !filesToDelete.Any())
            {
                StatusMessage = "Keine Dateien zum Löschen gefunden.";
                return;
            }

            ProgressBarVisibility = Visibility.Visible;
            ProgressMaximum = filesToDelete.Count;
            ProgressValue = 0;

            await Task.Run(() =>
            {
                Parallel.ForEach(filesToDelete, file =>
                {
                    if (string.IsNullOrEmpty(file))
                    {
                        return;
                    }

                    try
                    {
                        if (!IsFileLocked(file))
                        {
                            var fileInfo = new FileInfo(file);
                            if (fileInfo.Exists)
                            {
                               
                                if (_config.DeleteDirectly)
                                {
                                    fileInfo.Delete();
                                }
                                else
                                {
                                    FileSystem.DeleteFile(file, UIOption.OnlyErrorDialogs, RecycleOption.SendToRecycleBin);
                                }
                                Application.Current?.Dispatcher.Invoke(() =>
                                {
                                    ProgressValue++;
                                    StatusMessage = $"Gelöscht: {file}";
                                });
                            }
                        }
                        else
                        {
                            Application.Current?.Dispatcher.Invoke(() =>
                            {
                                StatusMessage = $"Datei gesperrt: {file}";
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Application.Current?.Dispatcher.Invoke(() =>
                        {
                            StatusMessage = $"Fehler beim Löschen der Datei {file}: {ex.Message}";
                        });
                    }

                });
            });

            ProgressBarVisibility = Visibility.Collapsed;
            StatusMessage = $"{operationType} abgeschlossen.";
        }



        public async Task CleanupAsync()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                StatusMessage = "Der angegebene Pfad existiert nicht.";
                return;
            }

            var deletionThreshold = DateTime.Now.AddDays(-OlderThanDays);
            Debug.WriteLine($"Berechneter Löschgrenzwert: {deletionThreshold}");

            var filesToDelete = await Task.Run(() => GetFilesToProcess()
                .Where(file => File.GetLastWriteTime(file) < deletionThreshold)
                .ToList());

           

            if (!filesToDelete.Any())
            {
                StatusMessage = "Keine Dateien zum Löschen gefunden.";
                return;
            }

            long totalBytes = filesToDelete.Sum(f => new FileInfo(f).Length);
            double spaceFreedInMb = totalBytes / (1024.0 * 1024.0);
            await LogCleanupAsync(filesToDelete.Count, spaceFreedInMb, "Standard");
            await DeleteFilesAsync(filesToDelete, "Löschvorgang");
          

        }

        private async Task CleanJunkFilesAsync()
        {
            string tempPath = Path.GetTempPath();
            var junkFiles = Directory.GetFiles(tempPath, "*.*", System.IO.SearchOption.AllDirectories)
                                      .Where(f => IsJunkFile(f))
                                      .ToList();

            if (!junkFiles.Any())
            {
                StatusMessage = "Keine Junk-Dateien gefunden.";
                return;
            }

            long totalBytes = junkFiles.Sum(f => new FileInfo(f).Length);
            double spaceFreedInMb = totalBytes / (1024.0 * 1024.0);
            await LogCleanupAsync(junkFiles.Count, spaceFreedInMb, "Junk");
            await DeleteFilesAsync(junkFiles, "Löschen der Junk-Dateien");
           

        }

        private bool IsJunkFile(string filePath)
        {
            string[] junkExtensions = { ".tmp", ".log", ".bak", ".old", ".dmp", ".swp" };
            return junkExtensions.Contains(Path.GetExtension(filePath).ToLower());
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


        private async Task RemoveDuplicateFilesAsync()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                StatusMessage = "Der angegebene Pfad existiert nicht.";
                return;
            }

            var fileHashes = new Dictionary<string, List<string>>();
            var filesToDelete = new List<string>();
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

                long totalBytes = filesToDelete.Sum(f => new FileInfo(f).Length);
                double spaceFreedInMb = totalBytes / (1024.0 * 1024.0);
                await LogCleanupAsync(filesToDelete.Count, spaceFreedInMb, "Duplikate");
                await DeleteFilesAsync(filesToDelete, "Löschen der Duplikate");
               

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
    }
}
