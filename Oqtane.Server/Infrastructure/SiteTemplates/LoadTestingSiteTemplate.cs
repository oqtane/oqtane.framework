using Oqtane.Models;
using Oqtane.Infrastructure;
using System.Collections.Generic;
using Oqtane.Repository;
using Microsoft.AspNetCore.Hosting;
using Oqtane.Shared;
using System.IO;
using System;

namespace Oqtane.SiteTemplates
{
    public class LoadTestingSiteTemplate : ISiteTemplate
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ISiteRepository _siteRepository;
        private readonly IFolderRepository _folderRepository;
        private readonly IFileRepository _fileRepository;

        int _pages = 5; // root level navigation items
        int _children = 10; // submenu items
        int _hidden = 150; // hidden pages not part of navigation
        int _modules = 10; // modules per page
        int _panes = 10; // panes per page (22 max in default theme)

        string[] _content = new string[5]; // random content
        string[] _panenames = new string[5]; // default theme panes

        public LoadTestingSiteTemplate(IWebHostEnvironment environment, ISiteRepository siteRepository, IFolderRepository folderRepository, IFileRepository fileRepository)
        {
            _environment = environment;
            _siteRepository = siteRepository;
            _folderRepository = folderRepository;
            _fileRepository = fileRepository;

            _content[0] = "<p>Lorem ipsum dolor sit amet. Nam atque accusantium vel omnis obcaecati nam magnam fugit aut omnis repudiandae. Non reiciendis inventore sit voluptas vero et modi distinctio qui voluptate corrupti qui neque minima aut culpa rerum? Cum dolorem cupiditate ut voluptatem tempore aut sunt dolor aut facilis veniam. Sit culpa sapiente est consequatur saepe ut dolore quasi id fugit totam non debitis natus ea quis autem?</p><p>Et quasi veritatis ad aliquam beatae in voluptatem galisum in sunt ducimus. Sed corporis autem ut voluptatum cumque aut nisi expedita et nulla aliquam qui placeat aperiam? Est blanditiis nostrum et laborum mollitia non consequatur molestiae. Sed iste ducimus est eaque animi 33 autem quaerat.</p>";
            _content[1] = "<p>Consectetur adipiscing elit, sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ornare aenean euismod elementum nisi quis eleifend quam. Mauris pharetra et ultrices neque ornare aenean euismod. Amet consectetur adipiscing elit duis tristique. Elit scelerisque mauris pellentesque pulvinar pellentesque. Tellus in metus vulputate eu. Risus nec feugiat in fermentum posuere urna nec tincidunt. Porttitor leo a diam sollicitudin tempor id eu. Elit sed vulputate mi sit. Aliquet bibendum enim facilisis gravida. Dictum varius duis at consectetur lorem donec massa sapien. Ipsum nunc aliquet bibendum enim facilisis. In eu mi bibendum neque egestas congue quisque. Eget duis at tellus at. Nunc sed velit dignissim sodales ut. Elementum curabitur vitae nunc sed velit. Lacinia quis vel eros donec ac odio tempor orci. Id volutpat lacus laoreet non curabitur gravida arcu ac tortor. Facilisis mauris sit amet massa vitae tortor.</p>";
            _content[2] = "<p>Rhoncus dolor purus non enim praesent elementum facilisis leo. Aenean pharetra magna ac placerat vestibulum lectus mauris. Facilisis leo vel fringilla est ullamcorper eget nulla facilisi. Elementum nibh tellus molestie nunc. Id leo in vitae turpis massa sed elementum. Feugiat in ante metus dictum at tempor. Elit sed vulputate mi sit amet mauris commodo quis imperdiet. Non quam lacus suspendisse faucibus interdum posuere lorem ipsum. Mi quis hendrerit dolor magna eget est lorem. Integer malesuada nunc vel risus commodo viverra maecenas accumsan. Odio ut enim blandit volutpat. Nec sagittis aliquam malesuada bibendum arcu vitae elementum. Volutpat maecenas volutpat blandit aliquam etiam erat velit scelerisque in. Egestas integer eget aliquet nibh. Pellentesque habitant morbi tristique senectus et. Fermentum leo vel orci porta non pulvinar neque. Ut sem nulla pharetra diam sit amet. Proin nibh nisl condimentum id venenatis.</p>";
            _content[3] = "<p>Pellentesque habitant morbi tristique senectus. In metus vulputate eu scelerisque felis imperdiet proin fermentum. Dolor purus non enim praesent. Sed adipiscing diam donec adipiscing tristique risus nec feugiat. Elementum nisi quis eleifend quam adipiscing. Dapibus ultrices in iaculis nunc sed augue lacus viverra. Enim blandit volutpat maecenas volutpat. Ut placerat orci nulla pellentesque dignissim enim sit amet venenatis. Eget gravida cum sociis natoque penatibus et magnis dis parturient. In hac habitasse platea dictumst quisque sagittis purus. Scelerisque varius morbi enim nunc faucibus a pellentesque sit. Pretium viverra suspendisse potenti nullam ac tortor vitae purus. Sagittis orci a scelerisque purus semper eget duis at tellus. Sed vulputate mi sit amet mauris commodo. Tincidunt praesent semper feugiat nibh sed. Condimentum vitae sapien pellentesque habitant morbi tristique senectus et. Elementum eu facilisis sed odio morbi quis. Mauris in aliquam sem fringilla ut morbi tincidunt augue. Diam quam nulla porttitor massa id neque.</p>";
            _content[4] = "<p>Est nemo nemo hic quisquam dolores et dignissimos voluptas? Est adipisci quos et omnis dolorum qui galisum vero ut galisum accusantium vel accusamus odit ab fuga voluptatem. At officiis illo quo quia modi eos dicta dolor sed voluptates quis id eligendi distinctio sed ratione consequatur. Et voluptatibus consequatur quo voluptatem architecto est sapiente voluptas.</p>";

            _panenames = (PaneNames.Default + ",Top Full Width,Top 100%,Left 50%,Right 50%,Left 33%,Center 33%,Right 33%,Left Outer 25%,Left Inner 25%,Right Inner 25%,Right Outer 25%,Left 25%,Center 50%,Right 25%,Left Sidebar 66%,Right Sidebar 33%,Left Sidebar 33%,Right Sidebar 66%,Bottom 100%,Bottom Full Width,Footer").Split(',');
        }

        public string Name
        {
            get { return "Load Testing Site Template"; }
        }

        public List<PageTemplate> CreateSite(Site site)
        {
            List<PageTemplate> _pageTemplates = new List<PageTemplate>();

            // pages
            for (int page = 1; page <= _pages; page++)
            {
                _pageTemplates.Add(new PageTemplate
                {
                    Name = $"Page{page}",
                    Parent = "",
                    Path = (page == 1) ? "/" : $"page{page}",
                    Order = (page * 2) - 1 + 10,
                    Icon = "oi oi-home",
                    IsNavigation = true,
                    IsClickable = true,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    PageTemplateModules = GetPageTemplateModules()
                });

                for (int child = 1; child <= _children; child++)
                {
                    _pageTemplates.Add(new PageTemplate
                    {
                        Name = $"Child{child}",
                        Parent = (page == 1) ? "/" : $"page{page}",
                        Path = (page == 1) ? "" : $"page{page}/child{child}",
                        Order = (child * 2) - 1,
                        Icon = "oi oi-caret-right",
                        IsNavigation = true,
                        IsClickable = true,
                        IsPersonalizable = false,
                        PermissionList = new List<Permission>
                        {
                            new Permission(PermissionNames.View, RoleNames.Everyone, true),
                            new Permission(PermissionNames.View, RoleNames.Admin, true),
                            new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                        },
                        PageTemplateModules = GetPageTemplateModules()
                    });
                }
            }

            // hidden pages
            for (int hidden = 1; hidden <= _hidden; hidden++)
            {
                _pageTemplates.Add(new PageTemplate
                {
                    Name = $"Hidden{hidden}",
                    Parent = "",
                    Path = $"hidden{hidden}",
                    Order = (hidden * 2) - 1 + 10,
                    Icon = "",
                    IsNavigation = false,
                    IsClickable = false,
                    IsPersonalizable = false,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    PageTemplateModules = GetPageTemplateModules()
                });
            }

            if (System.IO.File.Exists(Path.Combine(_environment.WebRootPath, "images", "logo-white.png")))
            {
                string folderpath = Utilities.PathCombine(_environment.ContentRootPath, "Content", "Tenants", site.TenantId.ToString(), "Sites", site.SiteId.ToString(), Path.DirectorySeparatorChar.ToString());
                Directory.CreateDirectory(folderpath);
                if (!System.IO.File.Exists(Path.Combine(folderpath, "logo-white.png")))
                {
                    System.IO.File.Copy(Path.Combine(_environment.WebRootPath, "images", "logo-white.png"), Path.Combine(folderpath, "logo-white.png"));
                }
                Folder folder = _folderRepository.GetFolder(site.SiteId, "");
                Oqtane.Models.File file = _fileRepository.AddFile(new Oqtane.Models.File { FolderId = folder.FolderId, Name = "logo-white.png", Extension = "png", Size = 8192, ImageHeight = 80, ImageWidth = 250 });
                site.LogoFileId = file.FileId;
                _siteRepository.UpdateSite(site);
            }

            return _pageTemplates;
        }

        private List<PageTemplateModule> GetPageTemplateModules()
        {
            Random rnd = new Random();
            var _pageTemplateModules = new List<PageTemplateModule>();
            for (int module = 1; module <= _modules; module++)
            {
                _pageTemplateModules.Add(new PageTemplateModule
                {
                    ModuleDefinitionName = "Oqtane.Modules.HtmlText, Oqtane.Client",
                    Title = $"Module{module}",
                    Pane = _panenames[rnd.Next(0, _panes - 1)],
                    Order = (module* 2) - 1,
                    PermissionList = new List<Permission>
                    {
                        new Permission(PermissionNames.View, RoleNames.Everyone, true),
                        new Permission(PermissionNames.View, RoleNames.Admin, true),
                        new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                    },
                    Content = _content[rnd.Next(0, 4)]
                });
            }
            return _pageTemplateModules;
        }
    }
}
