#region License
/***
 * Copyright © 2018-2021, 张强 (943620963@qq.com).
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
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using SQLBuilder.Core.Configuration;
using SQLBuilder.Core.Diagnostics;
using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using SQLBuilder.Core.LoadBalancer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Sql = SQLBuilder.Core.Entry.SqlBuilder;

namespace SQLBuilder.Core.Repositories
{
    /// <summary>
    /// 数据操作仓储抽象基类
    /// </summary>
    public abstract class BaseRepository : IRepository
    {
        #region Field
        /// <summary>
        /// 诊断日志
        /// </summary>
        private static readonly DiagnosticListener _diagnosticListener =
            new(DiagnosticStrings.DiagnosticListenerName);

        /// <summary>
        /// 事务数据库连接对象
        /// </summary>
        private DbConnection _tranConnection;
        #endregion

        #region Property
        /// <summary>
        /// 超时时长，默认240s
        /// </summary>
        public virtual int CommandTimeout { get; set; } = 240;

        /// <summary>
        /// 是否主库操作
        /// </summary>
        public virtual bool Master { get; set; } = true;

        /// <summary>
        /// 主库数据库连接字符串
        /// </summary>
        public virtual string MasterConnectionString { get; set; }

        /// <summary>
        /// 从库数据库连接字符串及权重集合
        /// </summary>
        public virtual (string connectionString, int weight)[] SlaveConnectionStrings { get; set; }

        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public virtual DbConnection Connection
        {
            get
            {
                DbConnection connection;

                if (!Master && SlaveConnectionStrings?.Length > 0 && LoadBalancer != null)
                {
                    var connectionStrings = SlaveConnectionStrings.Select(x => x.connectionString);
                    var weights = SlaveConnectionStrings.Select(x => x.weight).ToArray();
                    var connectionString = LoadBalancer.Get(MasterConnectionString, connectionStrings, weights);

                    connection = GetDbConnection(connectionString);
                }
                else
                {
                    connection = GetDbConnection(MasterConnectionString);
                }

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                return connection;
            }
        }

        /// <summary>
        /// 事务对象
        /// </summary>
        public virtual DbTransaction Transaction { get; set; }

        /// <summary>
        /// 是否启用对表名和列名格式化，默认不启用，注意：只针对Lambda表达式解析生成的sql，默认false
        /// </summary>
        public virtual bool IsEnableFormat { get; set; } = false;

        /// <summary>
        /// 是否启用null实体属性值insert、update，默认false
        /// </summary>
        public virtual bool IsEnableNullValue { get; set; } = false;

        /// <summary>
        /// 分页计数语法，默认COUNT(*)
        /// </summary>
        public virtual string CountSyntax { get; set; } = "COUNT(*)";

        /// <summary>
        /// sql拦截委托
        /// </summary>
        public virtual Func<string, object, string> SqlIntercept { get; set; }

        /// <summary>
        /// 从库负载均衡接口
        /// </summary>
        public virtual ILoadBalancer LoadBalancer { get; set; }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public virtual DatabaseType DatabaseType { get; set; }
        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">主库连接字符串，或者链接字符串名称</param>
        public BaseRepository(string connectionString)
        {
            //判断是链接字符串，还是链接字符串名称
            if (connectionString?.Contains(":") == true)
                MasterConnectionString = ConfigurationManager.GetValue<string>(connectionString);
            else
                MasterConnectionString = ConfigurationManager.GetConnectionString(connectionString);

            if (MasterConnectionString.IsNullOrEmpty())
                MasterConnectionString = connectionString;
        }
        #endregion

        #region Queue
        #region Sync
        /// <summary>
        /// 同步委托队列(Queue)
        /// </summary>
        public virtual ConcurrentQueue<Func<IRepository, bool>> Queue { get; } = new();

        /// <summary>
        /// 加入同步委托队列(Queue)
        /// </summary>
        /// <param name="func">自定义委托</param>
        /// <returns></returns>
        public virtual void AddQueue(Func<IRepository, bool> func)
        {
            Queue.Enqueue(func);
        }

        /// <summary>
        /// 保存同步委托队列(Queue)
        /// </summary>
        /// <param name="transaction">是否开启事务</param>
        /// <returns></returns>
        public virtual bool SaveQueue(bool transaction = true)
        {
            try
            {
                if (Queue.IsEmpty)
                    return false;

                if (transaction)
                    BeginTransaction();

                var res = true;

                while (!Queue.IsEmpty && Queue.TryDequeue(out var func))
                    res = res && func(this);

                if (transaction)
                    Commit();

                return res;
            }
            catch (Exception)
            {
                if (transaction)
                    Rollback();

                throw;
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 异步委托队列(AsyncQueue)
        /// </summary>
        public virtual ConcurrentQueue<Func<IRepository, Task<bool>>> AsyncQueue { get; } = new();

        /// <summary>
        /// 加入异步委托队列(AsyncQueue)
        /// </summary>
        /// <param name="func">自定义委托</param>
        /// <returns></returns>
        public virtual void AddQueue(Func<IRepository, Task<bool>> func)
        {
            AsyncQueue.Enqueue(func);
        }

        /// <summary>
        /// 保存异步委托队列(AsyncQueue)
        /// </summary>
        /// <param name="transaction">是否开启事务</param>
        /// <returns></returns>
        public virtual async Task<bool> SaveQueueAsync(bool transaction = true)
        {
            try
            {
                if (AsyncQueue.IsEmpty)
                    return false;

                if (transaction)
                    await BeginTransactionAsync();

                var res = true;

                while (!AsyncQueue.IsEmpty && AsyncQueue.TryDequeue(out var func))
                    res = res && await func(this);

                if (transaction)
                    await CommitAsync();

                return res;
            }
            catch (Exception)
            {
                if (transaction)
                    await RollbackAsync();

                throw;
            }
        }
        #endregion
        #endregion

        #region UseMasterOrSlave
        /// <summary>
        /// 使用主库/从库
        /// <para>注意使用从库必须满足：配置从库连接字符串 + 切换为从库 + 配置从库负载均衡，否则依然使用主库</para>
        /// </summary>
        /// <param name="master">是否使用主库，默认使用主库</param>
        /// <returns></returns>
        public virtual IRepository UseMasterOrSlave(bool master = true)
        {
            Master = master;
            return this;
        }
        #endregion

        #region Transaction
        #region Sync
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns>IRepository</returns>
        public virtual IRepository BeginTransaction()
        {
            if (Transaction?.Connection == null)
            {
                _tranConnection = Connection;
                Transaction = _tranConnection.BeginTransaction();
            }

            return this;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public virtual void Commit()
        {
            Transaction?.Commit();
            Transaction?.Dispose();

            Close();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public virtual void Rollback()
        {
            Transaction?.Rollback();
            Transaction?.Dispose();

            Close();
        }

        /// <summary>
        /// 执行事务，内部自动开启事务、提交和回滚事务
        /// </summary>
        /// <param name="handler">自定义委托</param>
        /// <param name="rollback">事务回滚处理委托</param>
        public virtual void ExecuteTransaction(Action<IRepository> handler, Action<Exception> rollback = null)
        {
            IRepository repository = null;
            try
            {
                if (handler != null)
                {
                    repository = BeginTransaction();
                    handler(repository);
                    repository.Commit();
                }
            }
            catch (Exception ex)
            {
                repository?.Rollback();

                if (rollback != null)
                    rollback(ex);
                else
                    throw;
            }
        }

        /// <summary>
        /// 执行事务，根据自定义委托返回值内部自动开启事务、提交和回滚事务
        /// </summary>
        /// <param name="handler">自定义委托</param>
        /// <param name="rollback">事务回滚处理委托，注意：自定义委托返回false时，rollback委托的异常参数为null</param>
        public virtual bool ExecuteTransaction(Func<IRepository, bool> handler, Action<Exception> rollback = null)
        {
            IRepository repository = null;
            try
            {
                if (handler != null)
                {
                    repository = BeginTransaction();
                    var res = handler(repository);
                    if (res)
                        repository.Commit();
                    else
                    {
                        repository.Rollback();

                        rollback?.Invoke(null);
                    }

                    return res;
                }
            }
            catch (Exception ex)
            {
                repository?.Rollback();

                if (rollback != null)
                    rollback(ex);
                else
                    throw;
            }

            return false;
        }
        #endregion

        #region Async
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns>IRepository</returns>
        public virtual async Task<IRepository> BeginTransactionAsync()
        {
            if (Transaction?.Connection == null)
            {
                _tranConnection = Connection;
                Transaction = await _tranConnection.BeginTransactionAsync();
            }

            return this;
        }

        /// <summary>
        /// 提交事务
        /// </summary>
        public virtual async Task CommitAsync()
        {
            if (Transaction != null)
                await Transaction.CommitAsync();

            if (Transaction != null)
                await Transaction.DisposeAsync();

            await CloseAsync();
        }

        /// <summary>
        /// 回滚事务
        /// </summary>
        public virtual async Task RollbackAsync()
        {
            if (Transaction != null)
                await Transaction.RollbackAsync();

            if (Transaction != null)
                await Transaction.DisposeAsync();

            await CloseAsync();
        }

        /// <summary>
        /// 执行事务，内部自动开启事务、提交和回滚事务
        /// </summary>
        /// <param name="handler">自定义委托</param>
        /// <param name="rollback">事务回滚处理委托</param>
        public virtual async Task ExecuteTransactionAsync(Func<IRepository, Task> handler, Func<Exception, Task> rollback = null)
        {
            IRepository repository = null;
            try
            {
                if (handler != null)
                {
                    repository = await BeginTransactionAsync();
                    await handler(repository);
                    await repository.CommitAsync();
                }
            }
            catch (Exception ex)
            {
                if (repository != null)
                    await repository.RollbackAsync();

                if (rollback != null)
                    await rollback(ex);
                else
                    throw;
            }
        }

        /// <summary>
        /// 执行事务，根据自定义委托返回值内部自动开启事务、提交和回滚事务
        /// </summary>
        /// <param name="handler">自定义委托</param>
        /// <param name="rollback">事务回滚处理委托，注意：自定义委托返回false时，rollback委托的异常参数为null</param>
        public virtual async Task<bool> ExecuteTransactionAsync(Func<IRepository, Task<bool>> handler, Func<Exception, Task> rollback = null)
        {
            IRepository repository = null;
            try
            {
                if (handler != null)
                {
                    repository = await BeginTransactionAsync();
                    var res = await handler(repository);
                    if (res)
                        await repository.CommitAsync();
                    else
                    {
                        await repository.RollbackAsync();

                        if (rollback != null)
                            await rollback(null);
                    }

                    return res;
                }
            }
            catch (Exception ex)
            {
                if (repository != null)
                    await repository.RollbackAsync();

                if (rollback != null)
                    await rollback(ex);
                else
                    throw;
            }

            return false;
        }
        #endregion
        #endregion

        #region Page
        #region Sync
        /// <summary>
        /// 获取分页语句
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">原始sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public abstract string GetPageSql(bool isWithSyntax, string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex);

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">表实体泛型</typeparam>
        /// <param name="builder">SqlBuilder构建对象</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public virtual (IEnumerable<T> list, long total) PageQuery<T>(SqlBuilderCore<T> builder, string orderField, bool isAscending, int pageSize, int pageIndex) where T : class
        {
            if (Transaction?.Connection != null)
            {
                var sqlQuery = GetPageSql(false, builder.Sql, builder.Parameters, orderField, isAscending, pageSize, pageIndex);
                return QueryMultiple<T>(Transaction.Connection, sqlQuery, builder.DynamicParameters, Transaction);
            }
            else
            {
                using var connection = Connection;
                var sqlQuery = GetPageSql(false, builder.Sql, builder.Parameters, orderField, isAscending, pageSize, pageIndex);
                return QueryMultiple<T>(connection, sqlQuery, builder.DynamicParameters);
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public virtual (IEnumerable<T> list, long total) PageQuery<T>(bool isWithSyntax, string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            if (Transaction?.Connection != null)
            {
                var sqlQuery = GetPageSql(isWithSyntax, sql, parameter, orderField, isAscending, pageSize, pageIndex);
                return QueryMultiple<T>(Transaction.Connection, sqlQuery, parameter, Transaction);
            }
            else
            {
                using var connection = Connection;
                var sqlQuery = GetPageSql(isWithSyntax, sql, parameter, orderField, isAscending, pageSize, pageIndex);
                return QueryMultiple<T>(connection, sqlQuery, parameter);
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public virtual (DataTable table, long total) PageQuery(bool isWithSyntax, string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            if (Transaction?.Connection != null)
            {
                var sqlQuery = GetPageSql(isWithSyntax, sql, parameter, orderField, isAscending, pageSize, pageIndex);
                return QueryMultiple(Transaction.Connection, sqlQuery, parameter, Transaction);
            }
            else
            {
                using var connection = Connection;
                var sqlQuery = GetPageSql(isWithSyntax, sql, parameter, orderField, isAscending, pageSize, pageIndex);
                return QueryMultiple(connection, sqlQuery, parameter);
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">表实体泛型</typeparam>
        /// <param name="builder">SqlBuilder构建对象</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public virtual async Task<(IEnumerable<T> list, long total)> PageQueryAsync<T>(SqlBuilderCore<T> builder, string orderField, bool isAscending, int pageSize, int pageIndex) where T : class
        {
            if (Transaction?.Connection != null)
            {
                var sqlQuery = GetPageSql(false, builder.Sql, builder.Parameters, orderField, isAscending, pageSize, pageIndex);
                return await QueryMultipleAsync<T>(Transaction.Connection, sqlQuery, builder.DynamicParameters, Transaction);
            }
            else
            {
                await using var connection = Connection;
                var sqlQuery = GetPageSql(false, builder.Sql, builder.Parameters, orderField, isAscending, pageSize, pageIndex);
                return await QueryMultipleAsync<T>(connection, sqlQuery, builder.DynamicParameters);
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public virtual async Task<(IEnumerable<T> list, long total)> PageQueryAsync<T>(bool isWithSyntax, string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            if (Transaction?.Connection != null)
            {
                var sqlQuery = GetPageSql(isWithSyntax, sql, parameter, orderField, isAscending, pageSize, pageIndex);
                return await QueryMultipleAsync<T>(Transaction.Connection, sqlQuery, parameter, Transaction);
            }
            else
            {
                await using var connection = Connection;
                var sqlQuery = GetPageSql(isWithSyntax, sql, parameter, orderField, isAscending, pageSize, pageIndex);
                return await QueryMultipleAsync<T>(connection, sqlQuery, parameter);
            }
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public virtual async Task<(DataTable table, long total)> PageQueryAsync(bool isWithSyntax, string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            if (Transaction?.Connection != null)
            {
                var sqlQuery = GetPageSql(isWithSyntax, sql, parameter, orderField, isAscending, pageSize, pageIndex);
                return await QueryMultipleAsync(Transaction.Connection, sqlQuery, parameter, Transaction);
            }
            else
            {
                await using var connection = Connection;
                var sqlQuery = GetPageSql(isWithSyntax, sql, parameter, orderField, isAscending, pageSize, pageIndex);
                return await QueryMultipleAsync(connection, sqlQuery, parameter);
            }
        }
        #endregion
        #endregion

        #region Query
        #region Sync
        /// <summary>
        /// 查询集合数据
        /// </summary>
        /// <typeparam name="T">表实体类型</typeparam>
        /// <param name="builder">SqlBuilder构建对象</param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query<T>(SqlBuilderCore<T> builder) where T : class
        {
            DiagnosticsMessage message = null;
            try
            {
                IEnumerable<T> result = null;

                var sql = builder.Sql;
                var parameter = builder.DynamicParameters;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.Query<T>(sql, parameter, Transaction, commandTimeout: CommandTimeout);
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.Query<T>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询集合数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual IEnumerable<T> Query<T>(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                IEnumerable<T> result = null;

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.Query<T>(sql, parameter, Transaction, commandTimeout: CommandTimeout);
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.Query<T>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询集合数据
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual DataTable Query(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                DataTable result = null;

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.ExecuteReader(sql, parameter, Transaction, CommandTimeout).ToDataTable();
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.ExecuteReader(sql, parameter, commandTimeout: CommandTimeout).ToDataTable();
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询首条数据
        /// </summary>
        /// <typeparam name="T">表实体泛型</typeparam>
        /// <param name="builder">SqlBuilder构建对象</param>
        /// <returns></returns>
        public virtual T QueryFirstOrDefault<T>(SqlBuilderCore<T> builder) where T : class
        {
            DiagnosticsMessage message = null;
            try
            {
                var result = default(T);

                var sql = builder.Sql;
                var parameter = builder.DynamicParameters;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.QueryFirstOrDefault<T>(sql, parameter, Transaction, CommandTimeout);
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.QueryFirstOrDefault<T>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询首条数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual T QueryFirstOrDefault<T>(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                var result = default(T);

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.QueryFirstOrDefault<T>(sql, parameter, Transaction, CommandTimeout);
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.QueryFirstOrDefault<T>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询多结果集数据
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual List<IEnumerable<dynamic>> QueryMultiple(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                var list = new List<IEnumerable<dynamic>>();

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    var result = Transaction.Connection.QueryMultiple(sql, parameter, Transaction, CommandTimeout);
                    while (result?.IsConsumed == false)
                    {
                        list.Add(result.Read());
                    }
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    var result = connection.QueryMultiple(sql, parameter, commandTimeout: CommandTimeout);
                    while (result?.IsConsumed == false)
                    {
                        list.Add(result.Read());
                    }
                }

                ExecuteAfter(message);

                return list;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询多结果集数据
        /// </summary>
        /// <param name="connection">数据库连接对像</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public virtual (IEnumerable<T> list, long total) QueryMultiple<T>(DbConnection connection, string sql, object parameter, DbTransaction transaction = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                message = ExecuteBefore(sql, parameter, connection.DataSource);

                var multiQuery = connection.QueryMultiple(sql, parameter, transaction, CommandTimeout);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var list = multiQuery?.Read<T>();

                ExecuteAfter(message);

                return (list, total);
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询多结果集数据
        /// </summary>
        /// <param name="connection">数据库连接对像</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public virtual (DataTable table, long total) QueryMultiple(DbConnection connection, string sql, object parameter, DbTransaction transaction = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                message = ExecuteBefore(sql, parameter, connection.DataSource);

                var multiQuery = connection.QueryMultiple(sql, parameter, transaction, CommandTimeout);
                var total = multiQuery?.ReadFirstOrDefault<long>() ?? 0;
                var table = multiQuery?.Read()?.ToList()?.ToDataTable();
                if (table?.Columns?.Contains("ROWNUMBER") == true)
                    table.Columns.Remove("ROWNUMBER");

                ExecuteAfter(message);

                return (table, total);
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 查询集合数据
        /// </summary>
        /// <typeparam name="T">表实体泛型</typeparam>
        /// <param name="builder">SqlBuilder构建对象</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> QueryAsync<T>(SqlBuilderCore<T> builder) where T : class
        {
            DiagnosticsMessage message = null;
            try
            {
                IEnumerable<T> result = null;

                var sql = builder.Sql;
                var parameter = builder.DynamicParameters;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = await Transaction.Connection.QueryAsync<T>(sql, parameter, Transaction, CommandTimeout);
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = await connection.QueryAsync<T>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询集合数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> QueryAsync<T>(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                IEnumerable<T> result = null;

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = await Transaction.Connection.QueryAsync<T>(sql, parameter, Transaction, CommandTimeout);
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = await connection.QueryAsync<T>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询集合数据
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual async Task<DataTable> QueryAsync(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                DataTable result = null;

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    var reader = await Transaction.Connection.ExecuteReaderAsync(sql, parameter, Transaction, CommandTimeout);
                    result = reader.ToDataTable();
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    var reader = await connection.ExecuteReaderAsync(sql, parameter, commandTimeout: CommandTimeout);
                    result = reader.ToDataTable();
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询首条数据
        /// </summary>
        /// <typeparam name="T">表实体泛型</typeparam>
        /// <param name="builder">SqlBuilder构建对象</param>
        /// <returns></returns>
        public virtual async Task<T> QueryFirstOrDefaultAsync<T>(SqlBuilderCore<T> builder) where T : class
        {
            DiagnosticsMessage message = null;
            try
            {
                var result = default(T);

                var sql = builder.Sql;
                var parameter = builder.DynamicParameters;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = await Transaction.Connection.QueryFirstOrDefaultAsync<T>(sql, parameter, Transaction, CommandTimeout);
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询首条数据
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual async Task<T> QueryFirstOrDefaultAsync<T>(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                var result = default(T);

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = await Transaction.Connection.QueryFirstOrDefaultAsync<T>(sql, parameter, Transaction, CommandTimeout);
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = await connection.QueryFirstOrDefaultAsync<T>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询多结果集数据
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual async Task<List<IEnumerable<dynamic>>> QueryMultipleAsync(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                var list = new List<IEnumerable<dynamic>>();

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    var result = await Transaction.Connection.QueryMultipleAsync(sql, parameter, Transaction, CommandTimeout);
                    while (result?.IsConsumed == false)
                    {
                        list.Add(await result.ReadAsync());
                    }
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    var result = await connection.QueryMultipleAsync(sql, parameter, commandTimeout: CommandTimeout);
                    while (result?.IsConsumed == false)
                    {
                        list.Add(await result.ReadAsync());
                    }
                }

                ExecuteAfter(message);

                return list;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询多结果集数据
        /// </summary>
        /// <param name="connection">数据库连接对像</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public virtual async Task<(IEnumerable<T> list, long total)> QueryMultipleAsync<T>(DbConnection connection, string sql, object parameter, DbTransaction transaction = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                message = ExecuteBefore(sql, parameter, connection.DataSource);

                var multiQuery = await connection.QueryMultipleAsync(sql, parameter, transaction, CommandTimeout);
                var total = (long)((await multiQuery?.ReadFirstOrDefaultAsync<dynamic>())?.TOTAL ?? 0);
                var list = await multiQuery?.ReadAsync<T>();

                ExecuteAfter(message);

                return (list, total);
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 查询多结果集数据
        /// </summary>
        /// <param name="connection">数据库连接对像</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public virtual async Task<(DataTable table, long total)> QueryMultipleAsync(DbConnection connection, string sql, object parameter, DbTransaction transaction = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                message = ExecuteBefore(sql, parameter, connection.DataSource);

                var multiQuery = await connection.QueryMultipleAsync(sql, parameter, transaction, CommandTimeout);
                var total = (long)((await multiQuery?.ReadFirstOrDefaultAsync<dynamic>())?.TOTAL ?? 0);
                var reader = await multiQuery?.ReadAsync();
                var table = reader?.ToList()?.ToDataTable();
                if (table?.Columns?.Contains("ROWNUMBER") == true)
                    table.Columns.Remove("ROWNUMBER");

                ExecuteAfter(message);

                return (table, total);
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }
        #endregion
        #endregion

        #region Execute
        #region Sync
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <typeparam name="T">表实体泛型</typeparam>
        /// <param name="builder">SqlBuilder构建对象</param>
        /// <param name="command">命令类型</param>
        /// <returns></returns>
        public virtual int Execute<T>(SqlBuilderCore<T> builder, CommandType? command = null) where T : class
        {
            DiagnosticsMessage message = null;
            try
            {
                var result = 0;

                var sql = builder.Sql;
                var parameter = builder.DynamicParameters;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.Execute(sql, parameter, Transaction, CommandTimeout, command);
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.Execute(sql, parameter, null, CommandTimeout, command);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="command">命令类型</param>
        /// <returns></returns>
        public virtual int Execute(string sql, object parameter, CommandType? command = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                var result = 0;

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.Execute(sql, parameter, Transaction, CommandTimeout, command);
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.Execute(sql, parameter, null, CommandTimeout, command);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="command">命令类型</param>
        /// <returns></returns>
        public virtual IEnumerable<T> Execute<T>(string sql, object parameter, CommandType? command = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                IEnumerable<T> result = null;

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.Query<T>(sql, parameter, Transaction, commandTimeout: CommandTimeout, commandType: command);
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.Query<T>(sql, parameter, null, commandTimeout: CommandTimeout, commandType: command);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 执行sql，查询首行首列数据
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual object ExecuteScalar(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                object result = null;

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = Transaction.Connection.ExecuteScalar<object>(sql, parameter, Transaction, CommandTimeout);
                }
                else
                {
                    using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = connection.ExecuteScalar<object>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }
        #endregion

        #region Async
        /// <summary>
        /// 执行sql
        /// </summary>
        /// <typeparam name="T">表实体泛型</typeparam>
        /// <param name="builder">SqlBuilder构建对象</param>
        /// <param name="command">命令类型</param>
        /// <returns></returns>
        public virtual async Task<int> ExecuteAsync<T>(SqlBuilderCore<T> builder, CommandType? command = null) where T : class
        {
            DiagnosticsMessage message = null;
            try
            {
                var result = 0;

                var sql = builder.Sql;
                var parameter = builder.DynamicParameters;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = await Transaction.Connection.ExecuteAsync(sql, parameter, Transaction, CommandTimeout, command);
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = await connection.ExecuteAsync(sql, parameter, null, CommandTimeout, command);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="command">命令类型</param>
        /// <returns></returns>
        public virtual async Task<int> ExecuteAsync(string sql, object parameter, CommandType? command = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                var result = 0;

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = await Transaction.Connection.ExecuteAsync(sql, parameter, Transaction, CommandTimeout, command);
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = await connection.ExecuteAsync(sql, parameter, null, CommandTimeout, command);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 执行sql
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="command">命令类型</param>
        /// <returns></returns>
        public virtual async Task<IEnumerable<T>> ExecuteAsync<T>(string sql, object parameter, CommandType? command = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                IEnumerable<T> result = null;

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = await Transaction.Connection.QueryAsync<T>(sql, parameter, Transaction, CommandTimeout, command);
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = await connection.QueryAsync<T>(sql, parameter, commandTimeout: CommandTimeout, commandType: command);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }

        /// <summary>
        /// 执行sql，查询首行首列数据
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <returns></returns>
        public virtual async Task<object> ExecuteScalarAsync(bool isWithSyntax, string sql, object parameter)
        {
            DiagnosticsMessage message = null;
            try
            {
                object result = null;

                if (isWithSyntax)
                    sql = $"{sql} SELECT * FROM T";

                sql = SqlIntercept?.Invoke(sql, parameter) ?? sql;

                if (Transaction?.Connection != null)
                {
                    message = ExecuteBefore(sql, parameter, Transaction.Connection.DataSource);
                    result = await Transaction.Connection.ExecuteScalarAsync<object>(sql, parameter, Transaction, CommandTimeout);
                }
                else
                {
                    await using var connection = Connection;
                    message = ExecuteBefore(sql, parameter, connection.DataSource);
                    result = await connection.ExecuteScalarAsync<object>(sql, parameter, commandTimeout: CommandTimeout);
                }

                ExecuteAfter(message);

                return result;
            }
            catch (Exception ex)
            {
                ExecuteError(message, ex);
                throw;
            }
        }
        #endregion
        #endregion

        #region ExecuteBySql
        #region Sync
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回受影响行数</returns>
        public virtual int ExecuteBySql(string sql)
        {
            return Execute(sql, null);
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual int ExecuteBySql(string sql, object parameter)
        {
            return Execute(sql, parameter);
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual int ExecuteBySql(string sql, params DbParameter[] dbParameter)
        {
            return Execute(sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <returns>返回受影响行数</returns>
        public virtual int ExecuteByProc(string procName)
        {
            return Execute(procName, null, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual int ExecuteByProc(string procName, object parameter)
        {
            return Execute(procName, parameter, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual IEnumerable<T> ExecuteByProc<T>(string procName, object parameter)
        {
            return Execute<T>(procName, parameter, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual int ExecuteByProc(string procName, params DbParameter[] dbParameter)
        {
            return Execute(procName, dbParameter.ToDynamicParameters(), CommandType.StoredProcedure);
        }
        #endregion

        #region Async
        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> ExecuteBySqlAsync(string sql)
        {
            return await ExecuteAsync(sql, null);
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> ExecuteBySqlAsync(string sql, object parameter)
        {
            return await ExecuteAsync(sql, parameter);
        }

        /// <summary>
        /// 执行sql语句
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> ExecuteBySqlAsync(string sql, params DbParameter[] dbParameter)
        {
            return await ExecuteAsync(sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> ExecuteByProcAsync(string procName)
        {
            return await ExecuteAsync(procName, null, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> ExecuteByProcAsync(string procName, object parameter)
        {
            return await ExecuteAsync(procName, parameter, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<IEnumerable<T>> ExecuteByProcAsync<T>(string procName, object parameter)
        {
            return await ExecuteAsync<T>(procName, parameter, CommandType.StoredProcedure);
        }

        /// <summary>
        /// 执行sql存储过程
        /// </summary>
        /// <param name="procName">存储过程名称</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> ExecuteByProcAsync(string procName, params DbParameter[] dbParameter)
        {
            return await ExecuteAsync(procName, dbParameter.ToDynamicParameters(), CommandType.StoredProcedure);
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
        public virtual int Insert<T>(T entity) where T : class
        {
            var builder = Sql.Insert<T>(() => entity, DatabaseType, IsEnableNullValue, SqlIntercept, IsEnableFormat);
            return Execute(builder);
        }

        /// <summary>
        /// 插入多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要插入的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Insert<T>(IEnumerable<T> entities) where T : class
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
                    BeginTransaction();
                    foreach (var item in entities)
                    {
                        result += Insert(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    throw;
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
        public virtual int Insert<T>(List<T> entities) where T : class
        {
            return Insert(entities.AsEnumerable());
        }
        #endregion

        #region Async
        /// <summary>
        ///  插入单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> InsertAsync<T>(T entity) where T : class
        {
            var builder = Sql.Insert<T>(() => entity, DatabaseType, IsEnableNullValue, SqlIntercept, IsEnableFormat);
            return await ExecuteAsync(builder);
        }

        /// <summary>
        /// 插入多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要插入的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> InsertAsync<T>(IEnumerable<T> entities) where T : class
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
                    await BeginTransactionAsync();
                    foreach (var item in entities)
                    {
                        result += await InsertAsync(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    throw;
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
        public virtual async Task<int> InsertAsync<T>(List<T> entities) where T : class
        {
            return await InsertAsync(entities.AsEnumerable());
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
        public virtual int Delete<T>() where T : class
        {
            var builder = Sql.Delete<T>(DatabaseType, SqlIntercept, IsEnableFormat);
            return Execute(builder);
        }

        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要删除的实体</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Delete<T>(T entity) where T : class
        {
            var builder = Sql.Delete<T>(DatabaseType, SqlIntercept, IsEnableFormat).WithKey(entity);
            return Execute(builder);
        }

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要删除的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Delete<T>(IEnumerable<T> entities) where T : class
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
                    BeginTransaction();
                    foreach (var item in entities)
                    {
                        result += Delete(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    throw;
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
        public virtual int Delete<T>(List<T> entities) where T : class
        {
            return Delete(entities.AsEnumerable());
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">删除条件</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Delete<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Delete<T>(DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate);
            return Execute(builder);
        }

        /// <summary>
        /// 根据主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="keyValues">主键，多个值表示联合主键或者多个主键批量删除</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Delete<T>(params object[] keyValues) where T : class
        {
            var result = 0;
            var keys = Sql.GetPrimaryKey<T>(DatabaseType, IsEnableFormat);
            //多主键或者单主键
            if (keys.Count > 1 || keyValues.Length == 1)
            {
                var builder = Sql.Delete<T>(DatabaseType, SqlIntercept, IsEnableFormat).WithKey(keyValues);
                result = Execute(builder);
            }
            else
            {
                if (Transaction?.Connection != null)
                {
                    foreach (var key in keyValues)
                    {
                        result += Delete<T>(key);
                    }
                }
                else
                {
                    try
                    {
                        BeginTransaction();
                        foreach (var key in keyValues)
                        {
                            result += Delete<T>(key);
                        }
                        Commit();
                    }
                    catch (Exception)
                    {
                        Rollback();
                        throw;
                    }
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
        public virtual int Delete<T>(string propertyName, object propertyValue) where T : class
        {
            var parameterPrefix = new SqlWrapper { DatabaseType = DatabaseType }.DbParameterPrefix;
            var sql = $"DELETE FROM {Sql.GetTableName<T>(DatabaseType, IsEnableFormat)} WHERE {propertyName} = {parameterPrefix}PropertyValue";
            var parameter = new { PropertyValue = propertyValue };
            return Execute(sql, parameter);
        }
        #endregion

        #region Async
        /// <summary>
        /// 删除全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> DeleteAsync<T>() where T : class
        {
            var builder = Sql.Delete<T>(DatabaseType, SqlIntercept, IsEnableFormat);
            return await ExecuteAsync(builder);
        }

        /// <summary>
        /// 删除单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要删除的实体</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> DeleteAsync<T>(T entity) where T : class
        {
            var builder = Sql.Delete<T>(DatabaseType, SqlIntercept, IsEnableFormat).WithKey(entity);
            return await ExecuteAsync(builder);
        }

        /// <summary>
        /// 删除多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要删除的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> DeleteAsync<T>(IEnumerable<T> entities) where T : class
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
                    await BeginTransactionAsync();
                    foreach (var item in entities)
                    {
                        result += await DeleteAsync(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    throw;
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
        public virtual async Task<int> DeleteAsync<T>(List<T> entities) where T : class
        {
            return await DeleteAsync(entities.AsEnumerable());
        }

        /// <summary>
        /// 根据条件删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">删除条件</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> DeleteAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Delete<T>(DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate);
            return await ExecuteAsync(builder);
        }

        /// <summary>
        /// 根据主键删除实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="keyValues">主键，多个值表示联合主键或者多个主键批量删除</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> DeleteAsync<T>(params object[] keyValues) where T : class
        {
            var result = 0;
            var keys = Sql.GetPrimaryKey<T>(DatabaseType, IsEnableFormat);
            //多主键或者单主键
            if (keys.Count > 1 || keyValues.Length == 1)
            {
                var builder = Sql.Delete<T>(DatabaseType, SqlIntercept, IsEnableFormat).WithKey(keyValues);
                result = await ExecuteAsync(builder);
            }
            else
            {
                if (Transaction?.Connection != null)
                {
                    foreach (var key in keyValues)
                    {
                        result += await DeleteAsync<T>(key);
                    }
                }
                else
                {
                    try
                    {
                        await BeginTransactionAsync();
                        foreach (var key in keyValues)
                        {
                            result += await DeleteAsync<T>(key);
                        }
                        Commit();
                    }
                    catch (Exception)
                    {
                        Rollback();
                        throw;
                    }
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
        public virtual async Task<int> DeleteAsync<T>(string propertyName, object propertyValue) where T : class
        {
            var parameterPrefix = new SqlWrapper { DatabaseType = DatabaseType }.DbParameterPrefix;
            var sql = $"DELETE FROM {Sql.GetTableName<T>(DatabaseType, IsEnableFormat)} WHERE {propertyName} = {parameterPrefix}PropertyValue";
            var parameter = new { PropertyValue = propertyValue };
            return await ExecuteAsync(sql, parameter);
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
        public virtual int Update<T>(T entity) where T : class
        {
            var builder = Sql.Update<T>(() => entity, DatabaseType, IsEnableNullValue, SqlIntercept, IsEnableFormat).WithKey(entity);
            return Execute(builder);
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要更新的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Update<T>(IEnumerable<T> entities) where T : class
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
                    BeginTransaction();
                    foreach (var item in entities)
                    {
                        result += Update(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    throw;
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
        public virtual int Update<T>(List<T> entities) where T : class
        {
            return Update(entities.AsEnumerable());
        }

        /// <summary>
        /// 根据条件更新实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">更新条件</param>
        /// <param name="entity">要更新的实体</param>
        /// <returns>返回受影响行数</returns>
        public virtual int Update<T>(Expression<Func<T, bool>> predicate, Expression<Func<object>> entity) where T : class
        {
            var builder = Sql.Update<T>(entity, DatabaseType, IsEnableNullValue, SqlIntercept, IsEnableFormat).Where(predicate);
            return Execute(builder);
        }
        #endregion

        #region Async
        /// <summary>
        /// 更新单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要更新的实体</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> UpdateAsync<T>(T entity) where T : class
        {
            var builder = Sql.Update<T>(() => entity, DatabaseType, IsEnableNullValue, SqlIntercept, IsEnableFormat).WithKey(entity);
            return await ExecuteAsync(builder);
        }

        /// <summary>
        /// 更新多个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entities">要更新的实体集合</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> UpdateAsync<T>(IEnumerable<T> entities) where T : class
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
                    await BeginTransactionAsync();
                    foreach (var item in entities)
                    {
                        result += await UpdateAsync(item);
                    }
                    Commit();
                }
                catch (Exception)
                {
                    Rollback();
                    throw;
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
        public virtual async Task<int> UpdateAsync<T>(List<T> entities) where T : class
        {
            return await UpdateAsync(entities.AsEnumerable());
        }

        /// <summary>
        /// 根据条件更新实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">更新条件</param>
        /// <param name="entity">要更新的实体</param>
        /// <returns>返回受影响行数</returns>
        public virtual async Task<int> UpdateAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<object>> entity) where T : class
        {
            var builder = Sql.Update<T>(entity, DatabaseType, IsEnableNullValue, SqlIntercept, IsEnableFormat).Where(predicate);
            return await ExecuteAsync(builder);
        }
        #endregion
        #endregion

        #region Any
        #region Sync
        /// <summary>
        /// 是否存在任意一个满足查询条件的实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回查询结果</returns>
        public virtual bool Any<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return Count(predicate) > 0;
        }
        #endregion

        #region Async
        /// <summary>
        /// 是否存在任意一个满足查询条件的实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">查询条件</param>
        /// <returns>返回查询结果</returns>
        public virtual async Task<bool> AnyAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            return await CountAsync(predicate) > 0;
        }


        #endregion
        #endregion

        #region Count
        #region Sync
        /// <summary>
        /// 根据条件计数
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回计数结果</returns>
        public virtual long Count<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Count<T>(databaseType: DatabaseType, isEnableFormat: IsEnableFormat).Where(predicate);
            var res = FindObject(builder.Sql, builder.DynamicParameters);

            return res.To<long>();
        }

        /// <summary>
        /// 根据条件计数
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回计数结果</returns>
        public virtual long Count<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Count<T>(selector, DatabaseType, null, IsEnableFormat).Where(predicate);
            var res = FindObject(builder.Sql, builder.DynamicParameters);

            return res.To<long>();
        }
        #endregion

        #region Async
        /// <summary>
        /// 根据条件计数
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">查询条件</param>
        /// <returns>返回计数结果</returns>
        public virtual async Task<long> CountAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Count<T>(databaseType: DatabaseType, isEnableFormat: IsEnableFormat).Where(predicate);
            var res = await FindObjectAsync(builder.Sql, builder.DynamicParameters);

            return res.To<long>();
        }

        /// <summary>
        /// 根据条件计数
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回计数结果</returns>
        public virtual async Task<long> CountAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Count<T>(selector, DatabaseType, null, IsEnableFormat).Where(predicate);
            var res = await FindObjectAsync(builder.Sql, builder.DynamicParameters);

            return res.To<long>();
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
        public virtual object FindObject(string sql)
        {
            return FindObject(sql, null);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        public virtual object FindObject(string sql, object parameter)
        {
            return ExecuteScalar(false, sql, parameter);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        public virtual object FindObject(string sql, params DbParameter[] dbParameter)
        {
            return ExecuteScalar(false, sql, dbParameter.ToDynamicParameters());
        }
        #endregion

        #region Async
        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果对象</returns>
        public virtual async Task<object> FindObjectAsync(string sql)
        {
            return await FindObjectAsync(sql, null);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        public virtual async Task<object> FindObjectAsync(string sql, object parameter)
        {
            return await ExecuteScalarAsync(false, sql, parameter);
        }

        /// <summary>
        /// 查询单个对象
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果对象</returns>
        public virtual async Task<object> FindObjectAsync(string sql, params DbParameter[] dbParameter)
        {
            return await ExecuteScalarAsync(false, sql, dbParameter.ToDynamicParameters());
        }
        #endregion
        #endregion

        #region FindEntity
        #region Sync
        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="keyValues">主键，多个值表示联合主键</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(params object[] keyValues) where T : class
        {
            if (keyValues == null)
                return default;

            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).WithKey(keyValues);
            return QueryFirstOrDefault<T>(builder);
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(string sql)
        {
            return QueryFirstOrDefault<T>(false, sql, null);
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(string sql, object parameter)
        {
            return QueryFirstOrDefault<T>(false, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(string sql, params DbParameter[] dbParameter)
        {
            return QueryFirstOrDefault<T>(false, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="keyValues">主键，多个值表示联合主键</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(Expression<Func<T, object>> selector, params object[] keyValues) where T : class
        {
            if (keyValues == null)
                return default;

            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).WithKey(keyValues);
            return QueryFirstOrDefault<T>(builder);
        }

        /// <summary>
        /// 根据条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).Where(predicate);
            return QueryFirstOrDefault<T>(builder);
        }

        /// <summary>
        /// 根据条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate);
            return QueryFirstOrDefault<T>(builder);
        }

        /// <summary>
        /// 根据条件、排序查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).Where(predicate).OrderBy(orderField, orderTypes).Top(1);
            return QueryFirstOrDefault<T>(builder);
        }

        /// <summary>
        /// 根据条件、排序查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型</param>
        /// <returns>返回实体</returns>
        public virtual T FindEntity<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate).OrderBy(orderField, orderTypes).Top(1);
            return QueryFirstOrDefault<T>(builder);
        }
        #endregion

        #region Async
        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="keyValues">主键，多个值表示联合主键</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(params object[] keyValues) where T : class
        {
            if (keyValues == null)
                return default;

            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).WithKey(keyValues);
            return await QueryFirstOrDefaultAsync<T>(builder);
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(string sql)
        {
            return await QueryFirstOrDefaultAsync<T>(false, sql, null);
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(string sql, object parameter)
        {
            return await QueryFirstOrDefaultAsync<T>(false, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(string sql, params DbParameter[] dbParameter)
        {
            return await QueryFirstOrDefaultAsync<T>(false, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// 根据主键查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="keyValues">主键，多个值表示联合主键</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(Expression<Func<T, object>> selector, params object[] keyValues) where T : class
        {
            if (keyValues == null)
                return default;

            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).WithKey(keyValues);
            return await QueryFirstOrDefaultAsync(builder);
        }

        /// <summary>
        /// 根据条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">查询条件</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).Where(predicate);
            return await QueryFirstOrDefaultAsync(builder);
        }

        /// <summary>
        /// 根据条件查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate);
            return await QueryFirstOrDefaultAsync(builder);
        }

        /// <summary>
        /// 根据条件、排序查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).Where(predicate).OrderBy(orderField, orderTypes).Top(1);
            return await QueryFirstOrDefaultAsync(builder);
        }

        /// <summary>
        /// 根据条件、排序查询单个实体
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型</param>
        /// <returns>返回实体</returns>
        public virtual async Task<T> FindEntityAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate).OrderBy(orderField, orderTypes).Top(1);
            return await QueryFirstOrDefaultAsync(builder);
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
        public virtual IEnumerable<T> FindList<T>() where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat);
            return Query<T>(builder);
        }

        /// <summary>
        /// 查询指定列
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat);
            return Query<T>(builder);
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">查询条件</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).Where(predicate);
            return Query<T>(builder);
        }

        /// <summary>
        /// 根据条件查询指定列
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate);
            return Query<T>(builder);
        }

        /// <summary>
        /// 查询指定列并排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型，默认正序排序</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).OrderBy(orderField, orderTypes);
            return Query<T>(builder);
        }

        /// <summary>
        /// 根据条件查询指定列并排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型，默认正序排序</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).Where(predicate).OrderBy(orderField, orderTypes);
            return Query<T>(builder);
        }

        /// <summary>
        /// 根据条件查询指定列并排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型，默认正序排序</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate).OrderBy(orderField, orderTypes);
            return Query<T>(builder);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(string sql)
        {
            return FindList<T>(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(string sql, object parameter)
        {
            return Query<T>(false, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindList<T>(string sql, params DbParameter[] dbParameter)
        {
            return Query<T>(false, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public virtual (IEnumerable<T> list, long total) FindList<T>(string orderField, bool isAscending, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, isEnableFormat: IsEnableFormat);
            return PageQuery<T>(builder, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据条件分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public virtual (IEnumerable<T> list, long total) FindList<T>(Expression<Func<T, bool>> predicate, string orderField, bool isAscending, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, isEnableFormat: IsEnableFormat).Where(predicate);
            return PageQuery<T>(builder, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据条件分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public virtual (IEnumerable<T> list, long total) FindList<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, string orderField, bool isAscending, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, isEnableFormat: IsEnableFormat).Where(predicate);
            return PageQuery<T>(builder, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public virtual (IEnumerable<T> list, long total) FindList<T>(string sql, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return FindList<T>(sql, null, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public virtual (IEnumerable<T> list, long total) FindList<T>(string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return PageQuery<T>(false, sql, parameter, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public virtual (IEnumerable<T> list, long total) FindList<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return PageQuery<T>(false, sql, dbParameter.ToDynamicParameters(), orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindListByWith<T>(string sql)
        {
            return FindListByWith<T>(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindListByWith<T>(string sql, object parameter)
        {
            return Query<T>(true, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回集合</returns>
        public virtual IEnumerable<T> FindListByWith<T>(string sql, params DbParameter[] dbParameter)
        {
            return Query<T>(true, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public virtual (IEnumerable<T> list, long total) FindListByWith<T>(string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return PageQuery<T>(true, sql, parameter, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回集合和总记录数</returns>
        public virtual (IEnumerable<T> list, long total) FindListByWith<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return PageQuery<T>(true, sql, dbParameter.ToDynamicParameters(), orderField, isAscending, pageSize, pageIndex);
        }
        #endregion

        #region Async
        /// <summary>
        /// 查询全部
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>() where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat);
            return await QueryAsync<T>(builder);
        }

        /// <summary>
        /// 查询指定列
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat);
            return await QueryAsync<T>(builder);
        }

        /// <summary>
        /// 根据条件查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">查询条件</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).Where(predicate);
            return await QueryAsync<T>(builder);
        }

        /// <summary>
        /// 根据条件查询指定列
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate);
            return await QueryAsync<T>(builder);
        }

        /// <summary>
        /// 查询指定列并排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型，默认正序排序</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).OrderBy(orderField, orderTypes);
            return await QueryAsync<T>(builder);
        }

        /// <summary>
        /// 根据条件查询指定列并排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型，默认正序排序</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, sqlIntercept: SqlIntercept, isEnableFormat: IsEnableFormat).Where(predicate).OrderBy(orderField, orderTypes);
            return await QueryAsync<T>(builder);
        }

        /// <summary>
        /// 根据条件查询指定列并排序
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="orderTypes">排序类型，默认正序排序</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, Expression<Func<T, object>> orderField, params OrderType[] orderTypes) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, SqlIntercept, IsEnableFormat).Where(predicate).OrderBy(orderField, orderTypes);
            return await QueryAsync<T>(builder);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(string sql)
        {
            return await FindListAsync<T>(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(string sql, object parameter)
        {
            return await QueryAsync<T>(false, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListAsync<T>(string sql, params DbParameter[] dbParameter)
        {
            return await QueryAsync<T>(false, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// 分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public virtual async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string orderField, bool isAscending, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, isEnableFormat: IsEnableFormat);
            return await PageQueryAsync<T>(builder, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据条件分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>        
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public virtual async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(Expression<Func<T, bool>> predicate, string orderField, bool isAscending, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(databaseType: DatabaseType, isEnableFormat: IsEnableFormat).Where(predicate);
            return await PageQueryAsync<T>(builder, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据条件分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="selector">选择指定列，null选择全部</param>
        /// <param name="predicate">查询条件</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public virtual async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(Expression<Func<T, object>> selector, Expression<Func<T, bool>> predicate, string orderField, bool isAscending, int pageSize, int pageIndex) where T : class
        {
            var builder = Sql.Select<T>(selector, DatabaseType, isEnableFormat: IsEnableFormat).Where(predicate);
            return await PageQueryAsync<T>(builder, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public virtual async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string sql, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await FindListAsync<T>(sql, null, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public virtual async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await PageQueryAsync<T>(false, sql, parameter, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public virtual async Task<(IEnumerable<T> list, long total)> FindListAsync<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await PageQueryAsync<T>(false, sql, dbParameter.ToDynamicParameters(), orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListByWithAsync<T>(string sql)
        {
            return await FindListByWithAsync<T>(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListByWithAsync<T>(string sql, object parameter)
        {
            return await QueryAsync<T>(true, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回集合</returns>
        public virtual async Task<IEnumerable<T>> FindListByWithAsync<T>(string sql, params DbParameter[] dbParameter)
        {
            return await QueryAsync<T>(true, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public virtual async Task<(IEnumerable<T> list, long total)> FindListByWithAsync<T>(string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await PageQueryAsync<T>(true, sql, parameter, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回集合和总记录数</returns>
        public virtual async Task<(IEnumerable<T> list, long total)> FindListByWithAsync<T>(string sql, DbParameter[] dbParameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await PageQueryAsync<T>(true, sql, dbParameter.ToDynamicParameters(), orderField, isAscending, pageSize, pageIndex);
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
        public virtual DataTable FindTable(string sql)
        {
            return FindTable(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public virtual DataTable FindTable(string sql, object parameter)
        {
            return Query(false, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public virtual DataTable FindTable(string sql, params DbParameter[] dbParameter)
        {
            return Query(false, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        public virtual (DataTable table, long total) FindTable(string sql, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return FindTable(sql, null, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        public virtual (DataTable table, long total) FindTable(string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return PageQuery(false, sql, parameter, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        public virtual (DataTable table, long total) FindTable(string sql, DbParameter[] dbParameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return PageQuery(false, sql, dbParameter.ToDynamicParameters(), orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataTable</returns>
        public virtual DataTable FindTableByWith(string sql)
        {
            return FindTableByWith(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public virtual DataTable FindTableByWith(string sql, object parameter)
        {
            return Query(true, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public virtual DataTable FindTableByWith(string sql, params DbParameter[] dbParameter)
        {
            return Query(true, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        public virtual (DataTable table, long total) FindTableByWith(string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return PageQuery(true, sql, parameter, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        public virtual (DataTable table, long total) FindTableByWith(string sql, DbParameter[] dbParameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return PageQuery(true, sql, dbParameter.ToDynamicParameters(), orderField, isAscending, pageSize, pageIndex);
        }
        #endregion

        #region Async
        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataTable</returns>
        public virtual async Task<DataTable> FindTableAsync(string sql)
        {
            return await FindTableAsync(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public virtual async Task<DataTable> FindTableAsync(string sql, object parameter)
        {
            return await QueryAsync(false, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public virtual async Task<DataTable> FindTableAsync(string sql, params DbParameter[] dbParameter)
        {
            return await QueryAsync(false, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回DataTable和总记录数</returns>
        public virtual async Task<(DataTable table, long total)> FindTableAsync(string sql, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await FindTableAsync(sql, null, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回DataTable和总记录数</returns>
        public virtual async Task<(DataTable table, long total)> FindTableAsync(string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await PageQueryAsync(false, sql, parameter, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns>返回DataTable和总记录数</returns>
        public virtual async Task<(DataTable table, long total)> FindTableAsync(string sql, DbParameter[] dbParameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await PageQueryAsync(false, sql, dbParameter.ToDynamicParameters(), orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回DataTable</returns>
        public virtual async Task<DataTable> FindTableByWithAsync(string sql)
        {
            return await FindTableByWithAsync(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public virtual async Task<DataTable> FindTableByWithAsync(string sql, object parameter)
        {
            return await QueryAsync(true, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回DataTable</returns>
        public virtual async Task<DataTable> FindTableByWithAsync(string sql, params DbParameter[] dbParameter)
        {
            return await QueryAsync(true, sql, dbParameter.ToDynamicParameters());
        }

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        public virtual async Task<(DataTable table, long total)> FindTableByWithAsync(string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await PageQueryAsync(true, sql, parameter, orderField, isAscending, pageSize, pageIndex);
        }

        /// <summary>
        /// with语法分页查询
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>        
        /// <returns>返回DataTable和总记录数</returns>
        public virtual async Task<(DataTable table, long total)> FindTableByWithAsync(string sql, DbParameter[] dbParameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            return await PageQueryAsync(true, sql, dbParameter.ToDynamicParameters(), orderField, isAscending, pageSize, pageIndex);
        }
        #endregion
        #endregion

        #region FindMultiple
        #region Sync
        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果集</returns>
        public virtual List<IEnumerable<dynamic>> FindMultiple(string sql)
        {
            return FindMultiple(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回查询结果集</returns>
        public virtual List<IEnumerable<dynamic>> FindMultiple(string sql, object parameter)
        {
            return QueryMultiple(false, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果集</returns>
        public virtual List<IEnumerable<dynamic>> FindMultiple(string sql, params DbParameter[] dbParameter)
        {
            return QueryMultiple(false, sql, dbParameter.ToDynamicParameters());
        }
        #endregion

        #region Async
        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <returns>返回查询结果集</returns>
        public virtual async Task<List<IEnumerable<dynamic>>> FindMultipleAsync(string sql)
        {
            return await FindMultipleAsync(sql, null);
        }

        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">对应参数</param>
        /// <returns>返回查询结果集</returns>
        public virtual async Task<List<IEnumerable<dynamic>>> FindMultipleAsync(string sql, object parameter)
        {
            return await QueryMultipleAsync(false, sql, parameter);
        }

        /// <summary>
        /// 根据sql语句查询返回多个结果集
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="dbParameter">对应参数</param>
        /// <returns>返回查询结果集</returns>
        public virtual async Task<List<IEnumerable<dynamic>>> FindMultipleAsync(string sql, params DbParameter[] dbParameter)
        {
            return await QueryMultipleAsync(false, sql, dbParameter.ToDynamicParameters());
        }
        #endregion
        #endregion

        #region Dispose
        #region Sync
        /// <summary>
        /// 释放资源
        /// </summary>
        public virtual void Dispose() => Close();
        #endregion

        #region Async
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <returns></returns>
        public virtual async ValueTask DisposeAsync() => await CloseAsync();
        #endregion
        #endregion

        #region Close
        #region Sync
        /// <summary>
        /// 关闭连接
        /// </summary>
        public virtual void Close()
        {
            _tranConnection?.Close();
            _tranConnection?.Dispose();

            Transaction = null;
        }
        #endregion

        #region Async
        /// <summary>
        /// 关闭连接
        /// </summary>
        public virtual async ValueTask CloseAsync()
        {
            if (_tranConnection != null)
                await _tranConnection.CloseAsync();

            if (_tranConnection != null)
                await _tranConnection.DisposeAsync();

            Transaction = null;
        }
        #endregion
        #endregion

        #region Diagnostics
        /// <summary>
        /// 执行前诊断
        /// </summary>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">sql参数</param>
        /// <param name="dataSource">数据源</param>
        /// <returns></returns>
        public virtual DiagnosticsMessage ExecuteBefore(string sql, object parameter, string dataSource)
        {
            if (!_diagnosticListener.IsEnabled(DiagnosticStrings.BeforeExecute))
                return null;

            var message = new DiagnosticsMessage
            {
                Sql = sql,
                Parameters = parameter,
                DataSource = dataSource,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds(),
                Operation = DiagnosticStrings.BeforeExecute,
                DatabaseType = DatabaseType
            };

            _diagnosticListener.Write(DiagnosticStrings.BeforeExecute, message);

            return message;
        }

        /// <summary>
        /// 执行后诊断
        /// </summary>
        /// <param name="message">诊断消息</param>
        /// <returns></returns>
        public virtual void ExecuteAfter(DiagnosticsMessage message)
        {
            if (message?.Timestamp != null && _diagnosticListener.IsEnabled(DiagnosticStrings.AfterExecute))
            {
                message.Operation = DiagnosticStrings.AfterExecute;
                message.ElapsedMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message.Timestamp.Value;

                _diagnosticListener.Write(DiagnosticStrings.AfterExecute, message);
            }
        }

        /// <summary>
        /// 执行异常诊断
        /// </summary>
        /// <param name="message">诊断消息</param>
        /// <param name="exception">异常</param>
        public virtual void ExecuteError(DiagnosticsMessage message, Exception exception)
        {
            if (exception != null && message?.Timestamp != null && _diagnosticListener.IsEnabled(DiagnosticStrings.ErrorExecute))
            {
                message.Exception = exception;
                message.Operation = DiagnosticStrings.ErrorExecute;
                message.ElapsedMilliseconds = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - message.Timestamp.Value;

                _diagnosticListener.Write(DiagnosticStrings.ErrorExecute, message);
            }
        }
        #endregion

        #region GetDbConnection
        /// <summary>
        /// 根据数据库类型获取DbConnection
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        public virtual DbConnection GetDbConnection(string connectionString) =>
            DatabaseType switch
            {
                DatabaseType.Sqlite => new SqliteConnection(connectionString),
                DatabaseType.SqlServer => new SqlConnection(connectionString),
                DatabaseType.MySql => new MySqlConnection(connectionString),
                DatabaseType.Oracle => new OracleConnection(connectionString),
                DatabaseType.PostgreSql => new NpgsqlConnection(connectionString),
                _ => new SqlConnection(connectionString),
            };
        #endregion
    }
}
