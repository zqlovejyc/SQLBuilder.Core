#region License
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

using Microsoft.Extensions.Configuration;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using System;
using System.Linq;
using System.Text.RegularExpressions;

namespace SQLBuilder.Core.Repositories
{
    /// <summary>
    /// PostgreSQL仓储实现类
    /// </summary>
    public class NpgsqlRepository : BaseRepository
    {
        #region Property
        /// <summary>
        /// 数据库类型
        /// </summary>
        public override DatabaseType DatabaseType => DatabaseType.PostgreSql;
        #endregion

        #region Constructor
        /// <summary>
        /// 构造函数
        /// </summary>
        public NpgsqlRepository() { }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="connectionString">主库连接字符串，或者链接字符串名称</param>
        /// <param name="configuration">数据库连接配置，默认：null，为null时则使用ConfigurationManager.Configuration</param>
        public NpgsqlRepository(string connectionString, IConfiguration configuration = null)
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
            var limit = pageSize;
            var offset = pageSize * (pageIndex - 1);

            //判断是否with语法
            if (isWithSyntax)
            {
                sqlQuery = $"{sql} SELECT {CountSyntax} AS \"TOTAL\" FROM T;";

                sqlQuery += $"{sql} SELECT * FROM T {orderField} LIMIT {limit} OFFSET {offset};";
            }
            else
            {
                sqlQuery = $"SELECT {CountSyntax} AS \"TOTAL\" FROM ({sql}) AS T;";

                sqlQuery += $"SELECT * FROM ({sql}) AS T {orderField} LIMIT {limit} OFFSET {offset};";
            }

            sqlQuery = SqlIntercept?.Invoke(sqlQuery, parameter) ?? sqlQuery;

            return sqlQuery;
        }
        #endregion
    }
}
