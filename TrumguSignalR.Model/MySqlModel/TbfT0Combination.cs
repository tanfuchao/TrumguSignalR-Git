using System;
using SqlSugar;

namespace TrumguSignalR.Model.MySqlModel
{
    [SugarTable("t_bf_t0combination")]
    public class TbfT0Combination
    {
        [SugarColumn(IsPrimaryKey = true, ColumnName = "guid")]
        public string Guid { get; set; }

        [SugarColumn(ColumnName = "combination_name")]
        public string CombinationName { get; set; }

        [SugarColumn(ColumnName = "createuserid")]
        public int CreateUserId { get; set; }

        [SugarColumn(ColumnName = "createusertime")]
        public DateTime CreateUserTime { get; set; }
    }
}
