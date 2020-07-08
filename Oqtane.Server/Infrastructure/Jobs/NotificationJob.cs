using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.DependencyInjection;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;

namespace Oqtane.Infrastructure
{
    public class NotificationJob : HostedServiceBase
    {
        // JobType = "Oqtane.Infrastructure.NotificationJob, Oqtane.Server"

        public NotificationJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory) {}

        public override string ExecuteJob(IServiceProvider provider)
        {
            string log = "";

            // iterate through aliases in this installation
            var aliasRepository = provider.GetRequiredService<IAliasRepository>();
            List<Alias> aliases = aliasRepository.GetAliases().ToList();
            foreach (Alias alias in aliases)
            {
                // use the SiteState to set the Alias explicitly so the tenant can be resolved
                var siteState = provider.GetRequiredService<SiteState>();
                siteState.Alias = alias;

                // get services which require tenant resolution
                var siteRepository = provider.GetRequiredService<ISiteRepository>();
                var settingRepository = provider.GetRequiredService<ISettingRepository>();
                var notificationRepository = provider.GetRequiredService<INotificationRepository>();

                // iterate through sites 
                List<Site> sites = siteRepository.GetSites().ToList();
                foreach (Site site in sites)
                {
                    log += "Processing Notifications For Site: " + site.Name + "<br />";

                    // get site settings
                    List<Setting> sitesettings = settingRepository.GetSettings(EntityNames.Site, site.SiteId).ToList();
                    Dictionary<string, string> settings = GetSettings(sitesettings);
                    if (settings.ContainsKey("SMTPHost") && settings["SMTPHost"] != "")
                    {
                        // construct SMTP Client 
                        var client = new SmtpClient()
                        {
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Host = settings["SMTPHost"],
                            Port = int.Parse(settings["SMTPPort"]),
                            EnableSsl = bool.Parse(settings["SMTPSSL"])
                        };
                        if (settings["SMTPUsername"] != "" && settings["SMTPPassword"] != "")
                        {
                            client.Credentials = new NetworkCredential(settings["SMTPUsername"], settings["SMTPPassword"]);
                        }

                        // iterate through notifications
                        int sent = 0;
                        List<Notification> notifications = notificationRepository.GetNotifications(site.SiteId, -1, -1).ToList();
                        foreach (Notification notification in notifications)
                        {
                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress(settings["SMTPUsername"], site.Name);
                            mailMessage.Subject = notification.Subject;
                            if (notification.FromUserId != null)
                            {
                                mailMessage.Body = "From: " + notification.FromDisplayName + "<" + notification.FromEmail + ">" + "\n";
                            }
                            else
                            {
                                mailMessage.Body = "From: " + site.Name + "\n";
                            }
                            mailMessage.Body += "Sent: " + notification.CreatedOn + "\n";
                            if (notification.ToUserId != null)
                            {
                                mailMessage.To.Add(new MailAddress(notification.ToEmail, notification.ToDisplayName));
                                mailMessage.Body += "To: " + notification.ToDisplayName + "<" + notification.ToEmail + ">" + "\n";
                            }
                            else
                            {
                                mailMessage.To.Add(new MailAddress(notification.ToEmail));
                                mailMessage.Body += "To: " + notification.ToEmail + "\n";
                            }
                            mailMessage.Body += "Subject: " + notification.Subject + "\n\n";
                            mailMessage.Body += notification.Body;
                            
                            // send mail
                            try
                            {
                                client.Send(mailMessage);
                                sent = sent++;
                                notification.IsDelivered = true;
                                notification.DeliveredOn = DateTime.UtcNow;
                                notificationRepository.UpdateNotification(notification);
                            }
                            catch (Exception ex)
                            {
                                // error
                                log += ex.Message + "<br />";
                            }
                        }
                        log += "Notifications Delivered: " + sent + "<br />";
                    }
                    else
                    {
                        log += "SMTP Not Configured" + "<br />";
                    }
                }
            }

            return log;
        }

        private Dictionary<string, string> GetSettings(List<Setting> settings)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (Setting setting in settings.OrderBy(item => item.SettingName).ToList())
            {
                dictionary.Add(setting.SettingName, setting.SettingValue);
            }
            return dictionary;
        }
    }
}
