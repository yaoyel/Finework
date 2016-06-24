using System;
using System.Net.Configuration;
using System.Net.Mail;
using NUnit.Framework;

namespace FineWork.Net.Mail
{
    [TestFixture]
    public class EmailServiceTests
    {
        [Ignore("Too many mails may fail for 'unusual sign-in activity‏'")]
        [Test]
        public void SendUsingSmtpConfiguration()
        {
            SmtpSection smtpSection = MailUtil.GetDefaultSmtpConfiguration(true);
            MailMessage mail = CreateMailMessageForTesting(smtpSection.From);
            MailUtil.SendUsingDefaultSmtpConfiguration(mail);
        }


        private MailMessage CreateMailMessageForTesting(String fromAddress)
        {
            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromAddress);
            mail.To.Add(new MailAddress("skyaoj@gmail.com"));
            mail.IsBodyHtml = false;
            mail.Subject = "FineWork EmailService test";
            mail.Body = String.Format("The mail is sent by {0} .", fromAddress);
            return mail;
        }
     }
}
