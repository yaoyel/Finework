using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla
{
    public interface IInvCodeManager
    {
        IList<InvCodeEntity> CreateInvCodes(int  len=6,int count = 100);

        bool ValidateInvCode(string code);

        void UpdateInvCode(InvCodeEntity invCode);

        InvCodeEntity FindByCode(string invCode);
    }
}
