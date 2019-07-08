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
            get { return CreateApiUrl(sitestate.Alias, "User"); }
        }

        public async Task<List<User>> GetUsersAsync()
        {
            List<User> users = await http.GetJsonAsync<List<User>>(apiurl);
            return users.OrderBy(item => item.DisplayName).ToList();
        }

        public async Task<User> GetUserAsync(int UserId)
        {
            List<User> users = await http.GetJsonAsync<List<User>>(apiurl);
            return users.Where(item => item.UserId == UserId).FirstOrDefault();
        }

        public async Task AddUserAsync(User user)
        {
            await http.PostJsonAsync(apiurl, user);
        }

        public async Task UpdateUserAsync(User user)
        {
            await http.PutJsonAsync(apiurl + "/" + user.UserId.ToString(), user);
        }
        public async Task DeleteUserAsync(int UserId)
        {
            await http.DeleteAsync(apiurl + "/" + UserId.ToString());
        }

        public async Task<User> GetCurrentUserAsync()
        {
            return await http.GetJsonAsync<User>(apiurl + "/current");
        }

        public async Task<User> LoginUserAsync(User user)
        {
            return await http.PostJsonAsync<User>(apiurl + "/login", user);
        }

        public async Task LogoutUserAsync()
        {
            // best practices recommend post is preferrable to get for logout
            await http.PostJsonAsync(apiurl + "/logout", null); 
        }

        // ACLs are stored in the format "!rolename1;![userid1];rolename2;rolename3;[userid2];[userid3]" where "!" designates Deny permissions
        public bool IsAuthorized(User user, string accesscontrollist)
        {
            bool isAllowed = false;

            if (user != null)
            {
                //super user always has full access
                isAllowed = user.IsSuperUser;
            }

            if (!isAllowed)
            {
                if (accesscontrollist != null)
                {
                    foreach (string permission in accesscontrollist.Split(new[] { ';' }))
                    {
                        bool? allowed = VerifyPermission(user, permission);
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
