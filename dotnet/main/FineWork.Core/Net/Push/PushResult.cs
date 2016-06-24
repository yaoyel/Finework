using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FineWork.Net.Push
{
    public class PushResult
    {
        public string MessageId { get; set; }

        public string TraceId { get; set; }

        public bool IsSuccess { get; set; }

        public string ErrorCode { get; set; }

        public string ErrorInfo { get; set; }
    }
}
