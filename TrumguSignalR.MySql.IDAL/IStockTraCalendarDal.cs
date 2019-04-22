using System;
using TrumguSignalR.Model.MySqlModel;

namespace TrumguSignalR.MySql.IDAL
{
    public interface IStockTraCalendarDal:IBaseDal<StockTraCalendar>
    {
        bool IsMarketDay(DateTime date);
    }
}
