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

        public NotificationJob(IServiceScopeFactory ServiceScopeFactory) : base(ServiceScopeFactory) {}

        public override string ExecuteJob(IServiceProvider provider)
        {
            string log = "";

            // iterate through aliases in this installation
            var Aliases = provider.GetRequiredService<IAliasRepository>();
            List<Alias> aliases = Aliases.GetAliases().ToList();
            foreach (Alias alias in aliases)
            {
                // use the SiteState to set the Alias explicitly so the tenant can be resolved
                var sitestate = provider.GetRequiredService<SiteState>();
                sitestate.Alias = alias;

                // get services which require tenant resolution
                var Sites = provider.GetRequiredService<ISiteRepository>();
                var Settings = provider.GetRequiredService<ISettingRepository>();
                var Notifications = provider.GetRequiredService<INotificationRepository>();

                // iterate through sites 
                List<Site> sites = Sites.GetSites().ToList();
                foreach (Site site in sites)
                {
                    log += "Processing Notifications For Site: " + site.Name + "\n\n";

                    // get site settings
                    List<Setting> sitesettings = Settings.GetSettings("Site", site.SiteId).ToList();
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
                        List<Notification> notifications = Notifications.GetNotifications(site.SiteId, -1, -1).ToList();
                        foreach (Notification notification in notifications)
                        {
                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress(settings["SMTPUsername"], site.Name);

                            if (notification.FromUserId != null)
                            {
                                mailMessage.Body = "From: " + notification.FromUser.DisplayName + "<" + notification.FromUser.Email + ">" + "\n";
                            }
                            else
                            {
                                mailMessage.Body = "From: " + site.Name + "\n";
                            }
                            mailMessage.Body += "Sent: " + notification.CreatedOn.ToString() + "\n";
                            if (notification.ToUserId != null)
                            {
                                mailMessage.To.Add(new MailAddress(notification.ToUser.Email, notification.ToUser.DisplayName));
                                mailMessage.Body += "To: " + notification.ToUser.DisplayName + "<" + notification.ToUser.Email + ">" + "\n";
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
                                notification.DeliveredOn = DateTime.Now;
                                Notifications.UpdateNotification(notification);
                            }
                            catch (Exception ex)
                            {
                                // error
                                log += ex.Message.ToString() + "\n\n";
                            }
                        }
                        log += "Notifications Delivered: " + sent.ToString() + "\n\n";
                    }
                    else
                    {
                        log += "SMTP Not Configured" + "\n\n";
                    }
                }
            }

            return log;
        }


        private Dictionary<string, string> GetSettings(List<Setting> Settings)
        {
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (Setting setting in Settings.OrderBy(item => item.SettingName).ToList())
            {
                dictionary.Add(setting.SettingName, setting.SettingValue);
            }
            return dictionary;
        }
    }
}
