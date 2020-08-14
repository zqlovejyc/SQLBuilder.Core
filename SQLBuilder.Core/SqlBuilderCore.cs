#region License
/***
 * Copyright © 2018-2020, 张强 (943620963@qq.com).
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
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace SQLBuilder.Core
{
    /// <summary>
    /// SqlBuilderCore
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
	public class SqlBuilderCore<T> where T : class
    {
        #region Private Field
        /// <summary>
        /// _sqlPack
        /// </summary>
        private SqlPack _sqlPack;
        #endregion

        #region Public Property
        /// <summary>
        /// SQL拦截委托
        /// </summary>
        public Func<string, object, string> SqlIntercept { get; set; }

        /// <summary>
        /// SQL语句
        /// </summary>
        public string Sql
        {
            get
            {
                var sql = this._sqlPack.ToString();
                //添加sql日志拦截
                return this.SqlIntercept?.Invoke(sql, this._sqlPack.DbParameters) ?? sql;
            }
        }

        /// <summary>
        /// SQL格式化参数
        /// </summary>
        public Dictionary<string, object> Parameters => this._sqlPack.DbParameters;

        /// <summary>
        /// Dapper格式化参数
        /// </summary>
        public DynamicParameters DynamicParameters => this._sqlPack.DbParameters.ToDynamicParameters();

        /// <summary>
        /// SQL格式化参数
        /// </summary>
        public DbParameter[] DbParameters
        {
            get
            {
                DbParameter[] parameters = null;
                switch (this._sqlPack.DatabaseType)
                {
                    case DatabaseType.SQLServer:
                        parameters = this._sqlPack.DbParameters.ToSqlParameters();
                        break;
                    case DatabaseType.MySQL:
                        parameters = this._sqlPack.DbParameters.ToMySqlParameters();
                        break;
                    case DatabaseType.SQLite:
                        parameters = this._sqlPack.DbParameters.ToSqliteParameters();
                        break;
                    case DatabaseType.Oracle:
                        parameters = this._sqlPack.DbParameters.ToOracleParameters();
                        break;
                    case DatabaseType.PostgreSQL:
                        parameters = this._sqlPack.DbParameters.ToNpgsqlParameters();
                        break;
                }
                return parameters;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// SqlBuilderCore
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="isEnableFormat">是否启用表名和列名格式化</param>
        public SqlBuilderCore(DatabaseType dbType, bool isEnableFormat)
        {
            this._sqlPack = new SqlPack
            {
                DatabaseType = dbType,
                DefaultType = typeof(T),
                IsEnableFormat = isEnableFormat
            };
        }

        /// <summary>
        /// SqlBuilderCore
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="sqlIntercept">SQL拦截委托</param>
        /// <param name="isEnableFormat">是否启用表名和列名格式化</param>
        public SqlBuilderCore(DatabaseType dbType, Func<string, object, string> sqlIntercept, bool isEnableFormat)
        {
            this._sqlPack = new SqlPack
            {
                DatabaseType = dbType,
                DefaultType = typeof(T),
                IsEnableFormat = isEnableFormat
            };
            this.SqlIntercept = sqlIntercept;
        }
        #endregion

        #region Public Methods
        #region Clear
        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            this._sqlPack.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// SelectParser
        /// </summary>
        /// <param name="array">可变数量参数</param>
        /// <returns>string</returns>
        private string Select(params Type[] array)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = false;
            if (array?.Length > 0)
            {
                foreach (var item in array)
                {
                    var tableName = this._sqlPack.GetTableName(item);
                    this._sqlPack.SetTableAlias(tableName);
                }
            }
            var _tableName = this._sqlPack.GetTableName(typeof(T));
            //Oracle表别名不支持AS关键字，列别名支持；
            if (this._sqlPack.DatabaseType == DatabaseType.Oracle)
                return $"SELECT {{0}} FROM {_tableName} {this._sqlPack.GetTableAlias(_tableName)}";
            else
                return $"SELECT {{0}} FROM {_tableName} AS {this._sqlPack.GetTableAlias(_tableName)}";
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select(Expression expression = null)
        {
            var sql = this.Select(typeof(T));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select(Expression<Func<T, object>> expression = null)
        {
            var sql = this.Select(typeof(T));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2>(Expression<Func<T, T2, object>> expression = null)
            where T2 : class
        {
            var sql = this.Select(typeof(T), typeof(T2));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3>(Expression<Func<T, T2, T3, object>> expression = null)
            where T2 : class
            where T3 : class
        {
            var sql = this.Select(typeof(T), typeof(T2), typeof(T3));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4>(Expression<Func<T, T2, T3, T4, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            var sql = this.Select(typeof(T), typeof(T2), typeof(T3), typeof(T4));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            var sql = this.Select(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            var sql = this.Select(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            var sql = this.Select(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            var sql = this.Select(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            var sql = this.Select(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            var sql = this.Select(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
            if (expression == null)
            {
                this._sqlPack.Sql.AppendFormat(sql, "*");
            }
            else
            {
                SqlBuilderProvider.Select(expression.Body, this._sqlPack);
                this._sqlPack.Sql.AppendFormat(sql, this._sqlPack.SelectFieldsStr);
            }
            return this;
        }
        #endregion

        #region Join
        /// <summary>
        /// Join
        /// </summary>
        /// <param name="sql">自定义Join语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Join(string sql)
        {
            this._sqlPack += " JOIN ";
            this._sqlPack += sql;
            return this;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="sql">自定义Join语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Join(StringBuilder sql)
        {
            this._sqlPack += " JOIN ";
            this._sqlPack.Sql.Append(sql);
            return this;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="join">连接类型</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2>(Expression<Func<T, T2, bool>> expression, string join)
            where T2 : class
        {
            string joinTableName = this._sqlPack.GetTableName(typeof(T2));
            this._sqlPack.SetTableAlias(joinTableName);
            if (this._sqlPack.DatabaseType == DatabaseType.Oracle)
                this._sqlPack.Sql.Append($"{(join.IsNullOrEmpty() ? "" : " " + join)} JOIN {joinTableName} {this._sqlPack.GetTableAlias(joinTableName)} ON ");
            else
                this._sqlPack.Sql.Append($"{(join.IsNullOrEmpty() ? "" : " " + join)} JOIN {joinTableName} AS {this._sqlPack.GetTableAlias(joinTableName)} ON ");
            SqlBuilderProvider.Join(expression.Body, this._sqlPack);
            return this;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="join">连接类型</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3>(Expression<Func<T2, T3, bool>> expression, string join)
            where T2 : class
            where T3 : class
        {
            string joinTableName = this._sqlPack.GetTableName(typeof(T3));
            this._sqlPack.SetTableAlias(joinTableName);
            if (this._sqlPack.DatabaseType == DatabaseType.Oracle)
                this._sqlPack.Sql.Append($"{(join.IsNullOrEmpty() ? "" : " " + join)} JOIN {joinTableName} {this._sqlPack.GetTableAlias(joinTableName)} ON ");
            else
                this._sqlPack.Sql.Append($"{(join.IsNullOrEmpty() ? "" : " " + join)} JOIN {joinTableName} AS {this._sqlPack.GetTableAlias(joinTableName)} ON ");
            SqlBuilderProvider.Join(expression.Body, this._sqlPack);
            return this;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.Join<T2>(expression, "");
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "");
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <param name="sql">自定义InnerJoin语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> InnerJoin(string sql)
        {
            this._sqlPack += " INNER JOIN ";
            this._sqlPack += sql;
            return this;
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <param name="sql">自定义InnerJoin语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> InnerJoin(StringBuilder sql)
        {
            this._sqlPack += " INNER JOIN ";
            this._sqlPack.Sql.Append(sql);
            return this;
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.Join<T2>(expression, "INNER");
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "INNER");
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <param name="sql">自定义LeftJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin(string sql)
        {
            this._sqlPack += " LEFT JOIN ";
            this._sqlPack += sql;
            return this;
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <param name="sql">自定义LeftJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin(StringBuilder sql)
        {
            this._sqlPack += " LEFT JOIN ";
            this._sqlPack.Sql.Append(sql);
            return this;
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.Join<T2>(expression, "LEFT");
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "LEFT");
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <param name="sql">自定义RightJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin(string sql)
        {
            this._sqlPack += " RIGHT JOIN ";
            this._sqlPack += sql;
            return this;
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <param name="sql">自定义RightJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin(StringBuilder sql)
        {
            this._sqlPack += " RIGHT JOIN ";
            this._sqlPack.Sql.Append(sql);
            return this;
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.Join<T2>(expression, "RIGHT");
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "RIGHT");
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <param name="sql">自定义FullJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin(string sql)
        {
            this._sqlPack += " FULL JOIN ";
            this._sqlPack += sql;
            return this;
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <param name="sql">自定义FullJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin(StringBuilder sql)
        {
            this._sqlPack += " FULL JOIN ";
            this._sqlPack.Sql.Append(sql);
            return this;
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.Join<T2>(expression, "FULL");
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "FULL");
        }
        #endregion

        #region Where
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Where(string sql)
        {
            this._sqlPack += " WHERE ";
            this._sqlPack += sql;
            return this;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Where(string sql, ref bool hasWhere)
        {
            if (hasWhere)
                this._sqlPack += " AND ";
            else
            {
                this._sqlPack += " WHERE ";
                hasWhere = true;
            }

            this._sqlPack += sql;

            return this;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Where(StringBuilder sql)
        {
            this._sqlPack += " WHERE ";
            this._sqlPack.Sql.Append(sql);
            return this;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Where(StringBuilder sql, ref bool hasWhere)
        {
            if (hasWhere)
                this._sqlPack += " AND ";
            else
            {
                this._sqlPack += " WHERE ";
                hasWhere = true;
            }

            this._sqlPack.Sql.Append(sql);

            return this;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        public SqlBuilderCore<T> Where(Expression expression)
        {
            if (!(expression.NodeType == ExpressionType.Constant && expression.ToObject() is bool b && b))
            {
                this._sqlPack += " WHERE ";

                SqlBuilderProvider.Where(expression, this._sqlPack);
            }

            return this;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        public SqlBuilderCore<T> Where(Expression expression, ref bool hasWhere)
        {
            if (!(expression.NodeType == ExpressionType.Constant && expression.ToObject() is bool b && b))
            {
                if (hasWhere)
                    this._sqlPack += " AND ";
                else
                {
                    this._sqlPack += " WHERE ";
                    hasWhere = true;
                }

                SqlBuilderProvider.Where(expression, this._sqlPack);
            }

            return this;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where(Expression<Func<T, bool>> expression)
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where(Expression<Func<T, bool>> expression, ref bool hasWhere)
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2>(Expression<Func<T, T2, bool>> expression, ref bool hasWhere)
            where T2 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3>(Expression<Func<T, T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3>(Expression<Func<T, T2, T3, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.Where(expression.Body);
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.Where(expression.Body, ref hasWhere);
        }
        #endregion

        #region AndWhere
        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere(string sql)
        {
            var str = this._sqlPack.ToString();
            if (str.Contains("WHERE") && !str.Substring("WHERE").Trim().IsNullOrEmpty())
            {
                this._sqlPack += " AND ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }

            this._sqlPack += sql;

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere(string sql, ref bool hasWhere)
        {
            if (hasWhere)
                this._sqlPack += " AND ";
            else
            {
                this._sqlPack += " WHERE ";
                hasWhere = true;
            }

            this._sqlPack += sql;

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere(StringBuilder sql)
        {
            var str = this._sqlPack.ToString();
            if (str.Contains("WHERE") && !str.Substring("WHERE").Trim().IsNullOrEmpty())
            {
                this._sqlPack += " AND ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }

            this._sqlPack.Sql.Append(sql);

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere(StringBuilder sql, ref bool hasWhere)
        {
            if (hasWhere)
                this._sqlPack += " AND ";
            else
            {
                this._sqlPack += " WHERE ";
                hasWhere = true;
            }

            this._sqlPack.Sql.Append(sql);

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere(Expression expression)
        {
            var sql = this._sqlPack.ToString();
            if (sql.Contains("WHERE") && !sql.Substring("WHERE").Trim().IsNullOrEmpty())
            {
                this._sqlPack += " AND ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }

            this._sqlPack += "(";
            SqlBuilderProvider.Where(expression, this._sqlPack);
            this._sqlPack += ")";

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere(Expression expression, ref bool hasWhere)
        {
            if (hasWhere)
                this._sqlPack += " AND ";
            else
            {
                this._sqlPack += " WHERE ";
                hasWhere = true;
            }

            this._sqlPack += "(";
            SqlBuilderProvider.Where(expression, this._sqlPack);
            this._sqlPack += ")";

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere(Expression<Func<T, bool>> expression)
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere(Expression<Func<T, bool>> expression, ref bool hasWhere)
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2>(Expression<Func<T, T2, bool>> expression, ref bool hasWhere)
            where T2 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3>(Expression<Func<T, T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3>(Expression<Func<T, T2, T3, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.AndWhere(expression.Body);
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.AndWhere(expression.Body, ref hasWhere);
        }
        #endregion

        #region OrWhere
        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> OrWhere(string sql)
        {
            var str = this._sqlPack.ToString();
            if (str.Contains("WHERE") && !str.Substring("WHERE").Trim().IsNullOrEmpty())
            {
                this._sqlPack += " OR ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }

            this._sqlPack += sql;

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> OrWhere(string sql, ref bool hasWhere)
        {
            if (hasWhere)
                this._sqlPack += " OR ";
            else
            {
                this._sqlPack += " WHERE ";
                hasWhere = true;
            }

            this._sqlPack += sql;

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> OrWhere(StringBuilder sql)
        {
            var str = this._sqlPack.ToString();
            if (str.Contains("WHERE") && !str.Substring("WHERE").Trim().IsNullOrEmpty())
            {
                this._sqlPack += " OR ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }

            this._sqlPack.Sql.Append(sql);

            return this;
        }


        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> OrWhere(StringBuilder sql, ref bool hasWhere)
        {
            if (hasWhere)
                this._sqlPack += " OR ";
            else
            {
                this._sqlPack += " WHERE ";
                hasWhere = true;
            }

            this._sqlPack.Sql.Append(sql);

            return this;
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        public SqlBuilderCore<T> OrWhere(Expression expression)
        {
            var sql = this._sqlPack.ToString();
            if (sql.Contains("WHERE") && !sql.Substring("WHERE").Trim().IsNullOrEmpty())
            {
                this._sqlPack += " OR ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }

            this._sqlPack += "(";
            SqlBuilderProvider.Where(expression, this._sqlPack);
            this._sqlPack += ")";

            return this;
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        public SqlBuilderCore<T> OrWhere(Expression expression, ref bool hasWhere)
        {
            if (hasWhere)
                this._sqlPack += " OR ";
            else
            {
                this._sqlPack += " WHERE ";
                hasWhere = true;
            }

            this._sqlPack += "(";
            SqlBuilderProvider.Where(expression, this._sqlPack);
            this._sqlPack += ")";

            return this;
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere(Expression<Func<T, bool>> expression)
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere(Expression<Func<T, bool>> expression, ref bool hasWhere)
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2>(Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2>(Expression<Func<T, T2, bool>> expression, ref bool hasWhere)
            where T2 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3>(Expression<Func<T, T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3>(Expression<Func<T, T2, T3, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.OrWhere(expression.Body);
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.OrWhere(expression.Body, ref hasWhere);
        }
        #endregion

        #region WhereIf
        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, string sql)
        {
            if (condition)
                this.AndWhere(sql);

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, string sql, Action callback)
        {
            if (condition)
            {
                this.AndWhere(sql);

                callback?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, string sql, ref bool hasWhere)
        {
            if (condition)
                this.AndWhere(sql, ref hasWhere);

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, string sql, ref bool hasWhere, Action callback)
        {
            if (condition)
            {
                this.AndWhere(sql, ref hasWhere);

                callback?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, StringBuilder sql)
        {
            if (condition)
                this.AndWhere(sql);

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, StringBuilder sql, Action callback)
        {
            if (condition)
            {
                this.AndWhere(sql);

                callback?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, StringBuilder sql, ref bool hasWhere)
        {
            if (condition)
                this.AndWhere(sql, ref hasWhere);

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, StringBuilder sql, ref bool hasWhere, Action callback)
        {
            if (condition)
            {
                this.AndWhere(sql, ref hasWhere);

                callback?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, Expression expression)
        {
            if (condition)
                this.AndWhere(expression);

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, Expression expression, Action callback)
        {
            if (condition)
            {
                this.AndWhere(expression);

                callback?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, Expression expression, ref bool hasWhere)
        {
            if (condition)
                this.AndWhere(expression, ref hasWhere);

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, Expression expression, ref bool hasWhere, Action callback)
        {
            if (condition)
            {
                this.AndWhere(expression, ref hasWhere);

                callback?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, Expression<Func<T, bool>> expression)
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, Expression<Func<T, bool>> expression, Action callback)
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, Expression<Func<T, bool>> expression, ref bool hasWhere)
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf(bool condition, Expression<Func<T, bool>> expression, ref bool hasWhere, Action callback)
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2>(bool condition, Expression<Func<T, T2, bool>> expression)
            where T2 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2>(bool condition, Expression<Func<T, T2, bool>> expression, Action callback)
            where T2 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2>(bool condition, Expression<Func<T, T2, bool>> expression, ref bool hasWhere)
            where T2 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2>(bool condition, Expression<Func<T, T2, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3>(bool condition, Expression<Func<T, T2, T3, bool>> expression)
            where T2 : class
            where T3 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3>(bool condition, Expression<Func<T, T2, T3, bool>> expression, Action callback)
            where T2 : class
            where T3 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3>(bool condition, Expression<Func<T, T2, T3, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3>(bool condition, Expression<Func<T, T2, T3, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
            where T3 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4>(bool condition, Expression<Func<T, T2, T3, T4, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4>(bool condition, Expression<Func<T, T2, T3, T4, bool>> expression, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4>(bool condition, Expression<Func<T, T2, T3, T4, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4>(bool condition, Expression<Func<T, T2, T3, T4, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5>(bool condition, Expression<Func<T, T2, T3, T4, T5, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5>(bool condition, Expression<Func<T, T2, T3, T4, T5, bool>> expression, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5>(bool condition, Expression<Func<T, T2, T3, T4, T5, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5>(bool condition, Expression<Func<T, T2, T3, T4, T5, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8, T9>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.WhereIf(condition, expression.Body);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.WhereIf(condition, expression.Body, callback);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression, ref bool hasWhere)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere);
        }

        /// <summary>
        /// WhereIf
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="condition">条件</param>
        /// <param name="expression">表达式树</param>
        /// <param name="hasWhere">指定是否已包含where关键字</param>
        /// <param name="callback">当条件满足时，执行完拼接后回调委托</param>
        /// <returns></returns>
        public SqlBuilderCore<T> WhereIf<T2, T3, T4, T5, T6, T7, T8, T9, T10>(bool condition, Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, bool>> expression, ref bool hasWhere, Action callback)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.WhereIf(condition, expression.Body, ref hasWhere, callback);
        }
        #endregion

        #region WithKey
        /// <summary>
        /// 添加主键条件，主要针对更新实体和删除实体操作
        /// </summary>
        /// <param name="entity">实体对象</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> WithKey(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("实体参数不能为空！");
            }
            var sql = this._sqlPack.ToString().ToUpper();
            if (!sql.Contains("SELECT") && !sql.Contains("UPDATE") && !sql.Contains("DELETE"))
            {
                throw new ArgumentException("此方法只能用于Select、Update、Delete方法！");
            }
            var tableName = this._sqlPack.GetTableName(typeof(T));
            var tableAlias = this._sqlPack.GetTableAlias(tableName);
            if (!tableAlias.IsNullOrEmpty()) tableAlias += ".";
            var keys = this._sqlPack.GetPrimaryKey(typeof(T));
            if (keys.Count > 0 && entity != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    var (key, property) = keys[i];
                    if (!key.IsNullOrEmpty())
                    {
                        var keyValue = typeof(T).GetProperty(property)?.GetValue(entity, null);
                        if (keyValue != null)
                        {
                            this._sqlPack += $" {(sql.Contains("WHERE") || i > 0 ? "AND" : "WHERE")} {(tableAlias + key)} = ";
                            this._sqlPack.AddDbParameter(keyValue);
                        }
                        else
                        {
                            throw new ArgumentNullException("主键值不能为空！");
                        }
                    }
                }
            }
            else
            {
                throw new ArgumentException("实体不存在Key属性！");
            }
            return this;
        }

        /// <summary>
        /// 添加主键条件，主要针对更新实体和删除实体操作
        /// </summary>
        /// <param name="keyValue">主键值</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> WithKey(params dynamic[] keyValue)
        {
            if (keyValue == null)
            {
                throw new ArgumentNullException("keyValue不能为空！");
            }
            if (!keyValue.Any(o => o.GetType().IsValueType || o.GetType() == typeof(string)))
            {
                throw new ArgumentException("keyValue只能为值类型或者字符串类型数据！");
            }
            var sql = this._sqlPack.ToString().ToUpper();
            if (!sql.Contains("SELECT") && !sql.Contains("UPDATE") && !sql.Contains("DELETE"))
            {
                throw new ArgumentException("WithKey方法只能用于Select、Update、Delete方法！");
            }
            var tableName = this._sqlPack.GetTableName(typeof(T));
            var tableAlias = this._sqlPack.GetTableAlias(tableName);
            if (!tableAlias.IsNullOrEmpty()) tableAlias += ".";
            var keys = this._sqlPack.GetPrimaryKey(typeof(T));
            if (keys.Count > 0 && keyValue != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    var (key, property) = keys[i];
                    if (!key.IsNullOrEmpty())
                    {
                        this._sqlPack += $" {(sql.Contains("WHERE") || i > 0 ? "AND" : "WHERE")} {(tableAlias + key)} = ";
                        this._sqlPack.AddDbParameter(keyValue[i]);
                    }
                }
            }
            else
            {
                throw new ArgumentException("实体不存在Key属性！");
            }
            return this;
        }
        #endregion

        #region GroupBy
        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy(string sql)
        {
            this._sqlPack += " GROUP BY ";
            this._sqlPack += sql;
            return this;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy(StringBuilder sql)
        {
            this._sqlPack += " GROUP BY ";
            this._sqlPack.Sql.Append(sql);
            return this;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy(Expression expression)
        {
            this._sqlPack += " GROUP BY ";
            SqlBuilderProvider.GroupBy(expression, this._sqlPack);
            return this;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy(Expression<Func<T, object>> expression)
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2>(Expression<Func<T, T2, object>> expression)
            where T2 : class
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2, T3>(Expression<Func<T, T2, T3, object>> expression)
            where T2 : class
            where T3 : class
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2, T3, T4>(Expression<Func<T, T2, T3, T4, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.GroupBy(expression.Body);
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.GroupBy(expression.Body);
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy(string sql)
        {
            this._sqlPack += " ORDER BY ";
            this._sqlPack += sql;
            return this;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy(StringBuilder sql)
        {
            this._sqlPack += " ORDER BY ";
            this._sqlPack.Sql.Append(sql);
            return this;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy(Expression expression, params OrderType[] orders)
        {
            this._sqlPack += " ORDER BY ";
            SqlBuilderProvider.OrderBy(expression, this._sqlPack, orders);
            return this;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy(Expression<Func<T, object>> expression, params OrderType[] orders)
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2>(Expression<Func<T, T2, object>> expression, params OrderType[] orders)
            where T2 : class
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2, T3>(Expression<Func<T, T2, T3, object>> expression, params OrderType[] orders)
            where T2 : class
            where T3 : class
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2, T3, T4>(Expression<Func<T, T2, T3, T4, object>> expression, params OrderType[] orders)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, object>> expression, params OrderType[] orders)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression, params OrderType[] orders)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression, params OrderType[] orders)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression, params OrderType[] orders)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression, params OrderType[] orders)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.OrderBy(expression.Body, orders);
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <typeparam name="T8">泛型类型8</typeparam>
        /// <typeparam name="T9">泛型类型9</typeparam>
        /// <typeparam name="T10">泛型类型10</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression, params OrderType[] orders)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
            where T10 : class
        {
            return this.OrderBy(expression.Body, orders);
        }
        #endregion

        #region Page
        /// <summary>
        /// Page
        /// </summary>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="parameters">自定义sql格式化参数</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Page(int pageSize, int pageIndex, string orderField, string sql = null, Dictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder();
            if (!orderField.ToUpper().Contains(" ASC") && !orderField.ToUpper().Contains(" DESC"))
                orderField = this._sqlPack.GetColumnName(orderField);
            if (!sql.IsNullOrEmpty())
            {
                this._sqlPack.DbParameters.Clear();
                if (parameters != null) this._sqlPack.DbParameters = parameters;
            }
            sql = sql.IsNullOrEmpty() ? this._sqlPack.Sql.ToString().TrimEnd(';') : sql.TrimEnd(';');
            //SQLServer
            if (this._sqlPack.DatabaseType == DatabaseType.SQLServer)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;{sql} SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY {orderField}) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN {((pageIndex - 1) * pageSize + 1)} AND {(pageIndex * pageSize)};DROP TABLE #TEMPORARY;");
                else
                    sb.Append($"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;SELECT * INTO #TEMPORARY FROM ({sql}) AS T;SELECT COUNT(1) AS Total FROM #TEMPORARY;SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY {orderField}) AS RowNumber, * FROM #TEMPORARY) AS N WHERE RowNumber BETWEEN {((pageIndex - 1) * pageSize + 1)} AND {(pageIndex * pageSize)};DROP TABLE #TEMPORARY;");
            }
            //Oracle，注意Oracle需要分开查询总条数和分页数据
            if (this._sqlPack.DatabaseType == DatabaseType.Oracle)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql},R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
                else
                    sb.Append($"SELECT * FROM (SELECT X.*,ROWNUM AS RowNumber FROM ({sql} ORDER BY {orderField}) X WHERE ROWNUM <= {pageSize * pageIndex}) T WHERE T.RowNumber >= {pageSize * (pageIndex - 1) + 1}");
            }
            //MySQL，注意8.0版本才支持WITH语法
            if (this._sqlPack.DatabaseType == DatabaseType.MySQL)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"DROP TEMPORARY TABLE IF EXISTS $TEMPORARY;CREATE TEMPORARY TABLE $TEMPORARY SELECT * FROM ({sql}) AS T;SELECT COUNT(1) AS Total FROM $TEMPORARY;SELECT * FROM $TEMPORARY AS X ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};DROP TABLE $TEMPORARY;");
            }
            //PostgreSQL
            if (this._sqlPack.DatabaseType == DatabaseType.PostgreSQL)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"DROP TABLE IF EXISTS TEMPORARY_TABLE;CREATE TEMPORARY TABLE TEMPORARY_TABLE AS SELECT * FROM ({sql}) AS T;SELECT COUNT(1) AS Total FROM TEMPORARY_TABLE;SELECT * FROM TEMPORARY_TABLE AS X ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};DROP TABLE TEMPORARY_TABLE;");
            }
            //SQLite
            if (this._sqlPack.DatabaseType == DatabaseType.SQLite)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"SELECT COUNT(1) AS Total FROM ({sql}) AS T;SELECT * FROM ({sql}) AS X ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            this._sqlPack.Sql.Clear().Append(sb);
            return this;
        }

        /// <summary>
        /// PageByWith
        /// </summary>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="orderField">排序字段</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="parameters">自定义sql格式化参数</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> PageByWith(int pageSize, int pageIndex, string orderField, string sql = null, Dictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder();
            if (!orderField.ToUpper().Contains(" ASC") && !orderField.ToUpper().Contains(" DESC"))
                orderField = this._sqlPack.GetColumnName(orderField);
            if (!sql.IsNullOrEmpty())
            {
                this._sqlPack.DbParameters.Clear();
                if (parameters != null) this._sqlPack.DbParameters = parameters;
            }
            sql = sql.IsNullOrEmpty() ? this._sqlPack.Sql.ToString().TrimEnd(';') : sql.TrimEnd(';');
            //SQLServer
            if (this._sqlPack.DatabaseType == DatabaseType.SQLServer)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;{sql} SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY {orderField}) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN {((pageIndex - 1) * pageSize + 1)} AND {(pageIndex * pageSize)};DROP TABLE #TEMPORARY;");
                else
                    sb.Append($"IF OBJECT_ID(N'TEMPDB..#TEMPORARY') IS NOT NULL DROP TABLE #TEMPORARY;WITH T AS ({sql}) SELECT * INTO #TEMPORARY FROM T;SELECT COUNT(1) AS Total FROM #TEMPORARY;WITH R AS (SELECT ROW_NUMBER() OVER (ORDER BY {orderField}) AS RowNumber,* FROM #TEMPORARY) SELECT * FROM R  WHERE RowNumber BETWEEN {((pageIndex - 1) * pageSize + 1)} AND {(pageIndex * pageSize)};DROP TABLE #TEMPORARY;");
            }
            //Oracle，注意Oracle需要分开查询总条数和分页数据
            if (this._sqlPack.DatabaseType == DatabaseType.Oracle)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql},R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
                else
                    sb.Append($"WITH T AS ({sql}),R AS (SELECT ROWNUM AS RowNumber,T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex} ORDER BY {orderField}) SELECT * FROM R WHERE RowNumber>={pageSize * (pageIndex - 1) + 1}");
            }
            //MySQL，注意8.0版本才支持WITH语法
            if (this._sqlPack.DatabaseType == DatabaseType.MySQL)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"WITH T AS ({sql}) SELECT COUNT(1) AS Total FROM T;WITH T AS ({sql}) SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            //PostgreSQL
            if (this._sqlPack.DatabaseType == DatabaseType.PostgreSQL)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"WITH T AS ({sql}) SELECT COUNT(1) AS Total FROM T;WITH T AS ({sql}) SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            //SQLite
            if (this._sqlPack.DatabaseType == DatabaseType.SQLite)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                    sb.Append($"{sql} SELECT COUNT(1) AS Total FROM T;{sql} SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                else
                    sb.Append($"WITH T AS ({sql}) SELECT COUNT(1) AS Total FROM T;WITH T AS ({sql}) SELECT * FROM T ORDER BY {orderField} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            this._sqlPack.Sql.Clear().Append(sb);
            return this;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete
        /// </summary>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Delete()
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            this._sqlPack += $"DELETE FROM {this._sqlPack.GetTableName(typeof(T))}";
            return this;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：是</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Update(Expression<Func<object>> expression = null, bool isEnableNullValue = true)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            this._sqlPack.IsEnableNullValue = isEnableNullValue;
            this._sqlPack += $"UPDATE {this._sqlPack.GetTableName(typeof(T))} SET ";
            SqlBuilderProvider.Update(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：是</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Insert(Expression<Func<object>> expression = null, bool isEnableNullValue = true)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            this._sqlPack.IsEnableNullValue = isEnableNullValue;
            this._sqlPack += $"INSERT INTO {this._sqlPack.GetTableName(typeof(T))} ({{0}}) {(this._sqlPack.DatabaseType == DatabaseType.Oracle ? "SELECT" : "VALUES")} ";
            SqlBuilderProvider.Insert(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Max
        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Max(Expression<Func<T, object>> expression)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            SqlBuilderProvider.Max(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Min
        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Min(Expression<Func<T, object>> expression)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            SqlBuilderProvider.Min(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Avg
        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Avg(Expression<Func<T, object>> expression)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            SqlBuilderProvider.Avg(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Count(Expression<Func<T, object>> expression = null)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            if (expression == null)
            {
                this._sqlPack.Sql.Append($"SELECT COUNT(*) FROM {this._sqlPack.GetTableName(typeof(T))}");
            }
            else
            {
                SqlBuilderProvider.Count(expression.Body, this._sqlPack);
            }
            return this;
        }
        #endregion

        #region Sum
        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Sum(Expression<Func<T, object>> expression)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = true;
            SqlBuilderProvider.Sum(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region Top
        /// <summary>
        /// Top
        /// </summary>
        /// <param name="topNumber">top数量</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Top(long topNumber)
        {
            if (this._sqlPack.DatabaseType == DatabaseType.SQLServer)
            {
                if (this._sqlPack.Sql.ToString().ToUpper().Contains("DISTINCT"))
                {
                    this._sqlPack.Sql.Replace("DISTINCT", $"DISTINCT TOP {topNumber}", this._sqlPack.Sql.ToString().IndexOf("DISTINCT"), 8);
                }
                else
                {
                    this._sqlPack.Sql.Replace("SELECT", $"SELECT TOP {topNumber}", this._sqlPack.Sql.ToString().IndexOf("SELECT"), 6);
                }
            }
            else if (this._sqlPack.DatabaseType == DatabaseType.Oracle)
            {
                if (this._sqlPack.Sql.ToString().ToUpper().Contains("WHERE"))
                {
                    this._sqlPack.Sql.Append($" AND ROWNUM <= {topNumber}");
                }
                else
                {
                    this._sqlPack.Sql.Append($" WHERE ROWNUM <= {topNumber}");
                }
            }
            else if (this._sqlPack.DatabaseType == DatabaseType.MySQL || this._sqlPack.DatabaseType == DatabaseType.SQLite || this._sqlPack.DatabaseType == DatabaseType.PostgreSQL)
            {
                this._sqlPack.Sql.Append($" LIMIT {topNumber} OFFSET 0");
            }
            return this;
        }
        #endregion

        #region Distinct
        /// <summary>
        /// Distinct
        /// </summary>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Distinct()
        {
            this._sqlPack.Sql.Replace("SELECT", $"SELECT DISTINCT", this._sqlPack.Sql.ToString().IndexOf("SELECT"), 6);
            return this;
        }
        #endregion

        #region GetTableName
        /// <summary>
        /// 获取实体对应的表名
        /// </summary>
        /// <returns></returns>
        public string GetTableName()
        {
            return this._sqlPack.GetTableName(typeof(T));
        }
        #endregion

        #region GetPrimaryKey
        /// <summary>
        /// 获取实体对应表的主键名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetPrimaryKey()
        {
            return this._sqlPack.GetPrimaryKey(typeof(T)).Select(o => o.key).ToList();
        }
        #endregion
        #endregion
    }
}