using Oqtane.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System.Collections.Generic;
using Oqtane.Documentation;
using Oqtane.Shared;
using System.Linq;

namespace Oqtane.Services
{
    /// <summary>
    /// Service to manage <see cref="Role"/>s on a <see cref="Site"/>
    /// </summary>
    public interface ISiteGroupMemberService
    {
        /// <summary>
        /// Get all <see cref="SiteGroupMember"/>s
        /// </summary>
        /// <returns></returns>
        Task<List<SiteGroupMember>> GetSiteGroupMembersAsync(int siteId, int siteGroupId);

        /// <summary>
        /// Get one specific <see cref="SiteGroupMember"/>
        /// </summary>
        /// <param name="siteGroupMemberId">ID-reference of a <see cref="SiteGroupMember"/></param>
        /// <returns></returns>
        Task<SiteGroupMember> GetSiteGroupMemberAsync(int siteGroupMemberId);

        /// <summary>
        /// Get one specific <see cref="SiteGroupMember"/>
        /// </summary>
        /// <param name="siteId">ID-reference of a <see cref="Site"/></param>
        /// <param name="siteGroupId">ID-reference of a <see cref="SiteGroup"/></param>
        /// <returns></returns>
        Task<SiteGroupMember> GetSiteGroupMemberAsync(int siteId, int siteGroupId);

        /// <summary>
        /// Add / save a new <see cref="SiteGroupMember"/> to the database.
        /// </summary>
        /// <param name="siteGroupMember"></param>
        /// <returns></returns>
        Task<SiteGroupMember> AddSiteGroupMemberAsync(SiteGroupMember siteGroupMember);

        /// <summary>
        /// Update a <see cref="SiteGroupMember"/> in the database.
        /// </summary>
        /// <param name="siteGroupMember"></param>
        /// <returns></returns>
        Task<SiteGroupMember> UpdateSiteGroupMemberAsync(SiteGroupMember siteGroupMember);

        /// <summary>
        /// Delete a <see cref="SiteGroupMember"/> in the database.
        /// </summary>
        /// <param name="siteGroupMemberId">ID-reference of a <see cref="SiteGroupMember"/></param>
        /// <returns></returns>
        Task DeleteSiteGroupMemberAsync(int siteGroupMemberId);
    }

    [PrivateApi("Don't show in the documentation, as everything should use the Interface")]
    public class SiteGroupMemberService : ServiceBase, ISiteGroupMemberService
    {
        public SiteGroupMemberService(HttpClient http, SiteState siteState) : base(http, siteState) { }

        private string Apiurl => CreateApiUrl("SiteGroupMember");

        public async Task<List<SiteGroupMember>> GetSiteGroupMembersAsync(int siteId, int siteGroupId)
        {
            return await GetJsonAsync<List<SiteGroupMember>>($"{Apiurl}?siteid={siteId}&groupid={siteGroupId}", Enumerable.Empty<SiteGroupMember>().ToList());
        }

        public async Task<SiteGroupMember> GetSiteGroupMemberAsync(int siteGroupMemberId)
        {
            return await GetJsonAsync<SiteGroupMember>($"{Apiurl}/{siteGroupMemberId}");
        }

        public async Task<SiteGroupMember> GetSiteGroupMemberAsync(int siteId, int siteGroupId)
        {
            var siteGroupMembers = await GetSiteGroupMembersAsync(siteId, siteGroupId);
            if (siteGroupMembers != null && siteGroupMembers.Count > 0)
            {
                return siteGroupMembers[0];
            }
            else
            {
                return null;
            }
        }

        public async Task<SiteGroupMember> AddSiteGroupMemberAsync(SiteGroupMember siteGroupMember)
        {
            return await PostJsonAsync<SiteGroupMember>(Apiurl, siteGroupMember);
        }

        public async Task<SiteGroupMember> UpdateSiteGroupMemberAsync(SiteGroupMember siteGroupMember)
        {
            return await PutJsonAsync<SiteGroupMember>($"{Apiurl}/{siteGroupMember.SiteGroupId}", siteGroupMember);
        }

        public async Task DeleteSiteGroupMemberAsync(int siteGroupMemberId)
        {
            await DeleteAsync($"{Apiurl}/{siteGroupMemberId}");
        }
    }
}
