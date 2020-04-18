using Oqtane.Models;
using Oqtane.Infrastructure;
using System.Collections.Generic;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.SiteTemplates
{
    public class EmptySiteTemplate : ISiteTemplate
    {
        private readonly IPermissionRepository _permissionRepository;
 
        public EmptySiteTemplate(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        public string Name
        {
            get { return "Empty Site Template"; }
        }

        public List<PageTemplate> CreateSite(Site site)
        {
            List<PageTemplate> _pageTemplates = new List<PageTemplate>();

            _pageTemplates.Add(new PageTemplate
            {
                Name = "Home",
                Parent = "",
                Path = "",
                Icon = "home",
                IsNavigation = true,
                IsPersonalizable = false,
                EditMode = false,
                PagePermissions = _permissionRepository.EncodePermissions( new List<Permission> {
                    new Permission(PermissionNames.View, Constants.AllUsersRole, true),
                    new Permission(PermissionNames.View, Constants.AdminRole, true),
                    new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                }),
                PageTemplateModules = new List<PageTemplateModule>()
            });

            return _pageTemplates;
        }
    }
}
