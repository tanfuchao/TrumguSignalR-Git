using System.Collections.Generic;
using TrumguSignalR.Model.MySqlModel;
using TrumguSignalR.MySql.IDAL;

namespace TrumguSignalR.MySql.DAL
{
    public class TbfT0CombinationFundDal:BaseDal<TbfT0CombinationFund>,ITbfT0CombinationFundDal
    {
        /// <summary>
        /// 根据组合中的guid得到stockCode
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public List<string> GetStockCodeByGuidList(string guid)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Queryable<TbfT0CombinationFund>().Where(m => m.Guid == guid).Select(m => m.StockCode)
                    .ToList();
                return result;
            }
        }
    }
}
