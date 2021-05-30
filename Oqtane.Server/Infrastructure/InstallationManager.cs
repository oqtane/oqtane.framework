using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Oqtane.Shared;
// ReSharper disable AssignNullToNotNullAttribute

namespace Oqtane.Infrastructure
{
    public class InstallationManager : IInstallationManager
    {
        private readonly IHostApplicationLifetime _hostApplicationLifetime;
        private readonly IWebHostEnvironment _environment;

        public InstallationManager(IHostApplicationLifetime hostApplicationLifetime, IWebHostEnvironment environment)
        {
            _hostApplicationLifetime = hostApplicationLifetime;
            _environment = environment;
        }

        public void InstallPackages()
        {
            if (!InstallPackages(_environment.WebRootPath, _environment.ContentRootPath))
            {
                // error installing packages
            }
        }

        public static bool InstallPackages(string webRootPath, string contentRootPath)
        {
            bool install = false;
            string binPath = Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location);

            string sourceFolder = Path.Combine(contentRootPath, "Packages");
            if (!Directory.Exists(sourceFolder))
            {
                Directory.CreateDirectory(sourceFolder);
            }

            // move packages to secure /Packages folder
            foreach (var folder in "Modules,Themes,Packages".Split(","))
            {
                foreach(var file in Directory.GetFiles(Path.Combine(webRootPath, folder), "*.nupkg*"))
                {
                    var destinationFile = Path.Combine(sourceFolder, Path.GetFileName(file));
                    if (File.Exists(destinationFile))
                    {
                        File.Delete(destinationFile);
                    }
                    if (destinationFile.ToLower().EndsWith(".nupkg.bak"))
                    {
                        // leave a copy in the current folder as it is distributed with the core framework
                        File.Copy(file, destinationFile);
                    }
                    else
                    {
                        // move to destination
                        File.Move(file, destinationFile);
                    }
                }
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
                            break;
                        }
                    }

                    // if compatible with framework version
                    if (frameworkversion == "" || Version.Parse(Constants.Version).CompareTo(Version.Parse(frameworkversion)) >= 0)
                    {
                        List<string> assets = new List<string>();
                        bool manifest = false;

                        // packages are in form of name.1.0.0.nupkg or name.culture.1.0.0.nupkg
                        string name = Path.GetFileNameWithoutExtension(packagename);
                        string[] segments = name?.Split('.');
                        // remove version information from name
                        if (segments != null) name = string.Join('.', segments, 0, segments.Length - 3);

                        // deploy to appropriate locations
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            string filename = "";

                            // evaluate entry root folder
                            switch (entry.FullName.Split('/')[0])
                            {
                                case "lib": // lib/net5.0/...
                                    filename = ExtractFile(entry, binPath, 2);
                                    break;
                                case "wwwroot": // wwwroot/...
                                    filename = ExtractFile(entry, webRootPath, 1);
                                    break;
                                case "runtimes": // runtimes/name/...
                                    filename = ExtractFile(entry, binPath, 0);
                                    break;
                                case "ref": // ref/net5.0/...
                                    filename = ExtractFile(entry, Path.Combine(binPath, "ref"), 2);
                                    break;
                            }

                            if (filename != "")
                            {
                                // ContentRootPath does not use different case for folder names as other framework methods  
                                filename = Regex.Replace(filename, Regex.Escape(contentRootPath), "", RegexOptions.IgnoreCase);
                                assets.Add(filename);
                                if (!manifest && Path.GetFileName(filename) == name + ".log")
                                {
                                    manifest = true;
                                }
                            }
                        }

                        // save dynamic list of assets
                        if (!manifest && assets.Count != 0)
                        {
                            string manifestpath = Path.Combine(sourceFolder, name + ".log");
                            if (File.Exists(manifestpath))
                            {
                                File.Delete(manifestpath);
                            }
                            if (!Directory.Exists(Path.GetDirectoryName(manifestpath)))
                            {
                                Directory.CreateDirectory(Path.GetDirectoryName(manifestpath));
                            }
                            File.WriteAllText(manifestpath, JsonSerializer.Serialize(assets, new JsonSerializerOptions { WriteIndented = true }));
                        }
                    }
                }

                // remove package
                File.Delete(packagename);
                install = true;
            }

            return install;
        }

        private static string ExtractFile(ZipArchiveEntry entry, string folder, int ignoreLeadingSegments)
        {
            string[] segments = entry.FullName.Split('/'); // ZipArchiveEntries always use unix path separator
            string filename = Path.Combine(folder, string.Join(Path.DirectorySeparatorChar, segments, ignoreLeadingSegments, segments.Length - ignoreLeadingSegments));

            try
            {
                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                }
                entry.ExtractToFile(filename, true);
            }
            catch
            {
                // an error occurred extracting the file
                filename = "";
            }
            return filename;
        }

        public bool UninstallPackage(string PackageName)
        {
            if (File.Exists(Path.Combine(_environment.WebRootPath, "Packages", PackageName + ".log")))
            {
                // use manifest to clean up file resources
                List<string> assets = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(Path.Combine(_environment.WebRootPath, "Packages", PackageName + ".log")));
                assets.Reverse();
                foreach (string asset in assets)
                {
                    // legacy support for assets that were stored as absolute paths
                    string filepath = asset.StartsWith("\\") ? Path.Combine(_environment.ContentRootPath, asset.Substring(1)) : asset;
                    if (File.Exists(filepath))
                    {
                        File.Delete(filepath);
                        if (!Directory.EnumerateFiles(Path.GetDirectoryName(filepath)).Any())
                        {
                            Directory.Delete(Path.GetDirectoryName(filepath));
                        }
                    }
                }
                return true;
            }
            return false;
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
