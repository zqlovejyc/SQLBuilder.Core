﻿#region License
/***
 * Copyright © 2018-2025, 张强 (943620963@qq.com).
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

using Microsoft.Extensions.Configuration;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Sql = SQLBuilder.Core.Entry.SqlBuilder;

namespace SQLBuilder.Core.Repositories
{
    /// <summary>
    /// Sqlserver仓储实现类
    /// </summary>
    public class SqlRepository : BaseRepository
    {
        #region Property
        /// <summary>
        /// 数据库类型
        /// </summary>
        public override DatabaseType DatabaseType => DatabaseType.SqlServer;
        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        public SqlRepository() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">主库连接字符串，或者链接字符串名称</param>
        /// <param name="configuration">数据库连接配置，默认：null，为null时则使用ConfigurationManager.Configuration</param>
        public SqlRepository(string connectionString, IConfiguration configuration = null)
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
            string order;
            if (orderField.IsNotNullOrEmpty())
            {
                if (orderField.Contains(@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)", RegexOptions.IgnoreCase))
                    order = $"ORDER BY {orderField}";
                else
                    order = $"ORDER BY {orderField} {(isAscending ? "ASC" : "DESC")}";
            }
            else
            {
                order = "ORDER BY (SELECT 0)";
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
                sqlQuery = $"{sql} SELECT {CountSyntax} AS [TOTAL] FROM T;";

                if (serverVersion > 10)
                    sqlQuery += $"{sql.Remove(sql.LastIndexOf(")"), 1)} {(orderField.IsNotNullOrEmpty() ? order : "")}) SELECT * FROM T OFFSET {offset} ROWS FETCH NEXT {next} ROWS ONLY;";
                else
                    sqlQuery += $"{sql},R AS (SELECT ROW_NUMBER() OVER ({order}) AS [ROWNUMBER], * FROM T) SELECT * FROM R WHERE [ROWNUMBER] BETWEEN {rowStart} AND {rowEnd};";
            }
            else
            {
                sqlQuery = $"SELECT {CountSyntax} AS [TOTAL] FROM ({sql}) AS T;";

                if (serverVersion > 10)
                    sqlQuery += $"{sql} {(orderField.IsNotNullOrEmpty() ? order : "")} OFFSET {offset} ROWS FETCH NEXT {next} ROWS ONLY;";
                else
                    sqlQuery += $"SELECT * FROM (SELECT ROW_NUMBER() OVER ({order}) AS [ROWNUMBER], * FROM ({sql}) AS T) AS N WHERE [ROWNUMBER] BETWEEN {rowStart} AND {rowEnd};";
            }

            sqlQuery = SqlIntercept?.Invoke(sqlQuery, parameter) ?? sqlQuery;

            return sqlQuery;
        }

        /// <summary>
        /// 获取分页语句(是否包含下一页，不返回总条数)
        /// </summary>
        /// <param name="isWithSyntax">是否with语法</param>
        /// <param name="sql">原始sql语句</param>
        /// <param name="parameter">参数</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="isAscending">是否升序排序</param>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <returns></returns>
        public override string GetNextPageSql(bool isWithSyntax, string sql, object parameter, string orderField, bool isAscending, int pageSize, int pageIndex)
        {
            //排序字段
            string order;
            if (orderField.IsNotNullOrEmpty())
            {
                if (orderField.Contains(@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)", RegexOptions.IgnoreCase))
                    order = $"ORDER BY {orderField}";
                else
                    order = $"ORDER BY {orderField} {(isAscending ? "ASC" : "DESC")}";
            }
            else
            {
                order = "ORDER BY (SELECT 0)";
            }

            var sqlQuery = string.Empty;
            var next = pageSize + 1;
            var offset = pageSize * (pageIndex - 1);
            var rowStart = pageSize * (pageIndex - 1) + 1;
            var rowEnd = pageSize * pageIndex + 1;
            var serverVersion = int.Parse(Connection.ServerVersion.Split('.')[0]);

            //判断是否with语法
            if (isWithSyntax)
            {
                if (serverVersion > 10)
                    sqlQuery += $"{sql.Remove(sql.LastIndexOf(")"), 1)} {(orderField.IsNotNullOrEmpty() ? order : "")}) SELECT * FROM T OFFSET {offset} ROWS FETCH NEXT {next} ROWS ONLY;";
                else
                    sqlQuery += $"{sql},R AS (SELECT ROW_NUMBER() OVER ({order}) AS [ROWNUMBER], * FROM T) SELECT * FROM R WHERE [ROWNUMBER] BETWEEN {rowStart} AND {rowEnd};";
            }
            else
            {
                if (serverVersion > 10)
                    sqlQuery += $"{sql} {(orderField.IsNotNullOrEmpty() ? order : "")} OFFSET {offset} ROWS FETCH NEXT {next} ROWS ONLY;";
                else
                    sqlQuery += $"SELECT * FROM (SELECT ROW_NUMBER() OVER ({order}) AS [ROWNUMBER], * FROM ({sql}) AS T) AS N WHERE [ROWNUMBER] BETWEEN {rowStart} AND {rowEnd};";
            }

            sqlQuery = SqlIntercept?.Invoke(sqlQuery, parameter) ?? sqlQuery;

            return sqlQuery;
        }
        #endregion

        #region Insert
        /// <summary>
        ///  插入单个实体 <para>注意：因为Oracle不支持自增列，所以我们需要使用序列(sequence)来实现自增列 </para>
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <param name="identity">是否返回自增主键值</param>
        /// <param name="identitySql">返回自增主键sql
        /// <list type="bullet">
        ///     <item>SqlServer: SELECT SCOPE_IDENTITY()</item>
        ///     <item>MySql: SELECT LAST_INSERT_ID()</item>
        ///     <item>Sqlite: SELECT LAST_INSERT_ROWID()</item>
        ///     <item>PostgreSql: RETURNING $PRIMARYKEY，其中$PRIMARYKEY为主键列名占位符</item>
        ///     <item>Oracle: SELECT $SEQUENCE.CURRVAL FROM DUAL，其中$SEQUENCE为自定义SEQUENCE名占位符</item>
        /// </list>
        /// </param>
        /// <returns>若 <paramref name="identity"/>为 true，则返回自增主键值，否则返回受影响行数</returns>
        public override long Insert<T>(T entity, bool identity, string identitySql = null) where T : class
        {
            if (!identity)
                return Insert(entity);

            identitySql ??= "SELECT SCOPE_IDENTITY()";

            var builder = Sql.Insert<T>(() => entity, DatabaseType, IsEnableNullValue, isEnableFormat: IsEnableFormat);

            return FindObject($"{builder.Sql};{identitySql.Trim(';')};", builder.DynamicParameters).To<long>();
        }

        /// <summary>
        ///  插入单个实体 <para>注意：因为Oracle不支持自增列，所以我们需要使用序列(sequence)来实现自增列 </para>
        /// </summary>
        /// <typeparam name="T">泛型类型</typeparam>
        /// <param name="entity">要插入的实体</param>
        /// <param name="identity">是否返回自增主键值</param>
        /// <param name="identitySql">返回自增主键sql
        /// <list type="bullet">
        ///     <item>SqlServer: SELECT SCOPE_IDENTITY()</item>
        ///     <item>MySql: SELECT LAST_INSERT_ID()</item>
        ///     <item>Sqlite: SELECT LAST_INSERT_ROWID()</item>
        ///     <item>PostgreSql: RETURNING $PRIMARYKEY，其中$PRIMARYKEY为主键列名占位符</item>
        ///     <item>Oracle: SELECT $SEQUENCE.CURRVAL FROM DUAL，其中$SEQUENCE为自定义SEQUENCE名占位符</item>
        /// </list>
        /// </param>
        /// <returns>若 <paramref name="identity"/>为 true，则返回自增主键值，否则返回受影响行数</returns>
        public override async Task<long> InsertAsync<T>(T entity, bool identity, string identitySql = null) where T : class
        {
            if (!identity)
                return await InsertAsync(entity);

            identitySql ??= "SELECT SCOPE_IDENTITY()";

            var builder = Sql.Insert<T>(() => entity, DatabaseType, IsEnableNullValue, isEnableFormat: IsEnableFormat);

            return (await FindObjectAsync($"{builder.Sql};{identitySql.Trim(';')};", builder.DynamicParameters)).To<long>();
        }
        #endregion
    }
}
