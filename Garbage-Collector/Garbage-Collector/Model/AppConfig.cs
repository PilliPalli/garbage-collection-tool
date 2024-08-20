using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;

public class AppConfig : INotifyPropertyChanged
{
    private string _searchPath;
    private List<string> _filePatterns;
    private int _olderThanDays;
    private bool _deleteDirectly;

    // Pfad relativ zum Verzeichnis der .exe-Datei definieren
    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

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

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        if (PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        SaveToJson(ConfigFilePath); // Speichere die Änderungen automatisch an den richtigen Pfad
    }

    public static AppConfig LoadFromJson(string filePath = null)
    {
        if (filePath == null)
        {
            filePath = ConfigFilePath;
        }

        if (!File.Exists(filePath))
        {
            // Datei existiert nicht, erzeuge sie mit Standardwerten
            var defaultConfig = new AppConfig
            {
                SearchPath = "C:\\",
                FilePatterns = new List<string> { "*.txt", "*.log" },
                OlderThanDays = 30,
                DeleteDirectly = false
            };
            defaultConfig.SaveToJson(filePath);
            return defaultConfig;
        }

        string jsonText = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<AppConfig>(jsonText);
    }

    public void SaveToJson(string filePath)
    {
        string jsonText = JsonConvert.SerializeObject(this, Formatting.Indented);
        File.WriteAllText(filePath, jsonText);
    }
}