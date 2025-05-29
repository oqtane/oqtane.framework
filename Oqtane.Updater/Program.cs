using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using Oqtane.Shared;

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
            // note additional arguments must be added in a backward compatible manner as older versions will not pass them
            // requires 2 arguments - the ContentRootPath and the WebRootPath of the site

            // for testing purposes you can uncomment and modify the logic below
            //Array.Resize(ref args, 3);
            //args[0] = @"C:\yourpath\oqtane.framework\Oqtane.Server";
            //args[1] = @"C:\yourpath\oqtane.framework\Oqtane.Server\wwwroot";
            //args[2] = @"true"; // parameter added in 6.1.2

            if (args.Length >= 2)
            {
                string contentrootfolder = args[0];
                string webrootfolder = args[1];

                bool backup = true;
                if (args.Length >= 3)
                {
                    backup = bool.Parse(args[2]);
                }

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

                    // create upgrade log file
                    var logFilePath = Path.Combine(deployfolder, $"{Path.GetFileNameWithoutExtension(packagename)}.log");
                    if (File.Exists(logFilePath))
                    {
                        File.Delete(logFilePath);
                    }

                    WriteLog(logFilePath, "Upgrade Process Started: " + DateTime.UtcNow.ToString() + Environment.NewLine);
                    WriteLog(logFilePath, "ContentRootPath: " + contentrootfolder + Environment.NewLine);
                    WriteLog(logFilePath, "WebRootPath: " + webrootfolder + Environment.NewLine);

                    if (packagename != "" && File.Exists(Path.Combine(webrootfolder, "app_offline.bak")))
                    {
                        WriteLog(logFilePath, "Located Upgrade Package: " + packagename + Environment.NewLine);

                        WriteLog(logFilePath, "Stopping Application Using: " + Path.Combine(contentrootfolder, "app_offline.htm") + Environment.NewLine);
                        var offlineTemplate = File.ReadAllText(Path.Combine(webrootfolder, "app_offline.bak"));
                        var offlineFilePath = Path.Combine(contentrootfolder, "app_offline.htm");

                        // get list of files in package with local paths
                        UpdateOfflineContent(offlineFilePath, offlineTemplate, 5, "Retrieving List Of Files From Upgrade Package");
                        WriteLog(logFilePath, "Retrieving List Of Files From Upgrade Package..." + Environment.NewLine);
                        List<string> files = new List<string>();
                        using (ZipArchive archive = ZipFile.OpenRead(packagename))
                        {
                            foreach (ZipArchiveEntry entry in archive.Entries)
                            {
                                if (!string.IsNullOrEmpty(entry.Name))
                                {
                                    files.Add(Path.Combine(contentrootfolder, entry.FullName));
                                    WriteLog(logFilePath, "Check File: " + entry.FullName + Environment.NewLine);
                                }
                            }
                        }

                        bool success = true;

                        if (backup)
                        {
                            UpdateOfflineContent(offlineFilePath, offlineTemplate, 10, "Preparing Backup Folder");
                            WriteLog(logFilePath, "Preparing Backup Folder: " + backupfolder + Environment.NewLine);

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
                                UpdateOfflineContent(offlineFilePath, offlineTemplate, 95, "Error Creating Backup Folder", "bg-danger");
                                WriteLog(logFilePath, "Error Creating Backup Folder: " + ex.Message + Environment.NewLine);
                                success = false;
                            }

                            // backup files
                            if (success)
                            {
                                UpdateOfflineContent(offlineFilePath, offlineTemplate, 15, "Backing Up Files");
                                WriteLog(logFilePath, "Backing Up Files..." + Environment.NewLine);
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

                                            try
                                            {
                                                // try optimistically to backup the file
                                                File.Copy(file, filename);
                                                WriteLog(logFilePath, "Copy File: " + filename + Environment.NewLine);
                                            }
                                            catch
                                            {
                                                // if the file is locked, wait until it is unlocked
                                                if (CanAccessFile(file))
                                                {
                                                    File.Copy(file, filename);
                                                    WriteLog(logFilePath, "Copy File: " + filename + Environment.NewLine);
                                                }
                                                else
                                                {
                                                    // file could not be backed up, upgrade unsuccessful
                                                    success = false;
                                                    WriteLog(logFilePath, "Error Backing Up Files" + Environment.NewLine);
                                                }
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        UpdateOfflineContent(offlineFilePath, offlineTemplate, 95, "Error Backing Up Files", "bg-danger");
                                        WriteLog(logFilePath, "Error Backing Up Files: " + ex.Message + Environment.NewLine);
                                        success = false;
                                        break;
                                    }
                                }
                            }
                        }

                        // extract files
                        if (success)
                        {
                            UpdateOfflineContent(offlineFilePath, offlineTemplate, 50, "Extracting Files From Upgrade Package");
                            WriteLog(logFilePath, "Extracting Files From Upgrade Package..." + Environment.NewLine);
                            try
                            {
                                using (ZipArchive archive = ZipFile.OpenRead(packagename))
                                {
                                    foreach (ZipArchiveEntry entry in archive.Entries)
                                    {
                                        if (success)
                                        {
                                            if (!string.IsNullOrEmpty(entry.Name))
                                            {
                                                string filename = Path.Combine(contentrootfolder, entry.FullName);
                                                if (!Directory.Exists(Path.GetDirectoryName(filename)))
                                                {
                                                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                                                }

                                                try
                                                {
                                                    // try optimistically to extract the file
                                                    entry.ExtractToFile(filename, true);
                                                    WriteLog(logFilePath, "Exact File: " + filename + Environment.NewLine);
                                                }
                                                catch
                                                {
                                                    // if the file is locked, wait until it is unlocked
                                                    if (CanAccessFile(filename))
                                                    {
                                                        entry.ExtractToFile(filename, true);
                                                        WriteLog(logFilePath, "Exact File: " + filename + Environment.NewLine);
                                                    }
                                                    else
                                                    {
                                                        // file could not be extracted, upgrade unsuccessful
                                                        success = false;
                                                        WriteLog(logFilePath, "Error Extracting Files From Upgrade Package" + Environment.NewLine);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                success = false;
                                WriteLog(logFilePath, "Error Extracting Files From Upgrade Package: " + ex.Message + Environment.NewLine);
                            }

                            if (success)
                            {
                                if (backup)
                                {
                                    UpdateOfflineContent(offlineFilePath, offlineTemplate, 90, "Removing Backup Folder");
                                    WriteLog(logFilePath, "Removing Backup Folder..." + Environment.NewLine);
                                    try
                                    {
                                        // clean up backup
                                        Directory.Delete(backupfolder, true);
                                        // delete package
                                        File.Delete(packagename);
                                    }
                                    catch (Exception ex)
                                    {
                                        UpdateOfflineContent(offlineFilePath, offlineTemplate, 95, "Error Extracting Files From Upgrade Package", "bg-warning");
                                        WriteLog(logFilePath, "Error Removing Backup Folder: " + ex.Message + Environment.NewLine);
                                    }
                                }
                            }
                            else
                            {
                                UpdateOfflineContent(offlineFilePath, offlineTemplate, 95, "Error Extracting Files From Upgrade Package", "bg-danger");

                                if (backup)
                                {
                                    UpdateOfflineContent(offlineFilePath, offlineTemplate, 50, "Upgrade Failed, Restoring Files From Backup Folder", "bg-warning");
                                    WriteLog(logFilePath, "Restoring Files From Backup Folder..." + Environment.NewLine);
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
                                                WriteLog(logFilePath, "Restore File: " + filename + Environment.NewLine);
                                            }
                                        }
                                        // clean up backup
                                        Directory.Delete(backupfolder, true);
                                    }
                                    catch (Exception ex)
                                    {
                                        UpdateOfflineContent(offlineFilePath, offlineTemplate, 95, "Error Restoring Files From Backup Folder", "bg-danger");
                                        WriteLog(logFilePath, "Error Restoring Files From Backup Folder: " + ex.Message + Environment.NewLine);
                                    }
                                }
                            }
                        }
                        else
                        {
                            UpdateOfflineContent(offlineFilePath, offlineTemplate, 95, "Upgrade Failed: Could Not Backup Files", "bg-danger");
                            WriteLog(logFilePath, "Upgrade Failed: Could Not Backup Files" + Environment.NewLine);
                        }


                        UpdateOfflineContent(offlineFilePath, offlineTemplate, 100, "Upgrade Process Finished, Reloading", success ? "" : "bg-danger");
                        Thread.Sleep(3000); //wait for 3 seconds to complete the upgrade process.
                        // bring the app back online
                        if (File.Exists(Path.Combine(contentrootfolder, "app_offline.htm")))
                        {
                            WriteLog(logFilePath, "Restarting Application By Removing: " + Path.Combine(contentrootfolder, "app_offline.htm") + Environment.NewLine);
                            File.Delete(Path.Combine(contentrootfolder, "app_offline.htm"));
                        }
                    }
                    else
                    {
                        WriteLog(logFilePath, "Framework Upgrade Package Not Found Or " + Path.Combine(webrootfolder, "app_offline.bak") + " Does Not Exist" + Environment.NewLine);
                    }

                    WriteLog(logFilePath, "Upgrade Process Ended: " + DateTime.UtcNow.ToString() + Environment.NewLine);
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

        private static bool CanAccessFile(string filepath)
        {
            // ensure file is not locked by another process
            int retries = 60;
            int sleep = 2;
            int attempts = 0;
            FileStream stream = null;

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
                Console.WriteLine("File Locked: " + filepath);
            }

            return !locked;
        }

        private static void UpdateOfflineContent(string filePath, string contentTemplate, int progress, string status,  string progressClass =  "")
        {
            var content = contentTemplate
                            .Replace("[BOOTSTRAPCSSURL]", Constants.BootstrapStylesheetUrl)
                            .Replace("[BOOTSTRAPCSSINTEGRITY]", Constants.BootstrapStylesheetIntegrity)
                            .Replace("[PROGRESS]", progress.ToString())
                            .Replace("[PROGRESSCLASS]", progressClass)
                            .Replace("[STATUS]", status);
            File.WriteAllText(filePath, content);
        }

        private static void WriteLog(string logFilePath, string logContent)
        {
            File.AppendAllText(logFilePath, $"[{DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff")}] {logContent}");
        }
    }
}
