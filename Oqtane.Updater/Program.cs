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
                    string log = "Upgrade Process Started: " + DateTime.UtcNow.ToString() + Environment.NewLine;
                    log += "ContentRootPath: " + contentrootfolder + Environment.NewLine;
                    log += "WebRootPath: " + webrootfolder + Environment.NewLine;

                    string packagename = "";
                    string[] packages = Directory.GetFiles(deployfolder, "Oqtane.Framework.*.Upgrade.zip");
                    if (packages.Length > 0)
                    {
                        packagename = packages[packages.Length - 1]; // use highest version 
                    }

                    if (packagename != "" && File.Exists(Path.Combine(webrootfolder, "app_offline.bak")))
                    {
                        log += "Located Upgrade Package: " + packagename + Environment.NewLine;

                        log += "Stopping Application Using: " + Path.Combine(contentrootfolder, "app_offline.htm") + Environment.NewLine;
                        File.Copy(Path.Combine(webrootfolder, "app_offline.bak"), Path.Combine(contentrootfolder, "app_offline.htm"), true);

                        // get list of files in package with local paths
                        log += "Retrieving List Of Files From Upgrade Package..." + Environment.NewLine;
                        List<string> files = new List<string>();
                        using (ZipArchive archive = ZipFile.OpenRead(packagename))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (!string.IsNullOrEmpty(entry.Name))
                                {
                                    files.Add(Path.Combine(contentrootfolder, entry.FullName));
                                }
                            }
                        }

                        // ensure files are not locked
                        if (CanAccessFiles(files))
                        {
                            log += "Preparing Backup Folder: " + backupfolder + Environment.NewLine;
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
                                log += "Error Creating Backup Folder: " + ex.Message + Environment.NewLine;
                                success = false;
                            }

                            // backup files
                            if (success)
                            {
                                log += "Backing Up Files..." + Environment.NewLine;
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
                                        log += "Error Backing Up Files: " + ex.Message + Environment.NewLine;
                                        success = false;
                                    }
                                }
                            }

                            // extract files
                            if (success)
                            {
                                log += "Extracting Files From Upgrade Package..." + Environment.NewLine;
                                try
                                {
                                    using (ZipArchive archive = ZipFile.OpenRead(packagename))
                                    {
                                        foreach (ZipArchiveEntry entry in archive.Entries)
                                        {
                                            if (!string.IsNullOrEmpty(entry.Name))
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
                                }
                                catch (Exception ex)
                                {
                                    success = false;
                                    log += "Error Extracting Files From Upgrade Package: " + ex.Message + Environment.NewLine;
                                }

                                if (success)
                                {
                                    log += "Removing Backup Folder..." + Environment.NewLine;
                                    try
                                    {
                                        // clean up backup
                                        Directory.Delete(backupfolder, true);
                                        // delete package
                                        File.Delete(packagename);
                                    }
                                    catch (Exception ex)
                                    {
                                        log += "Error Removing Backup Folder: " + ex.Message + Environment.NewLine;
                                    }
                                }
                                else
                                {
                                    log += "Restoring Files From Backup Folder..." + Environment.NewLine;
                                    try
                                    {
                                        // restore on failure
                                        foreach (string file in files)
                                        {
                                            File.Delete(file);
                                            string filename = Path.Combine(backupfolder, file.Replace(contentrootfolder + Path.DirectorySeparatorChar, ""));
                                            if (File.Exists(filename))
                                            {
                                                File.Copy(filename, file);
                                            }
                                        }
                                        // clean up backup
                                        Directory.Delete(backupfolder, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        log += "Error Restoring Files From Backup Folder: " + ex.Message + Environment.NewLine;
                                    }
                                }
                            }
                            else
                            {
                                log += "Upgrade Failed: Could Not Backup Files" + Environment.NewLine;
                            }
                        }
                        else
                        {
                            log += "Upgrade Failed: Some Files Are Locked By The Hosting Environment" + Environment.NewLine;
                        }

                        // bring the app back online
                        if (File.Exists(Path.Combine(contentrootfolder, "app_offline.htm")))
                        {
                            log += "Restarting Application By Removing: " + Path.Combine(contentrootfolder, "app_offline.htm") + Environment.NewLine;
                            File.Delete(Path.Combine(contentrootfolder, "app_offline.htm"));
                        }
                    }
                    else
                    {
                        log += "Framework Upgrade Package Not Found Or " + Path.Combine(webrootfolder, "app_offline.bak") + " Does Not Exist" + Environment.NewLine;
                    }

                    log += "Upgrade Process Ended: " + DateTime.UtcNow.ToString() + Environment.NewLine;

                    // create upgrade log file
                    string logfile = Path.Combine(deployfolder, Path.GetFileNameWithoutExtension(packagename) + ".log");
                    if (File.Exists(logfile))
                    {
                        File.Delete(logfile);
                    }
                    File.WriteAllText(logfile, log);
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
            // ensure files are not locked by another process
            // the IIS ShutdownTimeLimit defines the duration for app shutdown (default is 90 seconds)
            // websockets can delay application shutdown (ie. Blazor Server)
            int retries = 60;
            int sleep = 2;

            bool canAccess = true;
            FileStream stream = null;
            int i = 0;
            while (i < (files.Count - 1) && canAccess)
            {
                string filepath = files[i];
                int attempts = 0;
                bool locked = true;

                while (attempts < retries && locked)
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
                        Thread.Sleep(sleep * 1000); // wait
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
                    Console.WriteLine("File Locked: " + filepath);
                }
                i += 1;
            }
            return canAccess;
        }
    }
}
