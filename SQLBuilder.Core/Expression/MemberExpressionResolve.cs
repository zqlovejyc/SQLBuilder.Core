#region License
/***
 * Copyright © 2018-2019, 张强 (943620963@qq.com).
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

using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace SQLBuilder.Core
{
    /// <summary>
    /// 表示访问字段或属性
    /// </summary>
	public class MemberExpressionResolve : BaseSqlBuilder<MemberExpression>
    {
        #region Override Base Class Methods
        /// <summary>
        /// Insert
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Insert(MemberExpression expression, SqlPack sqlPack)
        {
            var objectArray = new List<object>();
            var fields = new List<string>();
            var obj = expression.ToObject();
            if (obj.GetType().IsArray)
            {
                objectArray.AddRange(obj as object[]);
            }
            else if (obj.GetType().Name == "List`1")
            {
                objectArray.AddRange(obj as IEnumerable<object>);
            }
            else
            {
                objectArray.Add(obj);
            }
            foreach (var item in objectArray)
            {
                sqlPack.Sql.Append("(");
                var properties = item?.GetType().GetProperties();
                foreach (var p in properties)
                {
                    var type = p.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : p.DeclaringType;
                    (string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, p);
                    if (isInsert)
                    {
                        var value = p.GetValue(item, null);
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
                    sqlPack.Sql.Append("),");
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
            {
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            sqlPack.Sql = new StringBuilder(string.Format(sqlPack.ToString(), string.Join(",", fields).TrimEnd(',')));
            return sqlPack;
        }

        /// <summary>
        /// Update
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
        public override SqlPack Update(MemberExpression expression, SqlPack sqlPack)
        {
            var obj = expression.ToObject();
            var properties = obj?.GetType().GetProperties();
            foreach (var item in properties)
            {
                var type = item.DeclaringType.ToString().Contains("AnonymousType") ? sqlPack.DefaultType : item.DeclaringType;
                (string columnName, bool isInsert, bool isUpdate) = sqlPack.GetColumnInfo(type, item);
                if (isUpdate)
                {
                    var value = item.GetValue(obj, null);
                    if (value != null || (sqlPack.IsEnableNullValue && value == null))
                    {
                        sqlPack += columnName + " = ";
                        sqlPack.AddDbParameter(value);
                        sqlPack += ",";
                    }
                }
            }
            if (sqlPack[sqlPack.Length - 1] == ',')
            {
                sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
            }
            return sqlPack;
        }

        /// <summary>
        /// Select
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Select(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            var tableName = sqlPack.GetTableName(type);
            sqlPack.SetTableAlias(tableName);
            string tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            sqlPack.SelectFields.Add(tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member, string.IsNullOrEmpty(tableAlias)).columnName);
            return sqlPack;
        }

        /// <summary>
        /// Join
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Join(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            var tableName = sqlPack.GetTableName(type);
            sqlPack.SetTableAlias(tableName);
            string tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member, string.IsNullOrEmpty(tableAlias)).columnName;
            return sqlPack;
        }

        /// <summary>
        /// Where
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Where(MemberExpression expression, SqlPack sqlPack)
        {
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                var tableName = sqlPack.GetTableName(type);
                sqlPack.SetTableAlias(tableName);
                var tableAlias = sqlPack.GetTableAlias(tableName);
                if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
                sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member, string.IsNullOrEmpty(tableAlias)).columnName;
            }
            else
            {
                sqlPack.AddDbParameter(expression.ToObject());
            }
            return sqlPack;
        }

        /// <summary>
        /// In
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack In(MemberExpression expression, SqlPack sqlPack)
        {
            var obj = expression.ToObject();
            if (obj is IEnumerable array)
            {
                sqlPack += "(";
                foreach (var item in array)
                {
                    SqlBuilderProvider.In(Expression.Constant(item), sqlPack);
                    sqlPack += ",";
                }
                if (sqlPack.Sql[sqlPack.Sql.Length - 1] == ',')
                {
                    sqlPack.Sql.Remove(sqlPack.Sql.Length - 1, 1);
                }
                sqlPack += ")";
            }
            return sqlPack;
        }

        /// <summary>
        /// GroupBy
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack GroupBy(MemberExpression expression, SqlPack sqlPack)
        {
            string tableName = string.Empty;
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                tableName = sqlPack.GetTableName(type);
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                tableName = sqlPack.GetTableName(sqlPack.DefaultType);
            }
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member, false).columnName + ",";
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
                            SqlBuilderProvider.GroupBy(Expression.Constant(item, item.GetType()), sqlPack);
                        }
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                    if (type == "List`1" && obj is List<string> list)
                    {
                        foreach (var item in list)
                        {
                            SqlBuilderProvider.GroupBy(Expression.Constant(item, item.GetType()), sqlPack);
                        }
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                    if (type == "String" && obj is string str)
                    {
                        SqlBuilderProvider.GroupBy(Expression.Constant(str, str.GetType()), sqlPack);
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                }
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
		public override SqlPack OrderBy(MemberExpression expression, SqlPack sqlPack, params OrderType[] orders)
        {
            string tableName = string.Empty;
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                var type = expression.Expression.Type != expression.Member.DeclaringType ?
                           expression.Expression.Type :
                           expression.Member.DeclaringType;
                tableName = sqlPack.GetTableName(type);
            }
            if (expression.Expression.NodeType == ExpressionType.Constant)
            {
                tableName = sqlPack.GetTableName(sqlPack.DefaultType);
            }
            sqlPack.SetTableAlias(tableName);
            var tableAlias = sqlPack.GetTableAlias(tableName);
            if (!string.IsNullOrEmpty(tableAlias)) tableAlias += ".";
            if (expression.Expression.NodeType == ExpressionType.Parameter)
            {
                sqlPack += tableAlias + sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member, false).columnName;
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
                            SqlBuilderProvider.OrderBy(Expression.Constant(array[i], array[i].GetType()), sqlPack);
                            if (i <= orders.Length - 1)
                            {
                                sqlPack += $" { (orders[i] == OrderType.Desc ? "DESC" : "ASC")},";
                            }
                            else
                            {
                                sqlPack += " ASC,";
                            }
                        }
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                    if (type == "List`1" && obj is List<string> list)
                    {
                        for (var i = 0; i < list.Count; i++)
                        {
                            SqlBuilderProvider.OrderBy(Expression.Constant(list[i], list[i].GetType()), sqlPack);
                            if (i <= orders.Length - 1)
                            {
                                sqlPack += $" { (orders[i] == OrderType.Desc ? "DESC" : "ASC")},";
                            }
                            else
                            {
                                sqlPack += " ASC,";
                            }
                        }
                        sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                    }
                    if (type == "String" && obj is string str)
                    {
                        SqlBuilderProvider.OrderBy(Expression.Constant(str, str.GetType()), sqlPack);
                        str = str.ToUpper();
                        if (!str.Contains("ASC") && !str.Contains("DESC"))
                        {
                            if (orders.Length >= 1)
                            {
                                sqlPack += $" { (orders[0] == OrderType.Desc ? "DESC" : "ASC")},";
                            }
                            else
                            {
                                sqlPack += " ASC,";
                            }
                            sqlPack.Sql.Remove(sqlPack.Length - 1, 1);
                        }
                    }
                }
            }
            return sqlPack;
        }

        /// <summary>
        /// Max
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Max(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlPack.Sql.Append($"SELECT MAX({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }

        /// <summary>
        /// Min
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Min(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlPack.Sql.Append($"SELECT MIN({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }

        /// <summary>
        /// Avg
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Avg(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlPack.Sql.Append($"SELECT AVG({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }

        /// <summary>
        /// Count
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Count(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlPack.Sql.Append($"SELECT COUNT({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }

        /// <summary>
        /// Sum
        /// </summary>
        /// <param name="expression">表达式树</param>
        /// <param name="sqlPack">sql打包对象</param>
        /// <returns>SqlPack</returns>
		public override SqlPack Sum(MemberExpression expression, SqlPack sqlPack)
        {
            var type = expression.Expression.Type != expression.Member.DeclaringType ?
                       expression.Expression.Type :
                       expression.Member.DeclaringType;
            sqlPack.Sql.Append($"SELECT SUM({sqlPack.GetColumnInfo(expression.Member.DeclaringType, expression.Member).columnName}) FROM {sqlPack.GetTableName(type)}");
            return sqlPack;
        }
        #endregion
    }
}