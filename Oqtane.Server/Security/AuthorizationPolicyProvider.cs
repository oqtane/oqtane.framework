using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Oqtane.Shared;
using System.Threading.Tasks;

namespace Oqtane.Security
{
    public class AuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
    {
        private readonly AuthorizationOptions _options;

        public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
            _options = options.Value;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // get policy
            policyName = GetPolicyName(policyName);
            var policy = await base.GetPolicyAsync(policyName);

            if (policy == null)
            {
                // policy names must be in the form of "EntityName:PermissionName:Roles"
                if (policyName.Contains(':'))
                {
                    var segments = policyName.Split(':');
                    if (segments.Length == 3)
                    {
                        // create policy
                        var builder = new AuthorizationPolicyBuilder();
                        builder.AddRequirements(new PermissionRequirement(segments[0], segments[1], segments[2]));
                        policy = builder.Build();

                        // add policy to the AuthorizationOptions
                        try
                        {
                            _options.AddPolicy(policyName, policy);
                        }
                        catch
                        {
                            // race condition - policy already added by another thread
                        }
                    }
                }
            }

            return policy;
        }

        private string GetPolicyName(string policyName)
        {
            // backward compatibility for legacy static policy names
            if (policyName == PolicyNames.ViewModule) policyName = $"{EntityNames.Module}:{PermissionNames.View}:{RoleNames.Admin}";
            if (policyName == PolicyNames.EditModule) policyName = $"{EntityNames.Module}:{PermissionNames.Edit}:{RoleNames.Admin}";
            return policyName;
        }
    }
}
