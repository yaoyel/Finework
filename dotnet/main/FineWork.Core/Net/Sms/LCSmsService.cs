using System;
using System.Collections.Generic; 
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using AVOSCloud;

namespace FineWork.Net.Sms
{
    public class LCSmsService:ISmsService
    { 
        private string m_AppId;
        private string m_AppKey;
   

        public LCSmsService(string appId, string appKey)
        {
            m_AppId = appId;
            m_AppKey = appKey;
            AVClient.Initialize(appId, appKey);
        }

        public void SendMessage(SmsMessage message)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string phoneNumber, string template, IDictionary<string, object> env)
        {
            try
            {
                if (string.IsNullOrEmpty(template))
                    AVCloud.RequestSMSCode(phoneNumber).Wait();
                else
                    AVCloud.RequestSMSCode(phoneNumber, template, env).Wait();
            }
            catch (AggregateException ex)
            {
                throw ex.InnerException;
            }


        }

        public bool VerifySmsCode(string phoneNumber, string smsCode)
        { 
            return AVCloud.VerifySmsCode(smsCode, phoneNumber).Result;
        }
    }
}
