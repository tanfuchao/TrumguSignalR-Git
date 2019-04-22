using System;
using MongoDB.Bson;

namespace TrumguSignalR.Model.MongoModel
{
    [Serializable]
    public class stock_signal_single
    {
        public ObjectId Id {get;set;}
        public string code { get; set; }
        public string price { get; set; }
        public string signal { get; set; }
        public DateTime time { get; set; }
        public string state { get; set; }
        public string name { get; set; }
        public DateTime createtime { get; set; }
    }
}
