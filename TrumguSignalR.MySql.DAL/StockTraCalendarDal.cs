using System;
using System.Linq;
using TrumguSignalR.Model.MySqlModel;
using TrumguSignalR.MySql.IDAL;

namespace TrumguSignalR.MySql.DAL
{
    public class StockTraCalendarDal : BaseDal<StockTraCalendar>, IStockTraCalendarDal
    {
        public bool IsMarketDay(DateTime date)
        {
            var calendars = QueryByIf(m=>m.CalDate==date);
            return calendars != null && calendars.Any();
        }
    }
}
