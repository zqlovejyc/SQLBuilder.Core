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
using SQLBuilder.Core.Enums;
using System;
using System.Collections.Generic;
using System.Data.Common;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// FormattableString扩展类
    /// </summary>
    public static class FormattableStringExtensions
    {
        /// <summary>
        /// sql参数化
        /// </summary>
        /// <param name="sql">内插sql字符串</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns></returns>
        public static (string sqlFormat, DbParameter[] parameters) ToDbParameter(this FormattableString sql, DatabaseType databaseType)
        {
            var (sqlFormat, parameter) = sql.ToParameter(databaseType);

            return (sqlFormat, databaseType switch
            {
                DatabaseType.SqlServer => parameter.ToSqlParameters(),
                DatabaseType.MySql => parameter.ToMySqlParameters(),
                DatabaseType.Sqlite => parameter.ToSqliteParameters(),
                DatabaseType.Oracle => parameter.ToOracleParameters(),
                DatabaseType.PostgreSql => parameter.ToNpgsqlParameters(),
                _ => null
            });
        }

        /// <summary>
        /// sql参数化
        /// </summary>
        /// <param name="sql">内插sql字符串</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns></returns>
        public static (string sqlFormat, DynamicParameters parameter) ToDynamicParameter(this FormattableString sql, DatabaseType databaseType)
        {
            var (sqlFormat, parameter) = sql.ToParameter(databaseType);

            return (sqlFormat, parameter.ToDynamicParameters());
        }

        /// <summary>
        /// sql参数化
        /// </summary>
        /// <param name="sql">内插sql字符串</param>
        /// <param name="databaseType">数据库类型</param>
        /// <returns></returns>
        public static (string sqlFormat, Dictionary<string, object> parameter) ToParameter(this FormattableString sql, DatabaseType databaseType)
        {
            if (sql == null)
                throw new ArgumentNullException(nameof(sql));

            var sqlFormat = sql.Format;
            var parameter = new Dictionary<string, object>();
            var arguments = sql.GetArguments();

            var prefix = databaseType switch
            {
                DatabaseType.Sqlite => "@",
                DatabaseType.SqlServer => "@",
                DatabaseType.MySql => "?",
                DatabaseType.Oracle => ":",
                DatabaseType.PostgreSql => ":",
                _ => "",
            };

            for (int i = 0; i < sql.ArgumentCount; i++)
            {
                var pName = $"{prefix}p__{i + 1}";

                sqlFormat = sqlFormat.Replace($"{{{i}}}", pName);

                parameter[pName] = arguments[i];
            }

            return (sqlFormat, parameter);
        }
    }
}
