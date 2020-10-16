using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Oqtane.Models;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Repository;

namespace Oqtane.Security
{
    public class ClaimsPrincipalFactory<TUser> : UserClaimsPrincipalFactory<TUser> where TUser : IdentityUser
    {
        private readonly IdentityOptions _options;
        private readonly ITenantResolver _tenants;
        private readonly IUserRepository _users;
        private readonly IUserRoleRepository _userRoles;

        public ClaimsPrincipalFactory(UserManager<TUser> userManager, IOptions<IdentityOptions> optionsAccessor, ITenantResolver tenants, IUserRepository users, IUserRoleRepository userroles) : base(userManager, optionsAccessor)
        {
            _options = optionsAccessor.Value;
            _tenants = tenants;
            _users = users; 
            _userRoles = userroles;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser identityuser)
        {
            var id = await base.GenerateClaimsAsync(identityuser);

            User user = _users.GetUser(identityuser.UserName);
            if (user != null)
            {
                id.AddClaim(new Claim(ClaimTypes.PrimarySid, user.UserId.ToString()));
                Alias alias = _tenants.GetAlias();
                List<UserRole> userroles = _userRoles.GetUserRoles(user.UserId, alias.SiteId).ToList();
                foreach (UserRole userrole in userroles)
                {
                    id.AddClaim(new Claim(_options.ClaimsIdentity.RoleClaimType, userrole.Role.Name));
                    // host users are members of every site
                    if (userrole.Role.Name == RoleNames.Host)
                    {
                        if (userroles.Where(item => item.Role.Name == RoleNames.Registered).FirstOrDefault() == null)
                        {
                            id.AddClaim(new Claim(_options.ClaimsIdentity.RoleClaimType, RoleNames.Registered));
                        }
                        if (userroles.Where(item => item.Role.Name == RoleNames.Admin).FirstOrDefault() == null)
                        {
                            id.AddClaim(new Claim(_options.ClaimsIdentity.RoleClaimType, RoleNames.Admin));
                        }
                    }
                }
            }

            return id;
        }
    }

}
