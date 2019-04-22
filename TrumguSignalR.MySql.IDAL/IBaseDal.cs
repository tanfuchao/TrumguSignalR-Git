using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace TrumguSignalR.MySql.IDAL
{
    public interface IBaseDal<T> where T:class,new()
    {
        #region 插入方法
        
        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="entity">实体数据</param>
        /// <returns>返回响应条数</returns>
        int Add(T entity);

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="entity">实体数据</param>
        /// <returns>返回主键</returns>
        int AddReturnIdentity(T entity);

        /// <summary>
        /// 添加一条数据
        /// 只是将identity添加到实体的参数里面并返回
        /// 没有查询2次库
        /// 所以有些默认值什么的变动是取不到的你们需要手动进行2次查询获取
        /// </summary>
        /// <param name="entity">实体数据</param>
        /// <returns>将identity添加到实体的参数里面并返回</returns>
        T AddReturnEntity(T entity);

        /// <summary>
        /// 添加一条数据
        /// </summary>
        /// <param name="entity">实体数据</param>
        /// <returns>返回布尔值</returns>
        bool AddReturnBool(T entity);

        /// <summary>
        /// 批量添加
        /// </summary>
        /// <param name="list">要添加的集合</param>
        /// <returns>返回响应行数</returns>
        int AddBatch(IEnumerable<T> list);
        #endregion

        #region 删除方法

        /// <summary>
        /// 根据实体中的主键删除
        /// </summary>
        /// <param name="entity">实体并必须有主键</param>
        /// <returns>返回影响行数</returns>
        int Delete(T entity);

        /// <summary>
        /// 根据集合中的主键删除
        /// </summary>
        /// <param name="list">集合并必须有主键</param>
        /// <returns>返回影响行数</returns>
        int Delete(IEnumerable<T> list);

        /// <summary>
        /// 根据主键删除
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns>返回影响行数</returns>
        int Delete(int id);

        /// <summary>
        /// 根据主键集合删除
        /// </summary>
        /// <param name="ids">主键数组</param>
        /// <returns>返回影响行数</returns>
        int Delete(int[] ids);

        int Delete(IEnumerable<int> ids);
        #endregion

        #region 更新方法

        /// <summary>
        /// 根据实体主键更新
        /// </summary>
        /// <param name="entity">实体并必须有主键</param>
        /// <returns>返回影响行数</returns>
        int Modify(T entity);

        /// <summary>
        /// 批量更新
        /// </summary>
        /// <param name="list">要更新的集合并必须有主键</param>
        /// <returns>返回影响行数</returns>
        int ModifyBatch(List<T> list);
        #endregion

        #region 查询方法

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <returns>返回实体集合</returns>
        IEnumerable<T> QueryAll();

        /// <summary>
        /// 查询top条数据
        /// </summary>
        /// <param name="top">要top几</param>
        /// <returns>返回实体集合</returns>
        IEnumerable<T> QueryTop(int top);

        /// <summary>
        /// 根据主键查询
        /// </summary>
        /// <param name="id">主键</param>
        /// <returns>返回对应实体</returns>
        T QueryById(int id);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="orderBy">排序字段</param>
        /// <param name="orderType"></param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">页长</param>
        /// <param name="totalCount">总数据</param>
        /// <returns>返回实体集合</returns>
        IEnumerable<T> QueryPage(Expression<Func<T, object>> orderBy, int orderType, int pageIndex, int pageSize, ref int totalCount);

        /// <summary>
        /// 条件查询
        /// </summary>
        /// <param name="where">lamada查询条件</param>
        /// <returns>返回实体集合</returns>
        IEnumerable<T> QueryByIf(Expression<Func<T, bool>> where);

        /// <summary>
        /// 分页条件查询
        /// </summary>
        /// <param name="orderBy">排序字段</param>
        /// <param name="orderType"></param>
        /// <param name="where">lamada查询条件</param>
        /// <param name="pageIndex">起始页</param>
        /// <param name="pageSize">页长</param>
        /// <param name="totalCount">总数据</param>
        /// <returns>返回实体集合</returns>
        IEnumerable<T> QueryByIfPage(Expression<Func<T, object>> orderBy, int orderType, Expression<Func<T, bool>> where,
            int pageIndex, int pageSize, ref int totalCount);


        #endregion
    }
}
