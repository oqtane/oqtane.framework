using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Oqtane.Models;

namespace Oqtane.Extensions
{
    public static class PermissionExtension
    {
        public static string EncodePermissions(this IEnumerable<Permission> permissions)
        {
            return JsonSerializer.Serialize(permissions);
        }
    }
}
