using System;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace FineWork.Net.IM
{
    public class ConvMessageModel : TableEntity
    {
        public ConvMessageModel(string convId,string msgId)
        {
            PartitionKey = convId;
            RowKey = msgId;
        }

        public ConvMessageModel() {  }

        [JsonProperty("msg-id")]
        public string MsgId { get; set; }

        [JsonProperty("conv-id")]
        public string ConvId { get; set; }

        //[JsonProperty("is-conv")]
        //public bool IsConv { get; set; }

        [JsonProperty("from")]
        public string From { get; set; }

        [JsonProperty("timestamp")] 
        public long TimeStamp { get; set; }

        //[JsonProperty("is-room")]
        //public bool IsRoom { get; set; }

        //[JsonProperty("from-ip")]
        //public string FromIp { get; set; }

        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; } 

        public DateTime? Time { get; set; }  
    }


    public class MsgAttrModel
    {
        [JsonProperty("to")]
        public string To { get; set; }

        [JsonProperty("kind")]
        public string Kind { get; set; }

        [JsonProperty("account")]
        public string AccountId { get; set; }
    }

    public class MsgData
    {

        [JsonProperty("_lcattrs")]
        public MsgAttrModel LcAttrs { get; set; }

        [JsonProperty("_lctext")]
        public string LcText { get; set; }

        [JsonProperty("_lctype")]
        public string LcType { get; set; }

    }


}