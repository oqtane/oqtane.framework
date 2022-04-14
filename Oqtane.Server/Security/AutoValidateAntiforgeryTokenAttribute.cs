using System;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Oqtane.Security
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AutoValidateAntiforgeryTokenAttribute : Attribute, IFilterFactory, IOrderedFilter
    {
        public int Order { get; set; } = 1000;

        public bool IsReusable => true;

        public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
        {
            return serviceProvider.GetRequiredService<AutoValidateAntiforgeryTokenFilter>();
        }
    }
}
