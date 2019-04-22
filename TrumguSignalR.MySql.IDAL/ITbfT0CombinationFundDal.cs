using System.Collections.Generic;
using TrumguSignalR.Model.MySqlModel;

namespace TrumguSignalR.MySql.IDAL
{
    public interface ITbfT0CombinationFundDal:IBaseDal<TbfT0CombinationFund>
    {
        List<string> GetStockCodeByGuidList(string guid);
    }
}
