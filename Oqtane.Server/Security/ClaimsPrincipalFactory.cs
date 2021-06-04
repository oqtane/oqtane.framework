using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Oqtane.Models;
using System.Collections.Generic;
using System.Linq;
using Oqtane.Repository;
using Oqtane.Infrastructure;

namespace Oqtane.Security
{
    public class ClaimsPrincipalFactory<TUser> : UserClaimsPrincipalFactory<TUser> where TUser : IdentityUser
    {
        private readonly ITenantManager _tenants;
        private readonly IUserRepository _users;
        private readonly IUserRoleRepository _userRoles;

        public ClaimsPrincipalFactory(UserManager<TUser> userManager, IOptions<IdentityOptions> optionsAccessor, ITenantManager tenants, IUserRepository users, IUserRoleRepository userroles) : base(userManager, optionsAccessor)
        {
            _tenants = tenants;
            _users = users; 
            _userRoles = userroles;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser identityuser)
        {
            var identity = await base.GenerateClaimsAsync(identityuser);

            User user = _users.GetUser(identityuser.UserName);
            if (user != null)
            {
                Alias alias = _tenants.GetAlias();
                if (alias != null)
                {
                    List<UserRole> userroles = _userRoles.GetUserRoles(user.UserId, alias.SiteId).ToList();
                    identity = UserSecurity.CreateClaimsIdentity(alias, user, userroles);
                }
            }

            return identity;
        }
    }

}
