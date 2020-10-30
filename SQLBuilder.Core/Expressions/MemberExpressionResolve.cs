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

using SQLBuilder.Core.Entry;
using SQLBuilder.Core.Enums;
using SQLBuilder.Core.Extensions;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SQLBuilder.Core.Expressions
{
    /// <summary>
    /// 表示访问字段或属性
    /// </summary>
    public class MemberExpressionResolve : BaseExpression<MemberExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Insert(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var objectArray = new List<object>();
            var fields = new List<string>();
            var obj = expression.ToObject();
            if (obj.GetType().IsArray)
                objectArray.AddRange(obj as object[]);
            else if (obj.GetType().Name == "List`1")
                objectArray.AddRange(obj as IEnumerable<object>);
            else
                objectArray.Add(obj);
            for (var i = 0; i < objectArray.Count; i++)
            {
                if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                    sqlWrapper.Sql.Append("(");
                if (i > 0 && sqlWrapper.DatabaseType == DatabaseType.Oracle)
                    sqlWrapper.Sql.Append(" UNION ALL SELECT ");
                var properties = objectArray[i]?.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var type = p.DeclaringType.ToString().Contains("AnonymousType") ? sqlWrapper.DefaultType : p.DeclaringType;
                    (string columnName, bool isInsert, bool isUpdate) = sqlWrapper.GetColumnInfo(type, p);
                    if (isInsert)
                    {
                        var value = p.GetValue(objectArray[i], null);
                        if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                        {
                            sqlWrapper.AddDbParameter(value);
                            if (!fields.Contains(columnName)) fields.Add(columnName);
                            sqlWrapper += ",";
                        }
                    }
                }
                if (sqlWrapper[sqlWrapper.Length - 1] == ',')
                {
                    sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
                    if (sqlWrapper.DatabaseType != DatabaseType.Oracle)
                        sqlWrapper.Sql.Append("),");
                    else
                        sqlWrapper.Sql.Append(" FROM DUAL");
                }
            }
            if (sqlWrapper[sqlWrapper.Length - 1] == ',')
                sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
            sqlWrapper.Sql = new StringBuilder(string.Format(sqlWrapper.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlWrapper;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
        public override SqlWrapper Update(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var obj = expression.ToObject();
            var properties = obj?.GetType().GetProperties();
            foreach (var item in properties)
            {
                var type = item.DeclaringType.ToString().Contains("AnonymousType") ? sqlWrapper.DefaultType : item.DeclaringType;
                (string columnName, bool isInsert, bool isUpdate) = sqlWrapper.GetColumnInfo(type, item);
                if (isUpdate)
                {
                    var value = item.GetValue(obj, null);
                    if (value != null || (sqlWrapper.IsEnableNullValue && value == null))
                    {
                        sqlWrapper += columnName + " = ";
                        sqlWrapper.AddDbParameter(value);
                        sqlWrapper += ",";
                    }
                }
            }
            if (sqlWrapper[sqlWrapper.Length - 1] == ',')
            {
                sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
            }
            return sqlWrapper;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Select(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;

            var tableName = sqlWrapper.GetTableName(type);

            var parameter = expression.Expression as ParameterExpression;
            sqlWrapper.SetTableAlias(tableName, parameter?.Name);
            string tableAlias = sqlWrapper.GetTableAlias(tableName, parameter?.Name);

            if (!tableAlias.IsNullOrEmpty())
                tableAlias += ".";

            sqlWrapper.SelectFields.Add(tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName);
            return sqlWrapper;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Join(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;

            var parameter = expression.Expression as ParameterExpression;

            var tableName = sqlWrapper.GetTableName(type);
            string tableAlias = sqlWrapper.GetTableAlias(tableName, parameter?.Name);
            if (!tableAlias.IsNullOrEmpty())
                tableAlias += ".";

            sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
            return sqlWrapper;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Where(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            //此处判断expression的Member是否是可空值类型
            if (expression.Expression?.NodeType == ExpressionType.MemberAccess &&
                expression.Member.DeclaringType.IsNullable())
            {
                expression = expression.Expression as MemberExpression;
            }
            if (expression != null)
            {
                if (expression.Expression?.NodeType == ExpressionType.Parameter)
                {
                    var type = expression.Expression.Type != expression.Member.DeclaringType ?
                               expression.Expression.Type :
                               expression.Member.DeclaringType;
                    var tableName = sqlWrapper.GetTableName(type);
                    var tableAlias = (expression.Expression as ParameterExpression)?.Name;
                    tableAlias = sqlWrapper.GetTableAlias(tableName, tableAlias);
                    if (!tableAlias.IsNullOrEmpty()) tableAlias += ".";
                    sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                    //字段是bool类型
                    if (expression.NodeType == ExpressionType.MemberAccess &&
                        expression.Type.GetCoreType() == typeof(bool))
                    {
                        sqlWrapper += " = 1";
                    }
                }
                else
                {
                    sqlWrapper.AddDbParameter(expression.ToObject());
                }
            }
            return sqlWrapper;
        }

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper In(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var obj = expression.ToObject();
            if (obj is IEnumerable array)
            {
                sqlWrapper += "(";
                foreach (var item in array)
                {
                    SqlExpressionProvider.In(Expression.Constant(item), sqlWrapper);
                    sqlWrapper += ",";
                }
                if (sqlWrapper.Sql[sqlWrapper.Sql.Length - 1] == ',')
                {
                    sqlWrapper.Sql.Remove(sqlWrapper.Sql.Length - 1, 1);
                }
                sqlWrapper += ")";
            }
            return sqlWrapper;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper GroupBy(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var tableAlias = string.Empty;
            var tableName = string.Empty;

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                tableName = sqlWrapper.GetTableName(type);
                tableAlias = (expression.Expression as ParameterExpression)?.Name;
            }

            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                tableName = sqlWrapper.GetTableName(sqlWrapper.DefaultType);
            }

            tableAlias = sqlWrapper.GetTableAlias(tableName, tableAlias);
            if (!tableAlias.IsNullOrEmpty()) tableAlias += ".";
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                var obj = expression.ToObject();
                if (obj != null)
                {
                    var type = obj.GetType().Name;
                    if (type == "String[]" && obj is string[] array)
                    {
                        foreach (var item in array)
                        {
                            SqlExpressionProvider.GroupBy(Expression.Constant(item, item.GetType()), sqlWrapper);
                        }
                        sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
                    }
                    if (type == "List`1" && obj is List<string> list)
                    {
                        foreach (var item in list)
                        {
                            SqlExpressionProvider.GroupBy(Expression.Constant(item, item.GetType()), sqlWrapper);
                        }
                        sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
                    }
                    if (type == "String" && obj is string str)
                    {
                        SqlExpressionProvider.GroupBy(Expression.Constant(str, str.GetType()), sqlWrapper);
                        sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
                    }
                }
            }
            return sqlWrapper;
        }

        /// <summary>
        /// OrderBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <param name="orders">排序方式</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper OrderBy(MemberExpression expression, SqlWrapper sqlWrapper, params OrderType[] orders)
        {
            var tableAlias = string.Empty;
            var tableName = string.Empty;

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                tableName = sqlWrapper.GetTableName(type);
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                tableName = sqlWrapper.GetTableName(sqlWrapper.DefaultType);
            }

            tableAlias = sqlWrapper.GetTableAlias(tableName, tableAlias);

            if (!tableAlias.IsNullOrEmpty())
                tableAlias += ".";

            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                sqlWrapper += tableAlias + sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName;
                if (orders?.Length > 0)
                    sqlWrapper += $" { (orders[0] == OrderType.Descending ? "DESC" : "ASC")}";
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                var obj = expression.ToObject();
                if (obj != null)
                {
                    var type = obj.GetType().Name;
                    if (type == "String[]" && obj is string[] array)
                    {
                        for (var i = 0; i < array.Length; i++)
                        {
                            SqlExpressionProvider.OrderBy(Expression.Constant(array[i], array[i].GetType()), sqlWrapper);
                            if (i <= orders.Length - 1)
                            {
                                sqlWrapper += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                            }
                            else if (!array[i].ToUpper().Contains("ASC") && !array[i].ToUpper().Contains("DESC"))
                            {
                                sqlWrapper += " ASC,";
                            }
                            else
                            {
                                sqlWrapper += ",";
                            }
                        }
                        sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
                    }
                    if (type == "List`1" && obj is List<string> list)
                    {
                        for (var i = 0; i < list.Count; i++)
                        {
                            SqlExpressionProvider.OrderBy(Expression.Constant(list[i], list[i].GetType()), sqlWrapper);
                            if (i <= orders.Length - 1)
                            {
                                sqlWrapper += $" { (orders[i] == OrderType.Descending ? "DESC" : "ASC")},";
                            }
                            else if (!list[i].ToUpper().Contains("ASC") && !list[i].ToUpper().Contains("DESC"))
                            {
                                sqlWrapper += " ASC,";
                            }
                            else
                            {
                                sqlWrapper += ",";
                            }
                        }
                        sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
                    }
                    if (type == "String" && obj is string str)
                    {
                        SqlExpressionProvider.OrderBy(Expression.Constant(str, str.GetType()), sqlWrapper);
                        str = str.ToUpper();
                        if (!str.Contains("ASC") && !str.Contains("DESC"))
                        {
                            if (orders.Length >= 1)
                            {
                                sqlWrapper += $" { (orders[0] == OrderType.Descending ? "DESC" : "ASC")},";
                            }
                            else
                            {
                                sqlWrapper += " ASC,";
                            }
                            sqlWrapper.Sql.Remove(sqlWrapper.Length - 1, 1);
                        }
                    }
                }
            }
            return sqlWrapper;
        }

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Max(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlWrapper.Sql.Append($"SELECT MAX({sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlWrapper.GetTableName(type)}");
            return sqlWrapper;
        }

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Min(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlWrapper.Sql.Append($"SELECT MIN({sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlWrapper.GetTableName(type)}");
            return sqlWrapper;
        }

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Avg(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlWrapper.Sql.Append($"SELECT AVG({sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlWrapper.GetTableName(type)}");
            return sqlWrapper;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Count(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlWrapper.Sql.Append($"SELECT COUNT({sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlWrapper.GetTableName(type)}");
            return sqlWrapper;
        }

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlWrapper">sql打包对象</param>
        /// <returns>SqlWrapper</returns>
		public override SqlWrapper Sum(MemberExpression expression, SqlWrapper sqlWrapper)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlWrapper.Sql.Append($"SELECT SUM({sqlWrapper.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlWrapper.GetTableName(type)}");
            return sqlWrapper;
        }
        #endregion
    }
}