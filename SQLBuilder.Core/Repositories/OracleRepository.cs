﻿#region License
/***
 * Copyright © 2018-2022, 张强 (943620963@qq.com).
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
using Microsoft.Extensions.Configuration;
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
    public class OracleRepository : BaseRepository
    {
        #region Property
        /// <summary>
        /// 数据库类型
        /// </summary>
        public override DatabaseType DatabaseType => DatabaseType.Oracle;
        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        public OracleRepository() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">主库连接字符串，或者链接字符串名称</param>
        /// <param name="configuration">数据库连接配置，默认：null，为null时则使用ConfigurationManager.Configuration</param>
        public OracleRepository(string connectionString, IConfiguration configuration = null)
            : base(connectionString, configuration) { }
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
            if (orderField.IsNotNullOrEmpty())
            {
                if (orderField.Contains(@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)", RegexOptions.IgnoreCase))
                    orderField = $"ORDER BY {orderField}";
                else
                    orderField = $"ORDER BY {orderField} {(isAscending ? "ASC" : "DESC")}";
            }

            string sqlQuery;
            var next = pageSize;
            var offset = pageSize * (pageIndex - 1);
            var rowStart = pageSize * (pageIndex - 1) + 1;
            var rowEnd = pageSize * pageIndex;
            var serverVersion = int.Parse(Connection.ServerVersion.Split('.')[0]);

            //判断是否with语法
            if (isWithSyntax)
            {
                sqlQuery = $"{sql} SELECT {CountSyntax} AS \"TOTAL\" FROM T;";

                if (serverVersion > 11)
                    sqlQuery += $"{sql.Remove(sql.LastIndexOf(")"), 1)} {orderField}) SELECT * FROM T OFFSET {offset} ROWS FETCH NEXT {next} ROWS ONLY";
                else
                    sqlQuery += $"{sql.Remove(sql.LastIndexOf(")"), 1)} {orderField}),R AS (SELECT ROWNUM AS ROWNUMBER,T.* FROM T WHERE ROWNUM <= {rowEnd}) SELECT * FROM R WHERE ROWNUMBER>={rowStart}";
            }
            else
            {
                sqlQuery = $"SELECT {CountSyntax} AS \"TOTAL\" FROM ({sql}) T;";

                if (serverVersion > 11)
                    sqlQuery += $"{sql} {orderField} OFFSET {offset} ROWS FETCH NEXT {next} ROWS ONLY";
                else
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
