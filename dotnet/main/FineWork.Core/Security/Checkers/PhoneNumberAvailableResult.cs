using AppBoot.Checks;
using FineWork.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Security.Checkers
{
    public class PhoneNumberAvailableResult : CheckResult
    {
        private PhoneNumberAvailableResult(bool isSucceed, String message, IAccount conflict)
            : base(isSucceed, message)
        {
            this.Conflict = conflict;
        }

        /// <summary> 若检查失败, 则包含相应的 <see cref="IAccount"/> 集合, 否则为<c>null</c>. </summary>
        public IAccount Conflict { get; private set; }


        public static PhoneNumberAvailableResult Check(IAccountManager accountManager, String phoneNumber)
        {
            if (accountManager == null) throw new ArgumentNullException(nameof(accountManager));
            if (String.IsNullOrEmpty(phoneNumber)) throw new ArgumentException(nameof(phoneNumber)); 

            var user = accountManager.FindAccountByPhoneNumber(phoneNumber);
            if (user != null)
            {
                String errorMessage = String.Format("此手机号已注册，请更换其它手机号.", phoneNumber);
                return new PhoneNumberAvailableResult(false, errorMessage, user);
            }

            return new PhoneNumberAvailableResult(true, null, null);
        }

        public override Exception CreateException(string message)
        {
            return new FineWorkException(message);
        }
    }
}
