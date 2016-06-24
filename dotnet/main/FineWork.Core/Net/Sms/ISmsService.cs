using System.Collections.Generic;

namespace FineWork.Net.Sms
{
    public interface ISmsService
    { 
        void SendMessage(SmsMessage message);


        /// <summary>
        /// 发送消息，可用模板发送,如果无模板默认发送验证码
        /// </summary>
        /// <param name="phoneNumber"></param>
        /// <param name="template"></param>
        /// <param name="env"></param>
        void SendMessage(string phoneNumber, string template, IDictionary<string, object> env);

        /// <summary>
        /// 验证短信验证码
        /// </summary> 
        /// <param name="phoneNumber"></param>
        /// <param name="smsCode"></param>
        /// <returns></returns>
        bool VerifySmsCode(string phoneNumber, string smsCode);

    }
}