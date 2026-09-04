using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Oqtane.Infrastructure.SiteTemplates;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Repository
{
    public interface ISiteTemplateRepository
    {
        IEnumerable<SiteTemplate> GetSiteTemplates();
    }

    public class SiteTemplateRepository : ISiteTemplateRepository
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfigManager _configManager;

        public SiteTemplateRepository(IServiceProvider serviceProvider, IConfigManager configManager)
        {
            _serviceProvider = serviceProvider;
            _configManager = configManager;
        }

        private List<SiteTemplate> LoadSiteTemplates()
        {
            var defaultSiteTemplate = _configManager.GetSetting("Installation:SiteTemplate", "");
            if (string.IsNullOrEmpty(defaultSiteTemplate) || Type.GetType(defaultSiteTemplate) == null)
            {
                defaultSiteTemplate = Constants.DefaultSiteTemplate;
            }

            List<SiteTemplate> siteTemplates = new List<SiteTemplate>();

            // iterate through Oqtane site template assemblies
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
                
            foreach (Assembly assembly in assemblies)
            {
                siteTemplates = LoadSiteTemplatesFromAssembly(siteTemplates, assembly, defaultSiteTemplate);
            }

            return siteTemplates;
        }

        private List<SiteTemplate> LoadSiteTemplatesFromAssembly(List<SiteTemplate> siteTemplates, Assembly assembly, string defaultSiteTemplate)
        {
            Type[] siteTemplateTypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(ISiteTemplate))).ToArray();
            foreach (Type siteTemplateType in siteTemplateTypes)
            {
                var siteTemplateObject = ActivatorUtilities.CreateInstance(_serviceProvider, siteTemplateType);
                if (siteTemplateObject != null)
                {
                    var typename = Utilities.GetFullTypeName(siteTemplateType.AssemblyQualifiedName);
                    var name = (string)siteTemplateType.GetProperty("Name")?.GetValue(siteTemplateObject);
                    if (typename != Constants.AdminSiteTemplate && !string.IsNullOrEmpty(name))
                    {
                        siteTemplates.Add(new SiteTemplate
                        {
                            Name = name,
                            TypeName = typename,
                            IsDefault = (typename == defaultSiteTemplate)
                        });
                    }
                }
            }
            return siteTemplates;
        }

        public IEnumerable<SiteTemplate> GetSiteTemplates()
        {
            return LoadSiteTemplates();
        }
    }
}
