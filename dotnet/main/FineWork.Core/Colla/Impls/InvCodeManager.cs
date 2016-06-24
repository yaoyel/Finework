using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AppBoot.Repos;
using AppBoot.Repos.Aef;
using AppBoot.Security.Crypto;

namespace FineWork.Colla.Impls
{
    public class InvCodeManager:AefEntityManager<InvCodeEntity, string>, IInvCodeManager
    {
        public InvCodeManager(ISessionProvider<AefSession> sessionProvider) : base(sessionProvider)
        {
            if (sessionProvider == null) throw new ArgumentException(nameof(sessionProvider));
        }

        public IList<InvCodeEntity> CreateInvCodes(int len=6,int count = 100)
        {
            var invCodes = new List<InvCodeEntity>();

            var ranStr = CryptoUtil.CreateRandomText(len, count);

            ranStr.ForEach(p =>
            {
                var invCode = new InvCodeEntity() {Id=p};
                this.InternalInsert(invCode);
                invCodes.Add(invCode);
            });
          
            return invCodes;
        }

        public bool ValidateInvCode(string code)
        {
            var invCode = this.InternalFind(code);
            return invCode?.ExpiredAt != null;
        }

        public void UpdateInvCode(InvCodeEntity invCode)
        {
            this.InternalUpdate(invCode);
        }

        public InvCodeEntity FindByCode(string invCode)
        {
            return this.InternalFind(invCode);
        }
    }
}
