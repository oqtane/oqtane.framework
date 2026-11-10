using System;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text;
using Xunit;

namespace Oqtane.Updater.Tests
{
    public class UpgradeTests
    {
        private const string PackageName = "Oqtane.Framework.10.3.1.Upgrade.zip";
        private const string NativePath = "runtimes/win-x64/native/e_sqlite3.dll";

        [Theory]
        [InlineData('/', false)]
        [InlineData('/', true)]
        [InlineData('\\', false)]
        [InlineData('\\', true)]
        public void UpgradeExtractsNestedFilesAndBacksUpTheSamePaths(char separator, bool trailingSeparator)
        {
            using var directory = CreateApplication();
            directory.WriteFile("app/" + NativePath, "old native binary");
            CreatePackage(directory,
                ("runtimes/win-x64/native/".Replace('/', separator), null),
                (NativePath.Replace('/', separator), "new native binary"),
                ("wwwroot/css/site.css".Replace('/', separator), "new stylesheet"),
                ("wwwroot/content/Članak 1.txt".Replace('/', separator), "UTF-8 content"));

            RunUpdater(directory, trailingSeparator);

            Assert.Equal("new native binary", File.ReadAllText(directory.GetPath("app/" + NativePath)));
            Assert.Equal("new stylesheet", File.ReadAllText(directory.GetPath("app/wwwroot/css/site.css")));
            Assert.Equal("UTF-8 content", File.ReadAllText(directory.GetPath("app/wwwroot/content/Članak 1.txt")));
            Assert.DoesNotContain(Directory.EnumerateFileSystemEntries(directory.GetPath("app"), "*", SearchOption.AllDirectories),
                path => Path.GetFileName(path).Contains('\\'));
            Assert.Contains("Copy File: " + directory.GetPath("app/Backup/" + NativePath), ReadLog(directory));
            Assert.False(Directory.Exists(directory.GetPath("app/Backup")));
            Assert.False(File.Exists(directory.GetPath("app/Packages/" + PackageName)));
            Assert.False(File.Exists(directory.GetPath("app/app_offline.htm")));
        }

        [Theory]
        [InlineData('/')]
        [InlineData('\\')]
        public void FailedExtractionRestoresNestedFilesAndRemovesNewFiles(char separator)
        {
            using var directory = CreateApplication();
            directory.WriteFile("app/" + NativePath, "original native binary");
            CreatePackage(directory,
                (NativePath.Replace('/', separator), "replacement"),
                ("wwwroot/new.txt".Replace('/', separator), "new file"),
                ("broken.bin", "unreadable compressed entry"));
            CorruptCompressionMethod(directory.GetPath("app/Packages/" + PackageName), "broken.bin");

            RunUpdater(directory, trailingSeparator: true);

            Assert.Equal("original native binary", File.ReadAllText(directory.GetPath("app/" + NativePath)));
            Assert.False(File.Exists(directory.GetPath("app/wwwroot/new.txt")));
            Assert.False(File.Exists(directory.GetPath("app/broken.bin")));
            Assert.False(Directory.Exists(directory.GetPath("app/Backup")));
            Assert.Contains("Restoring Files From Backup Folder", ReadLog(directory));
            Assert.Contains("Restore File: " + directory.GetPath("app/Backup/" + NativePath), ReadLog(directory));
        }

        [Theory]
        [InlineData("../escaped.txt")]
        [InlineData("..\\escaped.txt")]
        [InlineData("<absolute>")]
        [InlineData("subdir/../existing.txt")]
        public void InvalidEntryIsRejectedBeforeBackupOrExtraction(string entryName)
        {
            using var directory = CreateApplication();
            if (entryName == "<absolute>")
                entryName = directory.GetPath("escaped.txt");
            directory.WriteFile("app/existing.txt", "original");
            directory.WriteFile("app/Backup/keep.txt", "previous backup");
            CreatePackage(directory, ("existing.txt", "replacement"), (entryName, "outside application"));

            RunUpdater(directory);

            Assert.Equal("original", File.ReadAllText(directory.GetPath("app/existing.txt")));
            Assert.Equal("previous backup", File.ReadAllText(directory.GetPath("app/Backup/keep.txt")));
            Assert.False(File.Exists(directory.GetPath("escaped.txt")));
            Assert.Contains("Error Reading Upgrade Package", ReadLog(directory));
            Assert.DoesNotContain("Copy File:", ReadLog(directory));
        }

        private static TestDirectory CreateApplication()
        {
            var directory = new TestDirectory();
            directory.WriteFile("app/wwwroot/app_offline.bak", "[PROGRESS] [STATUS] [PROGRESSCLASS]");
            Directory.CreateDirectory(directory.GetPath("app/Packages"));
            return directory;
        }

        private static void RunUpdater(TestDirectory directory, bool trailingSeparator = false)
        {
            string root = directory.GetPath("app") + (trailingSeparator ? Path.DirectorySeparatorChar : "");
            Assembly.Load("Oqtane.Updater").EntryPoint!.Invoke(null,
                new object[] { new[] { root, directory.GetPath("app/wwwroot"), "true" } });
        }

        private static void CreatePackage(TestDirectory directory, params (string Name, string? Content)[] entries)
        {
            using var archive = ZipFile.Open(directory.GetPath("app/Packages/" + PackageName), ZipArchiveMode.Create);
            foreach (var item in entries)
            {
                var entry = archive.CreateEntry(item.Name);
                if (item.Content != null)
                {
                    using var writer = new StreamWriter(entry.Open());
                    writer.Write(item.Content);
                }
            }
        }

        private static string ReadLog(TestDirectory directory) =>
            File.ReadAllText(directory.GetPath("app/Packages/" + Path.ChangeExtension(PackageName, ".log")));

        private static void CorruptCompressionMethod(string path, string entryName)
        {
            // Keep the directory readable so backup succeeds, then fail when the last entry is opened.
            byte[] bytes = File.ReadAllBytes(path);
            for (int offset = 0; offset <= bytes.Length - 46; offset++)
            {
                if (BitConverter.ToUInt32(bytes, offset) != 0x02014b50)
                    continue;
                int nameLength = BitConverter.ToUInt16(bytes, offset + 28);
                if (Encoding.UTF8.GetString(bytes, offset + 46, nameLength) != entryName)
                    continue;
                bytes[offset + 10] = 99; // Unsupported ZIP compression method.
                bytes[offset + 11] = 0;
                File.WriteAllBytes(path, bytes);
                return;
            }
            throw new InvalidOperationException("Test entry was not found in the ZIP central directory.");
        }
    }
}
