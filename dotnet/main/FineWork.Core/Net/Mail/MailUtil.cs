using System;
using System.Configuration;
using System.Net.Configuration;
using System.Net.Mail;

namespace FineWork.Net.Mail
{
    public static class MailUtil
    {
        /// <summary> Gets the <see cref="SmtpSection"/> from the configuration file. </summary>
        /// <remarks> The configuration element path is: <c>system.net/mailSettings/smtp</c></remarks>
        public static SmtpSection GetDefaultSmtpConfiguration(bool isRequired)
        {
            var result = (SmtpSection) ConfigurationManager.GetSection("system.net/mailSettings/smtp");
            if (result == null && isRequired) throw MissingConfiguration("system.net/mailSettings/smtp");
            return result;
        }

        public static void SendUsingDefaultSmtpConfiguration(MailMessage mail)
        {
            if (mail == null) throw new ArgumentNullException("mail");

            using (SmtpClient smtp = new SmtpClient())
            {
                smtp.Send(mail);
            }
        }

        private static ConfigurationErrorsException MissingConfiguration(String name)
        {
            return new ConfigurationErrorsException(String.Format("Cannot find configuration element [{0}].", name));
        }
    }
}
