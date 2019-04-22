using System.Collections.Generic;

namespace TrumguSignalR.Model.HashModel
{
    public class HashPortfolio
    {
        public string StockCode { get; set; }
        public double? Price { get; set; }
        public double? Increase { get; set; }
        public List<string> Signals { get; set; }
    }
}
