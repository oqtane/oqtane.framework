using Oqtane.Shared;
using Oqtane.Models;
using System;
using System.Linq;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Oqtane.Services
{
    public class UserService : ServiceBase, IUserService
    {
        private readonly HttpClient http;
        private readonly SiteState sitestate;
        private readonly IUriHelper urihelper;

        public UserService(HttpClient http, SiteState sitestate, IUriHelper urihelper)
        {
            this.http = http;
            this.sitestate = sitestate;
            this.urihelper = urihelper;
        }

        private string apiurl
        {
            get { return CreateApiUrl(sitestate.Alias, urihelper.GetAbsoluteUri(), "User"); }
        }

        public async Task<List<User>> GetUsersAsync(int SiteId)
        {
            List<User> users = await http.GetJsonAsync<List<User>>(apiurl + "?siteid=" + SiteId.ToString());
            return users.OrderBy(item => item.DisplayName).ToList();
        }

        public async Task<User> GetUserAsync(int UserId, int SiteId)
        {
            return await http.GetJsonAsync<User>(apiurl + "/" + UserId.ToString() + "?siteid=" + SiteId.ToString());
        }

        public async Task<User> GetUserAsync(string Username, int SiteId)
        {
            return await http.GetJsonAsync<User>(apiurl + "/name/" + Username + "?siteid=" + SiteId.ToString());
        }

        public async Task<User> AddUserAsync(User User)
        {
            return await http.PostJsonAsync<User>(apiurl, User);
        }

        public async Task<User> UpdateUserAsync(User User)
        {
            return await http.PutJsonAsync<User>(apiurl + "/" + User.UserId.ToString(), User);
        }
        public async Task DeleteUserAsync(int UserId)
        {
            await http.DeleteAsync(apiurl + "/" + UserId.ToString());
        }

        public async Task<User> LoginUserAsync(User User, bool SetCookie, bool IsPersistent)
        {
            return await http.PostJsonAsync<User>(apiurl + "/login?setcookie=" + SetCookie.ToString() + "&persistent =" + IsPersistent.ToString(), User);
        }

        public async Task LogoutUserAsync()
        {
            // best practices recommend post is preferrable to get for logout
            await http.PostJsonAsync(apiurl + "/logout", null); 
        }

        // ACLs are stored in the format "!rolename1;![userid1];rolename2;rolename3;[userid2];[userid3]" where "!" designates Deny permissions
        public bool IsAuthorized(User User, string AccessControlList)
        {
            bool isAllowed = false;

            if (User != null)
            {
                // super user always has full access
                isAllowed = User.IsSuperUser;
            }

            if (!isAllowed)
            {
                if (AccessControlList != null)
                {
                    foreach (string permission in AccessControlList.Split(new[] { ';' }))
                    {
                        bool? allowed = VerifyPermission(User, permission);
                        if (allowed.HasValue)
                        {
                            isAllowed = allowed.Value;
                            break;
                        }
                    }
                }
            }
            return isAllowed;
        }

        private bool? VerifyPermission(User user, string permission)
        {
            bool? allowed = null;
            //permissions strings are encoded with deny permissions at the beginning and grant permissions at the end for optimal performance
            if (!String.IsNullOrEmpty(permission))
            {
                // deny permission
                if (permission.StartsWith("!"))
                {
                    string denyRole = permission.Replace("!", "");
                    if (denyRole == Constants.AllUsersRole || IsAllowed(user, denyRole))
                    {
                        allowed = false;
                    }
                }
                else // grant permission
                {
                    if (permission == Constants.AllUsersRole || IsAllowed(user, permission))
                    {
                        allowed = true;
                    }
                }
            }
            return allowed;
        }

        private bool IsAllowed(User user, string permission)
        {
            if (user != null)
            {
                if ("[" + user.UserId + "]" == permission)
                {
                    return true;
                }

                var roles = user.Roles;
                if (roles != null)
                {
                    return roles.IndexOf(";" + permission + ";") != -1;
                }
            }
            return false;
        }
    }
}
