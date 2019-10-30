using System.Reflection;
using System.IO;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using System.Xml;
using Oqtane.Shared;
using System;
using System.Diagnostics;

namespace Oqtane.Infrastructure
{
    public class InstallationManager : IInstallationManager
    {
        private readonly IHostApplicationLifetime HostApplicationLifetime;
        private readonly IWebHostEnvironment environment;

        public InstallationManager(IHostApplicationLifetime HostApplicationLifetime, IWebHostEnvironment environment)
        {
            this.HostApplicationLifetime = HostApplicationLifetime;
            this.environment = environment;
        }

        public void InstallPackages(string Folders, bool Restart)
        {
            bool install = false;
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            foreach (string Folder in Folders.Split(','))
            {
                string folder = Path.Combine(environment.WebRootPath, Folder);

                // create folder if it does not exist
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }

                // iterate through packages
                foreach (string packagename in Directory.GetFiles(folder, "*.nupkg"))
                {
                    string name = Path.GetFileNameWithoutExtension(packagename);
                    string[] segments = name.Split('.');
                    name = string.Join('.', segments, 0, segments.Length - 3);

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
                            // deploy to appropriate locations
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                string filename = Path.GetFileName(entry.FullName);
                                switch (Path.GetExtension(filename).ToLower())
                                {
                                    case ".pdb":
                                    case ".dll":
                                        entry.ExtractToFile(Path.Combine(binfolder, filename), true);
                                        break;
                                    case ".png":
                                    case ".jpg":
                                    case ".jpeg":
                                    case ".gif":
                                    case ".svg":
                                    case ".js":
                                    case ".css":
                                        filename = folder + "\\" + entry.FullName.Replace("wwwroot", name).Replace("/", "\\");
                                        if (!Directory.Exists(Path.GetDirectoryName(filename)))
                                        {
                                            Directory.CreateDirectory(Path.GetDirectoryName(filename));
                                        }
                                        entry.ExtractToFile(filename, true);
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

            if (install && Restart)
            {
                // restart application
                RestartApplication();
            }
        }

        public void UpgradeFramework()
        {
            string folder = Path.Combine(environment.WebRootPath, "Framework");
            if (Directory.Exists(folder))
            {
                string upgradepackage = "";

                // iterate through packages
                foreach (string packagename in Directory.GetFiles(folder, "Oqtane.Framework.*.nupkg"))
                {
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
                                // get framework version
                                XmlNode node = doc.SelectSingleNode("/package/metadata/version");
                                if (node != null)
                                {
                                    frameworkversion = node.InnerText;
                                }
                                reader.Close();
                            }
                        }

                        // ensure package version is higher than current framework
                        if (frameworkversion != "" && Version.Parse(Constants.Version).CompareTo(Version.Parse(frameworkversion)) < 0)
                        {
                            upgradepackage = packagename;
                        }
                        else
                        {
                            File.Delete(packagename);
                        }
                    }
                }

                if (upgradepackage != "")
                {
                    FinishUpgrade(upgradepackage);
                }
            }
        }

        private void FinishUpgrade(string packagename)
        {
            string folder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // check if upgrade application exists
            if (File.Exists(Path.Combine(folder, "Oqtane.Upgrade.exe")))
            {
                // unzip package
                using (ZipArchive archive = ZipFile.OpenRead(packagename))
                {
                    foreach (ZipArchiveEntry entry in archive.Entries)
                    {
                        string filename = Path.GetFileName(entry.FullName);
                        if (Path.GetExtension(filename) == ".dll")
                        {
                            entry.ExtractToFile(Path.Combine(Path.Combine(environment.WebRootPath, "Framework"), filename), true);
                        }
                    }
                }

                // delete package
                File.Delete(packagename);

                // run upgrade application
                var process = new Process();
                process.StartInfo.FileName = Path.Combine(folder, "Oqtane.Upgrade.exe");
                process.StartInfo.Arguments = "";
                process.StartInfo.ErrorDialog = false;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = true;
                process.StartInfo.RedirectStandardOutput = false;
                process.StartInfo.RedirectStandardError = false;
                process.Start();
                process.Dispose();

                // stop application so upgrade application can proceed
                RestartApplication();
            }
        }

        public void RestartApplication()
        {
            HostApplicationLifetime.StopApplication();
        }
    }
}
