using System;
using System.Collections.Generic;
using System.Linq;
using TrumguSignalR.Model.MySqlModel;
using TrumguSignalR.MySql.IDAL;

namespace TrumguSignalR.MySql.DAL
{
    public class TbfT0CombinationDal:BaseDal<TbfT0Combination>,ITbfT0CombinationDal
    {
        /// <summary>
        /// 获得T0Combination表中所有的用户id,去重
        /// </summary>
        /// <returns></returns>
        public List<int> GetAllCreateUserId()
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var userIdList = db.Queryable<TbfT0Combination>().Select(m => m.CreateUserId).ToList();
                var result = userIdList.Distinct().ToList();
                return result;
            }
        }

        /// <summary>
        /// 根据用户ID获取 该用户创建组合的 组合GUID
        /// </summary>
        /// <param name="createUserId"></param>
        /// <returns></returns>
        public List<string> GetCombinationByCreateId(int createUserId)
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var result = db.Queryable<TbfT0Combination>().Where(m => m.CreateUserId == createUserId)
                    .Select(m => m.Guid).ToList();
                return result;
            }
        }

        public List<int> GetTestCreateUserId()
        {
            using (var db = SqlSugarFactory.GetInstance())
            {
                var userIdList = db.Queryable<TbfT0Combination>().Where(m=>m.CreateUserTime<Convert.ToDateTime("2018-12-12")). Select(m => m.CreateUserId).ToList();
                var result = userIdList.Distinct().ToList();
                return result;
            }
        }
    }
}
