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

using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示对静态方法或实例方法的调用
    /// </summary>
	public class MethodCallExpressionResolver : BaseExpression<MethodCallExpression>
    {
        #region Private Static Methods
        /// <summary>
        /// methods
        /// </summary>
        private static readonly Dictionary<string, Action<MethodCallExpression, SqlWrapper>> methods = new Dictionary<string, Action<MethodCallExpression, SqlWrapper>>
        {
            ["Like"] = Like,
            ["LikeLeft"] = LikeLeft,
            ["LikeRight"] = LikeRight,
            ["NotLike"] = NotLike,
            ["In"] = SqlIn,
            ["NotIn"] = NotIn,
            ["Contains"] = Contains,
            ["IsNullOrEmpty"] = IsNullOrEmpty,
            ["Equals"] = Equals,
            ["ToUpper"] = ToUpper,
            ["ToLower"] = ToLower,
            ["Trim"] = Trim,
            ["TrimStart"] = TrimStart,
            ["TrimEnd"] = TrimEnd,
            ["Count"] = SqlCount,
            ["Sum"] = SqlSum,
            ["Avg"] = SqlAvg,
            ["Max"] = SqlMax,
            ["Min"] = SqlMin,
        };

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void SqlIn(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
            sqlWrapper += " IN ";
            SqlExpressionProvider.In(expression.Arguments[1], sqlWrapper);
        }

        /// <summary>
        /// Not In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void NotIn(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
            sqlWrapper += " NOT IN ";
            SqlExpressionProvider.In(expression.Arguments[1], sqlWrapper);
        }

        /// <summary>
        /// Like
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void Like(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
            {
                SqlExpressionProvider.Where(expression.Object, sqlWrapper);
            }
            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
            switch (sqlWrapper.DatabaseType)
            {
                case DatabaseType.SqlServer:
                    sqlWrapper += " LIKE '%' + ";
                    break;
                case DatabaseType.MySql:
                case DatabaseType.PostgreSql:
                    sqlWrapper += " LIKE CONCAT('%',";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.Sqlite:
                    sqlWrapper += " LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlExpressionProvider.Where(expression.Arguments[1], sqlWrapper);
            switch (sqlWrapper.DatabaseType)
            {
                case DatabaseType.SqlServer:
                    sqlWrapper += " + '%'";
                    break;
                case DatabaseType.MySql:
                case DatabaseType.PostgreSql:
                    sqlWrapper += ",'%')";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.Sqlite:
                    sqlWrapper += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// LikeLeft
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void LikeLeft(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
                SqlExpressionProvider.Where(expression.Object, sqlWrapper);

            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
            switch (sqlWrapper.DatabaseType)
            {
                case DatabaseType.SqlServer:
                    sqlWrapper += " LIKE '%' + ";
                    break;
                case DatabaseType.MySql:
                case DatabaseType.PostgreSql:
                    sqlWrapper += " LIKE CONCAT('%',";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.Sqlite:
                    sqlWrapper += " LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlExpressionProvider.Where(expression.Arguments[1], sqlWrapper);
            switch (sqlWrapper.DatabaseType)
            {
                case DatabaseType.MySql:
                case DatabaseType.PostgreSql:
                    sqlWrapper += ")";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// LikeRight
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void LikeRight(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
            {
                SqlExpressionProvider.Where(expression.Object, sqlWrapper);
            }
            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
            switch (sqlWrapper.DatabaseType)
            {
                case DatabaseType.SqlServer:
                case DatabaseType.Oracle:
                case DatabaseType.Sqlite:
                    sqlWrapper += " LIKE ";
                    break;
                case DatabaseType.MySql:
                case DatabaseType.PostgreSql:
                    sqlWrapper += " LIKE CONCAT(";
                    break;
                default:
                    break;
            }
            SqlExpressionProvider.Where(expression.Arguments[1], sqlWrapper);
            switch (sqlWrapper.DatabaseType)
            {
                case DatabaseType.SqlServer:
                    sqlWrapper += " + '%'";
                    break;
                case DatabaseType.MySql:
                case DatabaseType.PostgreSql:
                    sqlWrapper += ",'%')";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.Sqlite:
                    sqlWrapper += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// NotLike
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void NotLike(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
                SqlExpressionProvider.Where(expression.Object, sqlWrapper);

            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
            switch (sqlWrapper.DatabaseType)
            {
                case DatabaseType.SqlServer:
                    sqlWrapper += " NOT LIKE '%' + ";
                    break;
                case DatabaseType.MySql:
                case DatabaseType.PostgreSql:
                    sqlWrapper += " NOT LIKE CONCAT('%',";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.Sqlite:
                    sqlWrapper += " NOT LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlExpressionProvider.Where(expression.Arguments[1], sqlWrapper);
            switch (sqlWrapper.DatabaseType)
            {
                case DatabaseType.SqlServer:
                    sqlWrapper += " + '%'";
                    break;
                case DatabaseType.MySql:
                case DatabaseType.PostgreSql:
                    sqlWrapper += ",'%')";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.Sqlite:
                    sqlWrapper += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void Contains(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
            {
                if (typeof(IList).IsAssignableFrom(expression.Object.Type))
                {
                    SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
                    sqlWrapper += " IN ";
                    SqlExpressionProvider.In(expression.Object, sqlWrapper);
                }
                else
                {
                    SqlExpressionProvider.Where(expression.Object, sqlWrapper);
                    switch (sqlWrapper.DatabaseType)
                    {
                        case DatabaseType.SqlServer:
                            sqlWrapper += " LIKE '%' + ";
                            break;
                        case DatabaseType.MySql:
                        case DatabaseType.PostgreSql:
                            sqlWrapper += " LIKE CONCAT('%',";
                            break;
                        case DatabaseType.Oracle:
                        case DatabaseType.Sqlite:
                            sqlWrapper += " LIKE '%' || ";
                            break;
                        default:
                            break;
                    }
                    SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
                    switch (sqlWrapper.DatabaseType)
                    {
                        case DatabaseType.SqlServer:
                            sqlWrapper += " + '%'";
                            break;
                        case DatabaseType.MySql:
                        case DatabaseType.PostgreSql:
                            sqlWrapper += ",'%')";
                            break;
                        case DatabaseType.Oracle:
                        case DatabaseType.Sqlite:
                            sqlWrapper += " || '%'";
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (expression.Arguments.Count > 1 && expression.Arguments[1] is MemberExpression memberExpression)
            {
                SqlExpressionProvider.Where(memberExpression, sqlWrapper);
                sqlWrapper += " IN ";
                SqlExpressionProvider.In(expression.Arguments[0], sqlWrapper);
            }
        }

        /// <summary>
        /// IsNullOrEmpty
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void IsNullOrEmpty(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            sqlWrapper += "(";
            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
            sqlWrapper += " IS NULL OR ";
            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);
            sqlWrapper += " = ''";
            sqlWrapper += ")";
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void Equals(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
                SqlExpressionProvider.Where(expression.Object, sqlWrapper);

            var signIndex = sqlWrapper.Length;
            SqlExpressionProvider.Where(expression.Arguments[0], sqlWrapper);

            if (sqlWrapper.EndsWith("NULL"))
                sqlWrapper.Insert(signIndex, " IS ");
            else
                sqlWrapper.Insert(signIndex, " = ");
        }

        /// <summary>
        /// ToUpper
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void ToUpper(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
            {
                sqlWrapper += "UPPER(";
                SqlExpressionProvider.Where(expression.Object, sqlWrapper);
                sqlWrapper += ")";
            }
        }

        /// <summary>
        /// ToLower
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void ToLower(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
            {
                sqlWrapper += "LOWER(";
                SqlExpressionProvider.Where(expression.Object, sqlWrapper);
                sqlWrapper += ")";
            }
        }

        /// <summary>
        /// Trim
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void Trim(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
            {
                if (expression.Arguments?.Count > 0)
                {
                    var trimString = expression.Object.ToObject()?.ToString();
                    if (!trimString.IsNullOrEmpty())
                    {
                        string constant;
                        var argument = expression.Arguments[0].ToObject();
                        if (argument is char @char)
                            constant = trimString.Trim(@char);
                        else
                            constant = trimString.Trim((char[])argument);

                        SqlExpressionProvider.Where(Expression.Constant(constant), sqlWrapper);
                    }
                }
                else
                {
                    if (sqlWrapper.DatabaseType == DatabaseType.SqlServer)
                        sqlWrapper += "LTRIM(RTRIM(";
                    else
                        sqlWrapper += "TRIM(";

                    SqlExpressionProvider.Where(expression.Object, sqlWrapper);

                    if (sqlWrapper.DatabaseType == DatabaseType.SqlServer)
                        sqlWrapper += "))";
                    else
                        sqlWrapper += ")";
                }
            }
        }

        /// <summary>
        /// TrimStart
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void TrimStart(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
            {
                if (expression.Arguments?.Count > 0)
                {
                    var trimString = expression.Object.ToObject()?.ToString();
                    if (!trimString.IsNullOrEmpty())
                    {
                        string constant;
                        var argument = expression.Arguments[0].ToObject();
                        if (argument is char @char)
                            constant = trimString.TrimStart(@char);
                        else
                            constant = trimString.TrimStart((char[])argument);

                        SqlExpressionProvider.Where(Expression.Constant(constant), sqlWrapper);
                    }
                }
                else
                {
                    sqlWrapper += "LTRIM(";
                    SqlExpressionProvider.Where(expression.Object, sqlWrapper);
                    sqlWrapper += ")";
                }
            }
        }

        /// <summary>
        /// TrimEnd
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void TrimEnd(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Object != null)
            {
                if (expression.Arguments?.Count > 0)
                {
                    var trimString = expression.Object.ToObject()?.ToString();
                    if (!trimString.IsNullOrEmpty())
                    {
                        string constant;
                        var argument = expression.Arguments[0].ToObject();
                        if (argument is char @char)
                            constant = trimString.TrimEnd(@char);
                        else
                            constant = trimString.TrimEnd((char[])argument);

                        SqlExpressionProvider.Where(Expression.Constant(constant), sqlWrapper);
                    }
                }
                else
                {
                    sqlWrapper += "RTRIM(";
                    SqlExpressionProvider.Where(expression.Object, sqlWrapper);
                    sqlWrapper += ")";
                }
            }
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void SqlCount(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Arguments?.Count > 0)
            {
                sqlWrapper += "COUNT(";
                SqlExpressionProvider.Having(expression.Arguments[0], sqlWrapper);
                sqlWrapper += ")";
            }
        }

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void SqlSum(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Arguments?.Count > 0)
            {
                sqlWrapper += "SUM(";
                SqlExpressionProvider.Having(expression.Arguments[0], sqlWrapper);
                sqlWrapper += ")";
            }
        }

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void SqlAvg(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Arguments?.Count > 0)
            {
                sqlWrapper += "AVG(";
                SqlExpressionProvider.Having(expression.Arguments[0], sqlWrapper);
                sqlWrapper += ")";
            }
        }

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void SqlMax(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Arguments?.Count > 0)
            {
                sqlWrapper += "MAX(";
                SqlExpressionProvider.Having(expression.Arguments[0], sqlWrapper);
                sqlWrapper += ")";
            }
        }

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        private static void SqlMin(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            if (expression.Arguments?.Count > 0)
            {
                sqlWrapper += "MIN(";
                SqlExpressionProvider.Having(expression.Arguments[0], sqlWrapper);
                sqlWrapper += ")";
            }
        }
        #endregion

        #region Override Base Class Methods
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper In(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            var val = expression?.ToObject();
            if (val != null)
            {
                sqlWrapper += "(";
                if (val.GetType().IsArray || typeof(IList).IsAssignableFrom(val.GetType()))
                {
                    var list = val as IList;
                    if (list?.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            SqlExpressionProvider.In(Expression.Constant(item, item.GetType()), sqlWrapper);
                            sqlWrapper += ",";
                        }
                    }
                }
                else
                    SqlExpressionProvider.In(Expression.Constant(val, val.GetType()), sqlWrapper);

                if (sqlWrapper[sqlWrapper.Length - 1] == ',')
                    sqlWrapper.Remove(sqlWrapper.Length - 1, 1);

                sqlWrapper += ")";
            }

            return sqlWrapper;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Select(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            var field = expression.ToObject()?.ToString();
            if (!field.IsNullOrEmpty())
                sqlWrapper.AddField(field);

            return sqlWrapper;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Where(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            var key = expression.Method;
            if (key.IsGenericMethod)
                key = key.GetGenericMethodDefinition();

            //匹配到方法
            if (methods.TryGetValue(key.Name, out Action<MethodCallExpression, SqlWrapper> handler))
            {
                handler(expression, sqlWrapper);

                return sqlWrapper;
            }
            else
            {
                try
                {
                    sqlWrapper.AddDbParameter(expression.ToObject());

                    return sqlWrapper;
                }
                catch
                {
                    throw new NotImplementedException("无法解析方法" + expression.Method);
                }
            }
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="sqlWrapper"></param>
        /// <returns></returns>
        public override SqlWrapper Insert(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            var fields = new List<string>();
            var array = expression.ToObject() as object[];
            for (var i = 0; i < array.Length; i++)
            {
                if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                    sqlWrapper.Append("(");

                if (i > 0 && sqlWrapper.DatabaseType == DatabaseType.Oracle)
                    sqlWrapper.Append(" UNION ALL SELECT ");

                var properties = array[i]?.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var type = p.DeclaringType.IsAnonymousType() ?
                        sqlWrapper.DefaultType :
                        p.DeclaringType;

                    var (columnName, isInsert, isUpdate) = sqlWrapper.GetColumnInfo(type, p);
                    if (isInsert)
                    {
                        var value = p.GetValue(array[i], null);
                        if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                        {
                            sqlWrapper.AddDbParameter(value);
                            if (!fields.Contains(columnName))
                                fields.Add(columnName);
                            sqlWrapper += ",";
                        }
                    }
                }

                if (sqlWrapper[sqlWrapper.Length - 1] == ',')
                {
                    sqlWrapper.Remove(sqlWrapper.Length - 1, 1);
                    if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                        sqlWrapper.Append("),");
                    else
                        sqlWrapper.Append(" FROM DUAL");
                }
            }

            if (sqlWrapper[sqlWrapper.Length - 1] == ',')
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);

            sqlWrapper.Reset(string.Format(sqlWrapper.ToString(), string.Join(",", fields).TrimEnd(',')));

            return sqlWrapper;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper GroupBy(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlExpressionProvider.GroupBy(Expression.Constant(array[i], array[i].GetType()), sqlWrapper);
                }
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);
            }

            return sqlWrapper;
        }

        /// <summary>
        /// Having
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Having(MethodCallExpression expression, SqlWrapper sqlWrapper)
        {
            var key = expression.Method;
            if (key.IsGenericMethod)
                key = key.GetGenericMethodDefinition();

            //匹配到方法
            if (methods.TryGetValue(key.Name, out Action<MethodCallExpression, SqlWrapper> handler))
            {
                handler(expression, sqlWrapper);

                return sqlWrapper;
            }
            else
            {
                try
                {
                    sqlWrapper.AddDbParameter(expression.ToObject());

                    return sqlWrapper;
                }
                catch
                {
                    throw new NotImplementedException("无法解析方法" + expression.Method);
                }
            }
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper OrderBy(MethodCallExpression expression, SqlWrapper sqlWrapper, params OrderType[] orders)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlExpressionProvider.OrderBy(Expression.Constant(array[i], array[i].GetType()), sqlWrapper);

                    if (i <= orders.Length - 1)
                        sqlWrapper += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                    else if (!array[i].ToString().ContainsIgnoreCase("ASC") && !array[i].ToString().ContainsIgnoreCase("DESC"))
                        sqlWrapper += " ASC,";
                    else
                        sqlWrapper += ",";
                }
                sqlWrapper.Remove(sqlWrapper.Length - 1, 1);
            }

            return sqlWrapper;
        }
        #endregion
    }
}