using System.Collections.Generic;
using TrumguSignalR.Model.MySqlModel;

namespace TrumguSignalR.MySql.IDAL
{
    public interface ITbfT0CombinationDal:IBaseDal<TbfT0Combination>
    {
        List<int> GetAllCreateUserId();
        
        List<string> GetCombinationByCreateId(int createUserId);

        //库中用户太多,这里只取一部分
        List<int> GetTestCreateUserId();
    }
}
