using Microsoft.AspNetCore.Http;
using Oqtane.Extensions;
using Oqtane.Models;

namespace Oqtane.Infrastructure
{
    public class AliasAccessor : IAliasAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AliasAccessor(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Alias Alias => _httpContextAccessor.HttpContext.GetAlias();
    }
}
