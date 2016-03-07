﻿using System;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Frapid.Messaging;
using Frapid.Messaging.DTO;
using Frapid.Messaging.Helpers;
using Frapid.Messaging.Smtp;
using Frapid.WebsiteBuilder.ViewModels;

namespace Frapid.WebsiteBuilder.Emails
{
    public class SubscriptionWelcomeEmail
    {
        private const string TemplatePath =
            "~/Tenants/{0}/Areas/Frapid.WebsiteBuilder/EmailTemplates/email-subscription-welcome.html";

        private string GetMessage(string tenant, Subscribe model)
        {
            string siteUrl = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
            string domain = HttpContext.Current.Request.Url.Host;

            string file = HostingEnvironment.MapPath(string.Format(CultureInfo.InvariantCulture, TemplatePath, tenant));

            if (file == null || !File.Exists(file))
            {
                return string.Empty;
            }

            string message = File.ReadAllText(file, Encoding.UTF8);

            message = message.Replace("{{Domain}}", domain);
            message = message.Replace("{{SiteUrl}}", siteUrl);
            message = message.Replace("{{Email}}", model.EmailAddress);

            return message;
        }

        private EmailQueue GetEmail(string tenant, Subscribe model)
        {
            var config = EmailProcessor.GetDefaultConfig(tenant);
            string domain = HttpContext.Current.Request.Url.Host;
            string subject = string.Format(CultureInfo.InvariantCulture, "Thank you for subscribing to {0}", domain);

            return new EmailQueue
            {
                AddedOn = DateTime.Now,
                FromName = config.FromName,
                ReplyTo = config.FromEmail,
                Subject = subject,
                Message = this.GetMessage(tenant, model),
                SendTo = model.EmailAddress
            };
        }

        public async Task SendAsync(string teanant, Subscribe model)
        {
            try
            {
                var email = this.GetEmail(teanant, model);
                var manager = new MailQueueManager(teanant, email);
                manager.Add();

                var processor = EmailProcessor.GetDefault(teanant);

                if (string.IsNullOrWhiteSpace(email.ReplyTo))
                {
                    email.ReplyTo = processor.Config.FromEmail;
                }

                await manager.ProcessMailQueueAsync(processor);
            }
            catch
            {
                throw new HttpException(500, "Internal Server Error");
            }
        }
    }
}