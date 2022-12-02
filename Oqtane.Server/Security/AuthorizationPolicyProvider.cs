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

        public AuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) : base(options)
        {
            _options = options.Value;
        }

        public override async Task<AuthorizationPolicy> GetPolicyAsync(string policyName)
        {
            // check static policies first
            policyName = GetPolicyName(policyName);
            var policy = await base.GetPolicyAsync(policyName);

            if (policy == null)
            {
                if (policyName.Contains(':'))
                {
                    // policy names must be in the form of "EntityName:PermissionName:Roles" ie. "Module:Edit:Administrators" (roles are comma delimited)
                    var policySegments = policyName.Split(':');
                    if (policySegments.Length >= 3)
                    {
                        // check for optional RequireEntityId segment
                        var requireEntityId = false;
                        if (policySegments.Length == 4 && policySegments[3] == Constants.RequireEntityId)
                        {
                            requireEntityId = true;
                        }
                        policy = new AuthorizationPolicyBuilder()
                            .AddRequirements(new PermissionRequirement(policySegments[0], policySegments[1], policySegments[2], requireEntityId))
                            .Build();

                        // add policy to the AuthorizationOptions
                        _options.AddPolicy(policyName, policy);
                    }
                }
            }

            return policy;
        }

        private string GetPolicyName(string policyName)
        {
            // backward compatibility for legacy static policy names
            if (policyName == PolicyNames.ViewModule) policyName = $"{EntityNames.Module}:{PermissionNames.View}:{RoleNames.Admin}:{Constants.RequireEntityId}";
            if (policyName == PolicyNames.EditModule) policyName = $"{EntityNames.Module}:{PermissionNames.Edit}:{RoleNames.Admin}:{Constants.RequireEntityId}";
            return policyName;
        }
    }
}
