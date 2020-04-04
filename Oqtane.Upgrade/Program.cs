using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;

namespace Oqtane.Upgrade
{
    class Program
    {
        static void Main(string[] args)
        {
            string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);

            // assumes that the application executable must be deployed to the /bin of the Oqtane.Server project
            if (binfolder.Contains("Oqtane.Server\\bin"))
            {
                // ie. binfolder = Oqtane.Server\bin\Debug\netcoreapp3.0\
                string rootfolder = Directory.GetParent(binfolder).Parent.Parent.FullName;
                string deployfolder = Path.Combine(rootfolder, "wwwroot\\Framework");

                if (Directory.Exists(deployfolder))
                {
                    string packagename = "";
                    string[] packages = Directory.GetFiles(deployfolder, "Oqtane.Framework.*.nupkg");
                    if (packages.Length > 0)
                    {
                        packagename = packages[packages.Length - 1]; // use highest version 
                    }

                    if (packagename != "")
                    {
                        // take the app offline
                        if (File.Exists(Path.Combine(rootfolder, "app_offline.bak")))
                        {
                            File.Move(Path.Combine(rootfolder, "app_offline.bak"), Path.Combine(rootfolder, "app_offline.htm"));
                        }

                        // get list of files in package
                        List<string> files = new List<string>();
                        using (ZipArchive archive = ZipFile.OpenRead(packagename))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (Path.GetExtension(entry.FullName) == ".dll")
                                {
                                    files.Add(Path.GetFileName(entry.FullName));
                                }
                            }
                        }

                        // ensure files are not locked
                        string filename;
                        if (CanAccessFiles(files, binfolder))
                        {
                            // create backup
                            foreach (string file in files)
                            {
                                filename = Path.Combine(binfolder, Path.GetFileName(file));
                                if (File.Exists(filename.Replace(".dll", ".bak")))
                                {
                                    File.Delete(filename.Replace(".dll", ".bak"));
                                }
                                File.Move(filename, filename.Replace(".dll", ".bak"));
                            }

                            // extract files
                            bool success = true;
                            try
                            {
                                using (ZipArchive archive = ZipFile.OpenRead(packagename))
                                {
                                    foreach (ZipArchiveEntry entry in archive.Entries)
                                    {
                                        filename = Path.GetFileName(entry.FullName);
                                        if (files.Contains(filename))
                                        {
                                            entry.ExtractToFile(Path.Combine(binfolder, filename), true);
                                        }
                                    }
                                }

                            }
                            catch
                            {
                                // an error occurred extracting a file
                                success = false;
                            }

                            if (success) 
                            {
                                // clean up backup
                                foreach (string file in files)
                                {
                                    filename = Path.Combine(binfolder, Path.GetFileName(file));
                                    if (File.Exists(filename.Replace(".dll", ".bak")))
                                    {
                                        File.Delete(filename.Replace(".dll", ".bak"));
                                    }
                                }
                            }
                            else 
                            {
                                // restore on failure
                                foreach (string file in files)
                                {
                                    filename = Path.Combine(binfolder, Path.GetFileName(file));
                                    if (File.Exists(filename))
                                    {
                                        File.Delete(filename);
                                    }
                                    File.Move(filename.Replace(".dll", ".bak"), filename);
                                }
                            }

                            // delete package
                            File.Delete(packagename);
                        }

                        // bring the app back online
                        if (File.Exists(Path.Combine(rootfolder, "app_offline.htm")))
                        {
                            File.Move(Path.Combine(rootfolder, "app_offline.htm"), Path.Combine(rootfolder, "app_offline.bak"));
                        }
                    }
                }
            }
        }

        private static bool CanAccessFiles(List<string> files, string folder)
        {
            // ensure files are not locked by another process - the shutdownTimeLimit defines the duration for app shutdown
            bool canAccess = true;
            FileStream stream = null;
            int i = 0;
            while (i < (files.Count - 1) && canAccess)
            {
                string filepath = Path.Combine(folder, Path.GetFileName(files[i]));
                int attempts = 0;
                bool locked = true;
                // try up to 30 times
                while (attempts < 30 && locked)
                {
                    try
                    {
                        stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.None);
                        locked = false;
                    }
                    catch // file is locked by another process
                    {
                        Thread.Sleep(1000); // wait 1 second
                    }
                    finally
                    {
                        stream?.Close();
                    }
                    attempts += 1;
                }
                if (locked)
                {
                    canAccess = false;
                }
                i += 1;
            }
            return canAccess;
        }
    }
}
