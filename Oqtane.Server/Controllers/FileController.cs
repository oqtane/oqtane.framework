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
using System.Net;
using Oqtane.Enums;
using Oqtane.Infrastructure;
using Oqtane.Repository;
using Oqtane.Extensions;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

// ReSharper disable StringIndexOfIsCultureSpecific.1

namespace Oqtane.Controllers
{
    [Route(ControllerRoutes.ApiRoute)]
    public class FileController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IFileRepository _files;
        private readonly IFolderRepository _folders;
        private readonly IUserPermissions _userPermissions;
        private readonly ILogManager _logger;
        private readonly Alias _alias;

        public FileController(IWebHostEnvironment environment, IFileRepository files, IFolderRepository folders, IUserPermissions userPermissions, ILogManager logger, ITenantManager tenantManager)
        {
            _environment = environment;
            _files = files;
            _folders = folders;
            _userPermissions = userPermissions;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        // GET: api/<controller>?folder=x
        [HttpGet]
        public IEnumerable<Models.File> Get(string folder)
        {
            List<Models.File> files = new List<Models.File>();
            int folderid;
            if (int.TryParse(folder, out folderid))
            {
                Folder Folder = _folders.GetFolder(folderid);
                if (Folder != null && Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.Browse, Folder.Permissions))
                {
                    files = _files.GetFiles(folderid).ToList();
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Get Attempt {FolderId}", folder);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    files = null;
                }
            }
            else
            {
                if (User.IsInRole(RoleNames.Host))
                {
                    folder = GetFolderPath(folder);
                    if (Directory.Exists(folder))
                    {
                        foreach (string file in Directory.GetFiles(folder))
                        {
                            files.Add(new Models.File { Name = Path.GetFileName(file), Extension = Path.GetExtension(file)?.Replace(".", "") });
                        }
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Get Attempt {Folder}", folder);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    files = null;
                }
            }

            return files;
        }

        // GET: api/<controller>/siteId/folderPath
        [HttpGet("{siteId}/{path}")]
        public IEnumerable<Models.File> Get(int siteId, string path)
        {
            List<Models.File> files;

            Folder folder = _folders.GetFolder(siteId, WebUtility.UrlDecode(path));
            if (folder != null && folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.Browse, folder.Permissions))
            {
                files = _files.GetFiles(folder.FolderId).ToList();
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Get Attempt {SiteId} {Path}", siteId, path);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                files = null;
            }

            return files;
        }

        // GET api/<controller>/5
        [HttpGet("{id}")]
        public Models.File Get(int id)
        {
            Models.File file = _files.GetFile(id);
            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.Permissions))
            {
                return file;
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Get Attempt {FileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return null;
            }
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public Models.File Put(int id, [FromBody] Models.File file)
        {
            var File = _files.GetFile(file.FileId, false);
            if (ModelState.IsValid && File != null && File.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, EntityNames.Folder, file.FolderId, PermissionNames.Edit))
            {
                if (File.Name != file.Name || File.FolderId != file.FolderId)
                {
                    string folderpath = _folders.GetFolderPath(file.Folder);
                    if (!Directory.Exists(folderpath))
                    {
                        Directory.CreateDirectory(folderpath);
                    }
                    System.IO.File.Move(_files.GetFilePath(File), Path.Combine(folderpath, file.Name));
                }

                file.Extension = Path.GetExtension(file.Name).ToLower().Replace(".", "");
                file = _files.UpdateFile(file);
                _logger.Log(LogLevel.Information, this, LogFunction.Update, "File Updated {File}", file);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Put Attempt {File}", file);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                file = null;
            }

            return file;
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public void Delete(int id)
        {
            Models.File file = _files.GetFile(id);
            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, EntityNames.Folder, file.Folder.FolderId, PermissionNames.Edit))
            {
                _files.DeleteFile(id);

                string filepath = _files.GetFilePath(file);
                if (System.IO.File.Exists(filepath))
                {
                    // remove file and thumbnails
                    foreach(var f in Directory.GetFiles(Path.GetDirectoryName(filepath), Path.GetFileNameWithoutExtension(filepath) + ".*"))
                    {
                        System.IO.File.Delete(f);
                    }
                }

                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "File Deleted {File}", file);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Delete Attempt {FileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET api/<controller>/upload?url=x&folderid=y
        [HttpGet("upload")]
        public Models.File UploadFile(string url, string folderid)
        {
            Models.File file = null;

            Folder folder = null;
            int FolderId;
            if (int.TryParse(folderid, out FolderId))
            {
                folder = _folders.GetFolder(FolderId);
            }

            if (folder != null && folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.Edit, folder.Permissions))
            {
                string folderPath = _folders.GetFolderPath(folder);
                CreateDirectory(folderPath);

                string filename = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1);
                // check for allowable file extensions
                if (!Constants.UploadableFiles.Split(',').Contains(Path.GetExtension(filename).ToLower().Replace(".", "")))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "File Could Not Be Downloaded From Url Due To Its File Extension {Url}", url);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    return file;
                }

                if (!filename.IsPathOrFileValid())
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, $"File Could Not Be Downloaded From Url Due To Its File Name Not Allowed {url}");
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                    return file;
                }

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
                    file = CreateFile(filename, folder.FolderId, targetPath);
                    if (file != null)
                    {
                        file = _files.AddFile(file);
                    }
                }
                catch
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "File Could Not Be Downloaded From Url {Url}", url);
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Download Attempt {FolderId} {Url}", folderid, url);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            return file;
        }

        // POST api/<controller>/upload
        [HttpPost("upload")]
        public async Task UploadFile(string folder, IFormFile formfile)
        {
            if (formfile.Length <= 0)
            {
                return;
            }

            if (!formfile.FileName.IsPathOrFileValid())
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Conflict;
                return;
            }

            string folderPath = "";

            int FolderId;
            if (int.TryParse(folder, out FolderId))
            {
                Folder Folder = _folders.GetFolder(FolderId);
                if (Folder != null && Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.Edit, Folder.Permissions))
                {
                    folderPath = _folders.GetFolderPath(Folder);
                }
            }
            else
            {
                FolderId = -1;
                if (User.IsInRole(RoleNames.Host))
                {
                    folderPath = GetFolderPath(folder);
                }
            }

            if (!string.IsNullOrEmpty(folderPath))
            {
                CreateDirectory(folderPath);
                using (var stream = new FileStream(Path.Combine(folderPath, formfile.FileName), FileMode.Create))
                {
                    await formfile.CopyToAsync(stream);
                }

                string upload = await MergeFile(folderPath, formfile.FileName);
                if (upload != "" && FolderId != -1)
                {
                    var file = CreateFile(upload, FolderId, Path.Combine(folderPath, upload));
                    if (file != null)
                    {
                        _files.AddFile(file);
                    }
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Upload Attempt {Folder} {File}", folder, formfile.FileName);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        private async Task<string> MergeFile(string folder, string filename)
        {
            string merged = "";

            // parse the filename which is in the format of filename.ext.part_x_y
            string token = ".part_";
            string parts = Path.GetExtension(filename)?.Replace(token, ""); // returns "x_y"
            int totalparts = int.Parse(parts?.Substring(parts.IndexOf("_") + 1));

            filename = Path.GetFileNameWithoutExtension(filename); // base filename
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
                    if (!Constants.UploadableFiles.Split(',').Contains(Path.GetExtension(filename)?.ToLower().Replace(".", "")))
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
            var cleanupFiles = Directory.EnumerateFiles(folder, "*" + token + "*")
                .Where(f => Path.GetExtension(f).StartsWith(token));

            foreach (var file in cleanupFiles)
            {
                var createdDate = System.IO.File.GetCreationTime(file).ToUniversalTime();
                if (createdDate < DateTime.UtcNow.AddHours(-2))
                {
                    System.IO.File.Delete(file);
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


        /// <summary>
        /// Get file with header
        /// Content-Disposition: inline
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Disposition
        /// </summary>
        /// <param name="id">File Id from Oqtane filesystem </param>
        /// <returns>file content</returns>

        // GET api/<controller>/download/5
        [HttpGet("download/{id}")]
        public IActionResult DownloadInline(int id)
        {
            return Download(id, false);
        }
        /// <summary>
        /// Get file with header
        /// Content-Disposition: attachment; filename="filename.jpg"
        /// https://developer.mozilla.org/en-US/docs/Web/HTTP/Headers/Content-Disposition
        ///
        /// </summary>
        /// <param name="id">File Id from Oqtane filesystem</param>
        /// <returns></returns>

        // GET api/<controller>/download/5/attach
        [HttpGet("download/{id}/attach")]
        public IActionResult DownloadAttachment(int id)
        {
            return Download(id, true);
        }

        private IActionResult Download(int id, bool asAttachment)
        {
            var file = _files.GetFile(id);
            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.Permissions))
            {
                var filepath = _files.GetFilePath(file);
                if (System.IO.File.Exists(filepath))
                {
                    var result = asAttachment
                        ? PhysicalFile(filepath, file.GetMimeType(), file.Name)
                        : PhysicalFile(filepath, file.GetMimeType());
                    return result;
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Does Not Exist {FileId} {FilePath}", id, filepath);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Access Attempt {FileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            string errorPath = Path.Combine(GetFolderPath("images"), "error.png");
            return System.IO.File.Exists(errorPath) ? PhysicalFile(errorPath, MimeUtilities.GetMimeType(errorPath)) : null;
        }

        [HttpGet("image/{id}/{size}/{mode?}")]
        public IActionResult GetImage(int id, string size, string mode)
        {
            var file = _files.GetFile(id);
            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.Permissions))
            {
                if (Constants.ImageFiles.Split(',').Contains(file.Extension.ToLower()))
                {
                    var filepath = _files.GetFilePath(file);
                    if (System.IO.File.Exists(filepath))
                    {
                        size = size.ToLower();
                        mode = (string.IsNullOrEmpty(mode)) ? "crop" : mode;
                        if ((_userPermissions.IsAuthorized(User, PermissionNames.Edit, file.Folder.Permissions) || 
                          size.Contains("x") && !string.IsNullOrEmpty(file.Folder.ImageSizes) && file.Folder.ImageSizes.ToLower().Split(",").Contains(size))
                          && Enum.TryParse(mode, true, out ResizeMode resizemode))
                        {
                            var imagepath = CreateImage(filepath, size, resizemode.ToString());
                            if (!string.IsNullOrEmpty(imagepath))
                            {
                                return PhysicalFile(imagepath, file.GetMimeType());
                            }
                            else
                            {
                                _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error Creating Image For File {File} {Size}", file, size);
                                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Security, "Invalid Image Size For Folder Or Invalid Mode Specification {Folder} {Size} {Mode}", file.Folder, size, mode);
                            HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Does Not Exist {FileId} {FilePath}", id, filepath);
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Is Not An Image {File}", file);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Access Attempt {FileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            string errorPath = Path.Combine(GetFolderPath("images"), "error.png");
            return System.IO.File.Exists(errorPath) ? PhysicalFile(errorPath, MimeUtilities.GetMimeType(errorPath)) : null;
        }

        private string CreateImage(string filepath, string size, string mode)
        {
            string imagepath = filepath.Replace(Path.GetExtension(filepath), "." + size + "." + mode.ToLower() + ".png");

            if (!System.IO.File.Exists(imagepath))
            {
                try
                {
                    FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                    using (Image image = Image.Load(stream))
                    {
                        var parts = size.Split('x');
                        int width = (!string.IsNullOrEmpty(parts[0])) ? int.Parse(parts[0]) : 0;
                        int height = (!string.IsNullOrEmpty(parts[1])) ? int.Parse(parts[1]) : 0;
                        Enum.TryParse(mode, true, out ResizeMode resizemode);

                        image.Mutate(x =>
                            x.Resize(new ResizeOptions
                            {
                                Size = new Size(width, height),
                                Mode = resizemode
                            })
                            .BackgroundColor(new Rgba32(255, 255, 255, 0)));

                        image.Save(imagepath, new PngEncoder());
                    }
                    stream.Close();
                }
                catch // error creating image
                {
                    imagepath = "";
                }
            }

            return imagepath;
        }

        private string GetFolderPath(string folder)
        {
            return Utilities.PathCombine(_environment.ContentRootPath, folder);
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
                    path = Utilities.PathCombine(path, folder, Path.DirectorySeparatorChar.ToString());
                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path);
                    }
                }
            }
        }

        private Models.File CreateFile(string filename, int folderid, string filepath)
        {
            Models.File file = null;

            int size = 0;
            var folder = _folders.GetFolder(folderid);
            if (folder.Capacity != 0)
            {
                foreach (var f in _files.GetFiles(folderid))
                {
                    size += f.Size;
                }
            }

            FileInfo fileinfo = new FileInfo(filepath);
            if (folder.Capacity == 0 || ((size + fileinfo.Length) / 1000000) < folder.Capacity)
            {
                file = new Models.File();
                file.Name = filename;
                file.FolderId = folderid;

                file.Extension = fileinfo.Extension.ToLower().Replace(".", "");
                file.Size = (int)fileinfo.Length;
                file.ImageHeight = 0;
                file.ImageWidth = 0;

                if (Constants.ImageFiles.Split(',').Contains(file.Extension.ToLower()))
                {
                    try
                    {
                        FileStream stream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                        using (Image image = Image.Load(stream))
                        {
                            file.ImageHeight = image.Height;
                            file.ImageWidth = image.Width;
                        }
                        stream.Close();
                    }
                    catch
                    {
                        // error opening image file
                    }
                }
            }
            else
            {
                System.IO.File.Delete(filepath);
                _logger.Log(LogLevel.Warning, this, LogFunction.Create, "File Exceeds Folder Capacity {Folder} {File}", folder, filepath);
            }

            return file;
        }
    }
}
