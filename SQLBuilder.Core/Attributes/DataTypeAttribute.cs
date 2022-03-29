#region License
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

using Microsoft.Data.Sqlite;
using MySqlConnector;
using NpgsqlTypes;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Data;

namespace SQLBuilder.Core.Attributes
{
    /// <summary>
    /// 数据库字段类型
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class DataTypeAttribute : Attribute
    {
        /// <summary>
        /// 是否为通用数据类型
        /// </summary>
        public bool IsDbType { get; set; }

        /// <summary>
        /// 通用数据类型
        /// </summary>
        public DbType DbType { get; set; }

        /// <summary>
        /// 是否为Oracle特定数据类型
        /// </summary>
        public bool IsOracleDbType { get; set; }

        /// <summary>
        /// Oracle特定数据类型
        /// </summary>
        public OracleDbType OracleDbType { get; set; }

        /// <summary>
        /// 是否为SqlServer特定数据类型
        /// </summary>
        public bool IsSqlDbType { get; set; }

        /// <summary>
        /// SqlServer特定数据类型
        /// </summary>
        public SqlDbType SqlDbType { get; set; }

        /// <summary>
        /// 是否为MySql特定数据类型
        /// </summary>
        public bool IsMySqlDbType { get; set; }

        /// <summary>
        /// MySql特定数据类型
        /// </summary>
        public MySqlDbType MySqlDbType { get; set; }

        /// <summary>
        /// 是否为Npgsql特定数据类型
        /// </summary>
        public bool IsNpgsqlDbType { get; set; }

        /// <summary>
        /// Npgsql特定数据类型
        /// </summary>
        public NpgsqlDbType NpgsqlDbType { get; set; }

        /// <summary>
        /// 是否为Sqlite特定数据类型
        /// </summary>
        public bool IsSqliteType { get; set; }

        /// <summary>
        /// Sqlite特定数据库类型
        /// </summary>
        public SqliteType SqliteType { get; set; }

        /// <summary>
        /// 是否固定长度 
        /// </summary>
        public bool IsFixedLength { get; set; }

        /// <summary>
        /// 固定长度
        /// </summary>
        public int FixedLength { get; set; }
    }
}
