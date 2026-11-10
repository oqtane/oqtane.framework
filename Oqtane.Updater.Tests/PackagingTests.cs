using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xunit;

namespace Oqtane.Updater.Tests
{
    public class PackagingTests
    {
        [Theory]
        [InlineData("install.ps1", "Install", false)]
        [InlineData("upgrade.ps1", "Upgrade", false)]
        [InlineData("install.ps1", "Install", true)]
        [InlineData("upgrade.ps1", "Upgrade", true)]
        public void ReleaseScriptsProducePortableArchives(string scriptName, string packageKind, bool differentWorkingDirectory)
        {
            using var directory = new TestDirectory();
            string packageFolder = directory.GetPath("Oqtane.Package");
            Directory.CreateDirectory(packageFolder);
            foreach (string script in Directory.EnumerateFiles(Path.Combine(AppContext.BaseDirectory, "PackageScripts"), "*.ps1"))
                File.Copy(script, Path.Combine(packageFolder, Path.GetFileName(script)));

            const string publish = "Oqtane.Server/bin/Release/net10.0/publish/";
            directory.WriteFile(publish + "Oqtane.Server.dll", "server");
            directory.WriteFile(publish + "runtimes/win-x64/native/e_sqlite3.dll", "native binary");
            directory.WriteFile(publish + "wwwroot/content/Članak 1.txt", "UTF-8 content");
            directory.WriteFile(publish + ".deployment", "hidden publish asset");
            if (OperatingSystem.IsWindows())
                File.SetAttributes(directory.GetPath(publish + ".deployment"), FileAttributes.Hidden);
            Directory.CreateDirectory(directory.GetPath(publish + "wwwroot/empty"));

            string workingDirectory = differentWorkingDirectory ? directory.Root : packageFolder;
            RunScript(Path.Combine(packageFolder, scriptName), workingDirectory);
            string package = Assert.Single(Directory.GetFiles(packageFolder, "Oqtane.Framework.*." + packageKind + ".zip"));
            using (var archive = ZipFile.OpenRead(package))
            {
                Assert.DoesNotContain(archive.Entries, entry => entry.FullName.Contains('\\'));
                Assert.Equal(new[] { ".deployment", "Oqtane.Server.dll", "runtimes/win-x64/native/e_sqlite3.dll", "wwwroot/content/Članak 1.txt" },
                    archive.Entries.Where(entry => !entry.FullName.EndsWith('/')).Select(entry => entry.FullName).OrderBy(name => name, StringComparer.Ordinal));
                Assert.Contains(archive.Entries, entry => entry.FullName == "wwwroot/empty/");
                using var reader = new StreamReader(archive.GetEntry("wwwroot/content/Članak 1.txt")!.Open());
                Assert.Equal("UTF-8 content", reader.ReadToEnd());
            }

            directory.WriteFile(publish + "Oqtane.Server.dll", "updated server");
            RunScript(Path.Combine(packageFolder, scriptName), workingDirectory);
            using var replacement = ZipFile.OpenRead(package);
            using var replacementReader = new StreamReader(replacement.GetEntry("Oqtane.Server.dll")!.Open());
            Assert.Equal("updated server", replacementReader.ReadToEnd());
        }

        private static void RunScript(string scriptPath, string workingDirectory)
        {
            string executable = Environment.GetEnvironmentVariable("OQTANE_TEST_POWERSHELL")
                ?? (OperatingSystem.IsWindows() ? "powershell.exe" : "pwsh");
            var startInfo = new ProcessStartInfo(executable)
            {
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            foreach (string argument in new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-File", scriptPath })
                startInfo.ArgumentList.Add(argument);
            using var process = Process.Start(startInfo)!;
            var output = process.StandardOutput.ReadToEndAsync();
            var error = process.StandardError.ReadToEndAsync();
            if (!process.WaitForExit(30000))
            {
                process.Kill(entireProcessTree: true);
                throw new TimeoutException("Packaging script did not finish within 30 seconds.");
            }
            Assert.True(process.ExitCode == 0, output.GetAwaiter().GetResult() + error.GetAwaiter().GetResult());
        }
    }
}
