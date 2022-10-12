using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FluentEmail.Core;
using FluentEmail.Core.Models;
using RazorLight.Extensions;
using Spine.Common.Enums;
using Spine.Common.Extensions;
using Spine.Services.EmailTemplates.Models;

namespace Spine.Services
{
    public class AttachmentModel
    {
        public string fileName { get; set; }
        public MemoryStream fileStream { get; set; }
    }


    public interface IEmailSender
    {
        Task<bool> SendTextEmail(string toEmail, string subject, string body, bool isHtml = false, List<string> cc = null, List<string> bcc = null, List<AttachmentModel> attachments = null);
        Task<bool> SendTemplateEmail(string toEmail, string subject, EmailTemplateEnum template, ITemplateModel model, List<string> cc = null, List<string> bcc = null, List<AttachmentModel> attachments = null);
        Task<bool> SendMultipleTemplateEmail(List<string> toEmails, string subject, EmailTemplateEnum template, ITemplateModel model, List<string> cc = null, List<string> bcc = null, List<AttachmentModel> attachments = null);
    }

    public class EmailSender : IEmailSender
    {
        private readonly IFluentEmail _fluentEmail;
        private readonly IFluentEmailFactory _emailFactory;
        private const string TemplatePath = "Spine.Services.EmailTemplates.{0}.cshtml";

        public EmailSender(IFluentEmail fluentEmail, IFluentEmailFactory emailFactory)
        {
            _fluentEmail = fluentEmail;
            _emailFactory = emailFactory;
            //_fluentEmail.SetFrom("" ,"");
        }

        //public async Task<bool> SendTextEmail(string toEmail, string subject, string body)
        //{
        //    var email = _fluentEmail.To(toEmail).Subject(subject).Body(body);
        //    var send = await email.SendAsync();
        //    return send.Successful;
        //}

        public async Task<bool> SendTextEmail(string toEmail, string subject, string body, bool isHtml = false,
                                                                         List<string> cc = null, List<string> bcc = null, List<AttachmentModel> attachments = null)
        {
            var email = _fluentEmail.To(toEmail).Subject(subject).Body(body, isHtml).HandleOtherEmailStuff(cc, bcc, attachments);

            var send = await email.SendAsync();
            return send.Successful;
        }

        public async Task<bool> SendTemplateEmail(string toEmail, string subject, EmailTemplateEnum template, ITemplateModel model,
                                                                                                                List<string> cc = null, List<string> bcc = null, List<AttachmentModel> attachments = null)
        {
            try
            {
                var filePath = string.Format(TemplatePath, template);

                var convertedModel = model.ToExpando();
                var assembly = GetType().Assembly;

                var email = _fluentEmail.To(toEmail)
                     .Subject(subject)
                     .UsingTemplateFromEmbedded(filePath, model, assembly)
                     .HandleOtherEmailStuff(cc, bcc, attachments);

                var send = await email.SendAsync();
                return send.Successful;
            }
            catch (Exception e)
            {
                return false;
            }
        }

        public async Task<bool> SendMultipleTemplateEmail(List<string> toEmails, string subject, EmailTemplateEnum template, ITemplateModel model, List<string> cc = null, List<string> bcc = null, List<AttachmentModel> attachments = null)
        {
            try
            {
                var filePath = string.Format(TemplatePath, template);

                var convertedModel = model.ToExpando();
                var assembly = GetType().Assembly;

                foreach (var toEmail in toEmails)
                {
                    await _emailFactory.Create()
                            .To(toEmail)
                            .Subject(subject)
                            .UsingTemplateFromEmbedded(filePath, model, assembly).
                            HandleOtherEmailStuff(cc, bcc, attachments)
                            .SendAsync();
                }

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }

    }


    public static class EmailExtensions
    {
        public static IFluentEmail HandleOtherEmailStuff(this IFluentEmail email, List<string> cc = null, List<string> bcc = null, List<AttachmentModel> attachments = null)
        {
            if (!cc.IsNullOrEmpty())
            {
                var addresses = new List<Address>();
                foreach (var item in cc)
                {
                    addresses.Add(new Address { EmailAddress = item });
                }
                email.CC(addresses);
            }

            if (!bcc.IsNullOrEmpty())
            {
                var addresses = new List<Address>();
                foreach (var item in cc)
                {
                    addresses.Add(new Address { EmailAddress = item });
                }
                email.BCC(addresses);
            }

            if (!attachments.IsNullOrEmpty())
            {
                var toAttach = new List<Attachment>();
                foreach (var item in attachments)
                {
                    item.fileStream.Seek(0, SeekOrigin.Begin);
                    var attachment = new Attachment
                    {
                        Data = item.fileStream,
                        Filename = item.fileName,
                        ContentType = "application/octet-stream"
                    };

                    toAttach.Add(attachment);
                }

                email.Attach(toAttach);
            }

            return email;
        }

    }

}
