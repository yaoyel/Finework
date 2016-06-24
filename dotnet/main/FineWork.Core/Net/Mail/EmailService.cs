using System;
using System.Net.Mail;

namespace FineWork.Net.Mail
{
    public class EmailService : IEmailService
    {
        public EmailService()
        {
        }

        public void Send(MailMessage mailMessage)
        {
            if (mailMessage == null) throw new ArgumentNullException("mailMessage");
            MailUtil.SendUsingDefaultSmtpConfiguration(mailMessage);
        }
    }
}
