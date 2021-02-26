using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Oqtane.Shared;

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

        public void InstallPackages(string folders)
        {
            if (!InstallPackages(folders, _environment.WebRootPath, _environment.ContentRootPath))
            {
                // error installing packages
            }
        }

        public static bool InstallPackages(string folders, string webRootPath, string contentRootPath)
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
                            List<string> assets = new List<string>();
                            bool manifest = false;

                            // module and theme packages must be in form of name.1.0.0.nupkg
                            string name = Path.GetFileNameWithoutExtension(packagename);
                            string[] segments = name?.Split('.');
                            if (segments != null) name = string.Join('.', segments, 0, segments.Length - 3);

                            // deploy to appropriate locations
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string foldername = Path.GetDirectoryName(entry.FullName).Split(Path.DirectorySeparatorChar)[0];
                                string filename = Path.GetFileName(entry.FullName);

                                if (!manifest && filename == "assets.json")
                                {
                                    manifest = true;
                                }

                                switch (foldername)
                                {
                                    case "lib":
                                        filename = Path.Combine(binFolder, filename);
                                        ExtractFile(entry, filename);
                                        assets.Add(filename.Replace(contentRootPath, ""));
                                        break;
                                    case "wwwroot":
                                        filename = Path.Combine(webRootPath, Utilities.PathCombine(entry.FullName.Replace("wwwroot/", "").Split('/')));
                                        ExtractFile(entry, filename);
                                        assets.Add(filename.Replace(contentRootPath, ""));
                                        break;
                                    case "runtimes":
                                        var destSubFolder = Path.GetDirectoryName(entry.FullName);
                                        filename = Path.Combine(binFolder, destSubFolder, filename);
                                        ExtractFile(entry, filename);
                                        assets.Add(filename.Replace(contentRootPath, ""));
                                        break;
                                }
                            }

                            // save dynamic list of assets
                            if (!manifest && assets.Count != 0)
                            {
                                string manifestpath = Path.Combine(webRootPath, folder, name, "assets.json");
                                if (File.Exists(manifestpath))
                                {
                                    File.Delete(manifestpath);
                                }
                                if (!Directory.Exists(Path.GetDirectoryName(manifestpath)))
                                {
                                    Directory.CreateDirectory(Path.GetDirectoryName(manifestpath));
                                }
                                File.WriteAllText(manifestpath, JsonSerializer.Serialize(assets));
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

            try
            {
                entry.ExtractToFile(filename, true);
            }
            catch
            {
                // an error occurred extracting the file
            }
        }

        public void UpgradeFramework()
        {
            string folder = Path.Combine(_environment.WebRootPath, "Framework");
            if (Directory.Exists(folder))
            {
                // get package with highest version and clean up any others
                string packagename = "";
                foreach (string package in Directory.GetFiles(folder, "Oqtane.Framework.*.nupkg"))
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
                                break;
                            }
                        }
                    }

                    // ensure package version is greater than or equal to current framework version
                    if (packageversion != "" && Version.Parse(Constants.Version).CompareTo(Version.Parse(packageversion)) <= 0)
                    {
                        FinishUpgrade();
                    }
                }
            }
        }

        private void FinishUpgrade()
        {
            // check if upgrade application exists
            string Upgrader = "Oqtane.Upgrade.dll";
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);
            if (folder == null || !File.Exists(Path.Combine(folder, Upgrader))) return;

            // run upgrade application
            using (var process = new Process())
            {
                process.StartInfo = new ProcessStartInfo
                {
                    WorkingDirectory = folder,
                    FileName = "dotnet",
                    Arguments = Path.Combine(folder, Upgrader) + " \"" + _environment.ContentRootPath + "\" \"" + _environment.WebRootPath + "\"",
                    UseShellExecute = false,
                    ErrorDialog = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = false,
                    RedirectStandardError = false
                };
                process.Start();
            };
        }

        public void RestartApplication()
        {
            _hostApplicationLifetime.StopApplication();
        }
    }
}
