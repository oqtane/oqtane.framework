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
using System.Net.Http;
using Microsoft.AspNetCore.Cors;
using System.IO.Compression;
using Oqtane.Services;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using Oqtane.Providers;

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
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;
        private readonly ISettingRepository _settingRepository;
        private readonly IImageService _imageService;
        private readonly IFolderProviderFactory _folderProviderFactory;

        public FileController(
            IWebHostEnvironment environment,
            IFileRepository files,
            IFolderRepository folders,
            IUserPermissions userPermissions,
            ISettingRepository settingRepository,
            ISyncManager syncManager,
            ILogManager logger,
            ITenantManager tenantManager,
            IImageService imageService,
            IFolderProviderFactory folderProviderFactory)
        {
            _environment = environment;
            _files = files;
            _folders = folders;
            _userPermissions = userPermissions;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
            _settingRepository = settingRepository;
            _imageService = imageService;
            _folderProviderFactory = folderProviderFactory;
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
                if (Folder != null && Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.Browse, Folder.PermissionList))
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
            if (folder != null && folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.Browse, folder.PermissionList))
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
            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.PermissionList))
            {
                return file;
            }
            else
            {
                if (file != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Get Attempt {FileId}", id);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        [HttpGet("name/{name}/{folderId}")]
        public Models.File Get(string name, int folderId)
        {
            Models.File file = _files.GetFile(folderId, name);
            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.PermissionList))
            {
                return file;
            }
            else
            {
                if (file != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Get Attempt {Name} For Folder {FolderId}", name, folderId);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
                return null;
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Authorize(Roles = RoleNames.Registered)]
        public async Task<Models.File> Post([FromBody] Models.File file)
        {
            var folder = _folders.GetFolder(file.FolderId);
            if (ModelState.IsValid && folder != null && folder.SiteId == _alias.SiteId)
            {
                if (_userPermissions.IsAuthorized(User, folder.SiteId, EntityNames.Folder, file.FolderId, PermissionNames.Edit))
                {
                    if (HasValidFileExtension(file.Name) && file.Name.IsPathOrFileValid())
                    {
                        var folderProvider = _folderProviderFactory.GetProvider(folder.FolderConfigId);
                        if (await folderProvider.FileExistsAsync(folder, file.Name))
                        {
                            file = await CreateFileAsync(folderProvider, folder.FolderId, file.Name);
                            if (file != null)
                            {
                                file = _files.AddFile(file);
                                _syncManager.AddSyncEvent(_alias, EntityNames.File, file.FileId, SyncEventActions.Create);
                                _logger.Log(LogLevel.Information, this, LogFunction.Create, "File Added {File}", file);
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Does Not Exist At Path {Folder}{FilePath}", folder.Path, file.Name);
                            HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                            file = null;
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Name Is Invalid Or Contains Invalid Extension {File}", file.Name);
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        file = null;
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Post Attempt {File}", file);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    file = null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Post Attempt {File}", file);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                file = null;
            }

            return file;
        }

        // PUT api/<controller>/5
        [HttpPut("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public async Task<Models.File> Put(int id, [FromBody] Models.File file)
        {
            var exsitingFile = _files.GetFile(file.FileId, false);
            if (ModelState.IsValid && file.Folder.SiteId == _alias.SiteId && file.FileId == id && exsitingFile != null // ensure file exists
                && _userPermissions.IsAuthorized(User, file.Folder.SiteId, EntityNames.Folder, exsitingFile.FolderId, PermissionNames.Edit) // ensure user had edit rights to original folder
                && _userPermissions.IsAuthorized(User, file.Folder.SiteId, EntityNames.Folder, file.FolderId, PermissionNames.Edit)) // ensure user has edit rights to new folder
            {
                if (HasValidFileExtension(file.Name) && file.Name.IsPathOrFileValid())
                {
                    file.Folder = _folders.GetFolder(file.FolderId, false);
                    var folderProvider = _folderProviderFactory.GetProvider(file.Folder.FolderConfigId);
                    if (exsitingFile.Name != file.Name || exsitingFile.FolderId != file.FolderId)
                    {
                        await folderProvider.MoveFileAsync(exsitingFile, file.Folder, file.Name);
                    }

                    var newfile = await CreateFileAsync(folderProvider, file.Folder.FolderId, file.Name);
                    if (newfile != null)
                    {
                        file.Extension = newfile.Extension;
                        file.Size = newfile.Size;
                        file.ImageWidth = newfile.ImageWidth;
                        file.ImageHeight = newfile.ImageHeight;
                    }

                    file = _files.UpdateFile(file);
                    _syncManager.AddSyncEvent(_alias, EntityNames.File, file.FileId, SyncEventActions.Update);
                    _logger.Log(LogLevel.Information, this, LogFunction.Update, "File Updated {File}", file);
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Name Is Invalid Or Contains Invalid Extension {File}", file.Name);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    file = null;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Put Attempt {File}", file);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                file = null;
            }

            return file;
        }

        // PUT api/<controller>/unzip/5
        [HttpPut("unzip/{id}")]
        [Authorize(Roles = RoleNames.Admin)]
        public async Task Unzip(int id)
        {
            var zipfile = _files.GetFile(id, false);
            if (zipfile != null && zipfile.Folder.SiteId == _alias.SiteId && zipfile.Extension.ToLower() == "zip")
            {
                // extract files
                var folderProvider = _folderProviderFactory.GetProvider(zipfile.Folder.FolderConfigId);
                using (var archive = new ZipArchive(await folderProvider.GetFileStreamAsync(zipfile)))
                {
                    foreach (var entry in archive.Entries)
                    {
                        if (HasValidFileExtension(entry.Name) && entry.Name.IsPathOrFileValid())
                        {
                            using var memoryStream = new MemoryStream();
                            await (await entry.OpenAsync()).CopyToAsync(memoryStream);
                            await folderProvider.AddFileAsync(zipfile.Folder, entry.Name, memoryStream);
                            var file = await CreateFileAsync(folderProvider, zipfile.Folder.FolderId, entry.Name);
                            if (file != null)
                            {
                                if (file.FileId == 0)
                                {
                                    file = _files.AddFile(file);
                                }
                                else
                                {
                                    file = _files.UpdateFile(file);
                                }
                                _syncManager.AddSyncEvent(_alias, EntityNames.File, file.FileId, SyncEventActions.Create);
                                _logger.Log(LogLevel.Information, this, LogFunction.Create, "File Extracted {File}", file);
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Name Is Invalid Or Contains Invalid Extension {File}", entry.Name);
                        }
                    }
                }

                // delete zip file
                _files.DeleteFile(zipfile.FileId);
                await folderProvider.DeleteFileAsync(zipfile);

                _logger.Log(LogLevel.Information, this, LogFunction.Create, "Zip File Removed {File}", zipfile);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Unzip Attempt {FileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // DELETE api/<controller>/5
        [HttpDelete("{id}")]
        [Authorize(Roles = RoleNames.Registered)]
        public async Task Delete(int id)
        {
            Models.File file = _files.GetFile(id);
            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, file.Folder.SiteId, EntityNames.Folder, file.Folder.FolderId, PermissionNames.Edit))
            {
                var folderProvider = _folderProviderFactory.GetProvider(file.Folder.FolderConfigId);
                await folderProvider.DeleteFileAsync(file);

                _files.DeleteFile(id);
                _syncManager.AddSyncEvent(_alias, EntityNames.File, file.FileId, SyncEventActions.Delete);
                _logger.Log(LogLevel.Information, this, LogFunction.Delete, "File Deleted {File}", file);
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Delete Attempt {FileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }
        }

        // GET api/<controller>/upload?url=x&folderid=y&name=z
        [HttpGet("upload")]
        public async Task<Models.File> UploadFile(string url, string folderid, string name)
        {
            Models.File file = null;

            Folder folder = null;
            int FolderId;
            if (int.TryParse(folderid, out FolderId))
            {
                folder = _folders.GetFolder(FolderId);
            }

            if (folder != null && folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.Edit, folder.PermissionList))
            {
                if (string.IsNullOrEmpty(name))
                {
                    name = url.Substring(url.LastIndexOf("/", StringComparison.Ordinal) + 1);
                }

                if (HasValidFileExtension(name) && name.IsPathOrFileValid())
                {
                    try
                    {
                        var folderProvider = _folderProviderFactory.GetProvider(folder.FolderConfigId);
                        using (var client = new HttpClient())
                        {
                            using (var stream = await client.GetStreamAsync(url))
                            {
                                await folderProvider.AddFileAsync(folder, name, stream);
                            }
                        }

                        file = await CreateFileAsync(folderProvider, folder.FolderId, name);
                        if (file != null)
                        {
                            file = _files.AddFile(file);
                            _logger.Log(LogLevel.Information, this, LogFunction.Create, "File Downloaded {File}", file);
                            _syncManager.AddSyncEvent(_alias, EntityNames.File, file.FileId, SyncEventActions.Create);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "File Could Not Be Downloaded From Url {Url} {Error}", url, ex.Message);
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Name Is Invalid Or Contains Invalid Extension {File}", name);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    file = null;
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
        [EnableCors(Constants.MauiCorsPolicy)]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] string folder, IFormFile formfile)
        {
            if (string.IsNullOrEmpty(folder))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Upload Does Not Contain A Folder");
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            if (formfile == null || formfile.Length <= 0)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Upload Does Not Contain A File");
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // ensure filename is valid
            string fileName = formfile.FileName;
            if (Path.GetExtension(fileName).Contains(':'))
            {
                fileName = fileName.Substring(0, fileName.LastIndexOf(':')); // remove invalid suffix from extension
            }
            if (!fileName.IsPathOrFileValid() || !HasValidFileExtension(fileName))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Upload File Name Is Invalid Or Contains Invalid Extension {File}", fileName);
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // ensure headers exist
            if (!Request.Headers.TryGetValue("PartCount", out StringValues partcount) || !int.TryParse(partcount, out int partCount) || partCount <= 0 ||
                !Request.Headers.TryGetValue("TotalParts", out StringValues totalparts) || !int.TryParse(totalparts, out int totalParts) || totalParts <= 0)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Upload Is Missing Required Headers");
                return StatusCode((int)HttpStatusCode.Forbidden);
            }

            // create file name using header part values
            fileName += ".part_" + partCount.ToString("000") + "_" + totalParts.ToString("000");
            

            try
            {
                Models.Folder uploadFolder = null;
                if (int.TryParse(folder, out int folderId))
                {
                    Folder Folder = _folders.GetFolder(folderId);
                    if (Folder != null && Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.Edit, Folder.PermissionList))
                    {
                        uploadFolder = Folder;
                    }
                }

                if (uploadFolder != null || User.IsInRole(RoleNames.Host))
                {
                    var tempFolder = Path.GetTempPath();
                    using (var stream = new FileStream(Path.Combine(tempFolder, fileName), FileMode.Create))
                    {
                        await formfile.CopyToAsync(stream);
                    }

                    string upload = await MergeFile(tempFolder, fileName);
                    if (upload != "")
                    {
                        if (uploadFolder != null)
                        {
                            var folderProvider = _folderProviderFactory.GetProvider(uploadFolder.FolderConfigId);
                            using var stream = System.IO.File.OpenRead(Path.Combine(tempFolder, upload));

                            await folderProvider.AddFileAsync(uploadFolder, upload, stream);

                            var file = await CreateFileAsync(folderProvider, uploadFolder.FolderId, upload);
                            if (file != null)
                            {
                                if (file.FileId == 0)
                                {
                                    file = _files.AddFile(file);
                                }
                                else
                                {
                                    file = _files.UpdateFile(file);
                                }
                                _logger.Log(LogLevel.Information, this, LogFunction.Create, "File Uploaded {File}", Path.Combine(uploadFolder.Path, upload));
                                _syncManager.AddSyncEvent(_alias, EntityNames.File, file.FileId, SyncEventActions.Create);
                            }
                        }
                        else
                        {
                            var folderPath = GetFolderPath(folder);
                            var fileInfo = new FileInfo(Path.Combine(tempFolder, upload));
                            fileInfo.MoveTo(folderPath);
                        }
                    }
                    
                    return NoContent();
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Upload Attempt {Folder} {File}", folder, formfile.FileName);
                    return StatusCode((int)HttpStatusCode.Forbidden);
                }
            }
            catch (Exception ex)
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Create, ex, "File Upload Attempt Failed {Folder} {File}", folder, formfile.FileName);
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
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
        public async Task<IActionResult> DownloadInlineAsync(int id)
        {
            return await Download(id, false);
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
        public async Task<IActionResult> DownloadAttachmentAsync(int id)
        {
            return await Download(id, true);
        }

        private async Task<string> MergeFile(string folder, string filename)
        {
            string merged = "";

            // parse the filename which is in the format of filename.ext.part_001_999
            string token = ".part_";
            string parts = Path.GetExtension(filename)?.Replace(token, ""); // returns "001_999"
            int totalparts = int.Parse(parts?.Substring(parts.IndexOf("_") + 1));

            filename = Path.GetFileNameWithoutExtension(filename); // base filename including original file extension
            string[] fileparts = Directory.GetFiles(folder, filename + token + "*"); // list of all file parts

            // if all of the file parts exist (note that file parts can arrive out of order)
            if (fileparts.Length == totalparts && CanAccessFiles(fileparts))
            {
                // merge file parts into temp file (in case another user is trying to read the file)
                bool success = true;
                using (var stream = new FileStream(Path.Combine(folder, filename + ".tmp"), FileMode.Create))
                {
                    foreach (string filepart in fileparts.Order())
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

                // clean up file parts
                foreach (var file in fileparts)
                {
                    try
                    {
                        System.IO.File.Delete(file);
                    }
                    catch
                    {
                        // unable to delete part - ignore
                    }
                }

                // rename temp file
                if (success)
                {
                    // remove existing file (as well as any thumbnails)
                    foreach (var file in Directory.GetFiles(folder, Path.GetFileNameWithoutExtension(filename) + ".*"))
                    {
                        if (Path.GetExtension(file) != ".tmp")
                        {
                            System.IO.File.Delete(file);
                        }
                    }

                    // rename temp file now that the entire process is completed
                    System.IO.File.Move(Path.Combine(folder, filename + ".tmp"), Path.Combine(folder, filename));

                    // return filename
                    merged = filename;
                }
            }

            return merged;
        }

        private bool CanAccessFiles(string[] files)
        {
            // ensure files are not locked by another process
            FileStream stream = null;
            bool locked = false;
            foreach (string file in files)
            {
                locked = true;
                int attempts = 0;
                // note that this will wait a maximum of 15 seconds for a file to become unlocked
                while (attempts < 5 && locked)
                {
                    try
                    {
                        stream = System.IO.File.Open(file, FileMode.Open, FileAccess.Read, FileShare.None);
                        locked = false;
                    }
                    catch // file is locked by another process
                    {
                        attempts += 1;
                        Thread.Sleep(1000 * attempts); // progressive retry
                    }
                    finally
                    {
                        if (stream != null)
                        {
                            stream.Close();
                        }
                    }
                }
                if (locked)
                {
                    break; // file still locked after retrying
                }
            }
            return !locked;
        }

        private async Task<IActionResult> Download(int id, bool asAttachment)
        {
            var file = _files.GetFile(id);
            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.PermissionList))
            {
                var folderProvider = _folderProviderFactory.GetProvider(file.Folder.FolderConfigId);
                if (await folderProvider.FileExistsAsync(file.Folder, file.Name))
                {
                    if (asAttachment)
                    {
                        _syncManager.AddSyncEvent(_alias, EntityNames.File, file.FileId, "Download");
                        return File(await folderProvider.GetFileStreamAsync(file), file.GetMimeType(), file.Name);
                    }
                    else
                    {
                        return File(await folderProvider.GetFileStreamAsync(file), file.GetMimeType());
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Does Not Exist {FileId} {Folder}{FileName}", id, file.Folder.Path, file.Name);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }
            else
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Access Attempt {FileId}", id);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            }

            string errorPath = Path.Combine(GetFolderPath("wwwroot/images"), "error.png");
            return System.IO.File.Exists(errorPath) ? PhysicalFile(errorPath, MimeUtilities.GetMimeType(errorPath)) : null;
        }

        [HttpGet("image/{id}/{width}/{height}/{mode}/{position}/{background}/{rotate}/{recreate}")]
        public async Task<IActionResult> GetImageAsync(int id, int width, int height, string mode, string position, string background, string rotate, string recreate)
        {
            var file = _files.GetFile(id);

            var _ImageFiles = _settingRepository.GetSetting(EntityNames.Site, _alias.SiteId, "ImageFiles")?.SettingValue;
            _ImageFiles = (string.IsNullOrEmpty(_ImageFiles)) ? Constants.ImageFiles : _ImageFiles;

            if (file != null && file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.PermissionList))
            {
                var folderProvider = _folderProviderFactory.GetProvider(file.Folder.FolderConfigId);
                if (_ImageFiles.Split(',').Contains(file.Extension.ToLower()))
                {
                    if (await folderProvider.FileExistsAsync(file.Folder, file.Name))
                    {
                        if (!bool.TryParse(recreate, out _)) recreate = "false";

                        var format = "png";
                        Stream imageStream = null;
                        var imagename = file.Name.Replace(Path.GetExtension(file.Name), "." + width.ToString() + "x" + height.ToString() + "." + format);
                        if (!await folderProvider.FileExistsAsync(file.Folder, imagename) || bool.Parse(recreate))
                        {
                            // user has edit access to folder or folder supports the image size being created
                            if (_userPermissions.IsAuthorized(User, PermissionNames.Edit, file.Folder.PermissionList) ||
                              (!string.IsNullOrEmpty(file.Folder.ImageSizes) && (file.Folder.ImageSizes == "*" || file.Folder.ImageSizes.ToLower().Split(",").Contains(width.ToString() + "x" + height.ToString()))))
                            {
                                imageStream = await _imageService.CreateImageAsync(file, width, height, mode, position, background, rotate, format, imagename);
                            }
                            else
                            {
                                _logger.Log(LogLevel.Error, this, LogFunction.Security, "Invalid Image Size For Folder {Folder} {Width} {Height}", file.Folder, width, height);
                                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                            }
                        }
                        if (imageStream != null)
                        {
                            if (!string.IsNullOrEmpty(file.Folder.CacheControl))
                            {
                                HttpContext.Response.Headers.Append(HeaderNames.CacheControl, value: file.Folder.CacheControl);
                            }
                            return File(imageStream, file.GetMimeType());
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error Displaying Image For File {File} {Width} {Height}", file, width, height);
                            HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Does Not Exist {FileId} {Folder}{FilePath}", id, file.Folder.Path, file.Name);
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
                if (file != null)
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Access Attempt {FileId}", id);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
                else
                {
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                }
            }

            string errorPath = Path.Combine(GetFolderPath("wwwroot/images"), "error.png");
            return System.IO.File.Exists(errorPath) ? PhysicalFile(errorPath, MimeUtilities.GetMimeType(errorPath)) : null;
        }

        private string GetFolderPath(string folder)
        {
            return Utilities.PathCombine(_environment.ContentRootPath, folder);
        }

        

        private async Task<Models.File> CreateFileAsync(IFolderProvider folderProvider, int folderid, string fileName)
        {
            var _ImageFiles = _settingRepository.GetSetting(EntityNames.Site, _alias.SiteId, "ImageFiles")?.SettingValue;
            _ImageFiles = (string.IsNullOrEmpty(_ImageFiles)) ? Constants.ImageFiles : _ImageFiles;

            int size = 0;
            var folder = _folders.GetFolder(folderid, false);
            if (folder.Capacity != 0)
            {
                foreach (var f in _files.GetFiles(folderid, false))
                {
                    size += f.Size;
                }
            }

            var exsitingFile = _files.GetFile(folderid, fileName);
            var fileSize = await folderProvider.GetFileSizeAsync(folder, fileName);
            if (folder.Capacity == 0 || ((size + fileSize) / 1000000) < folder.Capacity)
            {
                
                if (exsitingFile == null)
                {
                    exsitingFile = new Models.File();
                }

                exsitingFile.Name = fileName;
                exsitingFile.FolderId = folderid;

                exsitingFile.Extension = Path.GetExtension(fileName).ToLower().Replace(".", "");
                exsitingFile.Size = (int)fileSize;
                exsitingFile.ImageHeight = 0;
                exsitingFile.ImageWidth = 0;

                if (_ImageFiles.Split(',').Contains(exsitingFile.Extension))
                {
                    try
                    {
                        using var stream = await folderProvider.GetFileStreamAsync(exsitingFile);
                        using var image = Image.Load(stream);
                        exsitingFile.ImageHeight = image.Height;
                        exsitingFile.ImageWidth = image.Width;
                    }
                    catch
                    {
                        // error opening image file
                    }
                }
            }
            else
            {
                await folderProvider.DeleteFileAsync(exsitingFile);
                if (exsitingFile != null)
                {
                    _files.DeleteFile(exsitingFile.FileId);
                }
                exsitingFile = null;
                _logger.Log(LogLevel.Warning, this, LogFunction.Create, "File Exceeds Folder Capacity And Has Been Removed {Folder} {File}", folder, fileName);
            }

            return exsitingFile;
        }

        private bool HasValidFileExtension(string fileName)
        {
            var _uploadableFiles = _settingRepository.GetSetting(EntityNames.Site, _alias.SiteId, "UploadableFiles")?.SettingValue;
            _uploadableFiles = (string.IsNullOrEmpty(_uploadableFiles)) ? Constants.UploadableFiles : _uploadableFiles;
            return _uploadableFiles.Split(',').Contains(Path.GetExtension(fileName).ToLower().Replace(".", ""));
        }
    }
}
