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
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Expressions;
using SQLBuilder.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;

namespace SQLBuilder.Core.Entry
{
    /// <summary>
    /// SqlBuilderCore
    /// </summary>
    /// <typeparam name="T">泛型类型</typeparam>
	public class SqlBuilderCore<T> where T : class
    {
        #region Private Field
        /// <summary>
        /// sqlWrapper
        /// </summary>
        private SqlWrapper sqlWrapper;
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
                var sql = this.sqlWrapper.ToString();

                //添加sql日志拦截
                return this.SqlIntercept?.Invoke(sql, this.sqlWrapper.DbParameters) ?? sql;
            }
        }

        /// <summary>
        /// SQL格式化参数
        /// </summary>
        public Dictionary<string, object> Parameters => this.sqlWrapper.DbParameters;

        /// <summary>
        /// Dapper格式化参数
        /// </summary>
        public DynamicParameters DynamicParameters => this.sqlWrapper.DbParameters.ToDynamicParameters();

        /// <summary>
        /// SQL格式化参数
        /// </summary>
        public DbParameter[] DbParameters =>
            this.sqlWrapper.DatabaseType switch
            {
                DatabaseType.SqlServer => this.sqlWrapper.DbParameters.ToSqlParameters(),
                DatabaseType.MySql => this.sqlWrapper.DbParameters.ToMySqlParameters(),
                DatabaseType.Sqlite => this.sqlWrapper.DbParameters.ToSqliteParameters(),
                DatabaseType.Oracle => this.sqlWrapper.DbParameters.ToOracleParameters(),
                DatabaseType.PostgreSql => this.sqlWrapper.DbParameters.ToNpgsqlParameters(),
                _ => null
            };
        #endregion

        #region Constructor
        /// <summary>
        /// SqlBuilderCore
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="isEnableFormat">是否启用表名和列名格式化</param>
        public SqlBuilderCore(DatabaseType dbType, bool isEnableFormat)
        {
            this.sqlWrapper = new SqlWrapper
            {
                DatabaseType = dbType,
                DefaultType = typeof(T),
                IsEnableFormat = isEnableFormat
            };

            this.sqlWrapper.AddJoinType(typeof(T));
        }

        /// <summary>
        /// SqlBuilderCore
        /// </summary>
        /// <param name="dbType">数据库类型</param>
        /// <param name="sqlIntercept">SQL拦截委托</param>
        /// <param name="isEnableFormat">是否启用表名和列名格式化</param>
        public SqlBuilderCore(DatabaseType dbType, Func<string, object, string> sqlIntercept, bool isEnableFormat)
        {
            this.sqlWrapper = new SqlWrapper
            {
                DatabaseType = dbType,
                DefaultType = typeof(T),
                IsEnableFormat = isEnableFormat
            };

            this.SqlIntercept = sqlIntercept;
            this.sqlWrapper.AddJoinType(typeof(T));
        }
        #endregion

        #region Public Methods
        #region Clear
        /// <summary>
        /// Clear
        /// </summary>
        public void Clear()
        {
            this.sqlWrapper.Clear();
        }
        #endregion

        #region Select
        /// <summary>
        /// 获取自定义别名及其对应类型
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="types">类型集合</param>
        /// <returns></returns>
        private (Type type, string alias)[] GetExpressionAlias(Expression expression, params Type[] types)
        {
            var list = new List<(Type type, string alias)>();

            if (expression != null && types?.Length > 0)
            {
                if (expression is NewExpression newExpression && newExpression.Arguments?.Count > 0)
                {
                    foreach (var item in newExpression.Arguments)
                    {
                        if (item.NodeType == ExpressionType.MemberAccess)
                        {
                            if (item is MemberExpression memberExpr && memberExpr.Expression is ParameterExpression parameterExpr && types.Any(x => x == parameterExpr.Type))
                                list.Add((parameterExpr.Type, parameterExpr.Name));
                        }
                        else if (item.NodeType == ExpressionType.Parameter)
                        {
                            if (item is ParameterExpression parameterExpr && types.Any(x => x == parameterExpr.Type))
                                list.Add((parameterExpr.Type, parameterExpr.Name));
                        }
                    }
                }

                else if (expression is LambdaExpression lambdaExpression && lambdaExpression.Parameters?.Count > 0)
                {
                    foreach (var item in lambdaExpression.Parameters)
                    {
                        if (item is ParameterExpression parameterExpr && types.Any(x => x == parameterExpr.Type))
                            list.Add((parameterExpr.Type, parameterExpr.Name));
                    }
                }

                else if (expression is ParameterExpression parameterExpression)
                {
                    if (types.Any(x => x == parameterExpression.Type))
                        list.Add((parameterExpression.Type, parameterExpression.Name));
                }

                else if (expression is UnaryExpression unaryExpression)
                {
                    if (unaryExpression.Operand is MemberExpression memberExpr && memberExpr.Expression is ParameterExpression parameterExpr)
                    {
                        if (types.Any(x => x == parameterExpr.Type))
                            list.Add((parameterExpr.Type, parameterExpr.Name));
                    }
                }

                else if (expression is MemberExpression memberExpression)
                {
                    if (memberExpression.Expression is ParameterExpression parameterExpr && types.Any(x => x == parameterExpr.Type))
                        list.Add((parameterExpr.Type, parameterExpr.Name));
                }

                else if (expression is ConstantExpression constantExpression)
                {
                    list.Add((types[0], null));
                }

                else if (expression is MemberInitExpression memberInitExpression)
                {
                    foreach (MemberAssignment ma in memberInitExpression.Bindings)
                    {
                        if (ma.Expression is MemberExpression memberExpr && memberExpr?.Expression is ParameterExpression parameterExpr)
                        {
                            if (types.Any(x => x == parameterExpr.Type))
                                list.Add((parameterExpr.Type, parameterExpr.Name));
                        }
                    }
                }
            }

            return list.Distinct().ToArray();
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <param name="array">可变数量参数</param>
        /// <returns>string</returns>
        private string Select(Func<string[], string> tableNameFunc = null, params (Type type, string alias)[] array)
        {
            this.sqlWrapper.IsSingleTable = false;
            if (array?.Length > 0)
            {
                foreach (var (type, alias) in array)
                {
                    var name = this.sqlWrapper.GetTableName(type);
                    this.sqlWrapper.SetTableAlias(name, alias);
                }
            }

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            var tableAlias = this.sqlWrapper.GetTableAlias(tableName, array?.FirstOrDefault().alias);

            //Oracle表别名不支持AS关键字，列别名支持；
            var @as = this.sqlWrapper.DatabaseType == DatabaseType.Oracle ? " " : " AS ";

            if (tableAlias.IsNullOrEmpty())
                @as = "";

            if (tableNameFunc.IsNull())
                return $"SELECT {{0}} FROM {tableName}{@as}{tableAlias}";
            else
                return $"SELECT {{0}} FROM {tableNameFunc(new[] { tableName, @as, tableAlias })}";
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sql">sql语句</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select(Expression expression = null, string sql = null, Func<string[], string> tableNameFunc = null)
        {
            var len = this.sqlWrapper.Length;

            var tableAlias = this.GetExpressionAlias(expression, typeof(T)).FirstOrDefault().alias;

            if (sql == null)
                sql = this.Select(tableNameFunc, (typeof(T), tableAlias));

            var selectFields = "*";
            if (expression != null)
            {
                SqlExpressionProvider.Select(expression, this.sqlWrapper);
                selectFields = this.sqlWrapper.SelectFieldsString;

                //移除默认的查询语句
                if (len > 0)
                {
                    var sqlReplace = string.Format(this.Select(tableNameFunc, (typeof(T), null)), "*");
                    var sqlNew = this.sqlWrapper.Replace(sqlReplace, "").ToString();
                    this.sqlWrapper.Reset(sqlNew);
                }
            }

            sql = string.Format(sql, selectFields);

            if (len == 0)
                this.sqlWrapper.Append(sql);
            else
                this.sqlWrapper.Reset($"{sql}{this.sqlWrapper.Replace("t", tableAlias)}");

            return this;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select(Expression<Func<T, object>> expression = null, Func<string[], string> tableNameFunc = null)
        {
            var expr = expression?.Body;
            if (expr?.NodeType == ExpressionType.Constant ||
                expr?.NodeType == ExpressionType.Parameter ||
                expr?.NodeType == ExpressionType.MemberInit ||
                expr?.NodeType == ExpressionType.New)
                expr = expression;

            return this.Select(expr, null, tableNameFunc);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2>(Expression<Func<T, T2, object>> expression = null, Func<string[], string> tableNameFunc = null)
            where T2 : class
        {
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2)));
            return this.Select(expression?.Body, sql, tableNameFunc);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3>(Expression<Func<T, T2, T3, object>> expression = null, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2), typeof(T3)));
            return this.Select(expression?.Body, sql, tableNameFunc);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4>(Expression<Func<T, T2, T3, T4, object>> expression = null, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2), typeof(T3), typeof(T4)));
            return this.Select(expression?.Body, sql, tableNameFunc);
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, object>> expression = null, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5)));
            return this.Select(expression?.Body, sql, tableNameFunc);
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
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression = null, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6)));
            return this.Select(expression?.Body, sql, tableNameFunc);
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
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression = null, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7)));
            return this.Select(expression?.Body, sql, tableNameFunc);
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
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression = null, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8)));
            return this.Select(expression?.Body, sql, tableNameFunc);
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
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression = null, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9)));
            return this.Select(expression?.Body, sql, tableNameFunc);
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
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Select<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression = null, Func<string[], string> tableNameFunc = null)
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
            var sql = this.Select(tableNameFunc, this.GetExpressionAlias(expression, typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5), typeof(T6), typeof(T7), typeof(T8), typeof(T9), typeof(T10)));
            return this.Select(expression?.Body, sql, tableNameFunc);
        }
        #endregion

        #region Join
        /// <summary>
        /// 获取目标Join类型
        /// </summary>
        /// <param name="types"></param>
        /// <returns></returns>
        private Type GetJoinType(params Type[] types)
        {
            Type type = null;

            for (int i = types.Length - 1; i >= 0; i--)
            {
                type = types[i];
                if (this.sqlWrapper.IsJoined(type))
                {
                    //倒序判断，第一个被Join后，则重置为最后一个
                    if (i == 0)
                        type = types.Last();

                    continue;
                }

                break;
            }

            return type;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="sql">自定义Join语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Join(string sql)
        {
            this.sqlWrapper += " JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="sql">自定义Join语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Join(StringBuilder sql)
        {
            this.sqlWrapper += " JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="type">Join的实体类型</param>
        /// <param name="expression">表达式树</param>
        /// <param name="join">连接类型</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join(Type type, Expression expression, string join, Func<string[], string> tableNameFunc = null)
        {
            this.sqlWrapper.AddJoinType(type);

            var alias = this.GetExpressionAlias(expression, type).Last().alias;
            var tableName = this.sqlWrapper.GetTableName(type);

            /***
             * 注释Join新增表别名逻辑，此时如果是多表查询，则要求Select方法内必须用lambda表达式显示指明每个表的别名
             * 此时每个Join内的lambda表达式形参命名可以随意命名
             * this.sqlWrapper.SetTableAlias(tableName, alias);
             */

            var tableAlias = this.sqlWrapper.GetTableAlias(tableName, alias);

            //Oracle表别名不支持AS关键字，列别名支持；
            var @as = this.sqlWrapper.DatabaseType == DatabaseType.Oracle ? " " : " AS ";

            if (tableAlias.IsNullOrEmpty())
                @as = "";

            if (tableNameFunc.IsNull())
                this.sqlWrapper.Append($"{(join.IsNullOrEmpty() ? "" : $" {join}")} JOIN {tableName}{@as}{tableAlias} ON ");
            else
                this.sqlWrapper.Append($"{(join.IsNullOrEmpty() ? "" : $" {join}")} JOIN {tableNameFunc(new[] { tableName, @as, tableAlias })} ON ");

            SqlExpressionProvider.Join(expression, this.sqlWrapper);

            return this;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="join">连接类型</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2>(Expression expression, string join, Func<string[], string> tableNameFunc = null)
            where T2 : class
        {
            var type = this.GetJoinType(typeof(T2));

            return this.Join(type, expression, join, tableNameFunc);
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="join">连接类型</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3>(Expression expression, string join, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            var type = this.GetJoinType(typeof(T2), typeof(T3));

            return this.Join(type, expression, join, tableNameFunc);
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="join">连接类型</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3, T4>(Expression expression, string join, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            var type = this.GetJoinType(typeof(T2), typeof(T3), typeof(T4));

            return this.Join(type, expression, join, tableNameFunc);
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2>(Expression<Func<T, T2, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
        {
            return this.Join<T2>(expression, "", tableNameFunc);
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3>(Expression<Func<T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "", tableNameFunc);
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3>(Expression<Func<T, T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "", tableNameFunc);
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3, T4>(Expression<Func<T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "", tableNameFunc);
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Join<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "", tableNameFunc);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <param name="sql">自定义InnerJoin语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> InnerJoin(string sql)
        {
            this.sqlWrapper += " INNER JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <param name="sql">自定义InnerJoin语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> InnerJoin(StringBuilder sql)
        {
            this.sqlWrapper += " INNER JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2>(Expression<Func<T, T2, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
        {
            return this.Join<T2>(expression, "INNER", tableNameFunc);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "INNER", tableNameFunc);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2, T3>(Expression<Func<T, T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "INNER", tableNameFunc);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2, T3, T4>(Expression<Func<T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "INNER", tableNameFunc);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> InnerJoin<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "INNER", tableNameFunc);
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <param name="sql">自定义LeftJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin(string sql)
        {
            this.sqlWrapper += " LEFT JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <param name="sql">自定义LeftJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin(StringBuilder sql)
        {
            this.sqlWrapper += " LEFT JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2>(Expression<Func<T, T2, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
        {
            return this.Join<T2>(expression, "LEFT", tableNameFunc);
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "LEFT", tableNameFunc);
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2, T3>(Expression<Func<T, T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "LEFT", tableNameFunc);
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2, T3, T4>(Expression<Func<T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "LEFT", tableNameFunc);
        }

        /// <summary>
        /// LeftJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> LeftJoin<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "LEFT", tableNameFunc);
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <param name="sql">自定义RightJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin(string sql)
        {
            this.sqlWrapper += " RIGHT JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <param name="sql">自定义RightJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin(StringBuilder sql)
        {
            this.sqlWrapper += " RIGHT JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2>(Expression<Func<T, T2, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
        {
            return this.Join<T2>(expression, "RIGHT", tableNameFunc);
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "RIGHT", tableNameFunc);
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2, T3>(Expression<Func<T, T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "RIGHT", tableNameFunc);
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2, T3, T4>(Expression<Func<T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "RIGHT", tableNameFunc);
        }

        /// <summary>
        /// RightJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> RightJoin<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "RIGHT", tableNameFunc);
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <param name="sql">自定义FullJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin(string sql)
        {
            this.sqlWrapper += " FULL JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <param name="sql">自定义FullJoin语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin(StringBuilder sql)
        {
            this.sqlWrapper += " FULL JOIN ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2>(Expression<Func<T, T2, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
        {
            return this.Join<T2>(expression, "FULL", tableNameFunc);
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2, T3>(Expression<Func<T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "FULL", tableNameFunc);
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2, T3>(Expression<Func<T, T2, T3, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3>(expression, "FULL", tableNameFunc);
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2, T3, T4>(Expression<Func<T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "FULL", tableNameFunc);
        }

        /// <summary>
        /// FullJoin
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> FullJoin<T2, T3, T4>(Expression<Func<T, T2, T3, T4, bool>> expression, Func<string[], string> tableNameFunc = null)
            where T2 : class
            where T3 : class
        {
            return this.Join<T2, T3, T4>(expression, "FULL", tableNameFunc);
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
            if (this.sqlWrapper.Length == 0)
                this.Select(expression: null);

            this.sqlWrapper += " WHERE ";
            this.sqlWrapper += sql;
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
            if (this.sqlWrapper.Length == 0)
                this.Select(expression: null);

            if (hasWhere)
                this.sqlWrapper += " AND ";
            else
            {
                this.sqlWrapper += " WHERE ";
                hasWhere = true;
            }

            this.sqlWrapper += sql;

            return this;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Where(StringBuilder sql)
        {
            if (this.sqlWrapper.Length == 0)
                this.Select(expression: null);

            this.sqlWrapper += " WHERE ";
            this.sqlWrapper += sql;
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
            if (this.sqlWrapper.Length == 0)
                this.Select(expression: null);

            if (hasWhere)
                this.sqlWrapper += " AND ";
            else
            {
                this.sqlWrapper += " WHERE ";
                hasWhere = true;
            }

            this.sqlWrapper += sql;

            return this;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        public SqlBuilderCore<T> Where(Expression expression)
        {
            if (this.sqlWrapper.Length == 0)
                this.Select(expression: null);

            if (!((expression.NodeType == ExpressionType.Constant
                && expression.ToObject() is bool b && b)
                || (expression is LambdaExpression lambdaExpression
                && lambdaExpression.Body.NodeType == ExpressionType.Constant
                && lambdaExpression.Body.ToObject() is bool r && r)))
            {
                this.sqlWrapper += " WHERE ";

                SqlExpressionProvider.Where(expression, this.sqlWrapper);
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
            if (this.sqlWrapper.Length == 0)
                this.Select(expression: null);

            if (!((expression.NodeType == ExpressionType.Constant
                && expression.ToObject() is bool b && b)
                || (expression is LambdaExpression lambdaExpression
                && lambdaExpression.Body.NodeType == ExpressionType.Constant
                && lambdaExpression.Body.ToObject() is bool r && r)))
            {
                if (hasWhere)
                    this.sqlWrapper += " AND ";
                else
                {
                    this.sqlWrapper += " WHERE ";
                    hasWhere = true;
                }

                SqlExpressionProvider.Where(expression, this.sqlWrapper);
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
            var str = this.sqlWrapper.ToString().ToUpper();

            if (str.Contains("WHERE") && str.Substring("WHERE").Trim().IsNotNullOrEmpty())
                this.sqlWrapper += " AND ";
            else
                this.sqlWrapper += " WHERE ";

            this.sqlWrapper += sql;

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
                this.sqlWrapper += " AND ";
            else
            {
                this.sqlWrapper += " WHERE ";
                hasWhere = true;
            }

            this.sqlWrapper += sql;

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere(StringBuilder sql)
        {
            var str = this.sqlWrapper.ToString().ToUpper();

            if (str.Contains("WHERE") && str.Substring("WHERE").Trim().IsNotNullOrEmpty())
                this.sqlWrapper += " AND ";
            else
                this.sqlWrapper += " WHERE ";

            this.sqlWrapper += sql;

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
                this.sqlWrapper += " AND ";
            else
            {
                this.sqlWrapper += " WHERE ";
                hasWhere = true;
            }

            this.sqlWrapper += sql;

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="expression"></param>
        /// <returns></returns>
        public SqlBuilderCore<T> AndWhere(Expression expression)
        {
            var sql = this.sqlWrapper.ToString().ToUpper();

            if (sql.Contains("WHERE") && sql.Substring("WHERE").Trim().IsNotNullOrEmpty())
                this.sqlWrapper += " AND ";
            else
                this.sqlWrapper += " WHERE ";

            this.sqlWrapper += "(";
            SqlExpressionProvider.Where(expression, this.sqlWrapper);
            this.sqlWrapper += ")";

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
                this.sqlWrapper += " AND ";
            else
            {
                this.sqlWrapper += " WHERE ";
                hasWhere = true;
            }

            this.sqlWrapper += "(";
            SqlExpressionProvider.Where(expression, this.sqlWrapper);
            this.sqlWrapper += ")";

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
            var str = this.sqlWrapper.ToString().ToUpper();

            if (str.Contains("WHERE") && str.Substring("WHERE").Trim().IsNotNullOrEmpty())
                this.sqlWrapper += " OR ";
            else
                this.sqlWrapper += " WHERE ";

            this.sqlWrapper += sql;

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
                this.sqlWrapper += " OR ";
            else
            {
                this.sqlWrapper += " WHERE ";
                hasWhere = true;
            }

            this.sqlWrapper += sql;

            return this;
        }

        /// <summary>
        /// AndWhere
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> OrWhere(StringBuilder sql)
        {
            var str = this.sqlWrapper.ToString().ToUpper();

            if (str.Contains("WHERE") && str.Substring("WHERE").Trim().IsNotNullOrEmpty())
                this.sqlWrapper += " OR ";
            else
                this.sqlWrapper += " WHERE ";

            this.sqlWrapper += sql;

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
                this.sqlWrapper += " OR ";
            else
            {
                this.sqlWrapper += " WHERE ";
                hasWhere = true;
            }

            this.sqlWrapper += sql;

            return this;
        }

        /// <summary>
        /// OrWhere
        /// </summary>
        /// <param name="expression">表达式树</param>
        public SqlBuilderCore<T> OrWhere(Expression expression)
        {
            var sql = this.sqlWrapper.ToString();

            if (sql.Contains("WHERE") && sql.Substring("WHERE").Trim().IsNotNullOrEmpty())
                this.sqlWrapper += " OR ";
            else
                this.sqlWrapper += " WHERE ";

            this.sqlWrapper += "(";
            SqlExpressionProvider.Where(expression, this.sqlWrapper);
            this.sqlWrapper += ")";

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
                this.sqlWrapper += " OR ";
            else
            {
                this.sqlWrapper += " WHERE ";
                hasWhere = true;
            }

            this.sqlWrapper += "(";
            SqlExpressionProvider.Where(expression, this.sqlWrapper);
            this.sqlWrapper += ")";

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
                throw new ArgumentNullException("实体参数不能为空！");

            var sql = this.sqlWrapper.ToString();

            if (!sql.ContainsIgnoreCase("SELECT", "UPDATE", "DELETE"))
                throw new ArgumentException("此方法只能用于Select、Update、Delete方法！");

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            var tableAlias = this.sqlWrapper.GetTableAlias(tableName);

            if (tableAlias.IsNotNullOrEmpty())
                tableAlias += ".";

            var keys = this.sqlWrapper.GetPrimaryKey(typeof(T));
            if (keys.Count > 0 && entity != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    var (key, property) = keys[i];
                    if (key.IsNotNullOrEmpty())
                    {
                        var keyValue = typeof(T).GetProperty(property)?.GetValue(entity, null);
                        if (keyValue != null)
                        {
                            this.sqlWrapper += $" {(sql.ContainsIgnoreCase("WHERE") || i > 0 ? "AND" : "WHERE")} {(tableAlias + key)} = ";
                            this.sqlWrapper.AddDbParameter(keyValue);
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
                throw new ArgumentNullException("keyValue不能为空！");

            if (!keyValue.Any(o => o.GetType().IsValueType || o.GetType() == typeof(string)))
                throw new ArgumentException("keyValue只能为值类型或者字符串类型数据！");

            var sql = this.sqlWrapper.ToString();
            if (!sql.ContainsIgnoreCase("SELECT", "UPDATE", "DELETE"))
                throw new ArgumentException("WithKey方法只能用于Select、Update、Delete方法！");

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            var tableAlias = this.sqlWrapper.GetTableAlias(tableName);

            if (tableAlias.IsNotNullOrEmpty())
                tableAlias += ".";

            var keys = this.sqlWrapper.GetPrimaryKey(typeof(T));
            if (keys.Count > 0 && keyValue != null)
            {
                for (int i = 0; i < keys.Count; i++)
                {
                    var (key, property) = keys[i];
                    if (key.IsNotNullOrEmpty())
                    {
                        this.sqlWrapper += $" {(sql.ContainsIgnoreCase("WHERE") || i > 0 ? "AND" : "WHERE")} {(tableAlias + key)} = ";
                        this.sqlWrapper.AddDbParameter(keyValue[i]);
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
            this.sqlWrapper += " GROUP BY ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy(StringBuilder sql)
        {
            this.sqlWrapper += " GROUP BY ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> GroupBy(Expression expression)
        {
            this.sqlWrapper += " GROUP BY ";
            SqlExpressionProvider.GroupBy(expression, this.sqlWrapper);
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

        #region Having
        /// <summary>
        /// Having
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Having(string sql)
        {
            this.sqlWrapper += " HAVING ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Having(StringBuilder sql)
        {
            this.sqlWrapper += " HAVING ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns></returns>
        public SqlBuilderCore<T> Having(Expression expression)
        {
            this.sqlWrapper += " HAVING ";
            SqlExpressionProvider.Having(expression, this.sqlWrapper);
            return this;
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Having(Expression<Func<T, object>> expression)
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Having<T2>(Expression<Func<T, T2, object>> expression)
            where T2 : class
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Having<T2, T3>(Expression<Func<T, T2, T3, object>> expression)
            where T2 : class
            where T3 : class
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Having<T2, T3, T4>(Expression<Func<T, T2, T3, T4, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Having<T2, T3, T4, T5>(Expression<Func<T, T2, T3, T4, T5, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Having<T2, T3, T4, T5, T6>(Expression<Func<T, T2, T3, T4, T5, T6, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <typeparam name="T2">泛型类型2</typeparam>
        /// <typeparam name="T3">泛型类型3</typeparam>
        /// <typeparam name="T4">泛型类型4</typeparam>
        /// <typeparam name="T5">泛型类型5</typeparam>
        /// <typeparam name="T6">泛型类型6</typeparam>
        /// <typeparam name="T7">泛型类型7</typeparam>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Having<T2, T3, T4, T5, T6, T7>(Expression<Func<T, T2, T3, T4, T5, T6, T7, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
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
        public SqlBuilderCore<T> Having<T2, T3, T4, T5, T6, T7, T8>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
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
        public SqlBuilderCore<T> Having<T2, T3, T4, T5, T6, T7, T8, T9>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, object>> expression)
            where T2 : class
            where T3 : class
            where T4 : class
            where T5 : class
            where T6 : class
            where T7 : class
            where T8 : class
            where T9 : class
        {
            return this.Having(expression.Body);
        }

        /// <summary>
        /// Having
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
        public SqlBuilderCore<T> Having<T2, T3, T4, T5, T6, T7, T8, T9, T10>(Expression<Func<T, T2, T3, T4, T5, T6, T7, T8, T9, T10, object>> expression)
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
            return this.Having(expression.Body);
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
            this.sqlWrapper += " ORDER BY ";
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderBy(StringBuilder sql)
        {
            this.sqlWrapper += " ORDER BY ";
            this.sqlWrapper += sql;
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
            this.sqlWrapper += " ORDER BY ";
            SqlExpressionProvider.OrderBy(expression, this.sqlWrapper, orders);
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

        #region OrderByDescending
        /// <summary>
        /// OrderByDescending
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> OrderByDescending(Expression<Func<T, object>> expression)
        {
            var orders = new List<OrderType>();
            if (expression?.Body is NewExpression newExpression)
            {
                for (int i = 0; i < newExpression.Arguments.Count; i++)
                {
                    orders.Add(OrderType.Descending);
                }
            }
            else
                orders.Add(OrderType.Descending);

            return this.OrderBy(expression.Body, orders.ToArray());
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
        /// <param name="countSyntax">分页计数语法，默认COUNT(*)</param>
        /// <param name="serverVersion">DbConnection的ServerVersion属性</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Page(int pageSize, int pageIndex, string orderField, string sql = null, Dictionary<string, object> parameters = null, string countSyntax = "COUNT(*)", string serverVersion = null)
        {
            var sb = new StringBuilder();

            //排序字段
            var order = string.Empty;
            if (orderField.IsNotNullOrEmpty())
            {
                if (orderField.Contains(@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)", RegexOptions.IgnoreCase))
                    order = $"ORDER BY {orderField}";
                else
                    order = $"ORDER BY {orderField} ASC";
            }
            else if (this.sqlWrapper.DatabaseType == DatabaseType.SqlServer)
            {
                order = "ORDER BY (SELECT 0)";
            }

            if (sql.IsNotNullOrEmpty())
            {
                this.sqlWrapper.DbParameters.Clear();
                if (parameters.IsNotNullOrEmpty())
                    this.sqlWrapper.DbParameters = parameters;
            }

            sql = sql.IsNullOrEmpty() ? this.sqlWrapper.ToString().TrimEnd(';') : sql.TrimEnd(';');

            //数据库版本
            var dbVersion = 0;
            if (serverVersion.IsNotNullOrEmpty())
                dbVersion = int.Parse(serverVersion.Split('.')[0]);

            //SQLServer
            if (this.sqlWrapper.DatabaseType == DatabaseType.SqlServer)
            {
                if (dbVersion > 10)
                    sb.Append($"SELECT {countSyntax} AS [TOTAL] FROM ({sql}) AS T;{sql} {(orderField.IsNotNullOrEmpty() ? order : "")} OFFSET {(pageIndex - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY;");
                else
                    sb.Append($"SELECT {countSyntax} AS [TOTAL] FROM ({sql}) AS T;SELECT * FROM (SELECT ROW_NUMBER() OVER ({order}) AS [ROWNUMBER], * FROM ({sql}) AS T) AS N WHERE [ROWNUMBER] BETWEEN {(pageIndex - 1) * pageSize + 1} AND {pageIndex * pageSize};");
            }

            //Oracle，注意Oracle需要Split(';')分开单独查询总条数和分页数据
            if (this.sqlWrapper.DatabaseType == DatabaseType.Oracle)
            {
                if (dbVersion > 11)
                    sb.Append($"SELECT {countSyntax} AS \"TOTAL\" FROM ({sql}) T;{sql} {order} OFFSET {pageSize * (pageIndex - 1)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                else
                    sb.Append($"SELECT {countSyntax} AS \"TOTAL\" FROM ({sql}) T;SELECT * FROM (SELECT X.*,ROWNUM AS \"ROWNUMBER\" FROM ({sql} {order}) X WHERE ROWNUM <= {pageSize * pageIndex}) T WHERE \"ROWNUMBER\" >= {pageSize * (pageIndex - 1) + 1}");
            }

            //MySQL，注意8.0版本才支持WITH语法
            if (this.sqlWrapper.DatabaseType == DatabaseType.MySql)
                sb.Append($"SELECT {countSyntax} AS `TOTAL` FROM ({sql}) AS T;{sql} {order} LIMIT {pageSize} OFFSET {pageSize * (pageIndex - 1)};");

            //PostgreSQL、SQLite
            if (this.sqlWrapper.DatabaseType == DatabaseType.PostgreSql || this.sqlWrapper.DatabaseType == DatabaseType.Sqlite)
                sb.Append($"SELECT {countSyntax} AS \"TOTAL\" FROM ({sql}) AS T;{sql} {order} LIMIT {pageSize} OFFSET {pageSize * (pageIndex - 1)};");

            this.sqlWrapper.Reset(sb);

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
        /// <param name="countSyntax">分页计数语法，默认COUNT(*)</param>
        /// <param name="serverVersion">DbConnection的ServerVersion属性</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> PageByWith(int pageSize, int pageIndex, string orderField, string sql = null, Dictionary<string, object> parameters = null, string countSyntax = "COUNT(*)", string serverVersion = null)
        {
            var sb = new StringBuilder();

            //排序字段
            var order = string.Empty;
            if (orderField.IsNotNullOrEmpty())
            {
                if (orderField.Contains(@"(/\*(?:|)*?\*/)|(\b(ASC|DESC)\b)", RegexOptions.IgnoreCase))
                    order = $"ORDER BY {orderField}";
                else
                    order = $"ORDER BY {orderField} ASC";
            }
            else if (this.sqlWrapper.DatabaseType == DatabaseType.SqlServer)
            {
                order = "ORDER BY (SELECT 0)";
            }

            if (sql.IsNotNullOrEmpty())
            {
                this.sqlWrapper.DbParameters.Clear();
                if (parameters.IsNotNullOrEmpty())
                    this.sqlWrapper.DbParameters = parameters;
            }

            sql = sql.IsNullOrEmpty() ? this.sqlWrapper.ToString().TrimEnd(';') : sql.TrimEnd(';');

            //数据库版本
            var dbVersion = 0;
            if (serverVersion.IsNotNullOrEmpty())
                dbVersion = int.Parse(serverVersion.Split('.')[0]);

            //SQLServer
            if (this.sqlWrapper.DatabaseType == DatabaseType.SqlServer)
            {
                if (dbVersion > 10)
                    sb.Append($"{sql} SELECT {countSyntax} AS [TOTAL] FROM T;{sql.Remove(sql.LastIndexOf(")"), 1)} {(orderField.IsNotNullOrEmpty() ? order : "")}) SELECT * FROM T OFFSET {(pageIndex - 1) * pageSize} ROWS FETCH NEXT {pageSize} ROWS ONLY;");
                else
                    sb.Append($"{sql} SELECT {countSyntax} AS [TOTAL] FROM T;{sql},R AS (SELECT ROW_NUMBER() OVER ({order}) AS [ROWNUMBER], * FROM T) SELECT * FROM R WHERE [ROWNUMBER] BETWEEN {(pageIndex - 1) * pageSize + 1} AND {pageIndex * pageSize};");
            }

            //Oracle，注意Oracle需要Split(';')分开单独查询总条数和分页数据
            if (this.sqlWrapper.DatabaseType == DatabaseType.Oracle)
            {
                if (dbVersion > 11)
                    sb.Append($"{sql} SELECT {countSyntax} AS \"TOTAL\" FROM T;{sql.Remove(sql.LastIndexOf(")"), 1)} {order}) SELECT * FROM T OFFSET {pageSize * (pageIndex - 1)} ROWS FETCH NEXT {pageSize} ROWS ONLY");
                else
                    sb.Append($"{sql} SELECT {countSyntax} AS \"TOTAL\" FROM T;{sql.Remove(sql.LastIndexOf(")"), 1)} {order}),R AS (SELECT ROWNUM AS \"ROWNUMBER\",T.* FROM T WHERE ROWNUM <= {pageSize * pageIndex}) SELECT * FROM R WHERE \"ROWNUMBER\">={pageSize * (pageIndex - 1) + 1}");
            }

            //MySQL，注意8.0版本才支持WITH语法
            if (this.sqlWrapper.DatabaseType == DatabaseType.MySql)
                sb.Append($"{sql} SELECT {countSyntax} AS `TOTAL` FROM T;{sql.Remove(sql.LastIndexOf(")"), 1)} {order}) SELECT * FROM T LIMIT {pageSize} OFFSET {pageSize * (pageIndex - 1)};");

            //PostgreSQL、SQLite
            if (this.sqlWrapper.DatabaseType == DatabaseType.PostgreSql || this.sqlWrapper.DatabaseType == DatabaseType.Sqlite)
                sb.Append($"{sql} SELECT {countSyntax} AS \"TOTAL\" FROM T;{sql.Remove(sql.LastIndexOf(")"), 1)} {order}) SELECT * FROM T LIMIT {pageSize} OFFSET {pageSize * (pageIndex - 1)};");

            this.sqlWrapper.Reset(sb);

            return this;
        }
        #endregion

        #region Delete
        /// <summary>
        /// Delete
        /// </summary>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Delete(Func<string, string> tableNameFunc = null)
        {
            this.Clear();
            this.sqlWrapper.IsSingleTable = true;

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            this.sqlWrapper += $"DELETE FROM {tableNameFunc?.Invoke(tableName) ?? tableName}";

            return this;
        }
        #endregion

        #region Update
        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：否</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Update(Expression<Func<object>> expression = null, bool isEnableNullValue = false, Func<string, string> tableNameFunc = null)
        {
            this.Clear();
            this.sqlWrapper.IsSingleTable = true;
            this.sqlWrapper.IsEnableNullValue = isEnableNullValue;

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            this.sqlWrapper += $"UPDATE {tableNameFunc?.Invoke(tableName) ?? tableName} SET ";

            SqlExpressionProvider.Update(expression.Body, this.sqlWrapper);

            return this;
        }
        #endregion

        #region Insert
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="isEnableNullValue">是否对null值属性进行sql拼接操作，默认：否</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Insert(Expression<Func<object>> expression = null, bool isEnableNullValue = false, Func<string, string> tableNameFunc = null)
        {
            this.Clear();
            this.sqlWrapper.IsSingleTable = true;
            this.sqlWrapper.IsEnableNullValue = isEnableNullValue;

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            this.sqlWrapper += $"INSERT INTO {tableNameFunc?.Invoke(tableName) ?? tableName} ({{0}}) {(this.sqlWrapper.DatabaseType == DatabaseType.Oracle ? "SELECT" : "VALUES")} ";

            SqlExpressionProvider.Insert(expression.Body, this.sqlWrapper);

            return this;
        }
        #endregion

        #region Max
        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Max(Expression<Func<T, object>> expression, Func<string, string> tableNameFunc = null)
        {
            this.Clear();
            this.sqlWrapper.IsSingleTable = true;

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            var sql = $"SELECT MAX({{0}}) FROM {tableNameFunc?.Invoke(tableName) ?? tableName}";

            SqlExpressionProvider.Max(expression.Body, this.sqlWrapper);
            this.sqlWrapper.AppendFormat(sql, this.sqlWrapper.SelectFieldsString);

            return this;
        }
        #endregion

        #region Min
        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Min(Expression<Func<T, object>> expression, Func<string, string> tableNameFunc = null)
        {
            this.Clear();
            this.sqlWrapper.IsSingleTable = true;

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            var sql = $"SELECT MIN({{0}}) FROM {tableNameFunc?.Invoke(tableName) ?? tableName}";

            SqlExpressionProvider.Min(expression.Body, this.sqlWrapper);
            this.sqlWrapper.AppendFormat(sql, this.sqlWrapper.SelectFieldsString);

            return this;
        }
        #endregion

        #region Avg
        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Avg(Expression<Func<T, object>> expression, Func<string, string> tableNameFunc = null)
        {
            this.Clear();
            this.sqlWrapper.IsSingleTable = true;

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            var sql = $"SELECT AVG({{0}}) FROM {tableNameFunc?.Invoke(tableName) ?? tableName}";

            SqlExpressionProvider.Avg(expression.Body, this.sqlWrapper);
            this.sqlWrapper.AppendFormat(sql, this.sqlWrapper.SelectFieldsString);

            return this;
        }
        #endregion

        #region Count
        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Count(Expression<Func<T, object>> expression = null, Func<string, string> tableNameFunc = null)
        {
            this.Clear();
            this.sqlWrapper.IsSingleTable = true;

            var field = "*";
            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            var sql = $"SELECT COUNT({{0}}) FROM {tableNameFunc?.Invoke(tableName) ?? tableName}";

            if (expression != null)
            {
                SqlExpressionProvider.Count(expression.Body, this.sqlWrapper);
                field = this.sqlWrapper.SelectFieldsString;
            }

            this.sqlWrapper.AppendFormat(sql, field);

            return this;
        }
        #endregion

        #region Sum
        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="tableNameFunc">表名自定义委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Sum(Expression<Func<T, object>> expression, Func<string, string> tableNameFunc = null)
        {
            this.Clear();
            this.sqlWrapper.IsSingleTable = true;

            var tableName = this.sqlWrapper.GetTableName(typeof(T));
            var sql = $"SELECT SUM({{0}}) FROM {tableNameFunc?.Invoke(tableName) ?? tableName}";

            SqlExpressionProvider.Sum(expression.Body, this.sqlWrapper);
            this.sqlWrapper.AppendFormat(sql, this.sqlWrapper.SelectFieldsString);

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
            if (this.sqlWrapper.DatabaseType == DatabaseType.SqlServer)
            {
                if (this.sqlWrapper.Contains("DISTINCT"))
                    this.sqlWrapper.Replace("DISTINCT", $"DISTINCT TOP {topNumber}", this.sqlWrapper.IndexOf("DISTINCT"), 8);
                else
                    this.sqlWrapper.Replace("SELECT", $"SELECT TOP {topNumber}", this.sqlWrapper.IndexOf("SELECT"), 6);
            }
            else if (this.sqlWrapper.DatabaseType == DatabaseType.Oracle)
            {
                this.sqlWrapper.Reset($"SELECT * FROM ({this.sqlWrapper}) T WHERE ROWNUM <= {topNumber}");
            }
            else if (this.sqlWrapper.DatabaseType == DatabaseType.MySql || this.sqlWrapper.DatabaseType == DatabaseType.Sqlite || this.sqlWrapper.DatabaseType == DatabaseType.PostgreSql)
            {
                this.sqlWrapper.Append($" LIMIT {topNumber} OFFSET 0");
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
            this.sqlWrapper.Replace("SELECT", $"SELECT DISTINCT", this.sqlWrapper.IndexOf("SELECT"), 6);
            return this;
        }
        #endregion

        #region Append
        /// <summary>
        /// Append
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Append(string sql)
        {
            this.sqlWrapper += sql;
            return this;
        }

        /// <summary>
        /// Append
        /// </summary>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> Append(StringBuilder sql)
        {
            this.sqlWrapper += sql;
            return this;
        }
        #endregion

        #region AppendIf
        /// <summary>
        /// AppendIf
        /// </summary>
        /// <param name="condition">自定义条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AppendIf(bool condition, string sql)
        {
            if (condition)
                this.sqlWrapper += sql;

            return this;
        }

        /// <summary>
        /// AppendIf
        /// </summary>
        /// <param name="condition">自定义条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="callback">回调委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AppendIf(bool condition, string sql, Action callback)
        {
            if (condition)
            {
                this.sqlWrapper += sql;
                callback?.Invoke();
            }

            return this;
        }

        /// <summary>
        /// AppendIf
        /// </summary>
        /// <param name="condition">自定义条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AppendIf(bool condition, StringBuilder sql)
        {
            if (condition)
                this.sqlWrapper += sql;

            return this;
        }

        /// <summary>
        /// AppendIf
        /// </summary>
        /// <param name="condition">自定义条件</param>
        /// <param name="sql">自定义sql语句</param>
        /// <param name="callback">回调委托</param>
        /// <returns>SqlBuilderCore</returns>
        public SqlBuilderCore<T> AppendIf(bool condition, StringBuilder sql, Action callback)
        {
            if (condition)
            {
                this.sqlWrapper += sql;
                callback?.Invoke();
            }

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
            return this.sqlWrapper.GetTableName(typeof(T));
        }
        #endregion

        #region GetPrimaryKey
        /// <summary>
        /// 获取实体对应表的主键名称
        /// </summary>
        /// <returns></returns>
        public List<string> GetPrimaryKey()
        {
            return this.sqlWrapper.GetPrimaryKey(typeof(T)).Select(o => o.key).ToList();
        }
        #endregion
        #endregion
    }
}