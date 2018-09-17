#region License
/***
 * Copyright © 2018, 张强 (943620963@qq.com).
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *      http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * without warranties or conditions of any kind, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
/****************************
* [Author] 张强
* [Date] 2018-07-23
* [Describe] 数据操作仓储接口
* **************************/
namespace SQLBuilder.Core.Repositories
{
    /// <summary>
    /// 数据操作仓储接口
    /// </summary>
    public interface IRepository
    {
        #region Transaction
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns>IRepository</returns>
        IRepository BeginTrans();

        /// <summary>
        /// 提交事务
        /// </summary>
        /// <returns></returns>
        void Commit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();

        /// <summary>
        /// 关闭连接
        /// </summary>
        void Close();
        #endregion

        #region ExecuteBySql
        #region Sync
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回受影响行数</returns>
        int ExecuteBySql(string sql);

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        int ExecuteBySql(string sql, object parameter);

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        int ExecuteBySql(string sql, params DbParameter[] dbParameter);

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <returns>返回受影响行数</returns>
        int ExecuteByProc(string procName);

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        int ExecuteByProc(string procName, object parameter);

        /// <summary>
        /// 执行sql存储过程进行查询
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="procName"></param>
        /// <param name="parameter"></param>
        /// <returns></returns>
        IEnumerable<T> ExecuteByProc<T>(string procName, object parameter);

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        int ExecuteByProc(string procName, params DbParameter[] dbParameter);
        #endregion

        #region Async
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回受影响行数</returns>
        Task<int> ExecuteBySqlAsync(string sql);

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        Task<int> ExecuteBySqlAsync(string sql, object parameter);

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        Task<int> ExecuteBySqlAsync(string sql, params DbParameter[] dbParameter);

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <returns>返回受影响行数</returns>
        Task<int> ExecuteByProcAsync(string procName);

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        Task<int> ExecuteByProcAsync(string procName, object parameter);

        /// <summary>
        /// 执行sql存储过程进行查询
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        Task<IEnumerable<T>> ExecuteByProcAsync<T>(string procName, object parameter);

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        Task<int> ExecuteByProcAsync(string procName, params DbParameter[] dbParameter);
        #endregion
        #endregion

        #region Insert
        #region Sync
        /// <summary>
        /// 插入单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <returns>返回受影响行数</returns>
        int Insert<T>(T entity) where T : class;

        /// <summary>
        /// 插入多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要插入的实体集合</param>
        /// <returns>返回受影响行数</returns>
        int Insert<T>(IEnumerable<T> entities) where T : class;
        #endregion

        #region Async
        /// <summary>
        /// 插入单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <returns>返回受影响行数</returns>
        Task<int> InsertAsync<T>(T entity) where T : class;

        /// <summary>
        /// 插入多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要插入的实体集合</param>
        /// <returns>返回受影响行数</returns>
        Task<int> InsertAsync<T>(IEnumerable<T> entities) where T : class;
        #endregion
        #endregion

        #region Delete
        #region Sync
        /// <summary>
        /// 删除全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回受影响行数</returns>
        int Delete<T>() where T : class;

        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要删除的实体</param>
        /// <returns>返回受影响行数</returns>
        int Delete<T>(T entity) where T : class;

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要删除的实体集合</param>
        /// <returns>返回受影响行数</returns>
        int Delete<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// 根据linq条件删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回受影响行数</returns>
        int Delete<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据单个主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="KeyValue">主键值</param>
        /// <returns>返回受影响行数</returns>
        int Delete<T>(object KeyValue) where T : class;

        /// <summary>
        /// 根据多个主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="KeyValue">主键集合</param>
        /// <returns>返回受影响行数</returns>
        int Delete<T>(object[] KeyValue) where T : class;

        /// <summary>
        /// 根据属性删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns>返回受影响行数</returns>
        int Delete<T>(string propertyName, object propertyValue) where T : class;
        #endregion

        #region Async
        /// <summary>
        /// 删除全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回受影响行数</returns>
        Task<int> DeleteAsync<T>() where T : class;

        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要删除的实体</param>
        /// <returns>返回受影响行数</returns>
        Task<int> DeleteAsync<T>(T entity) where T : class;

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要删除的实体集合</param>
        /// <returns>返回受影响行数</returns>
        Task<int> DeleteAsync<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// 根据linq条件删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回受影响行数</returns>
        Task<int> DeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据单个主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="KeyValue">主键值</param>
        /// <returns>返回受影响行数</returns>
        Task<int> DeleteAsync<T>(object KeyValue) where T : class;

        /// <summary>
        /// 根据多个主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="KeyValue">主键集合</param>
        /// <returns>返回受影响行数</returns>
        Task<int> DeleteAsync<T>(object[] KeyValue) where T : class;

        /// <summary>
        /// 根据属性删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns>返回受影响行数</returns>
        Task<int> DeleteAsync<T>(string propertyName, object propertyValue) where T : class;
        #endregion
        #endregion

        #region Update
        #region Sync
        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要更新的实体</param>
        /// <returns>返回受影响行数</returns>
        int Update<T>(T entity) where T : class;

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要更新的实体集合</param>
        /// <returns>返回受影响行数</returns>
        int Update<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// 根据linq条件更新实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <param name="entity">要更新的实体</param>
        /// <returns>返回受影响行数</returns>
        int Update<T>(Expression<Func<T, bool>> predicate, Expression<Func<object>> entity) where T : class;
        #endregion

        #region Async
        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要更新的实体</param>
        /// <returns>返回受影响行数</returns>
        Task<int> UpdateAsync<T>(T entity) where T : class;

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要更新的实体集合</param>
        /// <returns>返回受影响行数</returns>
        Task<int> UpdateAsync<T>(IEnumerable<T> entities) where T : class;

        /// <summary>
        /// 根据linq条件更新实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <param name="entity">要更新的实体</param>
        /// <returns>返回受影响行数</returns>
        Task<int> UpdateAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<object>> entity) where T : class;
        #endregion
        #endregion

        #region FindObject
        #region Sync
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果对象</returns>
        object FindObject(string sql);

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        object FindObject(string sql, object parameter);

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        object FindObject(string sql, params DbParameter[] dbParameter);
        #endregion

        #region Async
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果对象</returns>
        Task<object> FindObjectAsync(string sql);

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        Task<object> FindObjectAsync(string sql, object parameter);

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        Task<object> FindObjectAsync(string sql, params DbParameter[] dbParameter);
        #endregion
        #endregion

        #region FindEntity
        #region Sync
        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="KeyValue">主键值</param>
        /// <returns>返回实体</returns>
        T FindEntity<T>(object KeyValue) where T : class;

        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="KeyValue">主键值</param>
        /// <returns>返回实体</returns>
        T FindEntity<T>(Expression<Func<T, object>> selector, object KeyValue) where T : class;

        /// <summary>
        /// 根据linq条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回实体</returns>
        T FindEntity<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据linq条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回实体</returns>
        T FindEntity<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回实体</returns>
        T FindEntityBySql<T>(string sql);

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回实体</returns>
        T FindEntityBySql<T>(string sql, object parameter);

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回实体</returns>
        T FindEntityBySql<T>(string sql, params DbParameter[] dbParameter);
        #endregion

        #region Async
        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="KeyValue">主键值</param>
        /// <returns>返回实体</returns>
        Task<T> FindEntityAsync<T>(object KeyValue) where T : class;

        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="KeyValue">主键值</param>
        /// <returns>返回实体</returns>
        Task<T> FindEntityAsync<T>(Expression<Func<T, object>> selector, object KeyValue) where T : class;

        /// <summary>
        /// 根据linq条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回实体</returns>
        Task<T> FindEntityAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据linq条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回实体</returns>
        Task<T> FindEntityAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回实体</returns>
        Task<T> FindEntityBySqlAsync<T>(string sql);

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回实体</returns>
        Task<T> FindEntityBySqlAsync<T>(string sql, object parameter);

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回实体</returns>
        Task<T> FindEntityBySqlAsync<T>(string sql, params DbParameter[] dbParameter);
        #endregion
        #endregion

        #region IQueryable
        #region Sync
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回集合</returns>
        IQueryable<T> IQueryable<T>() where T : class;

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        IQueryable<T> IQueryable<T>(Expression<Func<T, object>> selector) where T : class;

        /// <summary>
        /// 根据linq查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        IQueryable<T> IQueryable<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据linq查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        IQueryable<T> IQueryable<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class;
        #endregion

        #region Async
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <returns>返回集合</returns>
        Task<IQueryable<T>> IQueryableAsync<T>() where T : class;

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        Task<IQueryable<T>> IQueryableAsync<T>(Expression<Func<T, object>> selector) where T : class;

        /// <summary>
        /// 根据linq查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        Task<IQueryable<T>> IQueryableAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据linq查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        Task<IQueryable<T>> IQueryableAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class;
        #endregion
        #endregion

        #region FindList
        #region Sync
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>() where T : class;

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector) where T : class;

        /// <summary>
        /// 查询并根据条件进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="orderby">排序字段</param>
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>(Func<T, object> orderby) where T : class;

        /// <summary>
        /// 查询并根据条件进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="orderby">排序字段</param>
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector, Func<T, object> orderby) where T : class;

        /// <summary>
        /// 根据linq条件进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据linq条件进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>(string sql);

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>(string sql, object parameter);

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回集合</returns>
        IEnumerable<T> FindList<T>(string sql, params DbParameter[] dbParameter);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        (IEnumerable<T> list, long total) FindList<T>(string orderField, bool isAsc, int pageSize, int pageIndex) where T : class;

        /// <summary>
        /// 根据linq条件进行分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>       
        /// <param name="predicate">linq条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        (IEnumerable<T> list, long total) FindList<T>(Expression<Func<T, bool>> predicate, string orderField, bool isAsc, int pageSize, int pageIndex) where T : class;

        /// <summary>
        /// 根据linq条件进行分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        (IEnumerable<T> list, long total) FindList<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, string orderField, bool isAsc, int pageSize, int pageIndex) where T : class;

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        (IEnumerable<T> list, long total) FindList<T>(string sql, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        (IEnumerable<T> list, long total) FindList<T>(string sql, object parameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        (IEnumerable<T> list, long total) FindList<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        (IEnumerable<T> list, long total) FindListByWith<T>(string sql, object parameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        (IEnumerable<T> list, long total) FindListByWith<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex);
        #endregion

        #region Async
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>() where T : class;

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector) where T : class;

        /// <summary>
        /// 查询并根据条件进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="orderby">排序字段</param>
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>(Func<T, object> orderby) where T : class;

        /// <summary>
        /// 查询并根据条件进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="orderby">排序字段</param>
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector, Func<T, object> orderby) where T : class;

        /// <summary>
        /// 根据linq条件进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据linq条件进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class;

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>(string sql);

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>(string sql, object parameter);

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回集合</returns>
        Task<IEnumerable<T>> FindListAsync<T>(string sql, params DbParameter[] dbParameter);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string orderField, bool isAsc, int pageSize, int pageIndex) where T : class;

        /// <summary>
        /// 根据linq条件进行分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        Task<(IEnumerable<T> list, long total)> FindListAsync<T>(Expression<Func<T, bool>> predicate, string orderField, bool isAsc, int pageSize, int pageIndex) where T : class;

        /// <summary>
        /// 根据linq条件进行分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        Task<(IEnumerable<T> list, long total)> FindListAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, string orderField, bool isAsc, int pageSize, int pageIndex) where T : class;

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string sql, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string sql, object parameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        Task<(IEnumerable<T> list, long total)> FindListByWithAsync<T>(string sql, object parameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        Task<(IEnumerable<T> list, long total)> FindListByWithAsync<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex);
        #endregion
        #endregion

        #region FindTable
        #region Sync
        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataTable</returns>
        DataTable FindTable(string sql);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        DataTable FindTable(string sql, object parameter);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        DataTable FindTable(string sql, params DbParameter[] dbParameter);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        (DataTable table, long total) FindTable(string sql, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        (DataTable table, long total) FindTable(string sql, object parameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        (DataTable table, long total) FindTable(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        (DataTable table, long total) FindTableByWith(string sql, object parameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        (DataTable table, long total) FindTableByWith(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex);
        #endregion

        #region Async
        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataTable</returns>
        Task<DataTable> FindTableAsync(string sql);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        Task<DataTable> FindTableAsync(string sql, object parameter);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        Task<DataTable> FindTableAsync(string sql, params DbParameter[] dbParameter);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        Task<(DataTable table, long total)> FindTableAsync(string sql, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        Task<(DataTable table, long total)> FindTableAsync(string sql, object parameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        Task<(DataTable table, long total)> FindTableAsync(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        Task<(DataTable table, long total)> FindTableByWithAsync(string sql, object parameter, string orderField, bool isAsc, int pageSize, int pageIndex);

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        Task<(DataTable table, long total)> FindTableByWithAsync(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex);
        #endregion
        #endregion

        #region FindMultiple
        #region Sync
        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果集</returns>
        List<IEnumerable<dynamic>> FindMultiple(string sql);

        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回查询结果集</returns>
        List<IEnumerable<dynamic>> FindMultiple(string sql, object parameter);

        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果集</returns>
        List<IEnumerable<dynamic>> FindMultiple(string sql, params DbParameter[] dbParameter);
        #endregion

        #region Async
        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果集</returns>
        Task<List<IEnumerable<dynamic>>> FindMultipleAsync(string sql);

        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回查询结果集</returns>
        Task<List<IEnumerable<dynamic>>> FindMultipleAsync(string sql, object parameter);

        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果集</returns>
        Task<List<IEnumerable<dynamic>>> FindMultipleAsync(string sql, params DbParameter[] dbParameter);
        #endregion
        #endregion
    }
}