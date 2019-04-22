using System;
using SqlSugar;

namespace TrumguSignalR.Model.MySqlModel
{
    [SugarTable("t_bf_t0combination_fund")]
    public class TbfT0CombinationFund
    {
        [SugarColumn(IsPrimaryKey = true, IsIdentity = true, ColumnName = "id")]
        public int Id { get; set; }

        [SugarColumn(ColumnName = "guid")]
        public string Guid { get; set; }

        [SugarColumn(ColumnName = "stock_code")]
        public string StockCode { get; set; }

        [SugarColumn(ColumnName = "istips")]
        public int IsTips { get; set; }

        [SugarColumn(ColumnName = "createtime")]
        public DateTime CreateTime { get; set; }
    }
}
