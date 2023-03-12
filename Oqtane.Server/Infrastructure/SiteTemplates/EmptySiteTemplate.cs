using Oqtane.Models;
using Oqtane.Infrastructure;
using System.Collections.Generic;
using Oqtane.Extensions;
using Oqtane.Repository;
using Oqtane.Shared;
using Oqtane.Documentation;

namespace Oqtane.SiteTemplates
{
    [PrivateApi("Mark Site-Template classes as private, since it's not very useful in the public docs")]
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
                Icon = "oi oi-home",
                IsNavigation = true,
                IsPersonalizable = false,
                PermissionList = new List<Permission> {
                    new Permission(PermissionNames.View, RoleNames.Everyone, true),
                    new Permission(PermissionNames.View, RoleNames.Admin, true),
                    new Permission(PermissionNames.Edit, RoleNames.Admin, true)
                },
                PageTemplateModules = new List<PageTemplateModule>()
            });

            return _pageTemplates;
        }
    }
}
