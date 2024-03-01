using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Oqtane.Enums;
using Oqtane.Extensions;
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
                var endPoint = context.GetEndpoint()?.DisplayName;
                var alias = context.GetAlias();
                if (alias != null)
                {
                    var _logger = provider.GetRequiredService<ILogManager>();
                    _logger.Log(alias.SiteId, Shared.LogLevel.Error, this, LogFunction.Other, exception, "Unhandled Exception: {Error} For Endpoint: {Endpoint}", exception.Message, endPoint);
                }
                else
                {
                    var _filelogger = provider.GetRequiredService<ILogger<ExceptionMiddleware>>();
                    _filelogger.LogError(Utilities.LogMessage(this, $"Endpoint: {endPoint} Unhandled Exception: {exception}"));
                }
            }
        }
    }
}
