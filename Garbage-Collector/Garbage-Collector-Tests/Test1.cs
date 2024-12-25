using Garbage_Collector.Model;
using Garbage_Collector.ViewModel;
using Newtonsoft.Json;
using System.Diagnostics;
using System.IO;

namespace Garbage_Collector_Tests
{
    [TestClass]
    public class CleanupVMTests
    {
        private CleanupVM _cleanupVM;
        private string tempDir;

        [TestInitialize]
        public void Setup()
        {
            tempDir = Path.Combine(Path.GetTempPath(), "TestDir");
            Directory.CreateDirectory(tempDir);

            var config = new AppConfig
            {
                SearchPath = tempDir,
                OlderThanDays = 10,
                FilePatterns = new List<string> { "*.txt" },
                DeleteDirectly = true
            };

            File.WriteAllText("config.json", JsonConvert.SerializeObject(config));

            _cleanupVM = new CleanupVM();

            if (_cleanupVM == null)
            {
                throw new InvalidOperationException("_cleanupVM konnte nicht initialisiert werden.");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public async Task Cleanup_Removes_Old_Files_And_Preserves_Recent_Files()
        {
            // Arrange
            string oldFile = Path.Combine(tempDir, "oldFile.txt");
            string recentFile = Path.Combine(tempDir, "recentFile.txt");

            File.WriteAllText(oldFile, "Old Content");
            File.SetLastWriteTime(oldFile, DateTime.Now.AddDays(-31));

            File.WriteAllText(recentFile, "Recent Content");
            File.SetLastWriteTime(recentFile, DateTime.Now.AddDays(-10).AddSeconds(1));

            // Act
            await _cleanupVM.CleanupAsync();

            // Assert
            Assert.IsFalse(File.Exists(oldFile), "Old file should be deleted.");
            Assert.IsTrue(File.Exists(recentFile), "Recent file should remain.");
        }
    }
}
