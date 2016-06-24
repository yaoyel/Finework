using System;
using System.Net.Mail;
using FineWork.Logging;
using Microsoft.Extensions.Logging;

namespace FineWork.Net.Mail
{
    public class NullEmailService : IEmailService
    {
        private static readonly ILogger m_Log = LogManager.GetLogger(typeof(NullEmailService));

        public void Send(MailMessage mailMessage)
        {
            if (mailMessage == null) throw new ArgumentNullException("mailMessage");

            m_Log.LogInformation("Send EmailMessage. \nFrom: {0} \nTo: {1}, \nSubject: {2}, \nBody: {3}",
                mailMessage.From, mailMessage.To, mailMessage.Subject, mailMessage.Body);
        }

        private static readonly NullEmailService m_Instance = new NullEmailService();

        public static IEmailService Instance
        {
            get { return m_Instance; }
        }
    }
}
