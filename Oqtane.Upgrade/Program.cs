using System;
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
            // for testing purposes set Oqtane.Upgrade as startup project and modify values below
            //Array.Resize(ref args, 2);
            //args[0] = @"C:\yourpath\oqtane.framework\Oqtane.Server";
            //args[1] = @"C:\yourpath\oqtane.framework\Oqtane.Server\wwwroot";

            // requires 2 arguments - the contentrootpath and the webrootpath of the site
            if (args.Length == 2)
            {
                string binfolder = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
                string contentrootfolder = args[0];
                string webrootfolder = args[1];
                string deployfolder = Path.Combine(webrootfolder, "Framework");

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
                        if (File.Exists(Path.Combine(webrootfolder, "app_offline.bak")))
                        {
                            File.Copy(Path.Combine(webrootfolder, "app_offline.bak"), Path.Combine(contentrootfolder, "app_offline.htm"), true);
                        }

                        // get list of files in package
                        List<string> files = new List<string>();
                        using (ZipArchive archive = ZipFile.OpenRead(packagename))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                switch (Path.GetDirectoryName(entry.FullName).Split('\\')[0])
                                {
                                    case "lib":
                                        files.Add(Path.Combine(binfolder, Path.GetFileName(entry.FullName)));
                                        break;
                                    case "wwwroot":
                                        files.Add(Path.Combine(webrootfolder, entry.FullName.Replace("wwwroot/", "").Replace("/","\\")));
                                        break;
                                }
                            }
                        }

                        // ensure files are not locked
                        if (CanAccessFiles(files))
                        {
                            // create backup
                            foreach (string file in files)
                            {
                                if (File.Exists(file + ".bak"))
                                {
                                    File.Delete(file + ".bak");
                                }
                                File.Move(file, file + ".bak");
                            }

                            // extract files
                            bool success = true;
                            try
                            {
                                using (ZipArchive archive = ZipFile.OpenRead(packagename))
                                {
                                    foreach (ZipArchiveEntry entry in archive.Entries)
                                    {
                                        string filename = "";
                                        switch (Path.GetDirectoryName(entry.FullName).Split('\\')[0])
                                        {
                                            case "lib":
                                                filename = Path.Combine(binfolder, Path.GetFileName(entry.FullName));
                                                break;
                                            case "wwwroot":
                                                filename = Path.Combine(webrootfolder, entry.FullName.Replace("wwwroot/", "").Replace("/", "\\"));
                                                break;
                                        }
                                        if (files.Contains(filename))
                                        {
                                            entry.ExtractToFile(filename, true);
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
                                    if (File.Exists(file + ".bak"))
                                    {
                                        File.Delete(file + ".bak");
                                    }
                                }

                                // delete package
                                File.Delete(packagename);
                            }
                            else
                            {
                                // restore on failure
                                foreach (string file in files)
                                {
                                    if (File.Exists(file))
                                    {
                                        File.Delete(file);
                                    }
                                    File.Move(file + ".bak", file);
                                }
                            }
                        }

                        // bring the app back online
                        if (File.Exists(Path.Combine(contentrootfolder, "app_offline.htm")))
                        {
                            File.Delete(Path.Combine(contentrootfolder, "app_offline.htm"));
                        }
                    }
                }
            }
        }

        private static bool CanAccessFiles(List<string> files)
        {
            // ensure files are not locked by another process - the shutdownTimeLimit defines the duration for app shutdown
            bool canAccess = true;
            FileStream stream = null;
            int i = 0;
            while (i < (files.Count - 1) && canAccess)
            {
                string filepath = files[i];
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
