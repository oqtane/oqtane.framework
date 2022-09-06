using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Oqtane.Shared;

namespace Oqtane.Security
{
    public class AutoValidateAntiforgeryTokenFilter : IAsyncAuthorizationFilter, IAntiforgeryPolicy
    {
        private readonly IAntiforgery _antiforgery;
        private readonly ILogger<AutoValidateAntiforgeryTokenFilter> _filelogger;

        public AutoValidateAntiforgeryTokenFilter(IAntiforgery antiforgery, ILogger<AutoValidateAntiforgeryTokenFilter> filelogger)
        {
            _antiforgery = antiforgery;
            _filelogger = filelogger;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (!context.IsEffectivePolicy<IAntiforgeryPolicy>(this))
            {
                return;
            }

            if (ShouldValidate(context))
            {
                try
                {
                    await _antiforgery.ValidateRequestAsync(context.HttpContext);
                }
                catch
                {
                    context.Result = new AntiforgeryValidationFailedResult();
                    _filelogger.LogError(Utilities.LogMessage(this, $"AutoValidateAntiforgeryTokenFilter Failure For {context.HttpContext.Request.GetEncodedUrl()}"));
                }
            }
        }

        protected virtual bool ShouldValidate(AuthorizationFilterContext context)
        {
            // ignore antiforgery validation if a bearer token was provided
            if (context.HttpContext.Request.Headers.ContainsKey("Authorization"))
            {
                return false;
            }

            // ignore antiforgery validation if client is a MAUI app
            if (context.HttpContext.Request.Headers["User-Agent"] == Constants.MauiUserAgent)
            {
                return false;
            }

            // ignore antiforgery validation for GET, HEAD, TRACE, OPTIONS
            var method = context.HttpContext.Request.Method;
            if (HttpMethods.IsGet(method) || HttpMethods.IsHead(method) || HttpMethods.IsTrace(method) || HttpMethods.IsOptions(method))
            {
                return false;
            }

            // everything else requires antiforgery validation (ie. POST, PUT, DELETE)
            return true;
        }
    }
}
