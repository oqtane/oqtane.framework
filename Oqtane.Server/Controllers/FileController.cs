using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Oqtane.Security;
using System.Linq;

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment environment;
        private readonly IFileRepository Files;
        private readonly IFolderRepository Folders;
        private readonly IUserPermissions UserPermissions;
        private readonly ITenantResolver Tenants;
        private readonly ILogManager logger;
        private readonly string WhiteList = "jpg,jpeg,jpe,gif,bmp,png,mov,wmv,avi,mp4,mp3,doc,docx,xls,xlsx,ppt,pptx,pdf,txt,zip,nupkg";

        public FileController(IWebHostEnvironment environment, IFileRepository Files, IFolderRepository Folders, IUserPermissions UserPermissions, ITenantResolver Tenants, ILogManager logger)
        {
            this.environment = environment;
            this.Files = Files;
            this.Folders = Folders;
            this.UserPermissions = UserPermissions;
            this.Tenants = Tenants;
            this.logger = logger;
        }

        // GET: api/<controller>?folder=x
        [HttpGet]
        public IEnumerable<Models.File> Get(string folder)
        {
            List<Models.File> files = new List<Models.File>();
            int folderid;
            if (int.TryParse(folder, out folderid))
            {
                Folder Folder = Folders.GetFolder(folderid);
                if (Folder != null && UserPermissions.IsAuthorized(User, "Browse", Folder.Permissions))
                {
                    files = Files.GetFiles(folderid).ToList();
                }
            }
            else
            {
                if (User.IsInRole(Constants.HostRole))
                {
                    folder = GetFolderPath(folder);
                    if (Directory.Exists(folder))
                    {
                        foreach (string file in Directory.GetFiles(folder))
                        {
                            files.Add(new Models.File { Name = Path.GetFileName(file), Extension = Path.GetExtension(file).Replace(".","") });
                        }
                    }
                }
            }
            return files;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Models.File Get(int id)
        {
            Models.File file = Files.GetFile(id);
            if (UserPermissions.IsAuthorized(User, "View", file.Folder.Permissions))
            {
                return file;
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access File {File}", file);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Models.File Put(int id, [FromBody] Models.File File)
        {
            if (ModelState.IsValid && UserPermissions.IsAuthorized(User, "Folder", File.Folder.FolderId, "Edit"))
            {
                File = Files.UpdateFile(File);
                logger.Log(LogLevel.Information, this, LogFunction.Update, "File Updated {File}", File);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update File {File}", File);
                HttpContext.Response.StatusCode = 401;
                File = null;
            }
            return File;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            Models.File File = Files.GetFile(id);
            if (UserPermissions.IsAuthorized(User, "Folder", File.Folder.FolderId, "Edit"))
            {
                Files.DeleteFile(id);

                string filepath = Path.Combine(GetFolderPath(File.Folder) + File.Name);
                if (System.IO.File.Exists(filepath))
                {
                    System.IO.File.Delete(filepath);
                }
                logger.Log(LogLevel.Information, this, LogFunction.Delete, "File Deleted {File}", File);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete File {FileId}", id);
                HttpContext.Response.StatusCode = 401;
            }
        }

        // GET api/<controller>/upload?url=x&folderid=y
        [HttpGet("upload")]
        public Models.File UploadFile(string url, string folderid)
        {
            Models.File file = null;
            Folder folder = Folders.GetFolder(int.Parse(folderid));
            if (folder != null && UserPermissions.IsAuthorized(User, "Edit", folder.Permissions))
            {
                string folderpath = GetFolderPath(folder);
                CreateDirectory(folderpath);
                string filename = url.Substring(url.LastIndexOf("/") + 1);
                try
                {
                    var client = new System.Net.WebClient();
                    client.DownloadFile(url, folderpath + filename);
                    FileInfo fileinfo = new FileInfo(folderpath + filename);
                    file = Files.AddFile(new Models.File { Name = filename, FolderId = folder.FolderId, Extension = fileinfo.Extension.Replace(".",""), Size = (int)fileinfo.Length });
                }
                catch
                {
                    logger.Log(LogLevel.Error, this, LogFunction.Create, "File Could Not Be Downloaded From Url {Url}", url);
                }
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Download File {Url} {FolderId}", url, folderid);
                HttpContext.Response.StatusCode = 401;
                file = null;
            }
            return file;
        }
        
        // POST api/<controller>/upload
        [HttpPost("upload")]
        public async Task UploadFile(string folder, IFormFile file)
        {
            if (file.Length > 0)
            {
                string folderpath = "";
                int folderid = -1;
                if (int.TryParse(folder, out folderid))
                {
                    Folder Folder = Folders.GetFolder(folderid);
                    if (Folder != null && UserPermissions.IsAuthorized(User, "Edit", Folder.Permissions))
                    {
                        folderpath = GetFolderPath(Folder); 
                    }
                }
                else
                {
                    if (User.IsInRole(Constants.HostRole))
                    {
                        folderpath = GetFolderPath(folder);
                    }
                }
                if (folderpath != "")
                {
                    CreateDirectory(folderpath);
                    using (var stream = new FileStream(Path.Combine(folderpath, file.FileName), FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    string upload = await MergeFile(folderpath, file.FileName);
                    if (upload != "" && folderid != -1)
                    {
                        FileInfo fileinfo = new FileInfo(folderpath + upload);
                        Files.AddFile(new Models.File { Name = upload, FolderId = folderid, Extension = fileinfo.Extension.Replace(".", ""), Size = (int)fileinfo.Length });
                    }
                }
                else
                {
                    logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Upload File {Folder} {File}", folder, file);
                    HttpContext.Response.StatusCode = 401;
                }
            }
        }

        private async Task<string> MergeFile(string folder, string filename)
        {
            string merged = "";

            // parse the filename which is in the format of filename.ext.part_x_y 
            string token = ".part_";
            string parts = Path.GetExtension(filename).Replace(token, ""); // returns "x_y"
            int totalparts = int.Parse(parts.Substring(parts.IndexOf("_") + 1));
            filename = filename.Substring(0, filename.IndexOf(token)); // base filename
            string[] fileparts = Directory.GetFiles(folder, filename + token + "*"); // list of all file parts

            // if all of the file parts exist ( note that file parts can arrive out of order )
            if (fileparts.Length == totalparts && CanAccessFiles(fileparts))
            {
                // merge file parts
                bool success = true;
                using (var stream = new FileStream(Path.Combine(folder, filename + ".tmp"), FileMode.Create))
                {
                    foreach (string filepart in fileparts)
                    {
                        try
                        {
                            using (FileStream chunk = new FileStream(filepart, FileMode.Open))
                            {
                                await chunk.CopyToAsync(stream);
                            }
                        }
                        catch
                        {
                            success = false;
                        }
                    }
                }

                // delete file parts and rename file
                if (success)
                {
                    foreach (string filepart in fileparts)
                    {
                        System.IO.File.Delete(filepart);
                    }

                    // check for allowable file extensions
                    if (!WhiteList.Contains(Path.GetExtension(filename).Replace(".", "")))
                    {
                        System.IO.File.Delete(Path.Combine(folder, filename + ".tmp"));
                    }
                    else
                    {
                        // rename file now that the entire process is completed
                        System.IO.File.Move(Path.Combine(folder, filename + ".tmp"), Path.Combine(folder, filename));
                        logger.Log(LogLevel.Information, this, LogFunction.Create, "File Uploaded {File}", Path.Combine(folder, filename));
                    }
                    merged = filename;
                }
            }

            // clean up file parts which are more than 2 hours old ( which can happen if a prior file upload failed )
            fileparts = Directory.GetFiles(folder, "*" + token + "*");
            foreach (string filepart in fileparts)
            {
                DateTime createddate = System.IO.File.GetCreationTime(filepart);
                if (createddate < DateTime.Now.AddHours(-2))
                {
                    System.IO.File.Delete(filepart);
                }
            }

            return merged;
        }

        private bool CanAccessFiles(string[] files)
        {
            // ensure files are not locked by another process ( ie. still being written to )
            bool canaccess = true;
            FileStream stream = null;
            foreach (string file in files)
            {
                int attempts = 0;
                bool locked = true;
                while (attempts < 5 && locked == true)
                {
                    try
                    {
                        stream = System.IO.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
                        locked = false;
                    }
                    catch // file is locked by another process
                    {                    
                        Thread.Sleep(1000); // wait 1 second
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Close();
                        }
                    }
                    attempts += 1;
                }
                if (locked && canaccess)
                {
                    canaccess = false;
                }
            }
            return canaccess;
        }

        // GET api/<controller>/download/5
        [HttpGet("download/{id}")]
        public IActionResult Download(int id)
        {
            Models.File file = Files.GetFile(id);
            if (file != null && UserPermissions.IsAuthorized(User, "View", file.Folder.Permissions))
            {
                byte[] filebytes = System.IO.File.ReadAllBytes(GetFolderPath(file.Folder) + file.Name);
                return File(filebytes, "application/octet-stream", file.Name);
            }
            else
            {
                logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access File {FileId}", id);
                HttpContext.Response.StatusCode = 401;
                return null;
            }
        }

        private string GetFolderPath(Folder folder)
        {
            return environment.ContentRootPath + "\\Content\\Tenants\\" + Tenants.GetTenant().TenantId.ToString() + "\\Sites\\" + folder.SiteId.ToString() + "\\" + folder.Path;
        }

        private string GetFolderPath(string folder)
        {
            return Path.Combine(environment.WebRootPath, folder);
        }

        private void CreateDirectory(string folderpath)
        {
            if (!Directory.Exists(folderpath))
            {
                string path = "";
                string[] folders = folderpath.Split(new char[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string folder in folders)
                {
                    path += folder + "\\";
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }
    }
}