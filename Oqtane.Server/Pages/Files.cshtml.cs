using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
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

        public FilesModel(IWebHostEnvironment environment, IFileRepository files, IUserPermissions userPermissions, IUrlMappingRepository urlMappings, ISyncManager syncManager, ILogManager logger, ITenantManager tenantManager)
        {
            _environment = environment;
            _files = files;
            _userPermissions = userPermissions;
            _urlMappings = urlMappings;
            _syncManager = syncManager;
            _logger = logger;
            _alias = tenantManager.GetAlias();
        }

        public IActionResult OnGet(string path)
        {
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

            if (file != null)
            {
                if (file.Folder.SiteId == _alias.SiteId && _userPermissions.IsAuthorized(User, PermissionNames.View, file.Folder.PermissionList))
                {
                    // calculate ETag using last modified date and file size
                    var etag = Convert.ToString(file.ModifiedOn.Ticks ^ file.Size, 16);

                    var header = "";
                    if (HttpContext.Request.Headers.ContainsKey(HeaderNames.IfNoneMatch))
                    {
                        header = HttpContext.Request.Headers[HeaderNames.IfNoneMatch].ToString();
                    }

                    if (!header.Equals(etag))
                    {
                        var filepath = _files.GetFilePath(file);
                        if (System.IO.File.Exists(filepath))
                        {
                            if (download)
                            {
                                _syncManager.AddSyncEvent(_alias.TenantId, EntityNames.File, file.FileId, "Download");
                                return PhysicalFile(filepath, file.GetMimeType(), file.Name);
                            }
                            else
                            {
                                HttpContext.Response.Headers.Add(HeaderNames.ETag, etag);
                                return PhysicalFile(filepath, file.GetMimeType());
                            }
                        }
                        else
                        {
                            _logger.Log(LogLevel.Error, this, LogFunction.Read, "File Does Not Exist {FilePath}", filepath);
                            HttpContext.Response.StatusCode = (int)HttpStatusCode.NotFound;
                        }
                    }
                    else
                    {
                        HttpContext.Response.StatusCode = (int)HttpStatusCode.NotModified;
                        return Content(String.Empty);
                    }
                }
                else
                {
                    _logger.Log(LogLevel.Error, this, LogFunction.Security, "Unauthorized File Access Attempt {SiteId} {Path}", _alias.SiteId, path);
                    HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                }
            }
            else
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
                    return RedirectPermanent(url);
                }
            }

            // broken link
            string errorPath = Path.Combine(Utilities.PathCombine(_environment.ContentRootPath, "wwwroot/images"), "error.png");
            return PhysicalFile(errorPath, MimeUtilities.GetMimeType(errorPath));
        }
    }
}
