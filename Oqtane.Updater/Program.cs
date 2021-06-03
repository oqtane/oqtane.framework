using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace Oqtane.Updater
{
    class Program
    {
        /// <summary>
        /// This console application is responsible for extracting the contents of a previously downloaded Oqtane Upgrade package
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            // requires 2 arguments - the ContentRootPath and the WebRootPath of the site

            // for testing purposes you can uncomment and modify the logic below
            //Array.Resize(ref args, 2);
            //args[0] = @"C:\yourpath\oqtane.framework\Oqtane.Server";
            //args[1] = @"C:\yourpath\oqtane.framework\Oqtane.Server\wwwroot";
            
            if (args.Length == 2)
            {
                string contentrootfolder = args[0];
                string webrootfolder = args[1];
                string deployfolder = Path.Combine(contentrootfolder, "Packages");
                string backupfolder = Path.Combine(contentrootfolder, "Backup");

                if (Directory.Exists(deployfolder))
                {
                    string packagename = "";
                    string[] packages = Directory.GetFiles(deployfolder, "Oqtane.Framework.*.Upgrade.zip");
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

                        // get list of files in package with local paths
                        List<string> files = new List<string>();
                        using (ZipArchive archive = ZipFile.OpenRead(packagename))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                files.Add(Path.Combine(contentrootfolder, entry.FullName));
                            }
                        }

                        // ensure files are not locked
                        if (CanAccessFiles(files))
                        {
                            bool success = true;
                            try
                            {
                                // clear out backup folder
                                if (Directory.Exists(backupfolder))
                                {
                                    Directory.Delete(backupfolder, true);
                                }
                                Directory.CreateDirectory(backupfolder);
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                success = false;
                            }

                            // backup files
                            if (success)
                            {
                                foreach (string file in files)
                                {
                                    string filename = Path.Combine(backupfolder, file.Replace(contentrootfolder + Path.DirectorySeparatorChar, ""));
                                    try
                                    {
                                        if (File.Exists(file))
                                        {
                                            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                                            {
                                                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                                            }
                                            File.Copy(file, filename);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine(ex.Message);
                                        success = false;
                                    }
                                }
                            }

                            // extract files
                            if (success)
                            {
                                try
                                {
                                    using (ZipArchive archive = ZipFile.OpenRead(packagename))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            string filename = Path.Combine(contentrootfolder, entry.FullName);
                                            if (!Directory.Exists(Path.GetDirectoryName(filename)))
                                            {
                                                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                                            }
                                            entry.ExtractToFile(filename, true);
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    // an error occurred extracting a file
                                    success = false;
                                    Console.WriteLine("Update Not Successful: Error Extracting Files From Package - " + ex.Message);
                                }

                                if (success)
                                {
                                    // clean up backup
                                    Directory.Delete(backupfolder, true);
                                    // delete package
                                    File.Delete(packagename);
                                }
                                else
                                {
                                    try
                                    {
                                        // restore on failure
                                        foreach (string file in files)
                                        {
                                            string filename = Path.Combine(backupfolder, file.Replace(contentrootfolder + Path.DirectorySeparatorChar, ""));
                                            if (File.Exists(filename))
                                            {
                                                File.Copy(filename, file);
                                            }
                                            else
                                            {
                                                File.Delete(file);
                                            }
                                        }
                                        // clean up backup
                                        Directory.Delete(backupfolder, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine("Update Not Successful: Error Restoring Files - " + ex.Message);
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Update Not Successful: Could Not Backup All Existing Files");
                            }
                        }
                        else
                        {
                            Console.WriteLine("Upgrade Not Successful: Some Files Are Locked");
                        }

                        // bring the app back online
                        if (File.Exists(Path.Combine(contentrootfolder, "app_offline.htm")))
                        {
                            File.Delete(Path.Combine(contentrootfolder, "app_offline.htm"));
                        }
                    }
                    else
                    {
                        Console.WriteLine("Framework Upgrade Package Not Found");
                    }
                }
                else
                {
                    Console.WriteLine("Framework Upgrade Folder " + deployfolder + " Does Not Exist");
                }
            }
            else
            {
                Console.WriteLine("Missing ContentRootPath and WebRootPath Parameters");
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
                        if (File.Exists(filepath))
                        {
                            stream = File.Open(filepath, FileMode.Open, FileAccess.Read, FileShare.None);
                            locked = false;
                        }
                        else
                        {
                            locked = false;
                        }
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
