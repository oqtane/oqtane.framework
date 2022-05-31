using System;
using System.Collections;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Oqtane.Shared;

namespace Oqtane.Infrastructure;

public class ContentManager : IContentManager
{
    private readonly IConfiguration _config;
    private readonly IWebHostEnvironment _environment;
    private string _root;

    public ContentManager(IConfiguration config, IWebHostEnvironment environment)
    {
        _config = config;
        _environment = environment;
    }

    private string GetContentRoot()
    {
        var root = _config.GetValue(SettingKeys.ContentRootKey, String.Empty);
        if (root.IsNullOrEmpty()) return _environment.ContentRootPath;
        if (Path.IsPathRooted(root)) return root;
        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, root!));
    }

    public string GetContentPath(params string[] segments)
    {
        var p = Path.Combine(ContentRootPath, Utilities.PathCombine(segments));
        return p;
    }

    public string ContentRootPath => _root??=GetContentRoot();
}
