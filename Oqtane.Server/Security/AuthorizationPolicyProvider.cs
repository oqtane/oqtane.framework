using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Oqtane.Shared;
using System.Threading.Tasks;

namespace Oqtane.Security
{
    public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;
        private readonly IConfiguration _configuration;

        public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options, IConfiguration configuration) : base(options)
        {
            _options = options.Value;
            _configuration = configuration;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // check static policies first
            policyName = GetPolicyName(policyName);
            var policy = await base.GetPolicyAsync(policyName);

            if (policy == null)
            {
                // policy names must be in the form of "Entity:Permission" ie. "User:Read"
                if (policyName.Contains(":"))
                {
                    var entityName = policyName.Split(':')[0];
                    var permissionName = policyName.Split(':')[1];

                    policy = new AuthorizationPolicyBuilder()
                        .AddRequirements(new PermissionRequirement(entityName, permissionName))
                        .Build();

                    // add policy to the AuthorizationOptions
                    _options.AddPolicy(policyName, policy);
                }
            }

            return policy;
        }

        private string GetPolicyName(string policyName)
        {
            // backward compatibility for legacy static policy names
            if (policyName == PolicyNames.ViewModule) policyName = "Module:View";
            if (policyName == PolicyNames.EditModule) policyName = "Module:Edit";
            return policyName;
        }
    }
}
