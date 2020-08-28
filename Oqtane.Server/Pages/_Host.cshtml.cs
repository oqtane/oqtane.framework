using Microsoft.AspNetCore.Mvc.RazorPages;
using Oqtane.Infrastructure;
using Oqtane.Shared;
using System;
using System.Linq;
using System.Reflection;

namespace Oqtane.Pages
{
    public class HostModel : PageModel
    {
        public string Resources = "";

        public void OnGet()
        {
            var assemblies = AppDomain.CurrentDomain.GetOqtaneAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                var types = assembly.GetTypes().Where(item => item.GetInterfaces().Contains(typeof(IHostResources)));
                foreach (var type in types)
                {
                    var obj = Activator.CreateInstance(type) as IHostResources;
                    foreach (var resource in obj.Resources)
                    {
                        switch (resource.ResourceType)
                        {
                            case ResourceType.Stylesheet:
                                Resources += "<link rel=\"stylesheet\" href=\"" + resource.Url + "\"" + CrossOrigin(resource.CrossOrigin) + Integrity(resource.Integrity) + " />" + Environment.NewLine;
                                break;
                            case ResourceType.Script:
                                Resources += "<script src=\"" + resource.Url + "\"" + CrossOrigin(resource.CrossOrigin) + Integrity(resource.Integrity) + "></script>" + Environment.NewLine;
                                break;
                        }
                    }
                }
            }
        }

        private string CrossOrigin(string crossorigin)
        {
            if (!string.IsNullOrEmpty(crossorigin))
            {
                return " crossorigin=\"" + crossorigin + "\"";
            }
            else
            {
                return "";
            }
        }
        private string Integrity(string integrity)
        {
            if (!string.IsNullOrEmpty(integrity))
            {
                return " integrity=\"" + integrity + "\"";
            }
            else
            {
                return "";
            }
        }
    }
}