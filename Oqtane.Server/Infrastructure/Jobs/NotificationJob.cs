using System;
using System.Collections.Generic;
using System.Linq;

using MailKit.Net.Smtp;

using Microsoft.Extensions.DependencyInjection;

using MimeKit;

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
                var settings = settingRepository.GetSettings(EntityNames.Site, site.SiteId, EntityNames.Host, -1);

                if (!site.IsDeleted && settingRepository.GetSettingValue(settings, "SMTPEnabled", "True") == "True")
                {
                    if (settingRepository.GetSettingValue(settings, "SMTPHost", "") != "" &&
                        settingRepository.GetSettingValue(settings, "SMTPPort", "") != "" &&
                        settingRepository.GetSettingValue(settings, "SMTPSender", "") != "")
                    {
                        // construct SMTP Client
                        using var client = new SmtpClient();

                        client.Connect(host: settingRepository.GetSettingValue(settings, "SMTPHost", ""), 
                                       port: int.Parse(settingRepository.GetSettingValue(settings, "SMTPPort", "")),
                                       options: MailKit.Security.SecureSocketOptions.Auto);

                        if (settingRepository.GetSettingValue(settings, "SMTPUsername", "") != "" && settingRepository.GetSettingValue(settings, "SMTPPassword", "") != "")
                        {
                            client.Authenticate(settingRepository.GetSettingValue(settings, "SMTPUsername", ""), 
                                                settingRepository.GetSettingValue(settings, "SMTPPassword", ""));
                        }

                        // iterate through undelivered notifications
                        int sent = 0;
                        List<Notification> notifications = notificationRepository.GetNotifications(site.SiteId, -1, -1).ToList();
                        foreach (Notification notification in notifications)
                        {
                            // get sender and receiver information from user object if not provided
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
                            if (string.IsNullOrEmpty(notification.ToEmail) || !MailboxAddress.TryParse(notification.ToEmail, out _))
                            {
                                log += $"NotificationId: {notification.NotificationId} - Has Missing Or Invalid Recipient {notification.ToEmail}<br />";
                                notification.IsDeleted = true;
                                notificationRepository.UpdateNotification(notification);
                            }
                            else
                            {
                                MimeMessage mailMessage = new MimeMessage();

                                // sender
                                if (settingRepository.GetSettingValue(settings, "SMTPRelay", "False") == "True" && !string.IsNullOrEmpty(notification.FromEmail))
                                {
                                    if (!string.IsNullOrEmpty(notification.FromDisplayName))
                                    {
                                        mailMessage.From.Add(new MailboxAddress(notification.FromDisplayName, notification.FromEmail));
                                    }
                                    else
                                    {
                                        mailMessage.From.Add(new MailboxAddress("", notification.FromEmail));
                                    }
                                }
                                else
                                {
                                    mailMessage.From.Add(new MailboxAddress((!string.IsNullOrEmpty(notification.FromDisplayName)) ? notification.FromDisplayName : site.Name,
                                                                            settingRepository.GetSettingValue(settings, "SMTPSender", "")));
                                }

                                // recipient
                                if (!string.IsNullOrEmpty(notification.ToDisplayName))
                                {
                                    mailMessage.To.Add(new MailboxAddress(notification.ToDisplayName, notification.ToEmail));
                                }
                                else
                                {
                                    mailMessage.To.Add(new MailboxAddress("", notification.ToEmail));
                                }

                                // subject
                                mailMessage.Subject = notification.Subject;

                                //body
                                var bodyText = notification.Body;

                                if (!bodyText.Contains('<') || !bodyText.Contains('>'))
                                {
                                    // plain text messages should convert line breaks to HTML tags to preserve formatting
                                    bodyText = bodyText.Replace("\n", "<br />");
                                }

                                mailMessage.Body = new TextPart("html", System.Text.Encoding.UTF8)
                                {
                                    Text = bodyText
                                };

                                // send mail
                                try
                                {
                                    client.Send(mailMessage);
                                    sent++;
                                    notification.IsDelivered = true;
                                    notification.DeliveredOn = DateTime.UtcNow;
                                    notificationRepository.UpdateNotification(notification);
                                }
                                catch (Exception ex)
                                {
                                    // error
                                    log += $"NotificationId: {notification.NotificationId} - {ex.Message}<br />";
                                }
                            }
                        }
                        client.Disconnect(true);
                        log += "Notifications Delivered: " + sent + "<br />";
                    }
                    else
                    {
                        log += "SMTP Not Configured Properly In Site Settings - Host, Port, And Sender Are All Required" + "<br />";
                    }
                }
                else
                {
                    log += "Site Deleted Or SMTP Disabled In Site Settings" + "<br />";
                }
            }

            return log;
        }
    }
}
