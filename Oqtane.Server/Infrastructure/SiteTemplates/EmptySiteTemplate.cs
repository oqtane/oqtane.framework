using Oqtane.Models;
using Oqtane.Infrastructure;
using System.Collections.Generic;
using Oqtane.Extensions;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.SiteTemplates
{
    public class EmptySiteTemplate : ISiteTemplate
    {
        public EmptySiteTemplate()
        {
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
                PagePermissions = new List<Permission> {
                    new Permission(PermissionNames.View, Constants.AllUsersRole, true),
                    new Permission(PermissionNames.View, Constants.AdminRole, true),
                    new Permission(PermissionNames.Edit, Constants.AdminRole, true)
                }.EncodePermissions(),
                PageTemplateModules = new List<PageTemplateModule>()
            });

            return _pageTemplates;
        }
    }
}
