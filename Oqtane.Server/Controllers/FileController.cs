using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Oqtane.Models;
using Oqtane.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Oqtane.Security;
using System.Linq;
using System.Drawing;
using System.Net;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Controllers
{
    [Route("{site}/api/[controller]")]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IFileRepository _files;
        private readonly IFolderRepository _folders;
        private readonly IUserPermissions _userPermissions;
        private readonly ITenantResolver _tenants;
        private readonly ILogManager _logger;

        public FileController(IWebHostEnvironment environment, IFileRepository files, IFolderRepository folders, IUserPermissions userPermissions, ITenantResolver tenants, ILogManager logger)
        {
            _environment = environment;
            _files = files;
            _folders = folders;
            _userPermissions = userPermissions;
            _tenants = tenants;
            _logger = logger;
        }

        // GET: api/<controller>?folder=x
        [HttpGet]
        public IEnumerable<Models.File> Get(string folder)
        {
            List<Models.File> files = new List<Models.File>();
            int folderid;
            if (int.TryParse(folder, out folderid))
            {
                Folder f = _folders.GetFolder(folderid);
                if (f != null && _userPermissions.IsAuthorized(User, PermissionNames.Browse, f.Permissions))
                {
                    files = _files.GetFiles(folderid).ToList();
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
                            files.Add(new Models.File {Name = Path.GetFileName(file), Extension = Path.GetExtension(file)?.Replace(".", "")});
                        }
                    }
                }
            }

            return files;
        }

        // GET: api/<controller>/siteId/folderPath
        [HttpGet("{siteId}/{path}")]
        public IEnumerable<Models.File> Get(int siteId, string path)
        {
            var folderPath = WebUtility.UrlDecode(path);
            Folder folder = _folders.GetFolder(siteId, folderPath);
            List<Models.File> files;
            if (folder != null)
            {
                if (_userPermissions.IsAuthorized(User, PermissionNames.Browse, folder.Permissions))
                {
                    files = _files.GetFiles(folder.FolderId).ToList();
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access Folder {folder}", folder);
                    HttpContext.Response.StatusCode = 401;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "Folder Not Found {SiteId} {Path}", siteId, path);
                HttpContext.Response.StatusCode = 404;
                return null;
            }

            return files;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Models.File Get(int id)
        {
            Models.File file = _files.GetFile(id);
            if (file != null)
            {
                if (_userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.Permissions))
                {
                    return file;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access File {File}", file);
                    HttpContext.Response.StatusCode = 401;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Not Found {FileId}", id);
                HttpContext.Response.StatusCode = 404;
                return null;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public Models.File Put(int id, [FromBody] Models.File file)
        {
            if (ModelState.IsValid && _userPermissions.IsAuthorized(User, EntityNames.Folder, file.Folder.FolderId, PermissionNames.Edit))
            {
                file = _files.UpdateFile(file);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "File Updated {File}", file);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Update, "User Not Authorized To Update File {File}", file);
                HttpContext.Response.StatusCode = 401;
                file = null;
            }

            return file;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = Constants.RegisteredRole)]
        public void Delete(int id)
        {
            Models.File file = _files.GetFile(id);
            if (file != null)
            {
                if (_userPermissions.IsAuthorized(User, EntityNames.Folder, file.Folder.FolderId, PermissionNames.Edit))
                {
                    _files.DeleteFile(id);

                    string filepath = Path.Combine(GetFolderPath(file.Folder), file.Name);
                    if (System.IO.File.Exists(filepath))
                    {
                        System.IO.File.Delete(filepath);
                    }

                    _logger.Log(LogLevel.Information, this, LogFunction.Delete, "File Deleted {File}", file);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Delete, "User Not Authorized To Delete File {FileId}", id);
                    HttpContext.Response.StatusCode = 401;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Delete, "File Not Found {FileId}", id);
                HttpContext.Response.StatusCode = 404;
            }
        }

        // GET api/<controller>/upload?url=x&folderid=y
        [HttpGet("upload")]
        public Models.File UploadFile(string url, string folderid)
        {
            Models.File file = null;
            Folder folder = _folders.GetFolder(int.Parse(folderid));
            if (folder != null && _userPermissions.IsAuthorized(User, PermissionNames.Edit, folder.Permissions))
            {
                string folderPath = GetFolderPath(folder);
                CreateDirectory(folderPath);
                string filename = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1);
                // check for allowable file extensions
                if (Constants.UploadableFiles.Contains(Path.GetExtension(filename).Replace(".", "")))
                {
                    try
                    {
                        var client = new WebClient();
                        string targetPath = Path.Combine(folderPath, filename);
                        // remove file if it already exists
                        if (System.IO.File.Exists(targetPath))
                        {
                            System.IO.File.Delete(targetPath);
                        }

                        client.DownloadFile(url, targetPath);
                        _files.AddFile(CreateFile(filename, folder.FolderId, targetPath));
                    }
                    catch
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Create, "File Could Not Be Downloaded From Url {Url}", url);
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "File Could Not Be Downloaded From Url Due To Its File Extension {Url}", url);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Download File {Url} {FolderId}", url, folderid);
                HttpContext.Response.StatusCode = 401;
            }

            return file;
        }

        // POST api/<controller>/upload
        [HttpPost("upload")]
        public async Task UploadFile(string folder, IFormFile file)
        {
            if (file.Length > 0)
            {
                string folderPath = "";

                if (int.TryParse(folder, out int folderId))
                {
                    Folder virtualFolder = _folders.GetFolder(folderId);
                    if (virtualFolder != null && _userPermissions.IsAuthorized(User, PermissionNames.Edit, virtualFolder.Permissions))
                    {
                        folderPath = GetFolderPath(virtualFolder);
                    }
                }
                else
                {
                    if (User.IsInRole(Constants.HostRole))
                    {
                        folderPath = GetFolderPath(folder);
                    }
                }

                if (folderPath != "")
                {
                    CreateDirectory(folderPath);
                    using (var stream = new FileStream(Path.Combine(folderPath, file.FileName), FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    string upload = await MergeFile(folderPath, file.FileName);
                    if (upload != "" && folderId != -1)
                    {
                        _files.AddFile(CreateFile(upload, folderId, Path.Combine(folderPath, upload)));
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "User Not Authorized To Upload File {Folder} {File}", folder, file);
                    HttpContext.Response.StatusCode = 401;
                }
            }
        }

        private async Task<string> MergeFile(string folder, string filename)
        {
            string merged = "";

            // parse the filename which is in the format of filename.ext.part_x_y 
            string token = ".part_";
            string parts = Path.GetExtension(filename)?.Replace(token, ""); // returns "x_y"    
            int totalparts = int.Parse(parts?.Substring(parts.IndexOf("_") + 1));
            filename = filename?.Substring(0, filename.IndexOf(token)); // base filename
            string[] fileParts = Directory.GetFiles(folder, filename + token + "*"); // list of all file parts

            // if all of the file parts exist ( note that file parts can arrive out of order )
            if (fileParts.Length == totalparts && CanAccessFiles(fileParts))
            {
                // merge file parts
                bool success = true;
                using (var stream = new FileStream(Path.Combine(folder, filename + ".tmp"), FileMode.Create))
                {
                    foreach (string filepart in fileParts)
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
                    foreach (string filepart in fileParts)
                    {
                        System.IO.File.Delete(filepart);
                    }

                    // check for allowable file extensions
                    if (!Constants.UploadableFiles.Contains(Path.GetExtension(filename)?.Replace(".", "")))
                    {
                        System.IO.File.Delete(Path.Combine(folder, filename + ".tmp"));
                    }
                    else
                    {
                        // remove file if it already exists
                        if (System.IO.File.Exists(Path.Combine(folder, filename)))
                        {
                            System.IO.File.Delete(Path.Combine(folder, filename));
                        }

                        // rename file now that the entire process is completed
                        System.IO.File.Move(Path.Combine(folder, filename + ".tmp"), Path.Combine(folder, filename));
                        _logger.Log(LogLevel.Information, this, LogFunction.Create, "File Uploaded {File}", Path.Combine(folder, filename));
                    }

                    merged = filename;
                }
            }

            // clean up file parts which are more than 2 hours old ( which can happen if a prior file upload failed )
            fileParts = Directory.GetFiles(folder, "*" + token + "*");
            foreach (string filepart in fileParts)
            {
                DateTime createddate = System.IO.File.GetCreationTime(filepart).ToUniversalTime();
                if (createddate < DateTime.UtcNow.AddHours(-2))
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
                while (attempts < 5 && locked)
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
            Models.File file = _files.GetFile(id);
            if (file != null)
            {
                if (_userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.Permissions))
                {
                    string filepath = Path.Combine(GetFolderPath(file.Folder) , file.Name);
                    if (System.IO.File.Exists(filepath))
                    {
                        byte[] filebytes = System.IO.File.ReadAllBytes(filepath);
                        return File(filebytes, "application/octet-stream", file.Name);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Does Not Exist {FileId} {FilePath}", id, filepath);
                        HttpContext.Response.StatusCode = 404;
                        return null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "User Not Authorized To Access File {FileId}", id);
                    HttpContext.Response.StatusCode = 401;
                    return null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Not Found {FileId}", id);
                HttpContext.Response.StatusCode = 404;
                return null;
            }
        }

        private string GetFolderPath(Folder folder)
        {
            return Utilities.PathCombine(_environment.ContentRootPath, "Content", "Tenants", _tenants.GetTenant().TenantId.ToString(), "Sites", folder.SiteId.ToString(), folder.Path);
        }

        private string GetFolderPath(string folder)
        {
            return Utilities.PathCombine(_environment.WebRootPath, folder);
        }

        private void CreateDirectory(string folderpath)
        {
            if (!Directory.Exists(folderpath))
            {
                string path = "";
                var separators = new char[] { Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar };
                string[] folders = folderpath.Split(separators, StringSplitOptions.RemoveEmptyEntries);
                foreach (string folder in folders)
                {
                    path = Utilities.PathCombine(path, folder,"\\");
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }

        private Models.File CreateFile(string filename, int folderid, string filepath)
        {
            Models.File file = new Models.File();
            file.Name = filename;
            file.FolderId = folderid;

            FileInfo fileinfo = new FileInfo(filepath);
            file.Extension = fileinfo.Extension.ToLower().Replace(".", "");
            file.Size = (int) fileinfo.Length;
            file.ImageHeight = 0;
            file.ImageWidth = 0;

            if (Constants.ImageFiles.Contains(file.Extension))
            {
                FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                using (var image = Image.FromStream(stream))
                {
                    file.ImageHeight = image.Height;
                    file.ImageWidth = image.Width;
                }

                stream.Close();
            }

            return file;
        }
    }
}
