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

using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using Npgsql;
using Oracle.ManagedDataAccess.Client;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace SQLBuilder.Core.Extensions
{
    /// <summary>
    /// IDictionary扩展类
    /// </summary>
    public static class IDictionaryExtensions
    {
        #region ToDynamicParameters
        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter[] @this)
        {
            if (@this == null || @this.Length == 0)
                return null;

            var args = new DynamicParameters();
            @this.ToList().ForEach(p => args.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size));
            return args;
        }

        /// <summary>
        /// DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this List<DbParameter> @this)
        {
            if (@this == null || @this.Count == 0)
                return null;

            var args = new DynamicParameters();
            @this.ForEach(p => args.Add(p.ParameterName, p.Value, p.DbType, p.Direction, p.Size));
            return args;
        }

        /// <summary>
        ///  DbParameter转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this DbParameter @this)
        {
            if (@this == null)
                return null;

            var args = new DynamicParameters();
            args.Add(@this.ParameterName, @this.Value, @this.DbType, @this.Direction, @this.Size);
            return args;
        }

        /// <summary>
        ///  IDictionary转换为DynamicParameters
        /// </summary>
        /// <param name="this"></param>        
        /// <returns></returns>
        public static DynamicParameters ToDynamicParameters(this IDictionary<string, object> @this)
        {
            if (@this == null || @this.Count == 0)
                return null;

            var args = new DynamicParameters();
            foreach (var item in @this)
                args.Add(item.Key, item.Value);

            return args;
        }
        #endregion

        #region ToDbParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="command">The command.</param>        
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbCommand command)
        {
            if (@this == null || @this.Count == 0)
                return null;

            return @this.Select(x =>
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = x.Key;
                parameter.Value = x.Value;
                return parameter;
            }).ToArray();
        }

        /// <summary>
        ///  An IDictionary&lt;string,object&gt; extension method that converts this object to a database parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>
        /// <param name="connection">The connection.</param>        
        /// <returns>The given data converted to a DbParameter[].</returns>
        public static DbParameter[] ToDbParameters(this IDictionary<string, object> @this, DbConnection connection)
        {
            if (@this == null || @this.Count == 0)
                return null;

            var command = connection.CreateCommand();
            return @this.Select(x =>
            {
                var parameter = command.CreateParameter();
                parameter.ParameterName = x.Key;
                parameter.Value = x.Value;
                return parameter;
            }).ToArray();
        }
        #endregion

        #region ToSqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a SQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a SqlParameter[].</returns>
        public static SqlParameter[] ToSqlParameters(this IDictionary<string, object> @this)
        {
            if (@this == null || @this.Count == 0)
                return null;

            return @this.Select(x => new SqlParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
        }
        #endregion

        #region ToMySqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a MySQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a MySqlParameter[].</returns>
        public static MySqlParameter[] ToMySqlParameters(this IDictionary<string, object> @this)
        {
            if (@this == null || @this.Count == 0)
                return null;

            return @this.Select(x => new MySqlParameter(x.Key.Replace("@", "?").Replace(":", "?"), x.Value)).ToArray();
        }
        #endregion

        #region ToSqliteParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Sqlite parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a SqliteParameter[].</returns>
        public static SqliteParameter[] ToSqliteParameters(this IDictionary<string, object> @this)
        {
            if (@this == null || @this.Count == 0)
                return null;

            return @this.Select(x => new SqliteParameter(x.Key.Replace("?", "@").Replace(":", "@"), x.Value)).ToArray();
        }
        #endregion

        #region ToOracleParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a Oracle parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a OracleParameter[].</returns>
        public static OracleParameter[] ToOracleParameters(this IDictionary<string, object> @this)
        {
            if (@this == null || @this.Count == 0)
                return null;

            return @this.Select(x => new OracleParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
        }
        #endregion

        #region ToNpgsqlParameters
        /// <summary>
        /// An IDictionary&lt;string,object&gt; extension method that converts the @this to a PostgreSQL parameters.
        /// </summary>
        /// <param name="this">The @this to act on.</param>        
        /// <returns>@this as a NpgsqlParameter[].</returns>
        public static NpgsqlParameter[] ToNpgsqlParameters(this IDictionary<string, object> @this)
        {
            if (@this == null || @this.Count == 0)
                return null;

            return @this.Select(x => new NpgsqlParameter(x.Key.Replace("?", ":").Replace("@", ":"), x.Value)).ToArray();
        }
        #endregion
    }
}
