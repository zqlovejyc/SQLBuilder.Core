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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace SQLBuilder.Core
{
    /// <summary>
    /// 表示对静态方法或实例方法的调用
    /// </summary>
	public class MethodCallExpressionResolve : BaseSqlBuilder<MethodCallExpression>
    {
        #region Private Static Methods
        /// <summary>
        /// methods
        /// </summary>
        private static readonly Dictionary<string, Action<MethodCallExpression, SqlPack>> methods = new Dictionary<string, Action<MethodCallExpression, SqlPack>>
        {
            ["Like"] = Like,
            ["LikeLeft"] = LikeLeft,
            ["LikeRight"] = LikeRight,
            ["NotLike"] = NotLike,
            ["In"] = IN,
            ["NotIn"] = NotIn,
            ["Contains"] = Contains,
            ["IsNullOrEmpty"] = IsNullOrEmpty,
            ["Equals"] = Equals,
            ["ToUpper"] = ToUpper,
            ["ToLower"] = ToLower,
            ["Trim"] = Trim,
            ["TrimStart"] = TrimStart,
            ["TrimEnd"] = TrimEnd
        };

        /// <summary>
        /// IN
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void IN(MethodCallExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            sqlPack += " IN ";
            SqlBuilderProvider.In(expression.Arguments[1], sqlPack);
        }

        /// <summary>
        /// Not In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void NotIn(MethodCallExpression expression, SqlPack sqlPack)
        {
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            sqlPack += " NOT IN ";
            SqlBuilderProvider.In(expression.Arguments[1], sqlPack);
        }

        /// <summary>
        /// Like
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void Like(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case DatabaseType.SQLServer:
                    sqlPack += " LIKE '%' + ";
                    break;
                case DatabaseType.MySQL:
                case DatabaseType.PostgreSQL:
                    sqlPack += " LIKE CONCAT('%',";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.SQLite:
                    sqlPack += " LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlBuilderProvider.Where(expression.Arguments[1], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case DatabaseType.SQLServer:
                    sqlPack += " + '%'";
                    break;
                case DatabaseType.MySQL:
                case DatabaseType.PostgreSQL:
                    sqlPack += ",'%')";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.SQLite:
                    sqlPack += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// LikeLeft
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void LikeLeft(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case DatabaseType.SQLServer:
                    sqlPack += " LIKE '%' + ";
                    break;
                case DatabaseType.MySQL:
                case DatabaseType.PostgreSQL:
                    sqlPack += " LIKE CONCAT('%',";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.SQLite:
                    sqlPack += " LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlBuilderProvider.Where(expression.Arguments[1], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case DatabaseType.MySQL:
                case DatabaseType.PostgreSQL:
                    sqlPack += ")";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// LikeRight
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void LikeRight(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case DatabaseType.SQLServer:
                case DatabaseType.Oracle:
                case DatabaseType.SQLite:
                    sqlPack += " LIKE ";
                    break;
                case DatabaseType.MySQL:
                case DatabaseType.PostgreSQL:
                    sqlPack += " LIKE CONCAT(";
                    break;
                default:
                    break;
            }
            SqlBuilderProvider.Where(expression.Arguments[1], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case DatabaseType.SQLServer:
                    sqlPack += " + '%'";
                    break;
                case DatabaseType.MySQL:
                case DatabaseType.PostgreSQL:
                    sqlPack += ",'%')";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.SQLite:
                    sqlPack += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// NotLike
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void NotLike(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case DatabaseType.SQLServer:
                    sqlPack += " NOT LIKE '%' + ";
                    break;
                case DatabaseType.MySQL:
                case DatabaseType.PostgreSQL:
                    sqlPack += " NOT LIKE CONCAT('%',";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.SQLite:
                    sqlPack += " NOT LIKE '%' || ";
                    break;
                default:
                    break;
            }
            SqlBuilderProvider.Where(expression.Arguments[1], sqlPack);
            switch (sqlPack.DatabaseType)
            {
                case DatabaseType.SQLServer:
                    sqlPack += " + '%'";
                    break;
                case DatabaseType.MySQL:
                case DatabaseType.PostgreSQL:
                    sqlPack += ",'%')";
                    break;
                case DatabaseType.Oracle:
                case DatabaseType.SQLite:
                    sqlPack += " || '%'";
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Contains
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void Contains(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                if (expression.Arguments[0].NodeType == ExpressionType.MemberAccess)
                {
                    SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
                    sqlPack += " IN ";
                    SqlBuilderProvider.In(expression.Object, sqlPack);
                }
                else
                {
                    SqlBuilderProvider.Where(expression.Object, sqlPack);
                    switch (sqlPack.DatabaseType)
                    {
                        case DatabaseType.SQLServer:
                            sqlPack += " LIKE '%' + ";
                            break;
                        case DatabaseType.MySQL:
                        case DatabaseType.PostgreSQL:
                            sqlPack += " LIKE CONCAT('%',";
                            break;
                        case DatabaseType.Oracle:
                        case DatabaseType.SQLite:
                            sqlPack += " LIKE '%' || ";
                            break;
                        default:
                            break;
                    }
                    SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
                    switch (sqlPack.DatabaseType)
                    {
                        case DatabaseType.SQLServer:
                            sqlPack += " + '%'";
                            break;
                        case DatabaseType.MySQL:
                        case DatabaseType.PostgreSQL:
                            sqlPack += ",'%')";
                            break;
                        case DatabaseType.Oracle:
                        case DatabaseType.SQLite:
                            sqlPack += " || '%'";
                            break;
                        default:
                            break;
                    }
                }
            }
            else if (expression.Arguments.Count > 1 && expression.Arguments[1] is MemberExpression memberExpression)
            {
                SqlBuilderProvider.Where(memberExpression, sqlPack);
                sqlPack += " IN ";
                SqlBuilderProvider.In(expression.Arguments[0], sqlPack);
            }
        }

        /// <summary>
        /// IsNullOrEmpty
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void IsNullOrEmpty(MethodCallExpression expression, SqlPack sqlPack)
        {
            sqlPack += "(";
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            sqlPack += " IS NULL OR ";
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            sqlPack += " = ''";
            sqlPack += ")";
        }

        /// <summary>
        /// Equals
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void Equals(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                SqlBuilderProvider.Where(expression.Object, sqlPack);
            }
            var signIndex = sqlPack.Length;
            SqlBuilderProvider.Where(expression.Arguments[0], sqlPack);
            if (sqlPack.ToString().ToUpper().EndsWith("NULL"))
            {
                sqlPack.Sql.Insert(signIndex, " IS ");
            }
            else
            {
                sqlPack.Sql.Insert(signIndex, " = ");
            }
        }

        /// <summary>
        /// ToUpper
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void ToUpper(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                sqlPack += "UPPER(";
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                sqlPack += ")";
            }
        }

        /// <summary>
        /// ToLower
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void ToLower(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                sqlPack += "LOWER(";
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                sqlPack += ")";
            }
        }

        /// <summary>
        /// Trim
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void Trim(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                if (sqlPack.DatabaseType == DatabaseType.SQLServer)
                {
                    sqlPack += "LTRIM(RTRIM(";
                }
                else
                {
                    sqlPack += "TRIM(";
                }
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                if (sqlPack.DatabaseType == DatabaseType.SQLServer)
                {
                    sqlPack += "))";
                }
                else
                {
                    sqlPack += ")";
                }
            }
        }

        /// <summary>
        /// TrimStart
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void TrimStart(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                sqlPack += "LTRIM(";
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                sqlPack += ")";
            }
        }

        /// <summary>
        /// TrimEnd
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        private static void TrimEnd(MethodCallExpression expression, SqlPack sqlPack)
        {
            if (expression.Object != null)
            {
                sqlPack += "RTRIM(";
                SqlBuilderProvider.Where(expression.Object, sqlPack);
                sqlPack += ")";
            }
        }
        #endregion

        #region Override Base Class Methods
        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack In(MethodCallExpression expression, SqlPack sqlPack)
        {
            var val = expression?.ToObject();
            if (val != null)
            {
                sqlPack += "(";
                if (val.GetType().IsArray || typeof(IList).IsAssignableFrom(val.GetType()))
                {
                    var list = val as IList;
                    if (list?.Count > 0)
                    {
                        foreach (var item in list)
                        {
                            SqlBuilderProvider.In(Expression.Constant(item, item.GetType()), sqlPack);
                            sqlPack += ",";
                        }
                    }
                }
                else
                {
                    SqlBuilderProvider.In(Expression.Constant(val, val.GetType()), sqlPack);
                }
                if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                    sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
                sqlPack += ")";
            }
            return sqlPack;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Where(MethodCallExpression expression, SqlPack sqlPack)
        {
            var key = expression.Method;
            if (key.IsGenericMethod)
                key = key.GetGenericMethodDefinition();
            if (methods.TryGetValue(key.Name, out Action<MethodCallExpression, SqlPack> action))
            {
                action(expression, sqlPack);
                return sqlPack;
            }
            throw new NotImplementedException("无法解析方法" + expression.Method);
        }

        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression"></param>
        /// <param name="sqlPack"></param>
        /// <returns></returns>
        public override SqlPack Insert(MethodCallExpression expression, SqlPack sqlPack)
        {
            var fields = new List<string>();
            var array = expression.ToObject() as object[];
            for (var i = 0; i < array.Length; i++)
            {
                if (sqlPack.DatabaseType != DatabaseType.Oracle)
                    sqlPack.Sql.Append("(");
                if (i > 0 && sqlPack.DatabaseType == DatabaseType.Oracle)
                    sqlPack.Sql.Append(" UNION ALL SELECT ");
                var properties = array[i]?.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var type = p.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : p.DeclaringType;
                    (string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, p);
                    if (isInsert)
                    {
                        var value = p.GetValue(array[i], null);
                        if (value != null || (sqlPack.IsEnableNullValue && value == null))
                        {
                            sqlPack.AddDbParameter(value);
                            if (!fields.Contains(columnName)) fields.Add(columnName);
                            sqlPack += ",";
                        }
                    }
                }
                if (sqlPack[sqlPack.Length - 1] == ',')
                {
                    sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    if (sqlPack.DatabaseType != DatabaseType.Oracle)
                        sqlPack.Sql.Append("),");
                    else
                        sqlPack.Sql.Append(" FROM DUAL");
                }
            }
            if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
            sqlPack.Sql = new StringBuilder(string.Format(sqlPack.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack GroupBy(MethodCallExpression expression, SqlPack sqlPack)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlBuilderProvider.GroupBy(Expression.Constant(array[i], array[i].GetType()), sqlPack);
                }
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlPack</returns>
        public override SqlPack OrderBy(MethodCallExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            var array = (expression.ToObject() as IEnumerable<object>)?.ToList();
            if (array != null)
            {
                for (var i = 0; i < array.Count; i++)
                {
                    SqlBuilderProvider.OrderBy(Expression.Constant(array[i], array[i].GetType()), sqlPack);
                    if (i <= orders.Length - 1)
                    {
                        sqlPack += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                    }
                    else if (!array[i].ToString().ToUpper().Contains("ASC") && !array[i].ToString().ToUpper().Contains("DESC"))
                    {
                        sqlPack += " ASC,";
                    }
                    else
                    {
                        sqlPack += ",";
                    }
                }
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }
        #endregion
    }
}