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
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Die Datei {filePath} wurde nicht gefunden.");

            string jsonText = File.ReadAllText(filePath);
            return JsonConvert.DeserializeObject<AppConfig>(jsonText);
        }

        public void SaveToJson(string filePath)
        {
            string jsonText = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText(filePath, jsonText);
        }
    }
}