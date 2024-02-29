using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Enums;
using Oqtane.Models;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    internal class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ExceptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IServiceProvider provider)
        {
            try
            {
                await _next(context);
            }
            catch (Exception exception)
            {
                var _logger = provider.GetRequiredService<ILogManager>();
                var endPoint = context.GetEndpoint()?.DisplayName;
                var contextAlias = context.Items.FirstOrDefault(i => i.Key.ToString() == "Alias");
                Alias alias;
                int siteId = -1;
                var defaultVal = default(KeyValuePair<int, string>);
                if (!contextAlias.Equals(defaultVal))
                {
                    alias = contextAlias.Value as Alias;
                    siteId = alias.SiteId;
                }
                _logger.Log(siteId, LogLevel.Error, endPoint, LogFunction.Other, exception, exception.Message, context.User?.Identity.Name);
            }
        }
    }
}
