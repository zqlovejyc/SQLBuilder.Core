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
using System.Data.Common;
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
        /// SQL语句
        /// </summary>
        public string Sql => this._sqlPack.ToString();

        /// <summary>
        /// SQL格式化参数
        /// </summary>
        public Dictionary<string, object> Parameters => this._sqlPack.DbParams;

        /// <summary>
        /// Dapper格式化参数
        /// </summary>
        public DynamicParameters DynamicParameters => this._sqlPack.DbParams.ToDynamicParameters();

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
                        parameters = this._sqlPack.DbParams.ToSqlParameters();
                        break;
                    case DatabaseType.MySQL:
                        parameters = this._sqlPack.DbParams.ToMySqlParameters();
                        break;
                    case DatabaseType.SQLite:
                        parameters = this._sqlPack.DbParams.ToSqliteParameters();
                        break;
                    case DatabaseType.Oracle:
                        parameters = this._sqlPack.DbParams.ToOracleParameters();
                        break;
                    case DatabaseType.PostgreSQL:
                        parameters = this._sqlPack.DbParams.ToNpgsqlParameters();
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
        public SqlBuilderCore(DatabaseType dbType)
        {
            this._sqlPack = new SqlPack
            {
                DatabaseType = dbType,
                DefaultType = typeof(T)
            };
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
        private string SelectParser(params Type[] array)
        {
            this._sqlPack.Clear();
            this._sqlPack.IsSingleTable = false;
            foreach (var item in array)
            {
                var tableName = this._sqlPack.GetTableName(item);
                this._sqlPack.SetTableAlias(tableName);
            }
            var _tableName = this._sqlPack.GetTableName(typeof(T));
            return "SELECT {0} FROM " + _tableName + " AS " + this._sqlPack.GetTableAlias(_tableName);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select(Expression<Func<T, object>> expression = null)
        {
            var sql = SelectParser(typeof(T));
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
            var sql = SelectParser(typeof(T), typeof(T2));
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
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3));
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
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4));
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
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
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
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6));
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
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7));
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
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8));
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
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9));
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
            var sql = SelectParser(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10));
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
        /// JoinParser
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="leftOrRightJoin">左连接或者右连接</param>
        /// <returns>SqlBuilderCore</returns>
        private SqlBuilderCore<T> JoinParser<T2>(Expression<Func<T, T2, bool>> expression, string leftOrRightJoin = "")
            where T2 : class
        {
            string joinTableName = this._sqlPack.GetTableName(typeof(T2));
            this._sqlPack.SetTableAlias(joinTableName);
            this._sqlPack.Sql.Append($"{(string.IsNullOrEmpty(leftOrRightJoin) ? "" : " " + leftOrRightJoin)} JOIN {(joinTableName + " AS " + this._sqlPack.GetTableAlias(joinTableName))} ON ");
            SqlBuilderProvider.Join(expression.Body, this._sqlPack);
            return this;
        }

        /// <summary>
        /// JoinParser2
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="leftOrRightJoin">左连接或者右连接</param>
        /// <returns>SqlBuilderCore</returns>
        private SqlBuilderCore<T> JoinParser2<T2, T3>(Expression<Func<T2, T3, bool>> expression, string leftOrRightJoin = "")
            where T2 : class
            where T3 : class
        {
            string joinTableName = this._sqlPack.GetTableName(typeof(T3));
            this._sqlPack.SetTableAlias(joinTableName);
            this._sqlPack.Sql.Append($"{(string.IsNullOrEmpty(leftOrRightJoin) ? "" : " " + leftOrRightJoin)} JOIN {(joinTableName + " AS " + this._sqlPack.GetTableAlias(joinTableName))} ON ");
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
            return JoinParser(expression);
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
            return JoinParser2(expression);
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
            return JoinParser(expression, "INNER");
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
            return JoinParser2(expression, "INNER");
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
            return JoinParser(expression, "LEFT");
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
            return JoinParser2(expression, "LEFT");
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
            return JoinParser(expression, "RIGHT");
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
            return JoinParser2(expression, "RIGHT");
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
            return JoinParser(expression, "FULL");
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
            return JoinParser2(expression, "FULL");
        }
        #endregion

        #region Where
        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Where(Expression<Func<T, bool>> expression)
        {
            if (!(expression.Body.NodeType == ExpressionType.Constant && expression.Body.ToObject() is bool b && b))
            {
                this._sqlPack += " WHERE ";
                SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            }
            return this;
        }
        #endregion

        #region AndWhere
        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AndWhere(Expression<Func<T, bool>> expression)
        {
            var sql = this._sqlPack.ToString();
            if (sql.Contains("WHERE") && !string.IsNullOrEmpty(sql.Substring("WHERE").Trim()))
            {
                this._sqlPack += " AND ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }
            SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region OrWhere
        /// <summary>
        /// OrWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrWhere(Expression<Func<T, bool>> expression)
        {
            var sql = this._sqlPack.ToString();
            if (sql.Contains("WHERE") && !string.IsNullOrEmpty(sql.Substring("WHERE").Trim()))
            {
                this._sqlPack += " OR ";
            }
            else
            {
                this._sqlPack += " WHERE ";
            }
            SqlBuilderProvider.Where(expression.Body, this._sqlPack);
            return this;
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
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            var (key, property) = this._sqlPack.GetPrimaryKey(typeof(T), string.IsNullOrEmpty(tableAlias));
            if (!string.IsNullOrEmpty(key) && entity != null)
            {
                var keyValue = typeof(T).GetProperty(property)?.GetValue(entity, null);
                if (keyValue != null)
                {
                    this._sqlPack += $" {(sql.Contains("WHERE") ? "AND" : "WHERE")} {(tableAlias + key)} = ";
                    this._sqlPack.AddDbParameter(keyValue);
                }
                else
                {
                    throw new ArgumentNullException("主键值不能为空！");
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
        public SqlBuilderCore<T> WithKey(dynamic keyValue)
        {
            if (keyValue == null)
            {
                throw new ArgumentNullException("keyValue不能为空！");
            }
            if (!(keyValue.GetType().IsValueType || keyValue.GetType() == typeof(string)))
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
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            var (key, property) = this._sqlPack.GetPrimaryKey(typeof(T), string.IsNullOrEmpty(tableAlias));
            if (!string.IsNullOrEmpty(key) && keyValue != null)
            {
                this._sqlPack += $" {(sql.Contains("WHERE") ? "AND" : "WHERE")} {(tableAlias + key)} = ";
                this._sqlPack.AddDbParameter(keyValue);
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
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy(Expression<Func<T, object>> expression)
        {
            this._sqlPack += " GROUP BY ";
            SqlBuilderProvider.GroupBy(expression.Body, this._sqlPack);
            return this;
        }
        #endregion

        #region OrderBy
        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy(Expression<Func<T, object>> expression, params OrderType[] orders)
        {
            this._sqlPack += " ORDER BY ";
            SqlBuilderProvider.OrderBy(expression.Body, this._sqlPack, orders);
            return this;
        }
        #endregion

        #region Page
        /// <summary>
        /// Page
        /// </summary>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="parameters">自定义sql格式化参数</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Page(int pageSize, int pageIndex, string orderBy, string sql = null, Dictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(sql))
            {
                this._sqlPack.DbParams.Clear();
                if (parameters != null) this._sqlPack.DbParams = parameters;
            }
            sql = string.IsNullOrEmpty(sql) ? this._sqlPack.Sql.ToString().TrimEnd(';') : sql.TrimEnd(';');
            //SQLServer、Oracle
            if (this._sqlPack.DatabaseType == DatabaseType.SQLServer || this._sqlPack.DatabaseType == DatabaseType.Oracle)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                {
                    sb.Append(sql);
                    sb.Append("SELECT COUNT(1) AS Records FROM T;");
                    sb.Append(sql);
                    sb.Append($",R AS (SELECT ROW_NUMBER() OVER (ORDER BY T.{orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};");
                }
                else
                {
                    sb.Append($"SELECT COUNT(1) AS Records FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER (ORDER BY X.{orderBy}) AS RowNumber,X.* FROM ({sql}) AS X) AS T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};");
                }
            }
            //MySQL、SQLite
            if (this._sqlPack.DatabaseType == DatabaseType.MySQL || this._sqlPack.DatabaseType == DatabaseType.SQLite)
            {
                sb.Append($"SELECT COUNT(1) AS Records FROM ({sql}) AS T;SELECT * FROM ({sql}) AS X ORDER BY X.{orderBy} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
            }
            this._sqlPack.Sql.Clear().Append(sb);
            return this;
        }

        /// <summary>
        /// PageByWith
        /// </summary>
        /// <param name="pageSize">每页数量</param>
        /// <param name="pageIndex">当前页码</param>
        /// <param name="orderBy">排序字段</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="parameters">自定义sql格式化参数</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> PageByWith(int pageSize, int pageIndex, string orderBy, string sql = null, Dictionary<string, object> parameters = null)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(sql))
            {
                this._sqlPack.DbParams.Clear();
                if (parameters != null) this._sqlPack.DbParams = parameters;
            }
            sql = string.IsNullOrEmpty(sql) ? this._sqlPack.Sql.ToString().TrimEnd(';') : sql.TrimEnd(';');
            //SQLServer、Oracle
            if (this._sqlPack.DatabaseType == DatabaseType.SQLServer || this._sqlPack.DatabaseType == DatabaseType.Oracle)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                {
                    sb.Append(sql);
                    sb.Append("SELECT COUNT(1) AS Records FROM T;");
                    sb.Append(sql);
                    sb.Append($",R AS (SELECT ROW_NUMBER() OVER (ORDER BY T.{orderBy}) AS RowNumber,T.* FROM T) SELECT * FROM R WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};");
                }
                else
                {
                    sb.Append($"WITH T AS ({sql}) SELECT COUNT(1) AS Records FROM T;WITH X AS ({sql}),T AS (SELECT ROW_NUMBER() OVER (ORDER BY X.{orderBy}) AS RowNumber,X.* FROM X) SELECT * FROM T WHERE RowNumber BETWEEN {(pageSize * (pageIndex - 1) + 1)} AND {(pageSize * pageIndex)};");
                }
            }
            //MySQL 8.0开始支持，8.0之前的版本不支持WITH语法、SQLite
            if (this._sqlPack.DatabaseType == DatabaseType.MySQL || this._sqlPack.DatabaseType == DatabaseType.SQLite)
            {
                if (Regex.IsMatch(sql, "WITH", RegexOptions.IgnoreCase))
                {
                    sb.Append(sql);
                    sb.Append("SELECT COUNT(1) AS Records FROM T;");
                    sb.Append(sql);
                    sb.Append($"SELECT * FROM T ORDER BY T.{orderBy} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                }
                else
                {
                    sb.Append($"WITH T AS ({sql}) SELECT COUNT(1) AS Records FROM T;WITH X AS ({sql}) SELECT * FROM X ORDER BY X.{orderBy} LIMIT {pageSize} OFFSET {(pageSize * (pageIndex - 1))};");
                }
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
            this._sqlPack += $"INSERT INTO {this._sqlPack.GetTableName(typeof(T))} ({{0}}) VALUES ";
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
            if (this._sqlPack.DatabaseType == DatabaseType.Oracle)
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
            if (this._sqlPack.DatabaseType == DatabaseType.MySQL || this._sqlPack.DatabaseType == DatabaseType.SQLite)
            {
                this._sqlPack.Sql.Append($" LIMIT 0,{topNumber}");
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

        #region GetTableKey
        /// <summary>
        /// 获取实体对应表的主键名称
        /// </summary>
        /// <returns></returns>
        public string GetPrimaryKey()
        {
            return this._sqlPack.GetPrimaryKey(typeof(T), false).key;
        }
        #endregion
        #endregion
    }
}