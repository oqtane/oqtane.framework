using System;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Net.Http.Headers;
using Oqtane.Enums;
using Oqtane.Extensions;
using Oqtane.Infrastructure;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Security;
using Oqtane.Services;
using Oqtane.Shared;

namespace Oqtane.Pages
{
    [AllowAnonymous]
    public class FilesModel : PageModel
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IFileRepository _files;
        private readonly IUserPermissions _userPermissions;
        private readonly IUrlMappingRepository _urlMappings;
        private readonly ISyncManager _syncManager;
        private readonly ILogManager _logger;
        private readonly Alias _alias;
        private readonly IImageService _imageService;
        private readonly ISettingRepository _settingRepository;

        public FilesModel(IWebHostEnvironment environment, IFileRepository files, IUserPermissions userPermissions, IUrlMappingRepository urlMappings, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager, IImageService imageService, ISettingRepository settingRepository)
        {
            _environment = environment;
            _files = files;
            _userPermissions = userPermissions;
            _urlMappings = urlMappings;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
            _imageService = imageService;
            _settingRepository = settingRepository;
        }

        public IActionResult OnGet(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                return BrokenFile();
            }

            path = path.Replace("\\", "/");
            var folderpath = "";
            var filename = "";

            bool download = false;
            if (Request.Query.ContainsKey("download"))
            {
                download = true;
            }

            var segments = path.Split('/');
            if (segments.Length > 0)
            {
                filename = segments[segments.Length - 1].ToLower();
                if (segments.Length > 1)
                {
                    folderpath = string.Join("/", segments, 0, segments.Length - 1).ToLower() + "/";
                }
            }

            Models.File file;
            if (folderpath == "id/" && int.TryParse(filename, out int fileid))
            {
                file = _files.GetFile(fileid, false);
            }
            else
            {
                file = _files.GetFile(_alias.SiteId, folderpath, filename);
            }

            if (file == null)
            {
                // look for url mapping

                var urlMapping = _urlMappings.GetUrlMapping(_alias.SiteId, "files/" + folderpath + filename);
                if (urlMapping != null && !string.IsNullOrEmpty(urlMapping.MappedUrl))
                {
                    var url = urlMapping.MappedUrl;
                    if (!url.StartsWith("http"))
                    {
                        var uri = new Uri(HttpContext.Request.GetEncodedUrl());
                        url = uri.Scheme + "://" + uri.Authority + ((!string.IsNullOrEmpty(_alias.Path)) ? "/" + _alias.Path : "") + "/" + url;
                    }

                    // appends the query string to the redirect url
                    if (Request.QueryString.HasValue && !string.IsNullOrWhiteSpace(Request.QueryString.Value))
                    {
                        if (url.Contains('?'))
                        {
                            url += "&";
                        }
                        else
                        {
                            url += "?";
                        }

                        url += Request.QueryString.Value.Substring(1);
                    }

                    return RedirectPermanent(url);
                }

                return BrokenFile();
            }

            if (file.Folder.SiteId != _alias.SiteId || !_userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.PermissionList))
            {
                if (!User.Identity.IsAuthenticated && download)
                {
                    return Redirect(Utilities.NavigateUrl(_alias.Path, "login", "?returnurl=" + WebUtility.UrlEncode(Request.Path)));
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Access Attempt For Site {SiteId} And Path {Path}", _alias.SiteId, path);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return BrokenFile();
                }
            }

            string etag = Convert.ToString(file.ModifiedOn.Ticks ^ file.Size, 16);
            string downloadName = file.Name;
            string filepath = _files.GetFilePath(file);

            var header = "";
            if (HttpContext.Request.Headers.TryGetValue(HeaderNames.IfNoneMatch, out var ifNoneMatch))
            {
                header = ifNoneMatch.ToString();
            }

            if (header.Equals(etag))
            {
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
                return Content(String.Empty);
            }

            if (!System.IO.File.Exists(filepath))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Does Not Exist {FilePath}", filepath);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return BrokenFile();
            }

            // evaluate any querystring parameters
            bool isRequestingImageManipulation = false;

            int width = 0;
            int height = 0;
            if (Request.Query.TryGetValue("width", out var widthStr) && int.TryParse(widthStr, out width) && width > 0)
            {
                isRequestingImageManipulation = true;
            }
            if (Request.Query.TryGetValue("height", out var heightStr) && int.TryParse(heightStr, out height) && height > 0)
            {
                isRequestingImageManipulation = true;
            }

            Request.Query.TryGetValue("mode", out var mode);
            Request.Query.TryGetValue("position", out var position);
            Request.Query.TryGetValue("background", out var background);

            int rotate;
            if (Request.Query.TryGetValue("rotate", out var rotateStr) && int.TryParse(rotateStr, out rotate) && 360 > rotate && rotate > 0)
            {
                isRequestingImageManipulation = true;
            }
            if (Request.Query.TryGetValue("format", out var format) && _imageService.GetAvailableFormats().Contains(format.ToString()))
            {
                isRequestingImageManipulation = true;
            }

            if (isRequestingImageManipulation)
            {
                var _ImageFiles = _settingRepository.GetSetting(EntityNames.Site, _alias.SiteId, "ImageFiles")?.SettingValue;
                _ImageFiles = (string.IsNullOrEmpty(_ImageFiles)) ? Constants.ImageFiles : _ImageFiles;

                if (!_ImageFiles.Split(',').Contains(file.Extension.ToLower()))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "File Is Not An Image {File}", file);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    return BrokenFile();
                }

                Request.Query.TryGetValue("recreate", out var recreate);

                if (!bool.TryParse(recreate, out _)) recreate = "false";
                if (!_imageService.GetAvailableFormats().Contains(format.ToString())) format = "png";
                if (width == 0 && height == 0)
                {
                    width = file.ImageWidth;
                    height = file.ImageHeight;
                }

                string imagepath = filepath.Replace(Path.GetExtension(filepath), "." + width.ToString() + "x" + height.ToString() + "." + format);
                if (!System.IO.File.Exists(imagepath) || bool.Parse(recreate))
                {
                    // user has edit access to folder or folder supports the image size being created
                    if (_userPermissions.IsAuthorized(User, PermissionNames.Edit, file.Folder.PermissionList) ||
                        (!string.IsNullOrEmpty(file.Folder.ImageSizes) && (file.Folder.ImageSizes == "*" || file.Folder.ImageSizes.ToLower().Split(",").Contains(width.ToString() + "x" + height.ToString()))))
                    {
                        imagepath = _imageService.CreateImage(filepath, width, height, mode, position, background, rotateStr, format, imagepath);
                    }
                    else
                    {
                        _logger.Log(LogLevel.Error, this, LogFunction.Security, "Invalid Image Size For Folder {Folder} {Width} {Height}", file.Folder, width, height);
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        return BrokenFile();
                    }
                }

                if (string.IsNullOrWhiteSpace(imagepath))
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Create, "Error Displaying Image For File {File} {Width} {Height}", file, widthStr, heightStr);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    return BrokenFile();
                }

                downloadName = file.Name.Replace(Path.GetExtension(filepath), "." + width.ToString() + "x" + height.ToString() + "." + format);
                filepath = imagepath;
            }

            if (!System.IO.File.Exists(filepath))
            {
                _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Does Not Exist {FilePath}", filepath);
                HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                return BrokenFile();
            }

            if (download)
            {
                _syncManager.AddSyncEvent(_alias, EntityNames.File, file.FileId, "Download");
                return PhysicalFile(filepath, MimeUtilities.GetMimeType(downloadName), downloadName);
            }
            else
            {
                if (!string.IsNullOrEmpty(file.Folder.CacheControl))
                {
                    HttpContext.Response.Headers.Append(HeaderNames.CacheControl, value: file.Folder.CacheControl);
                }
                HttpContext.Response.Headers.Append(HeaderNames.ETag, etag);
                return PhysicalFile(filepath, MimeUtilities.GetMimeType(downloadName));
            }
        }

        private PhysicalFileResult BrokenFile()
        {
            // broken link
            string errorPath = Path.Combine(Utilities.PathCombine(_environment.ContentRootPath, "wwwroot/images"), "error.png");
            return PhysicalFile(errorPath, MimeUtilities.GetMimeType(errorPath));
        }
    }
}
