using System;
using SqlSugar;

namespace TrumguSignalR.Model.MySqlModel
{
    [SugarTable("stock_sp1.stock_tra_calendar")]
    public class StockTraCalendar
    {
        [SugarColumn(ColumnName = "index")]
        public int Index { get; set; }

        [SugarColumn(ColumnName = "cal_date")]
        public DateTime CalDate { get; set; }
    }
}
