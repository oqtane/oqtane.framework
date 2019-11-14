using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;
using System.Collections.Generic;
using System.Linq;

namespace Oqtane.Security
{
    public class ClaimsPrincipalFactory<TUser> : UserClaimsPrincipalFactory<TUser> where TUser : IdentityUser
    {
        private readonly IdentityOptions options;
        private readonly ITenantResolver Tenants;
        private readonly IUserRepository Users;
        private readonly IUserRoleRepository UserRoles;

        public ClaimsPrincipalFactory(UserManager<TUser> userManager, IOptions<IdentityOptions> optionsAccessor, ITenantResolver tenants, IUserRepository users, IUserRoleRepository userroles) : base(userManager, optionsAccessor)
        {
            options = optionsAccessor.Value;
            Tenants = tenants;
            Users = users; 
            UserRoles = userroles;
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(TUser identityuser)
        {
            var id = await base.GenerateClaimsAsync(identityuser);

            User user = Users.GetUser(identityuser.UserName);
            if (user != null)
            {
                id.AddClaim(new Claim(ClaimTypes.PrimarySid, user.UserId.ToString()));
                Alias alias = Tenants.GetAlias();
                List<UserRole> userroles = UserRoles.GetUserRoles(user.UserId, alias.SiteId).ToList();
                foreach (UserRole userrole in userroles)
                {
                    id.AddClaim(new Claim(options.ClaimsIdentity.RoleClaimType, userrole.Role.Name));
                    // host users are members of every site
                    if (userrole.Role.Name == Constants.HostRole)
                    {
                        if (userroles.Where(item => item.Role.Name == Constants.RegisteredRole).FirstOrDefault() == null)
                        {
                            id.AddClaim(new Claim(options.ClaimsIdentity.RoleClaimType, Constants.RegisteredRole));
                        }
                        if (userroles.Where(item => item.Role.Name == Constants.AdminRole).FirstOrDefault() == null)
                        {
                            id.AddClaim(new Claim(options.ClaimsIdentity.RoleClaimType, Constants.AdminRole));
                        }
                    }
                }
            }

            return id;
        }
    }

}
