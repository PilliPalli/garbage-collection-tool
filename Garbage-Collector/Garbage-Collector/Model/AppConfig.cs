using Newtonsoft.Json;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Garbage_Collector.Model
{
    public class AppConfig : INotifyPropertyChanged
    {
        private string _searchPath;
        private List<string> _filePatterns;
        private int _olderThanDays;
        private bool _deleteDirectly;
        private bool _deleteRecursively;
        private string _connectionString;
       
        public static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

        public string SearchPath
        {
            get
            {
                return _searchPath;
            }
            set
            {
                if (_searchPath != value)
                {
                    _searchPath = value;
                    OnPropertyChanged();
                }
            }
        }

        public List<string> FilePatterns
        {
            get
            {
                return _filePatterns;
            }
            set
            {
                if (_filePatterns != value)
                {
                    _filePatterns = value;
                    OnPropertyChanged();
                }
            }
        }

        public int OlderThanDays
        {
            get
            {
                return _olderThanDays;
            }
            set
            {
                if (_olderThanDays != value)
                {
                    _olderThanDays = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool DeleteDirectly
        {
            get
            {
                return _deleteDirectly;
            }
            set
            {
                if (_deleteDirectly != value)
                {
                    _deleteDirectly = value;
                    OnPropertyChanged();
                }
            }
        }
        public bool DeleteRecursively
        {
            get
            {
                return _deleteRecursively;
            }
            set
            {
                if (_deleteRecursively != value)
                {
                    _deleteRecursively = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (_connectionString != value)
                {
                    _connectionString = value;
                    OnPropertyChanged();
                }

            }
        }



        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

            SaveToJson(ConfigFilePath); 
        }

        public static AppConfig LoadFromJson(string filePath = null)
        {
            if (filePath == null)
            {
                filePath = ConfigFilePath;
            }

            try
            {
                if (!File.Exists(filePath))
                {
                    var defaultConfig = new AppConfig
                    {
                        SearchPath = "C:\\",
                        FilePatterns = new List<string> { "*.txt", "*.log" },
                        OlderThanDays = 30,
                        DeleteDirectly = false,
                        DeleteRecursively = false,
                        ConnectionString = "Data Source=;Initial Catalog=GarbageCollectorDB;Encrypt=False;"
                    };

                    defaultConfig.SaveToJson(filePath);
                    return defaultConfig;
                }

                string jsonText = File.ReadAllText(filePath);
                var config = JsonConvert.DeserializeObject<AppConfig>(jsonText);

                if (config == null)
                    throw new JsonException("Deserialized config is null.");

                return config;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Die Datei 'config.json' ist beschädigt oder ungültig.\n" +
                    "Bitte korrigieren Sie sie und starten Sie das Programm neu.\n\n" +
                    $"Technische Info: {ex.Message}",
                    "Konfigurationsfehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );

                Environment.Exit(1);
                return null;
            }
        }


        public void SaveToJson(string filePath)
        {
            string jsonText = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, jsonText);
           
        }
    }
}
