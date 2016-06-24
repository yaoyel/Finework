using System;
using System.Collections.Generic;
using FineWork.Logging;
using FineWork.Net.Mail;
using Microsoft.Extensions.Logging;

namespace FineWork.Net.Sms
{
    public class NullSmsService : ISmsService
    {
        private static readonly ILogger m_Log = LogManager.GetLogger(typeof(NullEmailService));

        public void SendMessage(SmsMessage message)
        {
            if (message == null) throw new ArgumentNullException("message");

            m_Log.LogInformation("Send SmsMessage. \nDestination: {0}, \nBody: {1}",
                message.Destination, message.Body);
        }

        public void SendMessage(string phoneNumber, string template, IDictionary<string, object> env)
        {
            throw new NotImplementedException();
        }

        public bool VerifySmsCode(string phoneNumber, string smsCode)
        {
            throw new NotImplementedException();
        }
    }
}