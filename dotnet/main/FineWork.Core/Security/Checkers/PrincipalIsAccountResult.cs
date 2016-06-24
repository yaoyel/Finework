using System;
using System.Security.Principal;
using AppBoot.Checks;

namespace FineWork.Security.Checkers
{
    /// <summary> 检查系统用户是否代表某个 <see cref="IAccount"/>. </summary>
    public class PrincipalIsAccountResult : CheckResult
    {
        public PrincipalIsAccountResult(bool isSucceed, String message, IPrincipal principal, Guid accountId) 
            : base(isSucceed, message)
        {
            this.Principal = principal;
            this.AccountId = accountId;
        }

        public IPrincipal Principal { get; private set; }

        public Guid AccountId { get; private set; }

        /// <summary> 检查系统用户 (<see cref="IPrincipal"/>) 是否是相应的 <see cref="IAccount"/>. </summary>
        /// <returns> 若成功则返回 <c>true</c>, 否则返回 <c>false</c>. </returns>
        public static PrincipalIsAccountResult Check(IPrincipal principal, Guid accountId)
        {
            if (principal == null) throw new ArgumentNullException("principal");

            if (principal.Identity.IsAuthenticated == false)
            {
                var message = String.Format("The principal [{0}] has NOT been authenticated.", principal.Identity.Name);
                return new PrincipalIsAccountResult(false, message, principal, accountId);
            }

            Guid? actualAccountId = principal.FindAccountId();
            if ((!actualAccountId.HasValue) || (actualAccountId.Value != accountId))
            {
                var message = String.Format("The account Id is [{0}] while [{1}] is expected.", actualAccountId, accountId);
                return new PrincipalIsAccountResult(false, message, principal, accountId);
            }

            return new PrincipalIsAccountResult(true, null, principal, accountId);
        }
    }
}