using System;
using System.IO;

namespace Oqtane.Updater.Tests
{
    internal sealed class TestDirectory : IDisposable
    {
        public string Root { get; } = Path.Combine(Path.GetTempPath(), "Oqtane.Updater.Tests", Guid.NewGuid().ToString("N"));

        public TestDirectory()
        {
            Directory.CreateDirectory(Root);
        }

        public string GetPath(string relativePath) => Path.GetFullPath(Path.Combine(Root, relativePath));

        public void WriteFile(string relativePath, string content)
        {
            string path = GetPath(relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllText(path, content);
        }

        public void Dispose()
        {
            Directory.Delete(Root, true);
        }
    }
}
