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
                var settings = settingRepository.GetSettings(EntityNames.Site, site.SiteId, EntityNames.Host, -1);

                if (!site.IsDeleted && settingRepository.GetSettingValue(settings, "SMTPEnabled", "True") == "True")
                {
                    if (settingRepository.GetSettingValue(settings, "SMTPHost", "") != "" &&
                        settingRepository.GetSettingValue(settings, "SMTPPort", "") != "" &&
                        settingRepository.GetSettingValue(settings, "SMTPSender", "") != "")
                    {
                        // construct SMTP Client 
                        var client = new SmtpClient()
                        {
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            UseDefaultCredentials = false,
                            Host = settingRepository.GetSettingValue(settings, "SMTPHost", ""),
                            Port = int.Parse(settingRepository.GetSettingValue(settings, "SMTPPort", "")),
                            EnableSsl = bool.Parse(settingRepository.GetSettingValue(settings, "SMTPSSL", "False"))
                        };
                        if (settingRepository.GetSettingValue(settings, "SMTPUsername", "") != "" && settingRepository.GetSettingValue(settings, "SMTPPassword", "") != "")
                        {
                            client.Credentials = new NetworkCredential(settingRepository.GetSettingValue(settings, "SMTPUsername", ""), settingRepository.GetSettingValue(settings, "SMTPPassword", ""));
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
                            if (string.IsNullOrEmpty(notification.ToEmail) || !MailAddress.TryCreate(notification.ToEmail, out _))
                            {
                                log += $"NotificationId: {notification.NotificationId} - Has Missing Or Invalid Recipient {notification.ToEmail}<br />";
                                notification.IsDeleted = true;
                                notificationRepository.UpdateNotification(notification);
                            }
                            else
                            {
                                MailMessage mailMessage = new MailMessage();

                                // sender
                                if (settingRepository.GetSettingValue(settings, "SMTPRelay", "False") == "True" && !string.IsNullOrEmpty(notification.FromEmail))
                                {
                                    if (!string.IsNullOrEmpty(notification.FromDisplayName))
                                    {
                                        mailMessage.From = new MailAddress(notification.FromEmail, notification.FromDisplayName);
                                    }
                                    else
                                    {
                                        mailMessage.From = new MailAddress(notification.FromEmail);
                                    }
                                }
                                else
                                {
                                    mailMessage.From = new MailAddress(settingRepository.GetSettingValue(settings, "SMTPSender", ""), (!string.IsNullOrEmpty(notification.FromDisplayName)) ? notification.FromDisplayName : site.Name);
                                }

                                // recipient
                                if (!string.IsNullOrEmpty(notification.ToDisplayName))
                                {
                                    mailMessage.To.Add(new MailAddress(notification.ToEmail, notification.ToDisplayName));
                                }
                                else
                                {
                                    mailMessage.To.Add(new MailAddress(notification.ToEmail));
                                }

                                // subject
                                mailMessage.Subject = notification.Subject;

                                //body
                                mailMessage.Body = notification.Body;
                                if (!mailMessage.Body.Contains("<") || !mailMessage.Body.Contains(">"))
                                {
                                    // plain text messages should convert line breaks to HTML tags to preserve formatting
                                    mailMessage.Body = mailMessage.Body.Replace("\n", "<br />");
                                }

                                // encoding
                                mailMessage.SubjectEncoding = System.Text.Encoding.UTF8;
                                mailMessage.BodyEncoding = System.Text.Encoding.UTF8;
                                mailMessage.IsBodyHtml = true;

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
