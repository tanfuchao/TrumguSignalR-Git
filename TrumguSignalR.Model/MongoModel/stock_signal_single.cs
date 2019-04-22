using System;

namespace TrumguSignalR.Model.MongoModel
{
    public class stock_signal_single_ex
    {
        public string Id {get;set;}
        public string code { get; set; }
        public string price { get; set; }
        public string signal { get; set; }
        public DateTime time { get; set; }
        public string state { get; set; }
        public string name { get; set; }
        public DateTime createtime { get; set; }
    }
}
