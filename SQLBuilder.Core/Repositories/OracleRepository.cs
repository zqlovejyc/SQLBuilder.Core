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
using Oracle.ManagedDataAccess.Client;
using SQLBuilder.Core.Configuration;
using SQLBuilder.Core.Diagnostics;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SQLBuilder.Core.Repositories
{
    /// <summary>
    /// Oracle仓储实现类
    /// </summary>
    public class OracleRepository : BaseRepository, IRepository
    {
        #region Field
        /// <summary>
        /// 事务数据库连接对象
        /// </summary>
        private DbConnection _tranConnection;
        #endregion

        #region Property
        /// <summary>
        /// 数据库连接对象
        /// </summary>
        public override DbConnection Connection
        {
            get
            {
                OracleConnection connection;
                if (!Master && SlaveConnectionStrings?.Length > 0 && LoadBalancer != null)
                {
                    var connectionStrings = SlaveConnectionStrings.Select(x => x.connectionString);
                    var weights = SlaveConnectionStrings.Select(x => x.weight).ToArray();
                    var connectionString = LoadBalancer.Get(MasterConnectionString, connectionStrings, weights);

                    connection = new OracleConnection(connectionString);
                }
                else
                    connection = new OracleConnection(MasterConnectionString);

                if (connection.State != ConnectionState.Open)
                    connection.Open();

                return connection;
            }
        }

        /// <summary>
        /// 数据库类型
        /// </summary>
        public override DatabaseType DatabaseType => DatabaseType.Oracle;

        /// <summary>
        /// 仓储接口
        /// </summary>
        public override IRepository Repository => this;
        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">主库连接字符串，或者链接字符串名称</param>
        public OracleRepository(string connectionString)
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

        #region UseMasterOrSlave
        /// <summary>
        /// 使用主库/从库
        /// <para>注意使用从库必须满足：配置从库连接字符串 + 切换为从库 + 配置从库负载均衡，否则依然使用主库</para>
        /// </summary>
        /// <param name="master">是否使用主库，默认使用主库</param>
        /// <returns></returns>
        public IRepository UseMasterOrSlave(bool master = true)
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
        public override IRepository BeginTransaction()
        {
            if (Transaction?.Connection == null)
            {
                _tranConnection = Connection;
                Transaction = _tranConnection.BeginTransaction();
            }
            return this;
        }
        #endregion

        #region Async
        /// <summary>
        /// 开启事务
        /// </summary>
        /// <returns>IRepository</returns>
        public override async Task<IRepository> BeginTransactionAsync()
        {
            if (Transaction?.Connection == null)
            {
                _tranConnection = Connection;
                Transaction = await _tranConnection.BeginTransactionAsync();
            }
            return this;
        }
        #endregion
        #endregion

        #region Close
        #region Sync
        /// <summary>
        /// 关闭连接
        /// </summary>
        public override void Close()
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
        public override async ValueTask CloseAsync()
        {
            if (_tranConnection != null)
                await _tranConnection.CloseAsync();

            if (_tranConnection != null)
                await _tranConnection.DisposeAsync();

            Transaction = null;
        }
        #endregion
        #endregion

        #region Page
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
        public override string GetPageSql(bool isWithSyntax, string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            //排序字段
            if (!orderField.IsNullOrEmpty())
            {
                if (orderField.Contains(@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)", RegexOptions.IgnoreCase))
                    orderField = $"ORDER BY {orderField}";
                else
                    orderField = $"ORDER BY {orderField} {(isAscending ? "ASC" : "DESC")}";
            }

            string sqlQuery;
            var rowStart = pageSize * (pageIndex - 1) + 1;
            var rowEnd = pageSize * pageIndex;

            //判断是否with语法
            if (isWithSyntax)
            {
                sqlQuery = $"{sql} SELECT {CountSyntax} AS \"TOTAL\" FROM T;";

                sqlQuery += $"{sql.Remove(sql.LastIndexOf(")"), 1)} {orderField}),R AS (SELECT ROWNUM AS ROWNUMBER,T.* FROM T WHERE ROWNUM <= {rowEnd}) SELECT * FROM R WHERE ROWNUMBER>={rowStart}";
            }
            else
            {
                sqlQuery = $"SELECT {CountSyntax} AS \"TOTAL\" FROM ({sql}) T;";

                sqlQuery += $"SELECT * FROM (SELECT X.*,ROWNUM AS \"ROWNUMBER\" FROM ({sql} {orderField}) X WHERE ROWNUM <= {rowEnd}) T WHERE \"ROWNUMBER\" >= {rowStart}";
            }

            sqlQuery = SqlIntercept?.Invoke(sqlQuery, parameter) ?? sqlQuery;

            return sqlQuery;
        }
        #endregion

        #region Query
        #region Sync
        /// <summary>
        /// 查询多结果集数据
        /// </summary>
        /// <param name="connection">数据库连接对像</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public override (IEnumerable<T> list, long total) QueryMultiple<T>(DbConnection connection, string sql, object parameter, DbTransaction transaction = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                message = ExecuteBefore(sql, parameter, connection.DataSource);

                var sqlPage = sql.Split(';');
                var sqlCount = sqlPage[0];
                var sqlQuery = sqlPage[1];
                var total = connection.QueryFirstOrDefault<long>(sqlCount, parameter, transaction, CommandTimeout);
                var list = connection.Query<T>(sqlQuery, parameter, transaction, commandTimeout: CommandTimeout);

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
        public override (DataTable table, long total) QueryMultiple(DbConnection connection, string sql, object parameter, DbTransaction transaction = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                message = ExecuteBefore(sql, parameter, connection.DataSource);

                var sqlPage = sql.Split(';');
                var sqlCount = sqlPage[0];
                var sqlQuery = sqlPage[1];
                var total = connection.QueryFirstOrDefault<long>(sqlCount, parameter, transaction, CommandTimeout);
                var reader = connection.ExecuteReader(sqlQuery, parameter, transaction, CommandTimeout);
                var table = reader?.ToDataTable();
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
        /// 查询多结果集数据
        /// </summary>
        /// <param name="connection">数据库连接对像</param>
        /// <param name="sql">sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="transaction">事务</param>
        /// <returns></returns>
        public override async Task<(IEnumerable<T> list, long total)> QueryMultipleAsync<T>(DbConnection connection, string sql, object parameter, DbTransaction transaction = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                message = ExecuteBefore(sql, parameter, connection.DataSource);

                var sqlPage = sql.Split(';');
                var sqlCount = sqlPage[0];
                var sqlQuery = sqlPage[1];
                var total = await connection.QueryFirstOrDefaultAsync<long>(sqlCount, parameter, transaction, CommandTimeout);
                var list = await connection.QueryAsync<T>(sqlQuery, parameter, transaction, CommandTimeout);

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
        public override async Task<(DataTable table, long total)> QueryMultipleAsync(DbConnection connection, string sql, object parameter, DbTransaction transaction = null)
        {
            DiagnosticsMessage message = null;
            try
            {
                message = ExecuteBefore(sql, parameter, connection.DataSource);

                var sqlPage = sql.Split(';');
                var sqlCount = sqlPage[0];
                var sqlQuery = sqlPage[1];
                var total = await connection.QueryFirstOrDefaultAsync<long>(sqlCount, parameter, transaction, CommandTimeout);
                var reader = await connection.ExecuteReaderAsync(sqlQuery, parameter, transaction, CommandTimeout);
                var table = reader?.ToDataTable();
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
    }
}
