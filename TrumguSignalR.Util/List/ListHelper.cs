using System.Collections.Generic;


namespace TrumguSignalR.Util.List
{
    public static class ListHelper
    {
        /// <summary>
        /// 对比两个同类型的List返回差异List集合
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="newModel">修改后的数据集合</param>
        /// <param name="oldModel">原始数据集合</param>
        /// <returns>返回与原始集合有差异的集合</returns>
        public static List<T> GetModifyList<T>(List<T> newModel, List<T> oldModel)
        {
            var list = new List<T>();
            foreach (var newMod in newModel)
            {
                var isExist = false;
                foreach (var oldMol in oldModel)
                {
                    //取得老实体对象的属性集合
                    var pi = oldMol.GetType().GetProperties();
                    //定义记数器
                    var i = 0;
                    //将老实体对象的没一个属性值和新实体对象进行循环比较
                    foreach (var p in pi)
                    {
                        var oNew = newMod.GetType().GetProperty(p.Name)?.GetValue(newMod, null) ?? string.Empty;
                        var oOld = p.GetValue(oldMol, null) ?? string.Empty;
                        //新老实体比较并记录成功次数
                        if (Equals(oNew, oOld))
                        {
                            i++;
                        }
                        //若成功次数和属性数目相等则说明已经存在或者没有发生过修改条出循环
                        if (i != pi.Length)
                        {
                            continue;
                        }
                        isExist = true;
                        break;
                    }
                    //没有发生过修改条出循环
                    if (isExist)
                        break;
                }
                //如果不存在则添加该实体到List<T>中
                if (!isExist)
                    list.Add(newMod);
            }
            return list;
        }

    }
}
