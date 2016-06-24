using System.Net.Mail;

namespace FineWork.Net.Mail
{
    public interface IEmailService
    {
        void Send(MailMessage mailMessage);
    }
}
