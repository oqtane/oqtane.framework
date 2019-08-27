using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Threading.Tasks;
using Oqtane.Repository;
using Oqtane.Models;
using Oqtane.Shared;

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
                if (user.IsHost) // host users are part of every site by default
                {
                    id.AddClaim(new Claim(options.ClaimsIdentity.RoleClaimType, Constants.HostRole));
                    id.AddClaim(new Claim(options.ClaimsIdentity.RoleClaimType, Constants.AdminRole));
                }
                else
                {
                    Alias alias = Tenants.GetAlias();
                    foreach (UserRole userrole in UserRoles.GetUserRoles(user.UserId, alias.SiteId))
                    {
                        id.AddClaim(new Claim(options.ClaimsIdentity.RoleClaimType, userrole.Role.Name));
                    }
                }
            }

            return id;
        }
    }

}
