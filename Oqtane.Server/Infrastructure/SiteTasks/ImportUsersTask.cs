using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Managers;
using Oqtane.Models;
using Oqtane.Providers;
using Oqtane.Repository;

namespace Oqtane.Infrastructure
{
    public class ImportUsersTask : SiteTaskBase
    {
        public override async Task<string> ExecuteTaskAsync(IServiceProvider provider, Site site, string parameters)
        {
            string log = "";

            if (!string.IsNullOrEmpty(parameters) && parameters.Contains(":"))
            {
                var fileId = int.Parse(parameters.Split(':')[0]);
                var notify = bool.Parse(parameters.Split(':')[1]);

                var fileRepository = provider.GetRequiredService<IFileRepository>();
                var userManager = provider.GetRequiredService<IUserManager>();

                var file = fileRepository.GetFile(fileId);
                if (file != null)
                {
                    log += $"Importing Users From {file.Name}<br />";
                    var result = await userManager.ImportUsers(site.SiteId, file, notify);
                    if (result["Success"] == "True")
                    {
                        log += $"{result["Users"]} Users Imported<br />";
                    }
                    else
                    {
                        log += $"User Import Failed<br />";
                    }
                }
                else
                {
                    log += $"Import Users FileId {fileId} Does Not Exist<br />";
                }
            }

            return log;
        }
    }
}
