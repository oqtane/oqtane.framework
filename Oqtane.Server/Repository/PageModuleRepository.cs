using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Extensions;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public class PageModuleRepository : IPageModuleRepository
    {
        private TenantDBContext _db;
        private readonly IModuleDefinitionRepository _moduleDefinitions;
        private readonly IPermissionRepository _permissions;

        public PageModuleRepository(TenantDBContext context, IModuleDefinitionRepository moduleDefinitions, IPermissionRepository permissions)
        {
            _db = context;
            _moduleDefinitions = moduleDefinitions;
            _permissions = permissions;
        }

        public IEnumerable<PageModule> GetPageModules(int siteId)
        {
            var pagemodules = _db.PageModule
                .Include(item => item.Module) // eager load modules
                .Where(item => item.Module.SiteId == siteId).ToList();
            if (pagemodules.Any())
            {
                var moduledefinitions = _moduleDefinitions.GetModuleDefinitions(siteId).ToList();
                var permissions = _permissions.GetPermissions(siteId, EntityNames.Module).ToList();
                for (int index = 0; index < pagemodules.Count; index++)
                {
                    pagemodules[index] = GetPageModule(pagemodules[index], moduledefinitions, permissions);
                }
            }
            return pagemodules;
        }

        public PageModule AddPageModule(PageModule pageModule)
        {
            _db.PageModule.Add(pageModule);
            _db.SaveChanges();
            return pageModule;
        }

        public PageModule UpdatePageModule(PageModule pageModule)
        {
            _db.Entry(pageModule).State = EntityState.Modified;
            _db.SaveChanges();
            return pageModule;
        }

        public PageModule GetPageModule(int pageModuleId)
        {
            return GetPageModule(pageModuleId, true);
        }

        public PageModule GetPageModule(int pageModuleId, bool tracking)
        {
            PageModule pagemodule;
            if (tracking)
            {
                pagemodule = _db.PageModule.Include(item => item.Module) // eager load modules
                    .FirstOrDefault(item => item.PageModuleId == pageModuleId);
            }
            else
            {
                pagemodule = _db.PageModule.AsNoTracking().Include(item => item.Module) // eager load modules
                    .FirstOrDefault(item => item.PageModuleId == pageModuleId);
            }
            if (pagemodule != null)
            {
                var moduledefinitions = _moduleDefinitions.GetModuleDefinitions(pagemodule.Module.SiteId).ToList();
                var permissions = _permissions.GetPermissions(pagemodule.Module.SiteId, EntityNames.Module).ToList();
                pagemodule = GetPageModule(pagemodule, moduledefinitions, permissions);
            }
            return pagemodule;
        }

        public PageModule GetPageModule(int pageId, int moduleId)
        {
            PageModule pagemodule = _db.PageModule.Include(item => item.Module) // eager load modules
                .SingleOrDefault(item => item.PageId == pageId && item.ModuleId == moduleId);
            if (pagemodule != null)
            {
                var moduledefinitions = _moduleDefinitions.GetModuleDefinitions(pagemodule.Module.SiteId).ToList();
                var permissions = _permissions.GetPermissions(pagemodule.Module.SiteId, EntityNames.Module).ToList();
                pagemodule = GetPageModule(pagemodule, moduledefinitions, permissions);
            }
            return pagemodule;
        }

        public void DeletePageModule(int pageModuleId)
        {
            PageModule pageModule = _db.PageModule.Find(pageModuleId);
            _db.PageModule.Remove(pageModule);
            _db.SaveChanges();
        }

        private PageModule GetPageModule(PageModule pageModule, List<ModuleDefinition> moduleDefinitions, List<Permission> modulePermissions)
        {
            var permissions = modulePermissions.Where(item => item.EntityId == pageModule.ModuleId).ToList();

            // moduledefinition permissionnames can specify permissions for other entities (ie. API permissions)
            pageModule.Module.ModuleDefinition = moduleDefinitions.Find(item => item.ModuleDefinitionName == pageModule.Module.ModuleDefinitionName);
            if (pageModule.Module.ModuleDefinition != null && !string.IsNullOrEmpty(pageModule.Module.ModuleDefinition.PermissionNames) && pageModule.Module.ModuleDefinition.PermissionNames.Contains(":"))
            {
                foreach (var permissionname in pageModule.Module.ModuleDefinition.PermissionNames.Split(",", System.StringSplitOptions.RemoveEmptyEntries))
                {
                    if (permissionname.Contains(":"))
                    {
                        // moduledefinition permissionnames can be in the form of "EntityName:PermissionName:Roles"
                        var segments = permissionname.Split(':');
                        if (segments.Length == 3 && segments[0] != EntityNames.Module)
                        {
                            permissions.AddRange(_permissions.GetPermissions(pageModule.Module.SiteId, segments[0], segments[1]).Where(item => item.EntityId == -1));
                        }
                    }
                }
            }
            pageModule.Module.PermissionList = permissions?.ToList();
            return pageModule;
        }
    }
}
