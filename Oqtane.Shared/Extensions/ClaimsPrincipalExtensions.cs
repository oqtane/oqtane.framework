using System.Linq;
using System.Security.Claims;
using Oqtane.Shared;

namespace Oqtane.Extensions
{
    public static class ClaimsPrincipalExtensions
    {
        // extension methods cannot be properties - the methods below must include a () suffix when referenced

        public static bool IsAuthenticated(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.Identity != null)
            {
                return claimsPrincipal.Identity.IsAuthenticated;
            }
            else
            {
                return false;
            }
        }

        public static string Username(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.HasClaim(item => item.Type == ClaimTypes.Name))
            {
                return claimsPrincipal.Claims.FirstOrDefault(item => item.Type == ClaimTypes.Name).Value;
            }
            else
            {
                return "";
            }
        }

        public static int UserId(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.HasClaim(item => item.Type == ClaimTypes.NameIdentifier))
            {
                return int.Parse(claimsPrincipal.Claims.First(item => item.Type == ClaimTypes.NameIdentifier).Value);
            }
            else
            {
                return -1;
            }
        }

        public static string[] Roles(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.Claims.Where(item => item.Type == ClaimTypes.Role)
                .Select(item => item.Value).ToArray();
        }

        public static string SiteKey(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.HasClaim(item => item.Type == Constants.SiteKeyClaimType))
            {
                return claimsPrincipal.Claims.FirstOrDefault(item => item.Type == Constants.SiteKeyClaimType).Value;
            }
            else
            {
                return "";
            }
        }

        public static int TenantId(this ClaimsPrincipal claimsPrincipal)
        {
            var sitekey = SiteKey(claimsPrincipal);
            if (!string.IsNullOrEmpty(sitekey) && sitekey.Contains(":"))
            {
                return int.Parse(sitekey.Split(':')[0]);
            }
            return -1;
        }

        public static int SiteId(this ClaimsPrincipal claimsPrincipal)
        {
            var sitekey = SiteKey(claimsPrincipal);
            if (!string.IsNullOrEmpty(sitekey) && sitekey.Contains(":"))
            {
                return int.Parse(sitekey.Split(':')[1]);
            }
            return -1;
        }

        public static string SecurityStamp(this ClaimsPrincipal claimsPrincipal)
        {
            if (claimsPrincipal.HasClaim(item => item.Type == Constants.SecurityStampClaimType))
            {
                return claimsPrincipal.Claims.FirstOrDefault(item => item.Type == Constants.SecurityStampClaimType).Value;
            }
            else
            {
                return "";
            }
        }

        public static bool IsOnlyInRole(this ClaimsPrincipal claimsPrincipal, string role)
        {
            var identity = claimsPrincipal.Identities.FirstOrDefault(item => item.AuthenticationType == Constants.AuthenticationScheme);
            if (identity != null)
            {
                // check if user has role claim specified and no other role claims
                return identity.Claims.Any(item => item.Type == ClaimTypes.Role && item.Value == role) &&
                    !identity.Claims.Any(item => item.Type == ClaimTypes.Role && item.Value != role);
            }
            return false;
        }
    }
}
