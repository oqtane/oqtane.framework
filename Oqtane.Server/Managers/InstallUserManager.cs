using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Oqtane.Managers
{
    /// <summary>
    /// This class is only used for user validation during installation process.
    /// </summary>
    /// <typeparam name="TUser"></typeparam>
    internal class InstallUserManager<TUser> : UserManager<IdentityUser>
    {
        public InstallUserManager(IUserStore<IdentityUser> store, IOptions<IdentityOptions> optionsAccessor, IPasswordHasher<IdentityUser> passwordHasher, IEnumerable<IUserValidator<IdentityUser>> userValidators, IEnumerable<IPasswordValidator<IdentityUser>> passwordValidators, ILookupNormalizer keyNormalizer, IdentityErrorDescriber errors, IServiceProvider services, ILogger<UserManager<IdentityUser>> logger) : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
        }

        public override async Task<IdentityUser> FindByNameAsync(string userName)
        {
            await Task.CompletedTask;

            return null;
        }

        public override async Task<IdentityUser> FindByEmailAsync(string email)
        {
            await Task.CompletedTask;

            return null;
        }

        public override async Task<string> GetUserIdAsync(IdentityUser user)
        {
            await Task.CompletedTask;

            return null;
        }
    }
}
