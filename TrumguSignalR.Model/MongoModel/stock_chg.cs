using System;
using MongoDB.Bson;

namespace TrumguSignalR.Model.MongoModel
{
    [Serializable]
    public class stock_chg
    {
        public ObjectId Id { get; set; }

        public string tag { get; set; }

        public int? stock_tol { get; set; }

        public int? halt { get; set; }

        public DateTime? datetime { get; set; }

        public int? limit_down { get; set; }

        public int? limit_down_7 { get; set; }

        public int? limit_down_5 { get; set; }

        public int? limit_down_3 { get; set; }

        public int? limit_down_0 { get; set; }

        public int? limit_up_0 { get; set; }

        public int? limit_up_3 { get; set; }

        public int? limit_up_5 { get; set; }

        public int? limit_up_7 { get; set; }

        public double? limit_up { get; set; }
    }
}
