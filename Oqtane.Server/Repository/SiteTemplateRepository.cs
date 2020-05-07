using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Infrastructure;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class SiteTemplateRepository : ISiteTemplateRepository
    {
        private readonly IServiceProvider _serviceProvider;

        public SiteTemplateRepository(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        private List<SiteTemplate> LoadSiteTemplates()
        {
            List<SiteTemplate> siteTemplates = new List<SiteTemplate>();

            // iterate through Oqtane site template assemblies
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
                
            foreach (Assembly assembly in assemblies)
            {
                siteTemplates = LoadSiteTemplatesFromAssembly(siteTemplates, assembly);
            }

            return siteTemplates;
        }

        private List<SiteTemplate> LoadSiteTemplatesFromAssembly(List<SiteTemplate> siteTemplates, Assembly assembly)
        {
            SiteTemplate siteTemplate;
            Type[] siteTemplateTypes = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(ISiteTemplate))).ToArray();
            foreach (Type siteTemplateType in siteTemplateTypes)
            {
                var siteTemplateObject = ActivatorUtilities.CreateInstance(_serviceProvider, siteTemplateType);
                siteTemplate = new SiteTemplate
                {
                    Name = (string)siteTemplateType.GetProperty("Name")?.GetValue(siteTemplateObject),
                    TypeName = siteTemplateType.AssemblyQualifiedName
                };
                siteTemplates.Add(siteTemplate);
            }
            return siteTemplates;
        }

        public IEnumerable<SiteTemplate> GetSiteTemplates()
        {
            return LoadSiteTemplates();
        }
    }
}
