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

using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;
using SQLBuilder.Core;
using Sql = SQLBuilder.Core.SqlBuilder;
/****************************
* [Author] 张强
* [Date] 2018-07-27
* [Describe] Oracle仓储实现类
* **************************/
namespace SQLBuilder.Core.Repositories
{
    /// <summary>
    /// Oracle仓储实现类
    /// </summary>
    public class OracleRepository : IRepository, IDisposable
    {
        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connString">链接字符串，或者链接字符串名称</param>
        public OracleRepository(string connString)
        {
            //判断是链接字符串，还是链接字符串名称
            if (connString?.Contains(":") == true)
            {
                ConnectionString = ConfigurationManager.GetAppSettings<string>(connString);
            }
            else
            {
                ConnectionString = ConfigurationManager.GetConnectionString(connString);
            }
            if (string.IsNullOrEmpty(ConnectionString))
            {
                ConnectionString = connString;
            }
        }
        #endregion

        #region Property
        /// <summary>
        /// 数据库连接串
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// 数据库链接对象
        /// </summary>
        public DbConnection Connection
        {
            get
            {
                var dbconnection = new OracleConnection(ConnectionString);
                dbconnection.Open();
                return dbconnection;
            }
        }

        /// <summary>
        /// 事务对象
        /// </summary>
        public DbTransaction Transaction { get; set; }
        #endregion

        #region Transaction
        /// <summary>
        /// 事务开始
        /// </summary>
        /// <returns>返回IDatabase</returns>
        public IRepository BeginTrans()
        {
            var dbConnection = Connection;
            if (dbConnection.State == ConnectionState.Closed)
            {
                dbConnection.Open();
            }
            Transaction = dbConnection.BeginTransaction();
            return this;
        }

        /// <summary>
        /// 提交当前操作的结果
        /// </summary>
        public int Commit()
        {
            try
            {
                Transaction.Commit();
                return 1;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Close();
            }
        }

        /// <summary>
        /// 把当前操作回滚成未提交状态
        /// </summary>
        public void Rollback()
        {
            Transaction.Rollback();
            Transaction.Dispose();
            Close();
        }

        /// <summary>
        /// 关闭连接，回收内存
        /// </summary>
        public void Close()
        {
            var dbConnection = Transaction?.Connection;
            if (dbConnection != null && dbConnection.State != ConnectionState.Closed)
            {
                dbConnection.Close();
                Transaction = null;
            }
        }
        #endregion

        #region ExecuteBySql
        #region Sync
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回受影响行数</returns>
        public int ExecuteBySql(string sql)
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(sql, transaction: Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(sql);
                }
            }
            return result;
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public int ExecuteBySql(string sql, params DbParameter[] dbParameter)
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(sql, dbParameter.ToDynamicParameters(), Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(sql, dbParameter.ToDynamicParameters());
                }
            }
            return result;
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <returns>返回受影响行数</returns>
        public int ExecuteByProc(string procName)
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(procName, transaction: Transaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(procName, commandType: CommandType.StoredProcedure);
                }
            }
            return result;
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public int ExecuteByProc(string procName, params DbParameter[] dbParameter)
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(procName, dbParameter.ToDynamicParameters(), Transaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(procName, dbParameter.ToDynamicParameters(), commandType: CommandType.StoredProcedure);
                }
            }
            return result;
        }
        #endregion

        #region Async
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> ExecuteBySqlAsync(string sql)
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(sql, transaction: Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(sql);
                }
            }
            return result;
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> ExecuteBySqlAsync(string sql, params DbParameter[] dbParameter)
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(sql, dbParameter.ToDynamicParameters(), Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(sql, dbParameter.ToDynamicParameters());
                }
            }
            return result;
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> ExecuteByProcAsync(string procName)
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(procName, transaction: Transaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(procName, commandType: CommandType.StoredProcedure);
                }
            }
            return result;
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> ExecuteByProcAsync(string procName, params DbParameter[] dbParameter)
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(procName, dbParameter.ToDynamicParameters(), Transaction, commandType: CommandType.StoredProcedure);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(procName, dbParameter.ToDynamicParameters(), commandType: CommandType.StoredProcedure);
                }
            }
            return result;
        }
        #endregion
        #endregion

        #region Insert
        #region Sync
        /// <summary>
        ///  插入单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <returns>返回受影响行数</returns>
        public int Insert<T>(T entity) where T : class
        {
            var result = 0;
            var builder = Sql.Insert<T>(() => entity, DatabaseType.Oracle, false);
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 插入多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要插入的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public int Insert<T>(IEnumerable<T> entities) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                foreach (var item in entities)
                {
                    result += Insert(item);
                }
            }
            else
            {
                try
                {
                    BeginTrans();
                    foreach (var item in entities)
                    {
                        result += Insert(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    result = 0;
                }
            }
            return result;
        }
        #endregion

        #region Async
        /// <summary>
        ///  插入单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> InsertAsync<T>(T entity) where T : class
        {
            var result = 0;
            var builder = Sql.Insert<T>(() => entity, DatabaseType.Oracle, false);
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 插入多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要插入的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> InsertAsync<T>(IEnumerable<T> entities) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                foreach (var item in entities)
                {
                    result += await InsertAsync(item);
                }
            }
            else
            {
                try
                {
                    BeginTrans();
                    foreach (var item in entities)
                    {
                        result += await InsertAsync(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    result = 0;
                }
            }
            return result;
        }
        #endregion
        #endregion

        #region Delete
        #region Sync
        /// <summary>
        /// 删除全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回受影响行数</returns>
        public int Delete<T>() where T : class
        {
            var result = 0;
            var builder = Sql.Delete<T>(DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(builder.Sql);
                }
            }
            return result;
        }

        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要删除的实体</param>
        /// <returns>返回受影响行数</returns>
        public int Delete<T>(T entity) where T : class
        {
            var result = 0;
            var builder = Sql.Delete<T>(DatabaseType.Oracle).WithKey(entity);
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要删除的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public int Delete<T>(IEnumerable<T> entities) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                foreach (var item in entities)
                {
                    result += Delete(item);
                }
            }
            else
            {
                try
                {
                    BeginTrans();
                    foreach (var item in entities)
                    {
                        result += Delete(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    result = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据linq条件删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回受影响行数</returns>
        public int Delete<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var result = 0;
            var builder = Sql.Delete<T>(DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    result = dbConnection.Execute(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据单个主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="keyValue">主键值</param>
        /// <returns>返回受影响行数</returns>
        public int Delete<T>(object keyValue) where T : class
        {
            var result = 0;
            var builder = Sql.Delete<T>(DatabaseType.Oracle).WithKey(keyValue);
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据多个主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="keyValue">主键集合</param>
        /// <returns>返回受影响行数</returns>
        public int Delete<T>(object[] keyValue) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                foreach (var key in keyValue)
                {
                    result += Delete<T>(key);
                }
            }
            else
            {
                try
                {
                    BeginTrans();
                    foreach (var key in keyValue)
                    {
                        result += Delete<T>(key);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    result = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据属性删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>       
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns>返回受影响行数</returns>
        public int Delete<T>(string propertyName, object propertyValue) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute($"DELETE FROM {Sql.GetTableName<T>()} WHERE {propertyName}=@PropertyValue", new { PropertyValue = propertyValue }, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute($"DELETE FROM {Sql.GetTableName<T>()} WHERE {propertyName}=@PropertyValue", new { PropertyValue = propertyValue });
                }
            }
            return result;
        }
        #endregion

        #region Async
        /// <summary>
        /// 删除全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回受影响行数</returns>
        public async Task<int> DeleteAsync<T>() where T : class
        {
            var result = 0;
            var builder = Sql.Delete<T>(DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(builder.Sql);
                }
            }
            return result;
        }

        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要删除的实体</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> DeleteAsync<T>(T entity) where T : class
        {
            var result = 0;
            var builder = Sql.Delete<T>(DatabaseType.Oracle).WithKey(entity);
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要删除的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> DeleteAsync<T>(IEnumerable<T> entities) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                foreach (var item in entities)
                {
                    result += await DeleteAsync(item);
                }
            }
            else
            {
                try
                {
                    BeginTrans();
                    foreach (var item in entities)
                    {
                        result += await DeleteAsync(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    result = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据linq条件删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var result = 0;
            var builder = Sql.Delete<T>(DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    result = await dbConnection.ExecuteAsync(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据单个主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="keyValue">主键值</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> DeleteAsync<T>(object keyValue) where T : class
        {
            var result = 0;
            var builder = Sql.Delete<T>(DatabaseType.Oracle).WithKey(keyValue);
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 根据多个主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="keyValue">主键集合</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> DeleteAsync<T>(object[] keyValue) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                foreach (var key in keyValue)
                {
                    result += await DeleteAsync<T>(key);
                }
            }
            else
            {
                try
                {
                    BeginTrans();
                    foreach (var key in keyValue)
                    {
                        result += await DeleteAsync<T>(key);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    result = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据属性删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>       
        /// <param name="propertyName">属性名</param>
        /// <param name="propertyValue">属性值</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> DeleteAsync<T>(string propertyName, object propertyValue) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync($"DELETE FROM {Sql.GetTableName<T>()} WHERE {propertyName}=@PropertyValue", new { PropertyValue = propertyValue }, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync($"DELETE FROM {Sql.GetTableName<T>()} WHERE {propertyName}=@PropertyValue", new { PropertyValue = propertyValue });
                }
            }
            return result;
        }
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
        public int Update<T>(T entity) where T : class
        {
            var result = 0;
            var builder = Sql.Update<T>(() => entity, DatabaseType.Oracle, false).WithKey(entity);
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = connection.Execute(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要更新的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public int Update<T>(IEnumerable<T> entities) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                foreach (var item in entities)
                {
                    result += Update(item);
                }
            }
            else
            {
                try
                {
                    BeginTrans();
                    foreach (var item in entities)
                    {
                        result += Update(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    result = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据linq条件更新实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回受影响行数</returns>
        public int Update<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var result = 0;
            var builder = Sql.Update<T>(DatabaseType: DatabaseType.Oracle, isEnableNullValue: false).Where(predicate);
            if (Transaction?.Connection != null)
            {
                result = Transaction.Connection.Execute(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    result = dbConnection.Execute(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }
        #endregion

        #region Async
        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要更新的实体</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> UpdateAsync<T>(T entity) where T : class
        {
            var result = 0;
            var builder = Sql.Update<T>(() => entity, DatabaseType.Oracle, false).WithKey(entity);
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var connection = Connection)
                {
                    result = await connection.ExecuteAsync(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要更新的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> UpdateAsync<T>(IEnumerable<T> entities) where T : class
        {
            var result = 0;
            if (Transaction?.Connection != null)
            {
                foreach (var item in entities)
                {
                    result += await UpdateAsync(item);
                }
            }
            else
            {
                try
                {
                    BeginTrans();
                    foreach (var item in entities)
                    {
                        result += await UpdateAsync(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    result = 0;
                }
            }
            return result;
        }

        /// <summary>
        /// 根据linq条件更新实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回受影响行数</returns>
        public async Task<int> UpdateAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var result = 0;
            var builder = Sql.Update<T>(DatabaseType: DatabaseType.Oracle, isEnableNullValue: false).Where(predicate);
            if (Transaction?.Connection != null)
            {
                result = await Transaction.Connection.ExecuteAsync(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    result = await dbConnection.ExecuteAsync(builder.Sql, builder.DynamicParameters);
                }
            }
            return result;
        }
        #endregion
        #endregion

        #region FindObject
        #region Sync
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果对象</returns>
        public object FindObject(string sql)
        {
            return FindObject(sql, null);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        public object FindObject(string sql, DbParameter[] dbParameter)
        {
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.QueryFirstOrDefault<string>(sql, dbParameter.ToDynamicParameters(), Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.QueryFirstOrDefault<string>(sql, dbParameter.ToDynamicParameters());
                }
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果对象</returns>
        public async Task<object> FindObjectAsync(string sql)
        {
            return await FindObjectAsync(sql, null);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        public async Task<object> FindObjectAsync(string sql, DbParameter[] dbParameter)
        {
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryFirstOrDefaultAsync<string>(sql, dbParameter.ToDynamicParameters(), Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryFirstOrDefaultAsync<string>(sql, dbParameter.ToDynamicParameters());
                }
            }
        }
        #endregion
        #endregion

        #region FindEntity
        #region Sync
        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="keyValue">主键值</param>
        /// <returns>返回实体</returns>
        public T FindEntity<T>(object keyValue) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).WithKey(keyValue);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.QueryFirstOrDefault<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.QueryFirstOrDefault<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="keyValue">主键值</param>
        /// <returns>返回实体</returns>
        public T FindEntity<T>(Expression<Func<T, object>> selector, object keyValue) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).WithKey(keyValue);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.QueryFirstOrDefault<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.QueryFirstOrDefault<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据linq条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回实体</returns>
        public T FindEntity<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.QueryFirstOrDefault<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.QueryFirstOrDefault<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据linq条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回实体</returns>
        public T FindEntity<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.QueryFirstOrDefault<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.QueryFirstOrDefault<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回实体</returns>
        public T FindEntityBySql<T>(string sql)
        {
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.QueryFirstOrDefault<T>(sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.QueryFirstOrDefault<T>(sql);
                }
            }
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">对应参数</param>
        /// <returns>返回实体</returns>
        public T FindEntityBySql<T>(string sql, DbParameter[] parameters)
        {
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.QueryFirstOrDefault<T>(sql, parameters.ToDynamicParameters(), Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.QueryFirstOrDefault<T>(sql, parameters.ToDynamicParameters());
                }
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="keyValue">主键值</param>
        /// <returns>返回实体</returns>
        public async Task<T> FindEntityAsync<T>(object keyValue) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).WithKey(keyValue);
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryFirstOrDefaultAsync<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryFirstOrDefaultAsync<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="keyValue">主键值</param>
        /// <returns>返回实体</returns>
        public async Task<T> FindEntityAsync<T>(Expression<Func<T, object>> selector, object keyValue) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).WithKey(keyValue);
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryFirstOrDefaultAsync<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryFirstOrDefaultAsync<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据linq条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回实体</returns>
        public async Task<T> FindEntityAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryFirstOrDefaultAsync<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryFirstOrDefaultAsync<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据linq条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回实体</returns>
        public async Task<T> FindEntityAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryFirstOrDefaultAsync<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryFirstOrDefaultAsync<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回实体</returns>
        public async Task<T> FindEntityBySqlAsync<T>(string sql)
        {
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryFirstOrDefaultAsync<T>(sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryFirstOrDefaultAsync<T>(sql);
                }
            }
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameters">对应参数</param>
        /// <returns>返回实体</returns>
        public async Task<T> FindEntityBySqlAsync<T>(string sql, DbParameter[] parameters)
        {
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryFirstOrDefaultAsync<T>(sql, parameters.ToDynamicParameters(), Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryFirstOrDefaultAsync<T>(sql, parameters.ToDynamicParameters());
                }
            }
        }
        #endregion
        #endregion

        #region IQueryable
        #region Sync
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <returns>返回集合</returns>
        public IQueryable<T> IQueryable<T>() where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, transaction: Transaction).AsQueryable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql).AsQueryable();
                }
            }
        }

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        public IQueryable<T> IQueryable<T>(Expression<Func<T, object>> selector) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, transaction: Transaction).AsQueryable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql).AsQueryable();
                }
            }
        }

        /// <summary>
        /// 根据linq查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        public IQueryable<T> IQueryable<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, builder.DynamicParameters, Transaction).AsQueryable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql, builder.DynamicParameters).AsQueryable();
                }
            }
        }

        /// <summary>
        /// 根据linq查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        public IQueryable<T> IQueryable<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, builder.DynamicParameters, Transaction).AsQueryable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql, builder.DynamicParameters).AsQueryable();
                }
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <returns>返回集合</returns>
        public async Task<IQueryable<T>> IQueryableAsync<T>() where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                var query = await Transaction.Connection.QueryAsync<T>(builder.Sql, transaction: Transaction);
                return query.AsQueryable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var query = await dbConnection.QueryAsync<T>(builder.Sql);
                    return query.AsQueryable();
                }
            }
        }

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        public async Task<IQueryable<T>> IQueryableAsync<T>(Expression<Func<T, object>> selector) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                var query = await Transaction.Connection.QueryAsync<T>(builder.Sql, transaction: Transaction);
                return query.AsQueryable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var query = await dbConnection.QueryAsync<T>(builder.Sql);
                    return query.AsQueryable();
                }
            }
        }

        /// <summary>
        /// 根据linq查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        public async Task<IQueryable<T>> IQueryableAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                var query = await Transaction.Connection.QueryAsync<T>(builder.Sql, builder.DynamicParameters, Transaction);
                return query.AsQueryable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var query = await dbConnection.QueryAsync<T>(builder.Sql, builder.DynamicParameters);
                    return query.AsQueryable();
                }
            }
        }

        /// <summary>
        /// 根据linq查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        public async Task<IQueryable<T>> IQueryableAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                var query = await Transaction.Connection.QueryAsync<T>(builder.Sql, builder.DynamicParameters, Transaction);
                return query.AsQueryable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var query = await dbConnection.QueryAsync<T>(builder.Sql, builder.DynamicParameters);
                    return query.AsQueryable();
                }
            }
        }
        #endregion
        #endregion

        #region FindList
        #region Sync
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <returns>返回集合</returns>
        public IEnumerable<T> FindList<T>() where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql);
                }
            }
        }

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        public IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql);
                }
            }
        }

        /// <summary>
        /// 查询并根据条件进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="keySelector">排序字段</param>
        /// <returns>返回集合</returns>
        public IEnumerable<T> FindList<T>(Func<T, object> keySelector) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).OrderBy(o => keySelector(o));
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql);
                }
            }
        }

        /// <summary>
        /// 查询并根据条件进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="keySelector">排序字段</param>
        /// <returns>返回集合</returns>
        public IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector, Func<T, object> keySelector) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).OrderBy(o => keySelector(o));
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql);
                }
            }
        }

        /// <summary>
        /// 根据linq条件进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        public IEnumerable<T> FindList<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据linq条件进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        public IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回集合</returns>
        public IEnumerable<T> FindList<T>(string sql)
        {
            return FindList<T>(sql, null);
        }

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回集合</returns>
        public IEnumerable<T> FindList<T>(string sql, DbParameter[] dbParameter)
        {
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.Query<T>(sql, dbParameter.ToDynamicParameters(), Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.Query<T>(sql, dbParameter.ToDynamicParameters());
                }
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public (IEnumerable<T> list, long total) FindList<T>(string orderField, bool isAsc, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle);
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = Transaction.Connection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters, Transaction);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var list = multiQuery?.Read<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = dbConnection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters);
                    var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                    var list = multiQuery?.Read<T>();
                    return (list, total);
                }
            }
        }

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
        public (IEnumerable<T> list, long total) FindList<T>(Expression<Func<T, bool>> predicate, string orderField, bool isAsc, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).Where(predicate);
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = Transaction.Connection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters, Transaction);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var list = multiQuery?.Read<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = dbConnection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters);
                    var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                    var list = multiQuery?.Read<T>();
                    return (list, total);
                }
            }
        }

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
        public (IEnumerable<T> list, long total) FindList<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, string orderField, bool isAsc, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).Where(predicate);
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = Transaction.Connection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters, Transaction);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var list = multiQuery?.Read<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = dbConnection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters);
                    var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                    var list = multiQuery?.Read<T>();
                    return (list, total);
                }
            }
        }

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
        public (IEnumerable<T> list, long total) FindList<T>(string sql, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            return FindList<T>(sql, null, orderField, isAsc, pageSize, pageIndex);
        }

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
        public (IEnumerable<T> list, long total) FindList<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = Transaction.Connection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters(), Transaction);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var list = multiQuery?.Read<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = dbConnection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters());
                    var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                    var list = multiQuery?.Read<T>();
                    return (list, total);
                }
            }
        }

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
        public (IEnumerable<T> list, long total) FindListByWith<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = $"ORDER BY (SELECT 0)";
            }
            //暂未实现临时表with分页
            if (Transaction?.Connection != null)
            {
                var multiQuery = Transaction.Connection.QueryMultiple($"{sql} SELECT COUNT(1) AS Total FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters(), Transaction);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var list = multiQuery?.Read<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = dbConnection.QueryMultiple($"{sql} SELECT COUNT(1) AS Total FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters());
                    var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                    var list = multiQuery?.Read<T>();
                    return (list, total);
                }
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <returns>返回集合</returns>
        public async Task<IEnumerable<T>> FindListAsync<T>() where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryAsync<T>(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryAsync<T>(builder.Sql);
                }
            }
        }

        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        public async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle);
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryAsync<T>(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryAsync<T>(builder.Sql);
                }
            }
        }

        /// <summary>
        /// 查询并根据条件进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="keySelector">排序字段</param>
        /// <returns>返回集合</returns>
        public async Task<IEnumerable<T>> FindListAsync<T>(Func<T, object> keySelector) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).OrderBy(o => keySelector(o));
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryAsync<T>(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryAsync<T>(builder.Sql);
                }
            }
        }

        /// <summary>
        /// 查询并根据条件进行排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="keySelector">排序字段</param>
        /// <returns>返回集合</returns>
        public async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector, Func<T, object> keySelector) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).OrderBy(o => keySelector(o));
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryAsync<T>(builder.Sql, transaction: Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryAsync<T>(builder.Sql);
                }
            }
        }

        /// <summary>
        /// 根据linq条件进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        public async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryAsync<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryAsync<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据linq条件进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">linq选择指定列，null选择全部</param>
        /// <param name="predicate">linq条件</param>
        /// <returns>返回集合</returns>
        public async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).Where(predicate);
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryAsync<T>(builder.Sql, builder.DynamicParameters, Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryAsync<T>(builder.Sql, builder.DynamicParameters);
                }
            }
        }

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回集合</returns>
        public async Task<IEnumerable<T>> FindListAsync<T>(string sql)
        {
            return await FindListAsync<T>(sql, null);
        }

        /// <summary>
        /// 根据sql语句进行查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回集合</returns>
        public async Task<IEnumerable<T>> FindListAsync<T>(string sql, DbParameter[] dbParameter)
        {
            if (Transaction?.Connection != null)
            {
                return await Transaction.Connection.QueryAsync<T>(sql, dbParameter.ToDynamicParameters(), Transaction);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return await dbConnection.QueryAsync<T>(sql, dbParameter.ToDynamicParameters());
                }
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string orderField, bool isAsc, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle);
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = await Transaction.Connection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters, Transaction);
                var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                var list = await multiQuery?.ReadAsync<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = await dbConnection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters);
                    var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                    var list = await multiQuery?.ReadAsync<T>();
                    return (list, total);
                }
            }
        }

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
        public async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(Expression<Func<T, bool>> predicate, string orderField, bool isAsc, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(DatabaseType: DatabaseType.Oracle).Where(predicate);
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = await Transaction.Connection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters, Transaction);
                var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                var list = await multiQuery?.ReadAsync<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = await dbConnection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters);
                    var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                    var list = await multiQuery?.ReadAsync<T>();
                    return (list, total);
                }
            }
        }

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
        public async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, string orderField, bool isAsc, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType.Oracle).Where(predicate);
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = await Transaction.Connection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters, Transaction);
                var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                var list = await multiQuery?.ReadAsync<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = await dbConnection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({builder.Sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({builder.Sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", builder.DynamicParameters);
                    var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                    var list = await multiQuery?.ReadAsync<T>();
                    return (list, total);
                }
            }
        }

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
        public async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string sql, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            return await FindListAsync<T>(sql, null, orderField, isAsc, pageSize, pageIndex);
        }

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
        public async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = await Transaction.Connection.QueryMultipleAsync($"DSELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters(), Transaction);
                var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                var list = await multiQuery?.ReadAsync<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = await dbConnection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters());
                    var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                    var list = await multiQuery?.ReadAsync<T>();
                    return (list, total);
                }
            }
        }

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
        public async Task<(IEnumerable<T> list, long total)> FindListByWithAsync<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            //暂未实现临时表with分页
            if (Transaction?.Connection != null)
            {
                var multiQuery = await Transaction.Connection.QueryMultipleAsync($"{sql} SELECT COUNT(1) AS Total FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters(), Transaction);
                var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                var list = await multiQuery?.ReadAsync<T>();
                return (list, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = await dbConnection.QueryMultipleAsync($"{sql} SELECT COUNT(1) AS Total FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters());
                    var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                    var list = await multiQuery?.ReadAsync<T>();
                    return (list, total);
                }
            }
        }
        #endregion
        #endregion

        #region FindTable
        #region Sync
        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataTable</returns>
        public DataTable FindTable(string sql)
        {
            return FindTable(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public DataTable FindTable(string sql, DbParameter[] dbParameter)
        {
            if (Transaction?.Connection != null)
            {
                return Transaction.Connection.ExecuteReader(sql, dbParameter.ToDynamicParameters(), Transaction).ToDataTable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    return dbConnection.ExecuteReader(sql, dbParameter.ToDynamicParameters()).ToDataTable();
                }
            }
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        public (DataTable table, long total) FindTable(string sql, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            return FindTable(sql, null, orderField, isAsc, pageSize, pageIndex);
        }

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
        public (DataTable table, long total) FindTable(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = Transaction.Connection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters(), Transaction);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var table = multiQuery?.Read()?.ToList()?.ToDataTable();
                table?.Columns?.Remove("RowNumber");
                return (table, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = dbConnection.QueryMultiple($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters());
                    var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                    var table = multiQuery?.Read()?.ToList()?.ToDataTable();
                    table?.Columns?.Remove("RowNumber");
                    return (table, total);
                }
            }
        }

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
        public (DataTable table, long total) FindTableByWith(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            //暂未实现临时表with分页
            if (Transaction?.Connection != null)
            {
                var multiQuery = Transaction.Connection.QueryMultiple($"{sql} SELECT COUNT(1) AS Total FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters(), Transaction);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var table = multiQuery?.Read()?.ToList()?.ToDataTable();
                table?.Columns?.Remove("RowNumber");
                return (table, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = dbConnection.QueryMultiple($"{sql} SELECT COUNT(1) AS Total FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters());
                    var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                    var table = multiQuery?.Read()?.ToList()?.ToDataTable();
                    table?.Columns?.Remove("RowNumber");
                    return (table, total);
                }
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataTable</returns>
        public async Task<DataTable> FindTableAsync(string sql)
        {
            return await FindTableAsync(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public async Task<DataTable> FindTableAsync(string sql, DbParameter[] dbParameter)
        {
            if (Transaction?.Connection != null)
            {
                var reader = await Transaction.Connection.ExecuteReaderAsync(sql, dbParameter.ToDynamicParameters(), Transaction);
                return reader.ToDataTable();
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var reader = await dbConnection.ExecuteReaderAsync(sql, dbParameter.ToDynamicParameters());
                    return reader.ToDataTable();
                }
            }
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAsc">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回DataTable和总记录数</returns>
        public async Task<(DataTable table, long total)> FindTableAsync(string sql, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            return await FindTableAsync(sql, null, orderField, isAsc, pageSize, pageIndex);
        }

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
        public async Task<(DataTable table, long total)> FindTableAsync(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            if (Transaction?.Connection != null)
            {
                var multiQuery = await Transaction.Connection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters(), Transaction);
                var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                var reader = await multiQuery?.ReadAsync();
                var table = reader?.ToList()?.ToDataTable();
                table?.Columns?.Remove("RowNumber");
                return (table, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = await dbConnection.QueryMultipleAsync($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters());
                    var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                    var reader = await multiQuery?.ReadAsync();
                    var table = reader?.ToList()?.ToDataTable();
                    table?.Columns?.Remove("RowNumber");
                    return (table, total);
                }
            }
        }

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
        public async Task<(DataTable table, long total)> FindTableByWithAsync(string sql, DbParameter[] dbParameter, string orderField, bool isAsc, int pageSize, int pageIndex)
        {
            if (pageIndex == 0)
            {
                pageIndex = 1;
            }
            var orderBy = string.Empty;
            if (!string.IsNullOrEmpty(orderField))
            {
                if (orderField.ToUpper().IndexOf("ASC") + orderField.ToUpper().IndexOf("DESC") > 0)
                {
                    orderBy = $"ORDER BY {orderField}";
                }
                else
                {
                    orderBy = $"ORDER BY {orderField} {(isAsc ? "ASC" : "DESC")}";
                }
            }
            else
            {
                orderBy = "ORDER BY (SELECT 0)";
            }
            //暂未实现临时表with分页
            if (Transaction?.Connection != null)
            {
                var multiQuery = await Transaction.Connection.QueryMultipleAsync($"{sql} SELECT COUNT(1) AS Total FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters(), Transaction);
                var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                var reader = await multiQuery?.ReadAsync();
                var table = reader?.ToList()?.ToDataTable();
                table?.Columns?.Remove("RowNumber");
                return (table, total);
            }
            else
            {
                using (var dbConnection = Connection)
                {
                    var multiQuery = await dbConnection.QueryMultipleAsync($"{sql} SELECT COUNT(1) AS Total FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};", dbParameter.ToDynamicParameters());
                    var total = await multiQuery?.ReadFirstOrDefaultAsync<long>();
                    var reader = await multiQuery?.ReadAsync();
                    var table = reader?.ToList()?.ToDataTable();
                    table?.Columns?.Remove("RowNumber");
                    return (table, total);
                }
            }
        }
        #endregion
        #endregion

        #region Dispose
        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            Close();
        }
        #endregion
    }
}
