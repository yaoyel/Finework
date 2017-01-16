using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Colla.Models
{
    public class UpdateAnncIncentiveModel
    {
        public  Guid AnncId { get; set; }

        public List<Tuple<int,decimal>> Incentives { get; set; }

        public  bool IsGrant { get; set; }
    }
}
