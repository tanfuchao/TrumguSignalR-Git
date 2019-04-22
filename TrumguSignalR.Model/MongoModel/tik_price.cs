using System;
using MongoDB.Bson;

namespace TrumguSignalR.Model.MongoModel
{
    [Serializable]
    public class tik_price
    {
        public ObjectId Id { get; set; }

        public string code { get; set; }

        public double? price { get; set; }

        public DateTime? fs_time { get; set; }

        public double? high { get; set; }//最高

        public double? low { get; set; }//最低

        public double? open { get; set; }//开盘价

        public double? last_close { get; set; }//收盘价

        public double? chg { get; set; }//涨幅

        
    }
}
