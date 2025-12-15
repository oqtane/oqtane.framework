using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Client;
using MimeKit;
using Oqtane.Models;
using Oqtane.Repository;
using Oqtane.Shared;
using MailKit.Security;

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
        public async override Task<string> ExecuteJobAsync(IServiceProvider provider)
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

                List<Notification> notifications = notificationRepository.GetNotifications(site.SiteId, -1, -1).ToList();
                if (notifications.Count > 0)
                {
                    // get site settings
                    var settings = settingRepository.GetSettings(EntityNames.Site, site.SiteId, EntityNames.Host, -1);

                    if (!site.IsDeleted && settingRepository.GetSettingValue(settings, "SMTPEnabled", "True") == "True")
                    {
                        bool valid = true;
                        if (settingRepository.GetSettingValue(settings, "SMTPAuthentication", "Basic") == "Basic")
                        {
                            // basic
                            if (settingRepository.GetSettingValue(settings, "SMTPHost", "") == "" ||
                                settingRepository.GetSettingValue(settings, "SMTPPort", "") == "" ||
                                settingRepository.GetSettingValue(settings, "SMTPSender", "") == "")
                            {
                                log += "SMTP Not Configured Properly In Site Settings - Host, Port, And Sender Are All Required" + "<br />";
                                valid = false;
                            }
                        }
                        else
                        {
                            // oauth
                            if (settingRepository.GetSettingValue(settings, "SMTPHost", "") == "" ||
                                settingRepository.GetSettingValue(settings, "SMTPPort", "") == "" ||
                                settingRepository.GetSettingValue(settings, "SMTPAuthority", "") == "" ||
                                settingRepository.GetSettingValue(settings, "SMTPClientId", "") == "" ||
                                settingRepository.GetSettingValue(settings, "SMTPClientSecret", "") == "" ||
                                settingRepository.GetSettingValue(settings, "SMTPScopes", "") == "" ||
                                settingRepository.GetSettingValue(settings, "SMTPSender", "") == "")
                            {
                                log += "SMTP Not Configured Properly In Site Settings - Host, Port, Authority, Client ID, Client Secret, Scopes, And Sender Are All Required" + "<br />";
                                valid = false;
                            }
                        }

                        if (valid)
                        {
                            // construct SMTP Client
                            using var client = new SmtpClient();

                            try
                            {
                                var secureSocketOptions = SecureSocketOptions.Auto;
                                switch (settingRepository.GetSettingValue(settings, "SMTPSSL", "Auto"))
                                {
                                    case "None":
                                        secureSocketOptions = SecureSocketOptions.None;
                                        break;
                                    case "Auto":
                                        secureSocketOptions = SecureSocketOptions.Auto;
                                        break;
                                    case "StartTls":
                                        secureSocketOptions = SecureSocketOptions.StartTls;
                                        break;
                                    case "SslOnConnect":
                                    case "True": // legacy setting value
                                        secureSocketOptions = SecureSocketOptions.SslOnConnect;
                                        break;
                                    case "StartTlsWhenAvailable":
                                    case "False": // legacy setting value
                                        secureSocketOptions = SecureSocketOptions.StartTlsWhenAvailable;
                                        break;
                                }

                                await client.ConnectAsync(settingRepository.GetSettingValue(settings, "SMTPHost", ""),
                                        int.Parse(settingRepository.GetSettingValue(settings, "SMTPPort", "")),
                                        secureSocketOptions);
                            }
                            catch (Exception ex)
                            {
                                log += "SMTP Not Configured Properly In Site Settings - Could Not Connect To SMTP Server - " + ex.Message + "<br />";
                                valid = false;
                            }

                            if (valid)
                            {
                                if (settingRepository.GetSettingValue(settings, "SMTPAuthentication", "Basic") == "Basic")
                                {
                                    // it is possible to use basic without any authentication (not recommended)
                                    if (settingRepository.GetSettingValue(settings, "SMTPUsername", "") != "" && settingRepository.GetSettingValue(settings, "SMTPPassword", "") != "")
                                    {
                                        await client.AuthenticateAsync(settingRepository.GetSettingValue(settings, "SMTPUsername", ""),
                                            settingRepository.GetSettingValue(settings, "SMTPPassword", ""));
                                    }
                                }
                                else
                                {
                                    // oauth authentication
                                    var confidentialClientApplication = ConfidentialClientApplicationBuilder.Create(settingRepository.GetSettingValue(settings, "SMTPClientId", ""))
                                        .WithAuthority(settingRepository.GetSettingValue(settings, "SMTPAuthority", ""))
                                        .WithClientSecret(settingRepository.GetSettingValue(settings, "SMTPClientSecret", ""))
                                        .Build();
                                    try
                                    {
                                        var result = await confidentialClientApplication.AcquireTokenForClient(settingRepository.GetSettingValue(settings, "SMTPScopes", "").Split(',')).ExecuteAsync();
                                        var oauth2 = new SaslMechanismOAuth2(settingRepository.GetSettingValue(settings, "SMTPSender", ""), result.AccessToken);
                                        await client.AuthenticateAsync(oauth2);
                                    }
                                    catch (Exception ex)
                                    {
                                        log += "SMTP Not Configured Properly In Site Settings - OAuth Token Could Not Be Retrieved From Authority - " + ex.Message + "<br />";
                                        valid = false;
                                    }
                                }
                            }

                            if (valid)
                            {
                                // iterate through undelivered notifications
                                int sent = 0;
                                foreach (Notification notification in notifications)
                                {
                                    var fromEmail = notification.FromEmail ?? "";
                                    var fromName = notification.FromDisplayName ?? "";
                                    var toEmail = notification.ToEmail ?? "";
                                    var toName = notification.ToDisplayName ?? "";

                                    // get sender and receiver information from user information if available
                                    if ((string.IsNullOrEmpty(fromEmail) || string.IsNullOrEmpty(fromName)) && notification.FromUserId != null)
                                    {
                                        var user = userRepository.GetUser(notification.FromUserId.Value);
                                        if (user != null)
                                        {
                                            fromEmail = string.IsNullOrEmpty(fromEmail) ? user.Email ?? "" : fromEmail;
                                            fromName = string.IsNullOrEmpty(fromName) ? user.DisplayName ?? "" : fromName;
                                        }
                                    }
                                    if ((string.IsNullOrEmpty(toEmail) || string.IsNullOrEmpty(toName)) && notification.ToUserId != null)
                                    {
                                        var user = userRepository.GetUser(notification.ToUserId.Value);
                                        if (user != null)
                                        {
                                            toEmail = string.IsNullOrEmpty(toEmail) ? user.Email ?? "" : toEmail;
                                            toName = string.IsNullOrEmpty(toName) ? user.DisplayName ?? "" : toName;
                                        }
                                    }

                                    // create mailbox addresses
                                    MailboxAddress to = null;
                                    MailboxAddress from = null;
                                    var mailboxAddressValidationError = "";

                                    // sender
                                    if ((settingRepository.GetSettingValue(settings, "SMTPRelay", "False") == "True") && string.IsNullOrEmpty(fromEmail))
                                    {
                                        fromEmail = settingRepository.GetSettingValue(settings, "SMTPSender", "");
                                        fromName = string.IsNullOrEmpty(fromName) ? site.Name : fromName;
                                    }
                                    if (MailboxAddress.TryParse(fromEmail, out from))
                                    {
                                        from.Name = fromName;
                                    }
                                    else
                                    {

                                        mailboxAddressValidationError += $" Invalid Sender: {fromName} &lt;{fromEmail}&gt;";
                                    }

                                    // recipient
                                    if (MailboxAddress.TryParse(toEmail, out to))
                                    {
                                        to.Name = toName;
                                    }
                                    else
                                    {
                                        mailboxAddressValidationError += $" Invalid Recipient: {toName} &lt;{toEmail}&gt;";
                                    }

                                    // if mailbox addresses are valid
                                    if (from != null && to != null)
                                    {
                                        // create mail message
                                        MimeMessage mailMessage = new MimeMessage();
                                        mailMessage.From.Add(from);
                                        mailMessage.To.Add(to);

                                        // subject
                                        mailMessage.Subject = notification.Subject;

                                        // body
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
                                            await client.SendAsync(mailMessage);
                                            sent++;
                                            notification.IsDelivered = true;
                                            notification.DeliveredOn = DateTime.UtcNow;
                                            notificationRepository.UpdateNotification(notification);
                                        }
                                        catch (Exception ex)
                                        {
                                            log += $"Error Sending Notification Id: {notification.NotificationId} - {ex.Message}<br />";
                                        }
                                    }
                                    else
                                    {
                                        // invalid mailbox address
                                        log += $"Notification Id: {notification.NotificationId} Has An {mailboxAddressValidationError} And Has Been Deleted<br />";
                                        notification.IsDeleted = true;
                                        notificationRepository.UpdateNotification(notification);
                                    }
                                }

                                log += "Notifications Delivered: " + sent + "<br />";
                            }

                            await client.DisconnectAsync(true);
                        }
                    }
                    else
                    {
                        log += "Site Deleted Or SMTP Disabled In Site Settings<br />";
                    }
                }
                else
                {
                    log += "No Notifications To Deliver<br />";
                }
            }

            return log;
        }
    }
}
