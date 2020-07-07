using System.Reflection;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Xml;
using Oqtane.Shared;
using System;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

namespace Oqtane.Infrastructure
{
    public class InstallationManager : IInstallationManager
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IWebHostEnvironment _environment;
        private readonly IMemoryCache _cache;

        public InstallationManager(IHostApplicationLifetime hostApplicationLifetime, IWebHostEnvironment environment, IMemoryCache cache)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _environment = environment;
            _cache = cache;
        }

        public void InstallPackages(string folders, bool restart)
        {
            var webRootPath = _environment.WebRootPath;
            
            var install = InstallPackages(folders, webRootPath);

            if (install && restart)
            {
                RestartApplication();
            }
        }

        public static bool InstallPackages(string folders, string webRootPath)
        {
            bool install = false;
            string binFolder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            foreach (string folder in folders.Split(','))
            {
                string sourceFolder = Path.Combine(webRootPath, folder);
                if (!Directory.Exists(sourceFolder))
                {
                    Directory.CreateDirectory(sourceFolder);
                }

                // iterate through Nuget packages in source folder
                foreach (string packagename in Directory.GetFiles(sourceFolder, "*.nupkg"))
                {
                    // iterate through files
                    using (ZipArchive archive = ZipFile.OpenRead(packagename))
                    {
                        string frameworkversion = "";
                        // locate nuspec
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.ToLower().EndsWith(".nuspec"))
                            {
                                // open nuspec
                                XmlTextReader reader = new XmlTextReader(entry.Open());
                                reader.Namespaces = false; // remove namespace
                                XmlDocument doc = new XmlDocument();
                                doc.Load(reader);
                                // get framework dependency
                                XmlNode node = doc.SelectSingleNode("/package/metadata/dependencies/dependency[@id='Oqtane.Framework']");
                                if (node != null)
                                {
                                    frameworkversion = node.Attributes["version"].Value;
                                }

                                reader.Close();
                            }
                        }

                        // if compatible with framework version
                        if (frameworkversion == "" || Version.Parse(Constants.Version).CompareTo(Version.Parse(frameworkversion)) >= 0)
                        {
                            // module and theme packages must be in form of name.1.0.0.nupkg
                            string name = Path.GetFileNameWithoutExtension(packagename);
                            string[] segments = name?.Split('.');
                            if (segments != null) name = string.Join('.', segments, 0, segments.Length - 3);

                            // deploy to appropriate locations
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string foldername = Path.GetDirectoryName(entry.FullName).Split(Path.DirectorySeparatorChar)[0];
                                string filename = Path.GetFileName(entry.FullName);

                                switch (foldername)
                                {
                                    case "lib":
                                        filename = Path.Combine(binFolder, filename);
                                        ExtractFile(entry, filename);
                                        break;
                                    case "wwwroot":
                                        filename = Path.Combine(webRootPath, Utilities.PathCombine(entry.FullName.Replace($"wwwroot{Path.DirectorySeparatorChar}", "").Split(Path.DirectorySeparatorChar)));
                                        ExtractFile(entry, filename);
                                        break;
                                }
                            }
                        }
                    }

                    // remove package
                    File.Delete(packagename);
                    install = true;
                }
            }

            return install;
        }

        private static void ExtractFile(ZipArchiveEntry entry, string filename)
        {
            if (!Directory.Exists(Path.GetDirectoryName(filename)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
            }
            entry.ExtractToFile(filename, true);
        }

        public void UpgradeFramework()
        {
            string folder = Path.Combine(_environment.WebRootPath, "Framework");
            if (Directory.Exists(folder))
            {
                // get package with highest version and clean up any others
                string packagename = "";
                foreach(string package in Directory.GetFiles(folder, "Oqtane.Framework.*.nupkg"))
                {
                    if (packagename != "")
                    {
                        File.Delete(packagename);
                    }
                    packagename = package;
                }

                if (packagename != "")
                {
                    // verify package version
                    string packageversion = "";
                    using (ZipArchive archive = ZipFile.OpenRead(packagename))
                    {
                        // locate nuspec
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (entry.FullName.ToLower().EndsWith(".nuspec"))
                            {
                                // open nuspec
                                XmlTextReader reader = new XmlTextReader(entry.Open());
                                reader.Namespaces = false; // remove namespace
                                XmlDocument doc = new XmlDocument();
                                doc.Load(reader);
                                // get framework version
                                XmlNode node = doc.SelectSingleNode("/package/metadata/version");
                                if (node != null)
                                {
                                    packageversion = node.InnerText;
                                }
                                reader.Close();
                            }
                        }
                    }

                    // ensure package version is higher than current framework version
                    if (packageversion != "" && Version.Parse(Constants.Version).CompareTo(Version.Parse(packageversion)) < 0)
                    {
                        FinishUpgrade();
                    }
                }
            }
        }

        private void FinishUpgrade()
        {
            // check if upgrade application exists
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (folder == null || !File.Exists(Path.Combine(folder, "Oqtane.Upgrade.exe"))) return;

            // run upgrade application
            var process = new Process
            {
                StartInfo =
                {
                    FileName = Path.Combine(folder, "Oqtane.Upgrade.exe"),
                    Arguments = "\"" + _environment.ContentRootPath + "\" \"" + _environment.WebRootPath + "\"",
                    ErrorDialog = false,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                }
            };
            process.Start();
            process.Dispose();

            // stop application so upgrade application can proceed
            RestartApplication();
        }

        public void RestartApplication()
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}
