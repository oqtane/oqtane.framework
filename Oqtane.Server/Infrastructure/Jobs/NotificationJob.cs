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

        public NotificationJob(IServiceScopeFactory serviceScopeFactory) : base(serviceScopeFactory)
        {
            Name = "Notification Job";
            Frequency = "m"; // minute
            Interval = 1;
            IsEnabled = false;
        }

        // job is executed for each tenant in installation
        public override string ExecuteJob(IServiceProvider provider)
        {
            string log = "";

            // get services
            var siteRepository = provider.GetRequiredService<ISiteRepository>();
            var userRepository = provider.GetRequiredService<IUserRepository>();
            var settingRepository = provider.GetRequiredService<ISettingRepository>();
            var notificationRepository = provider.GetRequiredService<INotificationRepository>();

            // iterate through sites for current tenant
            List<Site> sites = siteRepository.GetSites().ToList();
            foreach (Site site in sites)
            {
                log += "Processing Notifications For Site: " + site.Name + "<br />";

                // get site settings
                List<Setting> sitesettings = settingRepository.GetSettings(EntityNames.Site, site.SiteId).ToList();
                Dictionary<string, string> settings = GetSettings(sitesettings);
                if (settings.ContainsKey("SMTPHost") && settings["SMTPHost"] != "" &&
                    settings.ContainsKey("SMTPPort") && settings["SMTPPort"] != "" &&
                    settings.ContainsKey("SMTPSSL") && settings["SMTPSSL"] != "" &&
                    settings.ContainsKey("SMTPSender") && settings["SMTPSender"] != "")
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

                    // iterate through undelivered notifications
                    int sent = 0;
                    List<Notification> notifications = notificationRepository.GetNotifications(site.SiteId, -1, -1).ToList();
                    foreach (Notification notification in notifications)
                    {
                        // get sender and receiver information if not provided
                        if ((string.IsNullOrEmpty(notification.FromEmail) || string.IsNullOrEmpty(notification.FromDisplayName)) && notification.FromUserId != null)
                        {
                            var user = userRepository.GetUser(notification.FromUserId.Value);
                            if (user != null)
                            {
                                notification.FromEmail = (string.IsNullOrEmpty(notification.FromEmail)) ? user.Email : notification.FromEmail;
                                notification.FromDisplayName = (string.IsNullOrEmpty(notification.FromDisplayName)) ? user.DisplayName : notification.FromDisplayName;
                            }
                        }
                        if ((string.IsNullOrEmpty(notification.ToEmail) || string.IsNullOrEmpty(notification.ToDisplayName)) && notification.ToUserId != null)
                        {
                            var user = userRepository.GetUser(notification.ToUserId.Value);
                            if (user != null)
                            {
                                notification.ToEmail = (string.IsNullOrEmpty(notification.ToEmail)) ? user.Email : notification.ToEmail;
                                notification.ToDisplayName = (string.IsNullOrEmpty(notification.ToDisplayName)) ? user.DisplayName : notification.ToDisplayName;
                            }
                        }

                        // validate recipient
                        if (string.IsNullOrEmpty(notification.ToEmail))
                        {
                            log += "Recipient Missing For NotificationId: " + notification.NotificationId + "<br />";
                            notification.IsDeleted = true;
                            notificationRepository.UpdateNotification(notification);
                        }
                        else
                        {
                            MailMessage mailMessage = new MailMessage();
                            mailMessage.From = new MailAddress(settings["SMTPSender"], site.Name);
                            mailMessage.Subject = notification.Subject;
                            if (!string.IsNullOrEmpty(notification.FromEmail) && !string.IsNullOrEmpty(notification.FromDisplayName))
                            {
                                mailMessage.Body = "From: " + notification.FromDisplayName + "<" + notification.FromEmail + ">" + "\n";
                            }
                            else
                            {
                                mailMessage.Body = "From: " + site.Name + "\n";
                            }
                            mailMessage.Body += "Sent: " + notification.CreatedOn + "\n";
                            if (!string.IsNullOrEmpty(notification.ToEmail) && !string.IsNullOrEmpty(notification.ToDisplayName))
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
                    }
                    log += "Notifications Delivered: " + sent + "<br />";
                }
                else
                {
                    log += "SMTP Not Configured Properly In Site Settings - Host, Port, SSL, And Sender Are All Required" + "<br />";
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
