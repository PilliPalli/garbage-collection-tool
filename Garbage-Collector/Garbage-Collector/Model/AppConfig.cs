using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace Garbage_Collector.Model
{
    public class AppConfig
    {
        public string SearchPath { get; set; }
        public List<string> FilePatterns { get; set; }
        public int OlderThanDays { get; set; }

        public static AppConfig LoadFromJson(string filePath)
        {
            // Stelle sicher, dass der Pfad relativ zum Ausführungsverzeichnis der Anwendung ist
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = Path.Combine(basePath, filePath);
            string jsonText = File.ReadAllText(fullPath);
            return JsonConvert.DeserializeObject<AppConfig>(jsonText);
        }

    }
}